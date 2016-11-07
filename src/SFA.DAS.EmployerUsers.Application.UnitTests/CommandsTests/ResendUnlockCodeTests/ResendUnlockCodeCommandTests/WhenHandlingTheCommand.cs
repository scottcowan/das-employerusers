﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerUsers.Application.Commands.ResendUnlockCode;
using SFA.DAS.EmployerUsers.Application.Events.AccountLocked;
using SFA.DAS.EmployerUsers.Application.Validation;

namespace SFA.DAS.EmployerUsers.Application.UnitTests.CommandsTests.ResendUnlockCodeTests.ResendUnlockCodeCommandTests
{
    public class WhenHandlingTheCommand
    {
        private ResendUnlockCodeCommandHandler _resendUnlockCodeCommandHandler;
        private Mock<IValidator<ResendUnlockCodeCommand>> _resendUnlockCodeCommandValidator;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _resendUnlockCodeCommandValidator = new Mock<IValidator<ResendUnlockCodeCommand>>();
            _mediator = new Mock<IMediator>();
            _resendUnlockCodeCommandHandler = new ResendUnlockCodeCommandHandler(_resendUnlockCodeCommandValidator.Object, _mediator.Object);
        }

        [Test]
        public async Task ThenTheValidatorIsCalled()
        {

            //Arrange
            _resendUnlockCodeCommandValidator.Setup(x => x.Validate(It.IsAny<ResendUnlockCodeCommand>())).Returns(new ValidationResult() );

            //Act
            await _resendUnlockCodeCommandHandler.Handle(new ResendUnlockCodeCommand());

            //Assert
            _resendUnlockCodeCommandValidator.Verify(x => x.Validate(It.IsAny<ResendUnlockCodeCommand>()),Times.Once());
        }

        [Test]
        public async Task ThenTheMediatorIsCalledIfTheCommandIsValid()
        {
            //Arrange
            var email = "test@local";
            _resendUnlockCodeCommandValidator.Setup(x => x.Validate(It.IsAny<ResendUnlockCodeCommand>())).Returns(new ValidationResult());

            //Act
            await _resendUnlockCodeCommandHandler.Handle(new ResendUnlockCodeCommand {Email = email});

            //Assert
            _mediator.Verify(x => x.PublishAsync(It.Is<AccountLockedEvent>(s=>s.ResendUnlockCode && s.User.Email == email)), Times.Once());
        }

        [Test]
        public void ThenTheMediatorIsNotCalledIfTheCommandIsNotValid()
        {
            //Arrange
            var email = "test@local";
            _resendUnlockCodeCommandValidator.Setup(x => x.Validate(It.IsAny<ResendUnlockCodeCommand>())).Returns(new ValidationResult {ValidationDictionary = new Dictionary<string, string> { { "", ""} }});

            //Act
            Assert.ThrowsAsync<InvalidRequestException>(async () => await _resendUnlockCodeCommandHandler.Handle(new ResendUnlockCodeCommand { Email = email }));
            
            //Assert
            _mediator.Verify(x => x.PublishAsync(It.IsAny<AccountLockedEvent>()), Times.Never());
        }
    }
}
