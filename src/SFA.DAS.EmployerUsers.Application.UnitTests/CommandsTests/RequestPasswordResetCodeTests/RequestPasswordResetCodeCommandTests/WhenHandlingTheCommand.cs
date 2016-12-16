﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.CodeGenerator;
using SFA.DAS.EmployerUsers.Application.Commands.RequestPasswordResetCode;
using SFA.DAS.EmployerUsers.Application.Services.Notification;
using SFA.DAS.EmployerUsers.Application.UnitTests.TestHelpers;
using SFA.DAS.EmployerUsers.Domain;
using SFA.DAS.EmployerUsers.Domain.Data;
using SFA.DAS.EmployerUsers.Domain.Links;
using SFA.DAS.TimeProvider;

namespace SFA.DAS.EmployerUsers.Application.UnitTests.CommandsTests.RequestPasswordResetCodeTests.RequestPasswordResetCodeCommandTests
{
    public class WhenHandlingTheCommand
    {
        private const string RegistrationLink = "register-here";

        private Mock<IUserRepository> _userRepository;
        private Mock<ICommunicationService> _communicationSerivce;
        private Mock<ICodeGenerator> _codeGenerator;
        private Mock<ILinkBuilder> _linkBuilder;
        private RequestPasswordResetCodeCommandHandler _commandHandler;

        [SetUp]
        public void Setup()
        {
            _userRepository = new Mock<IUserRepository>();

            _communicationSerivce = new Mock<ICommunicationService>();

            _codeGenerator = new Mock<ICodeGenerator>();

            _linkBuilder = new Mock<ILinkBuilder>();
            _linkBuilder.Setup(b => b.GetRegistrationUrl())
                .Returns(RegistrationLink);

            _commandHandler = new RequestPasswordResetCodeCommandHandler(new RequestPasswordResetCodeCommandValidator(), 
                _userRepository.Object, _communicationSerivce.Object, _codeGenerator.Object, _linkBuilder.Object);
        }

        [TearDown]
        public void Teardown()
        {
            DateTimeProvider.ResetToDefault();
        }

        [Test]
        public void InvalidCommandThrowsInvalidRequestException()
        {
            var command = new RequestPasswordResetCodeCommand();

            var invalidRequestException = Assert.ThrowsAsync<InvalidRequestException>(async () => await _commandHandler.Handle(command));

            Assert.That(invalidRequestException.ErrorMessages.Count, Is.EqualTo(1));
            Assert.That(invalidRequestException.ErrorMessages.Keys.Contains("Email"));
        }

        [Test]
        public async Task ThenItShouldSendNoAccountToPasswordResetEmailIfNoUserFound()
        {
            //Arrange
            var command = GetRequestPasswordResetCodeCommand();
            _userRepository.Setup(x => x.GetByEmailAddress(command.Email)).ReturnsAsync((User)null);

            //Act
            await _commandHandler.Handle(command);

            //Assert
            _communicationSerivce.Verify(x => x.SendNoAccountToPasswordResetMessage(It.IsAny<string>(), It.IsAny<string>(), RegistrationLink), Times.Once);
            _communicationSerivce.Verify(x => x.SendPasswordResetCodeMessage(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _userRepository.Verify(x => x.Update(It.IsAny<User>()), Times.Never);

        }

        [Test]
        public async Task KnownUserWithActiveResetResendsExistingCode()
        {
            var command = GetRequestPasswordResetCodeCommand();

            var code = "ABCDEF";
            var expiryTime = DateTimeProvider.Current.UtcNow.AddHours(1);
            var existingUser = new User
            {
                Email = command.Email,
                SecurityCodes = new[]
                {
                    new SecurityCode
                    {
                        Code = code,
                        CodeType = SecurityCodeType.PasswordResetCode,
                        ExpiryTime = expiryTime
                    }
                }
            };

            _userRepository.Setup(x => x.GetByEmailAddress(command.Email)).ReturnsAsync(existingUser);

            await _commandHandler.Handle(command);

            _communicationSerivce.Verify(x => x.SendPasswordResetCodeMessage(It.Is<User>(u => u.Email == existingUser.Email 
                                                                                           && u.SecurityCodes.Any(sc => sc.Code == code
                                                                                                                     && sc.CodeType == SecurityCodeType.PasswordResetCode
                                                                                                                     && sc.ExpiryTime == expiryTime)
                                                                                        ), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task KnownUserWithExpiredCodeSendsNewCode()
        {
            DateTimeProvider.Current = new FakeTimeProvider(DateTime.UtcNow);
            const string newCode = "FEDCBA";

            var command = GetRequestPasswordResetCodeCommand();

            var existingUser = new User
            {
                Email = command.Email,
                SecurityCodes = new[]
                {
                    new SecurityCode
                    {
                        Code = "ABCDEF",
                        CodeType = SecurityCodeType.PasswordResetCode,
                        ExpiryTime = DateTimeProvider.Current.UtcNow.AddHours(-1)
                    }
                }
            };

            _codeGenerator.Setup(x => x.GenerateAlphaNumeric(6)).Returns(newCode);

            _userRepository.Setup(x => x.GetByEmailAddress(command.Email)).ReturnsAsync(existingUser);

            await _commandHandler.Handle(command);

            _communicationSerivce.Verify(x => x.SendPasswordResetCodeMessage(It.Is<User>(u => u.Email == existingUser.Email 
                                                                                                      && u.SecurityCodes.Any(sc => sc.Code == newCode
                                                                                                                                && sc.CodeType == SecurityCodeType.PasswordResetCode
                                                                                                                                && sc.ExpiryTime == DateTimeProvider.Current.UtcNow.AddDays(1))
                                                                                        ), It.IsAny<string>()), Times.Once);
        }
        

        private RequestPasswordResetCodeCommand GetRequestPasswordResetCodeCommand()
        {
            return new RequestPasswordResetCodeCommand
            {
                Email = "test.user@test.org"
            };
        }
    }
}