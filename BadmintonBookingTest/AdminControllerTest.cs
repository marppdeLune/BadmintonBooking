using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Controllers;

namespace BadmintonBooking.Tests.Controllers
{
    [TestFixture]
    public class AdminControllerTest
    {
        private AdminController _controller;

        [SetUp]
        public void Setup()
        {
            // Initialize the admin controller
            _controller = new AdminController();
        }

        [TearDown]
        public void TearDown()
        {
            // Dispose the controller
            _controller.Dispose();
        }

        [Test]
        public void CreatePlayer_ReturnsViewResult()
        {
            // Act
            var result = _controller.CreatePlayer();
            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }
    }
}
