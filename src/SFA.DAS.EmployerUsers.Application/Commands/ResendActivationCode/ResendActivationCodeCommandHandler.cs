﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.EmployerUsers.Application.Services.Notification;
using SFA.DAS.EmployerUsers.Application.Validation;
using SFA.DAS.EmployerUsers.Domain.Data;

namespace SFA.DAS.EmployerUsers.Application.Commands.ResendActivationCode
{
    public class ResendActivationCodeCommandHandler : AsyncRequestHandler<ResendActivationCodeCommand>
    {
        private readonly IValidator<ResendActivationCodeCommand> _commandValidator;
        private readonly IUserRepository _userRepository;
        private readonly ICommunicationService _communicationService;

        public ResendActivationCodeCommandHandler(IValidator<ResendActivationCodeCommand> commandValidator, IUserRepository userRepository, ICommunicationService communicationService)
        {
            if (commandValidator == null)
                throw new ArgumentNullException(nameof(commandValidator));
            if (userRepository == null)
                throw new ArgumentNullException(nameof(userRepository));
            if (communicationService == null)
                throw new ArgumentNullException(nameof(communicationService));
            _commandValidator = commandValidator;
            _userRepository = userRepository;
            _communicationService = communicationService;
        }

        protected override async Task HandleCore(ResendActivationCodeCommand message)
        {
            var validationResult = _commandValidator.Validate(message);
            if (!validationResult.IsValid())
                throw new InvalidRequestException(validationResult.ValidationDictionary);

            var user = await _userRepository.GetById(message.UserId);

            if (user == null)
                throw new InvalidRequestException(new Dictionary<string,string> { {"UserNotFound","User not found"} });

            if (!user.IsActive)
            {
                await _communicationService.ResendActivationCodeMessage(user, Guid.NewGuid().ToString());
            }
        }
    }
}