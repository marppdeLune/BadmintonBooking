using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using BadmintonBooking.Controllers;
using BadmintonBooking.Data;
using BadmintonBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;


namespace BadmintonBooking.Tests.Controllers
{
    [TestFixture]
    public class ReceptionistBookingControllerTests
    {
        private Mock<BadmintonBookingContext> _mockContext;
        private ReceptionistBookingController _controller;

        [SetUp]
        public void Setup()
        {
            // Setup the mock database context
            _mockContext = new Mock<BadmintonBookingContext>();

            // Initialize the controller
            _controller = new ReceptionistBookingController(_mockContext.Object);
        }

        [Test]
        public async Task BookingSummaryForReceptionist_ReturnsViewResult_WithCorrectModel()
        {
            // Arrange
            int bookingId = 1;

            var court = new Court { CourtId = 1, CourtName = "Court 1", Price = 25.00M };
            var player = new Player { UserId = 1, Username = "player1" };
            var booking = new Booking
            {
                BookingId = bookingId,
                Court = court,
                User = player,
                StartTime = System.DateTime.Now.AddHours(1),
                EndTime = System.DateTime.Now.AddHours(2),
                Price = court.Price,
                Status = "Booked"
            };

            var bookings = new List<Booking> { booking }.AsQueryable();

            // Setup mock for Bookings in the context
            var mockSet = new Mock<DbSet<Booking>>();
            mockSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookings.Provider);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookings.Expression);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookings.ElementType);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookings.GetEnumerator());

            _mockContext.Setup(c => c.Bookings).Returns(mockSet.Object);

            // Act
            var result = await _controller.BookingSummaryForReceptionist(bookingId);

            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsInstanceOf<Booking>(viewResult.Model);
            var model = viewResult.Model as Booking;
            Assert.AreEqual(bookingId, model.BookingId);
        }

        [Test]
        public async Task BookingSummaryForReceptionist_ReturnsNotFound_WhenBookingNotExists()
        {
            // Arrange
            int nonExistentBookingId = 999;

            var bookings = new List<Booking>().AsQueryable();

            // Setup mock for Bookings in the context
            var mockSet = new Mock<DbSet<Booking>>();
            mockSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookings.Provider);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookings.Expression);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookings.ElementType);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookings.GetEnumerator());

            _mockContext.Setup(c => c.Bookings).Returns(mockSet.Object);

            // Act
            var result = await _controller.BookingSummaryForReceptionist(nonExistentBookingId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
