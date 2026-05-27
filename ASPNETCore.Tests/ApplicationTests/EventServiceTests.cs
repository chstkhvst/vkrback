using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Tests.ApplicationTests
{
    public class EventServiceTests
    {
        private readonly Mock<IEventRepository> _eventRepoMock;
        private readonly Mock<IAttendanceRepositiory> _attRepoMock;
        private readonly Mock<ILogger<EventService>> _loggerMock;

        private readonly EventService _service;

        public EventServiceTests()
        {
            _eventRepoMock = new Mock<IEventRepository>();
            _attRepoMock = new Mock<IAttendanceRepositiory>();
            _loggerMock = new Mock<ILogger<EventService>>();

            _service = new EventService(
                _eventRepoMock.Object,
                _attRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ReturnsNull_WhenEventDoesNotExist() //если ивента нет
        {
            // Arrange
            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((VolunteerEvent?)null);
            // Act
            var result = await _service.GetByIdAsync(1);
            // Assert
            Assert.Null(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString().Contains("Event with id 1 not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        [Fact]
        public async Task ReturnsEventDTO_WhenEventExists() //корректно возвращает который есть
        {
            // Arrange
            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Уборка"
            };
            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(ev);

            var result = await _service.GetByIdAsync(1);
            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Уборка", result.Name);
        }
        [Fact]
        public async Task CreatesEvent_WithPendingStatus() //добавляет ивент, статус 1
        {
            // Arrange
            var dto = new CreateVolunteerEventDTO
            {
                Name = "Event",
                Description = "Desc",
                Lat = "66.66",
                Lng = "4.90",
                Address = "Address",
                ParticipantsLimit = 10,
                EventPoints = 5,
                EventCategoryId = 1,
                CityId = 1
            };
            VolunteerEvent capturedEvent = null!;
            _eventRepoMock
                .Setup(r => r.AddAsync(It.IsAny<VolunteerEvent>()))
                .Callback<VolunteerEvent>(e => capturedEvent = e)
                .ReturnsAsync((VolunteerEvent e) => e);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns("wwwroot");
            // Act
            var result = await _service.AddAsync(dto, "user1", envMock.Object);
            // Assert
            Assert.Equal(1, capturedEvent.EventStatusId);
            Assert.Equal("user1", capturedEvent.UserId);

            _eventRepoMock.Verify(
                r => r.AddAsync(It.IsAny<VolunteerEvent>()),
                Times.Once);
        }
        [Fact]
        public async Task Throws_WhenEventNotFound() //нельзя апдейтнуть если не найден
        {
            // Arrange
            var dto = new UpdateEventDTO
            {
                Id = 1
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((VolunteerEvent?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.UpdateByOrganizerAsync(dto, "user1"));
        }
        [Fact]
        public async Task Throws_WhenUserIsNotOwner()//обновить свое можеттолько организатор
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "owner"
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            var dto = new UpdateEventDTO
            {
                Id = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _service.UpdateByOrganizerAsync(dto, "anotherUser"));
        }
        [Fact]
        public async Task UpdatesDescription_WhenProvided() //проверка апдейта поля
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "user1",
                Description = "Old"
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _eventRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<VolunteerEvent>()))
                .ReturnsAsync((VolunteerEvent e) => e);

            var dto = new UpdateEventDTO
            {
                Id = 1,
                Description = "New Description"
            };

            // Act
            var result = await _service.UpdateByOrganizerAsync(dto, "user1");

            // Assert
            Assert.Equal("New Description", result.Description);

            _eventRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<VolunteerEvent>()),
                Times.Once);
        }
        [Fact]
        public async Task Throws_WhenMovingEventTooFar() //нельзя передвинуть ивент дальше чем ограничено
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "user1",
                Lat = 66.66,
                Lng = 4.90,
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            var dto = new UpdateEventDTO
            {
                Id = 1,
                Lat =22.39,
                Lng = 23.91
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateByOrganizerAsync(dto, "user1"));

            Assert.Contains("Cannot move event", ex.Message);
        }
        [Fact]
        public async Task UpdatesCoordinates_WhenDistanceIsAllowed() //корректно меняет коорды после проверки что можно
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "user1",
                Lat = 66.66,
                Lng = 4.90
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _eventRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<VolunteerEvent>()))
                .ReturnsAsync((VolunteerEvent e) => e);

            var dto = new UpdateEventDTO
            {
                Id = 1,
                Lat = 66.67,
                Lng = 4.87
            };

            // Act
            var result = await _service.UpdateByOrganizerAsync(dto, "user1");

            // Assert
            Assert.Equal(66.67, result.Lat);
            Assert.Equal(4.87, result.Lng);
        }
        [Fact]
        public async Task Throws_WhenParticipantsExceedLimit() //попытка обновить на меньшее колво участников не должна пройти
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "user1",
                ParticipantsLimit = 10
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _attRepoMock
                .Setup(r => r.CountParticipantsAsync(1))
                .ReturnsAsync(15);

            var dto = new UpdateEventDTO
            {
                Id = 1,
                ParticipantsLimit = 5
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateByOrganizerAsync(dto, "user1"));

            Assert.Contains("Number of participants exceeds the limit", ex.Message);
        }
        [Fact]
        public async Task UpdatesParticipantsLimit_WhenValid() //изменение колва участников если можно
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "user1",
                ParticipantsLimit = 10
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _attRepoMock
                .Setup(r => r.CountParticipantsAsync(1))
                .ReturnsAsync(3);

            _eventRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<VolunteerEvent>()))
                .ReturnsAsync((VolunteerEvent e) => e);

            var dto = new UpdateEventDTO
            {
                Id = 1,
                ParticipantsLimit = 20
            };

            // Act
            var result = await _service.UpdateByOrganizerAsync(dto, "user1");

            // Assert
            Assert.Equal(20, result.ParticipantsLimit);
        }
        [Fact]
        public async Task LogsAndRethrows_WhenRepositoryThrows() //логает ошибки бд
        {
            // Arrange
            var existing = new VolunteerEvent
            {
                Id = 1,
                UserId = "user1"
            };

            _eventRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(existing);

            _eventRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<VolunteerEvent>()))
                .ThrowsAsync(new Exception("DB error"));

            var dto = new UpdateEventDTO
            {
                Id = 1,
                Description = "New"
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateByOrganizerAsync(dto, "user1"));

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
        public async Task MarksExpiredEventsAsCompleted()//отмечает ивенты если время прошло
        {
            // Arrange
            var oldEvent = new VolunteerEvent
            {
                Id = 1,
                EventDateTime = DateTime.Now.AddDays(-1),
                EventStatusId = 1
            };

            _eventRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<VolunteerEvent> { oldEvent });

            _eventRepoMock
                .Setup(r => r.GetPagedOrgForUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new PaginatedResponse<VolunteerEvent>
                {
                    Items = new List<VolunteerEvent>(),
                    TotalCount = 0,
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalPages = 0
                });

            // Act
            await _service.GetPagedOrgForUserAsync(
                "user1", 1, 10, null, null, null, null);

            // Assert
            Assert.Equal(5, oldEvent.EventStatusId);

            _eventRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<VolunteerEvent>()),
                Times.Once);
        }
        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public async Task DoesNotUpdateRejectedOrCancelledEvents(int statusId) //отмененные и отклоненные не меняют статус при проверке времени
        {
            // Arrange
            var ev = new VolunteerEvent
            {
                Id = 1,
                EventDateTime = DateTime.Now.AddDays(-1),
                EventStatusId = statusId
            };

            _eventRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<VolunteerEvent> { ev });

            _eventRepoMock
                .Setup(r => r.GetPagedOrgForUserAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<DateTime?>()))
                .ReturnsAsync(new PaginatedResponse<VolunteerEvent>
                {
                    Items = new List<VolunteerEvent>(),
                    TotalCount = 0,
                    CurrentPage = 1,
                    PageSize = 10,
                    TotalPages = 0
                });

            // Act
            await _service.GetPagedOrgForUserAsync(
                "user1", 1, 10, null, null, null, null);

            // Assert
            _eventRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<VolunteerEvent>()),
                Times.Never);
        }
    }
}
