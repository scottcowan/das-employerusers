﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerUsers.Application.Commands.ChangeEmail;
using SFA.DAS.EmployerUsers.Application.Validation;
using SFA.DAS.EmployerUsers.Domain;
using SFA.DAS.EmployerUsers.Domain.Data;

namespace SFA.DAS.EmployerUsers.Application.UnitTests.CommandsTests.ChangeEmailTests.ChangeEmailCommandHandlerTests
{
    public class WhenHandling
    {
        private const string UserId = "USER1";
        private const string SecurityCode = "1A2B3C0";
        private const string Password = "password";
        private const string OldEmail = "user.one@unit.tests";
        private const string NewEmail = "user1@unit.tests";
        private const string ReturnUrl = "http://unit.tests";

        private Mock<IValidator<ChangeEmailCommand>> _validator;
        private Mock<IUserRepository> _userRepository;
        private ChangeEmailCommandHandler _handler;
        private ChangeEmailCommand _command;

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<IValidator<ChangeEmailCommand>>();
            _validator.Setup(v => v.Validate(It.IsAny<ChangeEmailCommand>()))
                .Returns(new ValidationResult());

            _userRepository = new Mock<IUserRepository>();

            _handler = new ChangeEmailCommandHandler(_validator.Object, _userRepository.Object);

            _command = new ChangeEmailCommand
            {
                User = new Domain.User
                {
                    Id = UserId,
                    Email = OldEmail,
                    PendingEmail = NewEmail,
                    SecurityCodes = new[]
                    {
                        new Domain.SecurityCode
                        {
                            Code = SecurityCode,
                            CodeType = Domain.SecurityCodeType.ConfirmEmailCode,
                            ExpiryTime = DateTime.MaxValue,
                            ReturnUrl = ReturnUrl
                        },
                        new Domain.SecurityCode
                        {
                            Code = "WRONG-CODE",
                            CodeType = Domain.SecurityCodeType.ConfirmEmailCode,
                            ExpiryTime = DateTime.Now.AddDays(30),
                            ReturnUrl = "http://not-here"
                        }
                    }
                },
                SecurityCode = SecurityCode,
                Password = Password
            };
        }

        [Test]
        public async Task ThenItShouldReturnAnInstanceOfChangeEmailCommandResult()
        {
            // Act
            var actual = await _handler.Handle(_command);

            // Assert
            Assert.IsNotNull(actual);
        }

        [Test]
        public async Task ThenItShouldReturnReturnUrlFromSpecificSecurityCode()
        {
            // Act
            var actual = await _handler.Handle(_command);

            // Assert
            Assert.AreEqual(ReturnUrl, actual.ReturnUrl);
        }

        [Test]
        public void ThenItShouldThrowAnInvalidRequestExceptionIfTheCommandIsNotValid()
        {
            // Arrange
            _validator.Setup(v => v.Validate(It.IsAny<ChangeEmailCommand>()))
                .Returns(new ValidationResult
                {
                    ValidationDictionary = new Dictionary<string, string>
                    {
                        { "", "Error" }
                    }
                });

            // Act + Assert
            Assert.ThrowsAsync<InvalidRequestException>(async () => await _handler.Handle(_command));
        }

        [Test]
        public async Task ThenItShouldUpdateTheUserWithThePendingEmail()
        {
            // Act
            await _handler.Handle(_command);

            // Assert
            _userRepository.Verify(r => r.Update(It.Is<User>(u => u.Id == UserId && u.Email == NewEmail)), Times.Once);
        }

        [Test]
        public async Task ThenItShouldUpdateTheUserToRemoveThePendingEmail()
        {
            // Act
            await _handler.Handle(_command);

            // Assert
            _userRepository.Verify(r => r.Update(It.Is<User>(u => u.Id == UserId && string.IsNullOrEmpty(u.PendingEmail))), Times.Once);
        }

        [Test]
        public async Task ThenItShouldRemoveTheChangeEmailCodesFromTheUser()
        {
            // Act
            await _handler.Handle(_command);

            // Assert
            _userRepository.Verify(r => r.Update(It.Is<User>(u => u.Id == UserId && !u.SecurityCodes.Any(sc => sc.CodeType == SecurityCodeType.ConfirmEmailCode))), Times.Once);
        }
    }
}
