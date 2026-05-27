using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace ASPNETCore.Tests.ApplicationTests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepoMock;
        private readonly Mock<IAttendanceRepositiory> _attendanceRepoMock;
        private readonly Mock<ICatalogRepository> _catalogRepoMock;

        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly Mock<ILogger<AttendanceService>> _attendanceLoggerMock;

        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _notificationRepoMock = new Mock<INotificationRepository>();
            _attendanceRepoMock = new Mock<IAttendanceRepositiory>();
            _catalogRepoMock = new Mock<ICatalogRepository>();

            _loggerMock = new Mock<ILogger<NotificationService>>();
            _attendanceLoggerMock = new Mock<ILogger<AttendanceService>>();

            var attendanceService = new AttendanceService(
                _attendanceRepoMock.Object,
                _catalogRepoMock.Object,
                _attendanceLoggerMock.Object);

            _service = new NotificationService(
                _notificationRepoMock.Object,
                _loggerMock.Object,
                attendanceService);
        }

        [Fact]
        public async Task CreatesUnreadNotification() //создает правильные уведы с начальными данными
        {
            // Arrange
            var dto = new CreateNotificationDTO
            {
                RecipientId = "user1",
                Message = "Test message",
                TypeId = 1,
                EventId = 10
            };

            Notification capturedNotification = null!;

            _notificationRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Notification>()))
                .Callback<Notification>(n => capturedNotification = n)
                .ReturnsAsync((Notification n) => n);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal("user1", capturedNotification.RecipientId);
            Assert.Equal("Test message", capturedNotification.Message);
            Assert.Equal(1, capturedNotification.TypeId);
            Assert.Equal(10, capturedNotification.EventId);

            Assert.False(capturedNotification.IsRead);

            _notificationRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Notification>()),
                Times.Once);
        }

        [Fact]
        public async Task CreatesNotifications_OnlyForPendingAttendees() //уведы только тем у кого посещения ожидаются
        {
            // Arrange
            var attendees = new List<EventAttendance>
            {
                new EventAttendance
                {
                    UserId = "user1",
                    AttendanceStatusId = 1,
                    IsDeleted = false
                },
                new EventAttendance
                {
                    UserId = "user2",
                    AttendanceStatusId = 2,
                    IsDeleted = false
                },
                new EventAttendance
                {
                    UserId = "user3",
                    AttendanceStatusId = 1,
                    IsDeleted = true
                },
                new EventAttendance
                {
                    UserId = "user4",
                    AttendanceStatusId = 1,
                    IsDeleted = false
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByEventIdAsync(10))
                .ReturnsAsync(attendees);

            IEnumerable<Notification> capturedNotifications = null!;

            _notificationRepoMock
                .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()))
                .Callback<IEnumerable<Notification>>(n => capturedNotifications = n)
                .ReturnsAsync((IEnumerable<Notification> n) => n);

            var dto = new CreateNotificationDTO
            {
                Message = "Reminder",
                TypeId = 1,
                EventId = 10
            };

            // Act
            var result = await _service.AddRangeAsync(dto);

            // Assert
            Assert.Equal(2, capturedNotifications.Count());

            Assert.Contains(capturedNotifications,
                n => n.RecipientId == "user1");

            Assert.Contains(capturedNotifications,
                n => n.RecipientId == "user4");

            Assert.DoesNotContain(capturedNotifications,
                n => n.RecipientId == "user2");

            Assert.DoesNotContain(capturedNotifications,
                n => n.RecipientId == "user3");

            _notificationRepoMock.Verify(
                r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()),
                Times.Once);
        }

        [Fact]
        public async Task ReturnsEmpty_WhenNoEligibleAttendees() // если нет пользователей на ивент ничего не создается 
        {
            // Arrange
            var attendees = new List<EventAttendance>
            {
                new EventAttendance
                {
                    UserId = "user1",
                    AttendanceStatusId = 2,
                    IsDeleted = false
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByEventIdAsync(10))
                .ReturnsAsync(attendees);

            var dto = new CreateNotificationDTO
            {
                Message = "Reminder",
                TypeId = 1,
                EventId = 10
            };

            // Act
            var result = await _service.AddRangeAsync(dto);

            // Assert
            Assert.Empty(result);

            _notificationRepoMock.Verify(
                r => r.AddRangeAsync(It.IsAny<IEnumerable<Notification>>()),
                Times.Never);
        }

        [Fact]
        public async Task LogsAndRethrows_WhenMarkAsReadFails() // корректный лог для прочитанных
        {
            // Arrange
            _notificationRepoMock
                .Setup(r => r.MarkAsReadAsync(1))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.MarkAsReadAsync(1));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogsAndRethrows_WhenMarkAllAsReadFails() // лог для прочитанных по кнопке отметить все
        {
            // Arrange
            _notificationRepoMock
                .Setup(r => r.MarkAllAsReadAsync("user1"))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.MarkAllAsReadAsync("user1"));

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}