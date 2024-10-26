using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using BadmintonBooking.Controllers;
using BadmintonBooking.Data;
using BadmintonBooking.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace BadmintonBooking.Tests.Controllers
{
    [TestFixture]
    public class ReceptionistBookingControllerTests
    {
        private Mock<BadmintonBookingContext> _mockContext;
        private ReceptionistBookingController _controller;

        public ReceptionistBookingController GetController()
        {
            return _controller;
        }

        public void SetController(ReceptionistBookingController value)
        {
            _controller = value;
        }

        public ReceptionistBookingControllerTests(ReceptionistBookingController controller)
        {
            SetController(controller);
        }

        [SetUp]
        public void Setup()
        {
            // Setup the mock database context
            _mockContext = new Mock<BadmintonBookingContext>();

            // Initialize the controller
            SetController(new ReceptionistBookingController(_mockContext.Object));
        }
        [TearDown]
        public void TearDown()
        {
            if (_mockContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
            SetController(null);
        }


        [Test]
        public async Task BookingSummaryForReceptionist_ReturnsViewResult_WithCorrectModel()
        {
            // Arrange
            int bookingId = 1;
            string selectedTimeSlot = "09:00 AM - 10:00 AM";
            DateTime selectedDate = DateTime.Now;

            var court = new Court { CourtId = 1, CourtName = "Court 1", Price = 25.00M };
            var player = new Player { UserId = 1, Username = "player1" };
            var booking = new Booking
            {
                BookingId = bookingId,
                Court = court,
                User = player,
                StartTime = selectedDate.AddHours(9),
                EndTime = selectedDate.AddHours(10),
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
            var result = await GetController().BookingSummaryForReceptionist(bookingId, selectedTimeSlot, selectedDate);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.InstanceOf<Booking>());
            var model = viewResult.Model as Booking;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.BookingId, Is.EqualTo(bookingId));
        }

        [Test]
        public async Task BookingSummaryForReceptionist_ReturnsNotFound_WhenBookingNotExists()
        {
            // Arrange
            int nonExistentBookingId = 999;
            string selectedTimeSlot = "09:00 AM - 10:00 AM";
            DateTime selectedDate = DateTime.Now;

            var bookings = new List<Booking>().AsQueryable();

            // Setup mock for Bookings in the context
            var mockSet = new Mock<DbSet<Booking>>();
            mockSet.As<IQueryable<Booking>>().Setup(m => m.Provider).Returns(bookings.Provider);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.Expression).Returns(bookings.Expression);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.ElementType).Returns(bookings.ElementType);
            mockSet.As<IQueryable<Booking>>().Setup(m => m.GetEnumerator()).Returns(bookings.GetEnumerator());

            _mockContext.Setup(c => c.Bookings).Returns(mockSet.Object);

            // Act
            var result = await GetController().BookingSummaryForReceptionist(nonExistentBookingId, selectedTimeSlot, selectedDate);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        public override bool Equals(object? obj)
        {
            return obj is ReceptionistBookingControllerTests tests &&
                   EqualityComparer<ReceptionistBookingController>.Default.Equals(GetController(), tests.GetController());
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(GetController());
        }
    }
}
