using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace ASPNETCore.Tests.ApplicationTests
{
    public class ReportServiceTests
    {
        private readonly Mock<IReportRepository> _reportRepoMock;
        private readonly Mock<IBanRepository> _banRepoMock;
        private readonly Mock<ILogger<ReportService>> _loggerMock;

        private readonly ReportService _service;

        public ReportServiceTests()
        {
            _reportRepoMock = new Mock<IReportRepository>();
            _banRepoMock = new Mock<IBanRepository>();
            _loggerMock = new Mock<ILogger<ReportService>>();

            _service = new ReportService(
                _reportRepoMock.Object,
                _banRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ReturnsNull_WhenReportDoesNotExist() // нормальная обработка налл + лог
        {
            // Arrange
            _reportRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((UserReport?)null);

            // Act
            var result = await _service.GetByIdAsync(1);

            // Assert
            Assert.Null(result);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) =>
                        v.ToString()!.Contains("Report with id 1 not found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CreatesReport_WithPendingStatus() // создается жалоба с правильным статусом и поле модера не рассмотрено
        {
            // Arrange
            var dto = new CreateReportDTO
            {
                SenderUserId = "sender1",
                ReportedUserId = "reported1",
                ReportReason = "Spam"
            };

            UserReport capturedReport = null!;

            _reportRepoMock
                .Setup(r => r.AddAsync(It.IsAny<UserReport>()))
                .Callback<UserReport>(r => capturedReport = r)
                .ReturnsAsync((UserReport r) => r);

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal("sender1", capturedReport.SenderUserId);
            Assert.Equal("reported1", capturedReport.ReportedUserId);
            Assert.Equal("Spam", capturedReport.ReportReason);

            Assert.Equal(1, capturedReport.ReportStatusId);
            Assert.Null(capturedReport.ModeratedByUserId);
            Assert.False(capturedReport.IsDeleted);

            _reportRepoMock.Verify(
                r => r.AddAsync(It.IsAny<UserReport>()),
                Times.Once);
        }

        [Fact]
        public async Task MarksPendingReportsAsClosed() //правильно присваивается модер на рассмотрении и статус меняется
        {
            // Arrange
            var reports = new List<UserReport>
            {
                new UserReport
                {
                    Id = 1,
                    ReportStatusId = 1,
                    IsDeleted = false
                },
                new UserReport
                {
                    Id = 2,
                    ReportStatusId = 1,
                    IsDeleted = false
                }
            };

            _reportRepoMock
                .Setup(r => r.GetByReportedIdAsync("user1"))
                .ReturnsAsync(reports);

            _reportRepoMock
                .Setup(r => r.UpdateAsync(It.IsAny<UserReport>()))
                .ReturnsAsync((UserReport r) => r);

            // Act
            await _service.MarkReportsClosedAsync("user1", "moder1");

            // Assert
            Assert.All(reports, r =>
            {
                Assert.Equal(2, r.ReportStatusId);
                Assert.Equal("moder1", r.ModeratedByUserId);
            });

            _reportRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<UserReport>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task ReturnsWithoutUpdating_WhenNoPendingReports()//не вызывать лишний раз апдейт если нет жалоб
        {
            // Arrange
            var reports = new List<UserReport>
            {
                new UserReport
                {
                    ReportStatusId = 2
                },
                new UserReport
                {
                    ReportStatusId = 3
                }
            };

            _reportRepoMock
                .Setup(r => r.GetByReportedIdAsync("user1"))
                .ReturnsAsync(reports);

            // Act
            await _service.MarkReportsClosedAsync("user1", "moder1");

            // Assert
            _reportRepoMock.Verify(
                r => r.UpdateAsync(It.IsAny<UserReport>()),
                Times.Never);
        }

        [Fact]
        public async Task LogsAndRethrows_WhenClosingReportsFails() // логи и эксепшн если ошибки при попытке закрыть жалобы
        {
            // Arrange
            _reportRepoMock
                .Setup(r => r.GetByReportedIdAsync("user1"))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.MarkReportsClosedAsync("user1", "moder1"));

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