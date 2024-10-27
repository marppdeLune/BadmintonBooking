using Microsoft.AspNetCore.Mvc;
using BadmintonBooking.Controllers;
using BadmintonBooking.Data;
using Microsoft.Extensions.Logging;

namespace BadmintonBooking.Tests.Controllers
{
    [TestFixture]
    public class AdminControllerTest
    {
        private AdminController _controller;
        private BadmintonBookingContext _context;
        private readonly ILogger<AdminController> _logger;

        [SetUp]
        public void Setup()
        {
            // Initialize the admin controller
            _controller = new AdminController(_context, _logger);
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
