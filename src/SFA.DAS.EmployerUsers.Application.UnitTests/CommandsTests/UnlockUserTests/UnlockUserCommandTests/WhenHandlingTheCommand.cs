﻿using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SFA.DAS.EmployerUsers.Application.Commands.UnlockUser;
using Moq;
using SFA.DAS.EmployerUsers.Application.Validation;
using SFA.DAS.EmployerUsers.Domain;
using SFA.DAS.EmployerUsers.Domain.Data;

namespace SFA.DAS.EmployerUsers.Application.UnitTests.CommandsTests.UnlockUserTests.UnlockUserCommandTests
{
    public class WhenHandlingTheCommand
    {
        private UnlockUserCommandHandler _unlockUserCommand;
        private Mock<IValidator<UnlockUserCommand>> _unlockUserCommandValidator;
        private Mock<IUserRepository> _userRepositry;
        private const string ExpectedEmail =  "test@user.local";
        private const string NotAUser = "not@user.local";

        [SetUp]
        public void Arrange()
        {

            _unlockUserCommandValidator = new Mock<IValidator<UnlockUserCommand>>();
            _unlockUserCommandValidator.Setup(x => x.Validate(It.IsAny<UnlockUserCommand>())).Returns(new ValidationResult());
            _userRepositry = new Mock<IUserRepository>();
            _userRepositry.Setup(x => x.GetByEmailAddress(ExpectedEmail)).ReturnsAsync(new User {Email = ExpectedEmail});
            _userRepositry.Setup(x => x.GetByEmailAddress(NotAUser)).ReturnsAsync(null);
            _unlockUserCommand = new UnlockUserCommandHandler(_unlockUserCommandValidator.Object, _userRepositry.Object);
        }

        [Test]
        public async Task ThenTheCommandIsCheckedToSeeIfItIsValid()
        {
            //Arrange
            var unlockUserCommand = new UnlockUserCommand { Email = ExpectedEmail };

            //Act
            await _unlockUserCommand.Handle(unlockUserCommand);

            //Assert
            _unlockUserCommandValidator.Verify(x => x.Validate(unlockUserCommand), Times.Once);
        }

        [Test]
        public void ThenAnInvalidRequestExceptionIsThrownIfTheCommandIsNotValid()
        {
            //Arrange
            _unlockUserCommandValidator.Setup(x => x.Validate(It.IsAny<UnlockUserCommand>())).Returns(new ValidationResult { ValidationDictionary = { { "", "" } } });

            //Act
            Assert.ThrowsAsync<InvalidRequestException>(async () => await _unlockUserCommand.Handle(new UnlockUserCommand()));
        }

        [Test]
        public async Task ThenTheUserRespositoryIsCalledIfTheCommandIsValid()
        {
            //Arrange
            var unlockUserCommand = new UnlockUserCommand { Email = ExpectedEmail };

            //Act
            await _unlockUserCommand.Handle(unlockUserCommand);

            //Assert
            _userRepositry.Verify(x => x.Update(It.IsAny<User>()), Times.Once);
        }

        [Test]
        public async Task ThenTheUserIsRetrievedFromTheUserRepository()
        {
            //Arrange
            var unlockUserCommand = new UnlockUserCommand {Email = ExpectedEmail};

            //Act
            await _unlockUserCommand.Handle(unlockUserCommand);

            //Assert
            _userRepositry.Verify(x => x.GetByEmailAddress(ExpectedEmail), Times.Once);
        }

        [Test]
        public async Task ThenTheUserRespositoryIsCalledWithTheUserFromTheCommandAndTheUserIsSetToActive()
        {
            //Arrange
            var unlockUserCommand = new UnlockUserCommand
            {
                UnlockCode = "123RTY098",
                Email = ExpectedEmail
            };

            //Act
            await _unlockUserCommand.Handle(unlockUserCommand);

            //Assert
            _userRepositry.Verify(x => x.Update(It.Is<User>(c => !c.IsActive && c.Email == ExpectedEmail && !c.IsLocked && c.FailedLoginAttempts == 0)), Times.Once);
        }

        [Test]
        public async Task ThenTheUserRepositoryIsNotUpdatedIfTheUserDoesNotExist()
        {
            //Arrange
            var unlockUserCommand = new UnlockUserCommand
            {
                UnlockCode = "123RTY098",
                Email = NotAUser
            };

            //Act
            await _unlockUserCommand.Handle(unlockUserCommand);

            //Assert
            _userRepositry.Verify(x => x.Update(It.IsAny<User>()), Times.Never);
        }

        [Test]
        public void ThenAnArgumentNullExceptionIsThrownIfTheCommandIsNull()
        {
            //Act
            Assert.ThrowsAsync<ArgumentNullException>(async () => await _unlockUserCommand.Handle(null));
        }

    }
}