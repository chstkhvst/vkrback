using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace ASPNETCore.Tests.ApplicationTests
{
    public class AttendanceServiceTests
    {
        private readonly Mock<IAttendanceRepositiory> _attendanceRepoMock;
        private readonly Mock<ICatalogRepository> _catalogRepoMock;
        private readonly Mock<ILogger<AttendanceService>> _loggerMock;

        private readonly AttendanceService _service;

        public AttendanceServiceTests()
        {
            _attendanceRepoMock = new Mock<IAttendanceRepositiory>();
            _catalogRepoMock = new Mock<ICatalogRepository>();
            _loggerMock = new Mock<ILogger<AttendanceService>>();

            _service = new AttendanceService(
                _attendanceRepoMock.Object,
                _catalogRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreatesAttendance_WithCorrectInitialData() //данные переносятся корректно (юзер ид, ивент ид) и создается посещение
        {
            // Arrange
            var dto = new CreateEventAttendanceDTO
            {
                EventId = 1,
                UserId = "user1",
                AttendanceStatusId = 1
            };

            EventAttendance capturedAttendance = null!;

            _attendanceRepoMock
                .Setup(r => r.AddAsync(It.IsAny<EventAttendance>()))
                .Callback<EventAttendance>(a => capturedAttendance = a)
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal(1, capturedAttendance.EventId);
            Assert.Equal("user1", capturedAttendance.UserId);
            Assert.Equal(1, capturedAttendance.AttendanceStatusId);
            Assert.False(capturedAttendance.IsDeleted);

            _attendanceRepoMock.Verify(
                r => r.AddAsync(It.IsAny<EventAttendance>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdatesAttendance_WhenValid() //при апдейте собирается вручную нормально 
        {
            // Arrange
            var dto = new EventAttendanceDTO
            {
                Id = 1,
                EventId = 2,
                UserId = "user1",
                AttendanceStatusId = 3,
                IsDeleted = false
            };

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            var result = await _service.UpdateAsync(dto);

            // Assert
            Assert.Equal(3, result.AttendanceStatusId);

            _attendanceRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<EventAttendance>()),
                Times.Once);
        }

        [Fact]
        public async Task LogsAndRethrows_WhenUpdateFails() // прокидывает ошибку если при апдейте ошибка
        {
            // Arrange
            var dto = new EventAttendanceDTO
            {
                Id = 1
            };

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.UpdateAsync(dto));

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
        public async Task Throws_WhenAttendanceNotFound() //чтоб не упал бэк при попытке апдейта налл посещения
        {
            // Arrange
            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((EventAttendance?)null);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.MarkAttendanceAsync(1));

            Assert.Contains("Attendance 1 not found", ex.Message);
        }

        [Fact]
        public async Task ReturnsWithoutUpdating_WhenAttendanceAlreadyMarked() //не отмечает уже отмеченные посещения
        {
            // Arrange
            var attendance = new EventAttendance
            {
                Id = 1,
                AttendanceStatusId = 3
            };

            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(attendance);

            // Act
            var result = await _service.MarkAttendanceAsync(1);

            // Assert
            Assert.Equal(3, result.AttendanceStatusId);

            _attendanceRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<EventAttendance>()),
                Times.Never);
        }

        [Fact]
        public async Task Throws_WhenUserProfileNotFound() // если не найден профиль кидает ошибку
        {
            // Arrange
            var attendance = new EventAttendance
            {
                Id = 1,
                AttendanceStatusId = 1,
                VolunteerEvent = new VolunteerEvent
                {
                    EventPoints = 10
                },
                User = new User
                {
                    VolunteerProfile = null
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(attendance);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() =>
                _service.MarkAttendanceAsync(1));

            Assert.Contains("User profile not found", ex.Message);
        }

        [Fact]
        public async Task MarksAttendanceAsCompleted() // отметка посещения правилльная
        {
            // Arrange
            var attendance = new EventAttendance
            {
                Id = 1,
                AttendanceStatusId = 1,
                VolunteerEvent = new VolunteerEvent
                {
                    EventPoints = 5
                },
                User = new User
                {
                    VolunteerProfile = new VolunteerProfile
                    {
                        Points = 0,
                        RankId = 1
                    }
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(attendance);

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            var result = await _service.MarkAttendanceAsync(1);

            // Assert
            Assert.Equal(3, result.AttendanceStatusId);
        }

        [Fact]
        public async Task AddsEventPointsToProfile() // при отметке волонтера начисляет очки за ивент
        {
            // Arrange
            var profile = new VolunteerProfile
            {
                Points = 10,
                RankId = 1
            };

            var attendance = new EventAttendance
            {
                Id = 1,
                AttendanceStatusId = 1,
                VolunteerEvent = new VolunteerEvent
                {
                    EventPoints = 15
                },
                User = new User
                {
                    VolunteerProfile = profile
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(attendance);

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            await _service.MarkAttendanceAsync(1);

            // Assert
            Assert.Equal(25, profile.Points);
        }

        [Fact]
        public async Task UpdatesRank_WhenNewRankReached() //правильно обновляет если новый ранг волонтера достигнут
        {
            // Arrange
            var profile = new VolunteerProfile
            {
                Points = 90,
                RankId = 1
            };

            var newRank = new VolunteerRank
            {
                Id = 2
            };

            var attendance = new EventAttendance
            {
                Id = 1,
                AttendanceStatusId = 1,
                VolunteerEvent = new VolunteerEvent
                {
                    EventPoints = 20
                },
                User = new User
                {
                    VolunteerProfile = profile
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(attendance);

            _catalogRepoMock
                .Setup(r => r.GetRankForPointsAsync(110))
                .ReturnsAsync(newRank);

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            await _service.MarkAttendanceAsync(1);

            // Assert
            Assert.Equal(2, profile.RankId);
        }

        [Fact]
        public async Task DoesNotUpdateRank_WhenRankDidNotChange() //не обновляет ранг если новый не достигнут 
        {
            // Arrange
            var profile = new VolunteerProfile
            {
                Points = 50,
                RankId = 1
            };

            var sameRank = new VolunteerRank
            {
                Id = 1
            };

            var attendance = new EventAttendance
            {
                Id = 1,
                AttendanceStatusId = 1,
                VolunteerEvent = new VolunteerEvent
                {
                    EventPoints = 10
                },
                User = new User
                {
                    VolunteerProfile = profile
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(attendance);

            _catalogRepoMock
                .Setup(r => r.GetRankForPointsAsync(60))
                .ReturnsAsync(sameRank);

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            await _service.MarkAttendanceAsync(1);

            // Assert
            Assert.Equal(1, profile.RankId);
        }

        [Fact]
        public async Task LogsAndRethrows_WhenMarkAttendanceFails() //обработка ошибок при фейле отмены
        {
            // Arrange
            _attendanceRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.MarkAttendanceAsync(1));

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
        public async Task MarksUpcomingAttendancesAsNoShow() // несоклько отмечает как неявку
        {
            // Arrange
            var attendances = new List<EventAttendance>
            {
                new EventAttendance
                {
                    Id = 1,
                    AttendanceStatusId = 1
                },
                new EventAttendance
                {
                    Id = 2,
                    AttendanceStatusId = 1
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByEventIdAsync(1))
                .ReturnsAsync(attendances);

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            await _service.MarkNoShowAsync(1);

            // Assert
            Assert.All(attendances,
                a => Assert.Equal(4, a.AttendanceStatusId));

            _attendanceRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<EventAttendance>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ReturnsWithoutUpdating_WhenNoUpcomingAttendances() //если нечего обновить не обновляет
        {
            // Arrange
            var attendances = new List<EventAttendance>
            {
                new EventAttendance
                {
                    AttendanceStatusId = 2
                },
                new EventAttendance
                {
                    AttendanceStatusId = 3
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByEventIdAsync(1))
                .ReturnsAsync(attendances);

            // Act
            await _service.MarkNoShowAsync(1);

            // Assert
            _attendanceRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<EventAttendance>()),
                Times.Never);
        }

        [Fact]
        public async Task MarksUpcomingAttendancesAsCancelled() // корректная отмена посещения
        {
            // Arrange
            var attendances = new List<EventAttendance>
            {
                new EventAttendance
                {
                    AttendanceStatusId = 1
                }
            };

            _attendanceRepoMock
                .Setup(r => r.GetByEventIdAsync(1))
                .ReturnsAsync(attendances);

            _attendanceRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<EventAttendance>()))
                .ReturnsAsync((EventAttendance a) => a);

            // Act
            await _service.MarkCancelledAsync(1);

            // Assert
            Assert.Equal(2, attendances[0].AttendanceStatusId);
        }
    }
}