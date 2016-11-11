﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CodeGenerator;
using SFA.DAS.EmployerUsers.Application.Services.Notification;
using SFA.DAS.EmployerUsers.Application.Validation;
using SFA.DAS.EmployerUsers.Domain.Data;

namespace SFA.DAS.EmployerUsers.Application.Commands.RequestChangeEmail
{
    public class RequestChangeEmailCommandHandler : IAsyncRequestHandler<RequestChangeEmailCommand, Unit>
    {
        private readonly IValidator<RequestChangeEmailCommand> _validator;
        private readonly IUserRepository _userRepository;
        private readonly ICodeGenerator _codeGenerator;
        private readonly ICommunicationService _communicationService;

        public RequestChangeEmailCommandHandler(IValidator<RequestChangeEmailCommand> validator, IUserRepository userRepository, ICodeGenerator codeGenerator, ICommunicationService communicationService)
        {
            _validator = validator;
            _userRepository = userRepository;
            _codeGenerator = codeGenerator;
            _communicationService = communicationService;
        }

        public async Task<Unit> Handle(RequestChangeEmailCommand message)
        {
            var validationResult = _validator.Validate(message);
            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var user = await _userRepository.GetById(message.UserId);
            if (user == null)
            {
                throw new InvalidRequestException(new Dictionary<string, string> { { "", "Cannot find user" } });
            }

            user.PendingEmail = message.NewEmailAddress;
            user.AddSecurityCode(new Domain.SecurityCode
            {
                Code = _codeGenerator.GenerateAlphaNumeric(),
                CodeType = Domain.SecurityCodeType.ConfirmEmailCode,
                ExpiryTime = DateTime.UtcNow.AddDays(1)
            });
            await _userRepository.Update(user);

            await _communicationService.SendConfirmEmailChangeMessage(user, Guid.NewGuid().ToString());

            return Unit.Value;
        }
    }
}