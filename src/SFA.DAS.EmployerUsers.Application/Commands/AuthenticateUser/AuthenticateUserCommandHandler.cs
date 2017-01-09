﻿using System.Threading.Tasks;
using MediatR;
using NLog;
using SFA.DAS.Configuration;
using SFA.DAS.EmployerUsers.Application.Events.AccountLocked;
using SFA.DAS.EmployerUsers.Application.Services.Password;
using SFA.DAS.EmployerUsers.Application.Validation;
using SFA.DAS.EmployerUsers.Domain;
using SFA.DAS.EmployerUsers.Domain.Data;
using SFA.DAS.EmployerUsers.Infrastructure.Configuration;

namespace SFA.DAS.EmployerUsers.Application.Commands.AuthenticateUser
{
    public class AuthenticateUserCommandHandler : IAsyncRequestHandler<AuthenticateUserCommand, User>
    {
        private readonly ILogger _logger;
        private readonly IValidator<AuthenticateUserCommand> _validator;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordService _passwordService;
        private readonly IConfigurationService _configurationService;
        private readonly IMediator _mediator;

        public AuthenticateUserCommandHandler(IUserRepository userRepository, IPasswordService passwordService, IConfigurationService configurationService, IMediator mediator, ILogger logger, IValidator<AuthenticateUserCommand> validator)
        {
            _userRepository = userRepository;
            _passwordService = passwordService;
            _configurationService = configurationService;
            _mediator = mediator;
            _logger = logger;
            _validator = validator;
        }

        public async Task<User> Handle(AuthenticateUserCommand message)
        {
            _logger.Debug($"Received AuthenticateUserCommand for user '{message.EmailAddress}'");

            var validationResult = await _validator.ValidateAsync(message);

            if (!validationResult.IsValid())
            {
                throw new InvalidRequestException(validationResult.ValidationDictionary);
            }

            var user = await _userRepository.GetByEmailAddress(message.EmailAddress);
            if (user == null)
            {
                return null;
            }

            if (user.IsLocked)
            {
                throw new AccountLockedException(user);
            }

            var isPasswordCorrect = await _passwordService.VerifyAsync(message.Password, user.Password, user.Salt, user.PasswordProfileId);
            if (!isPasswordCorrect)
            {
                await ProcessFailedLogin(user, message.ReturnUrl);
                return null;
            }

            if (user.FailedLoginAttempts > 0)
            {
                user.FailedLoginAttempts = 0;
                await _userRepository.Update(user);
            }

            return user;
        }

        private async Task ProcessFailedLogin(User user, string returnUrl)
        {
            var config = await GetAccountConfiguration();

            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= config.AllowedFailedLoginAttempts)
            {
                _logger.Info($"Locking user '{user.Email}' (id: {user.Id})");
                user.IsLocked = true;
            }
            await _userRepository.Update(user);

            if (user.IsLocked)
            {
                _logger.Debug($"Publishing event for user '{user.Email}' (id: {user.Id}) being locked");
                await _mediator.PublishAsync(new AccountLockedEvent { ReturnUrl = returnUrl, User = user });
                throw new AccountLockedException(user);
            }
        }
        private async Task<AccountConfiguration> GetAccountConfiguration()
        {
            return (await _configurationService.GetAsync<EmployerUsersConfiguration>())?.Account;
        }
    }
}
