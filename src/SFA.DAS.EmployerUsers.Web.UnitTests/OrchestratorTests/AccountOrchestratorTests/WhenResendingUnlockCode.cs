﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerUsers.Application;
using SFA.DAS.EmployerUsers.Application.Commands.ResendUnlockCode;
using SFA.DAS.EmployerUsers.Web.Authentication;
using SFA.DAS.EmployerUsers.Web.Models;
using SFA.DAS.EmployerUsers.Web.Orchestrators;

namespace SFA.DAS.EmployerUsers.Web.UnitTests.OrchestratorTests.AccountOrchestratorTests
{
    public class WhenResendingUnlockCode
    {
        private Mock<IMediator> _mediator;
        private Mock<IOwinWrapper> _owinWrapper;
        private AccountOrchestrator _accountOrchestrator;

        [SetUp]
        public void Arrange()
        {

            _mediator = new Mock<IMediator>();
            _owinWrapper = new Mock<IOwinWrapper>();
            _accountOrchestrator = new AccountOrchestrator(_mediator.Object, _owinWrapper.Object);
        }

        [Test]
        public async Task ThenTheUnlockLockUserViewModelIsReturned()
        {

            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<ResendUnlockCodeCommand>())).ThrowsAsync(new InvalidRequestException(new Dictionary<string, string> { { "Email", "Some Error" } }));
            var model = new UnlockUserViewModel();

            //Act
            var actual = await _accountOrchestrator.ResendUnlockCode(model);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<UnlockUserViewModel>(actual);
            Assert.IsFalse(actual.UnlockCodeSent);
        }

        [Test]
        public async Task ThenTheMediatorIsCalledWithAAccountLockedEvent()
        {
            //Arrange
            var expectedEmail = "test@local.com";
            var model = new UnlockUserViewModel { Email = expectedEmail };

            //Act
            await _accountOrchestrator.ResendUnlockCode(model);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<ResendUnlockCodeCommand>(s => s.Email == expectedEmail)), Times.Once());
        }

        [Test]
        public async Task ThenAnInvalidRequestExceptionIsHandledAndTheErrorDictionaryIsPopulatedIfTheAccountLockedEventIsNotValid()
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<ResendUnlockCodeCommand>())).ThrowsAsync(new InvalidRequestException(new Dictionary<string, string> { { "Email", "Some Error" } }));

            //Act
            var actual = await _accountOrchestrator.ResendUnlockCode(new UnlockUserViewModel());

            //Assert
            Assert.IsNotEmpty(actual.ErrorDictionary);
            Assert.Contains(new KeyValuePair<string,string>("Email","Some Error"),actual.ErrorDictionary);
        }

        [Test]
        public async Task ThenTheResendUnlockCodeSuccessFlagIsSetToTrueIfAValidMessageIsSent()
        {
            //Arrange
            var expectedEmail = "test@local.com";
            var model = new UnlockUserViewModel { Email = expectedEmail };

            //Act
            var actual = await _accountOrchestrator.ResendUnlockCode(model);

            //Assert
            _mediator.Verify(x => x.SendAsync(It.Is<ResendUnlockCodeCommand>(s => s.Email == expectedEmail)), Times.Once());
            Assert.IsTrue(actual.UnlockCodeSent);
        }
    }
}
