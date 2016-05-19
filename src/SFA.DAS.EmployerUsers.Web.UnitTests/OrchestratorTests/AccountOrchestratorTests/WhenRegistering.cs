﻿using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.EmployerUsers.ApplicationLayer;
using SFA.DAS.EmployerUsers.ApplicationLayer.Commands.RegisterUser;
using SFA.DAS.EmployerUsers.Web.Models;
using SFA.DAS.EmployerUsers.Web.Orchestrators;

namespace SFA.DAS.EmployerUsers.Web.UnitTests.OrchestratorTests.AccountOrchestratorTests
{
    public class WhenRegistering
    {
        private AccountOrchestrator _accountOrchestrator;
        private Mock<IMediator> _mediator;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _accountOrchestrator = new AccountOrchestrator(_mediator.Object);
        }

        [Test]
        public async Task ThenABooleanValueIsReturned()
        {
            //Arrange
            var registerUserViewModel = new RegisterViewModel();

            //Act
            var actual = await _accountOrchestrator.Register(registerUserViewModel);

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsAssignableFrom<bool>(actual);
        }

        [Test]
        public async Task ThenTheRegisterUserCommandIsPassedOntoTheMediator()
        {
            //Arrange
            var email = "test@test.com";
            var password = "password";
            var confirmPassword = "password";
            var lastName = "tester";
            var firstName = "test";

            var registerUserViewModel = new RegisterViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = password,
                ConfirmPassword = confirmPassword
            };

            //Act
            var actual = await _accountOrchestrator.Register(registerUserViewModel);

            //Assert
            _mediator.Verify(x=>x.SendAsync(It.Is<RegisterUserCommand>(p=>p.Email.Equals(email) && p.FirstName.Equals(firstName) && p.LastName.Equals(lastName) && p.Password.Equals(password) && p.ConfirmPassword.Equals(confirmPassword))),Times.Once);
            Assert.IsTrue(actual);
        }

        [Test]
        public async Task ThenFalseIsReturnedWhenTheRegisterUserCommandHandlerThrowsAnException()
        {
            //Arrange
            _mediator.Setup(x => x.SendAsync(It.IsAny<RegisterUserCommand>())).ThrowsAsync(new InvalidRequestException(new [] {"Something"}));

            //Act
            var actual = await _accountOrchestrator.Register(new RegisterViewModel());

            //Assert
            Assert.IsFalse(actual);

        }
        
    }
}