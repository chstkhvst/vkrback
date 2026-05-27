using ASPNETCore.Application.DTO;
using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace ASPNETCore.Tests.ApplicationTests
{
    public class BanServiceTests
    {
        private readonly Mock<IBanRepository> _banRepoMock;
        private readonly Mock<IReportRepository> _reportRepoMock;
        private readonly Mock<ILogger<BanService>> _loggerMock;
        private readonly Mock<ILogger<ReportService>> _reportLoggerMock;

        private readonly BanService _service;

        public BanServiceTests()
        {
            _banRepoMock = new Mock<IBanRepository>();
            _reportRepoMock = new Mock<IReportRepository>();

            _loggerMock = new Mock<ILogger<BanService>>();
            _reportLoggerMock = new Mock<ILogger<ReportService>>();

            var reportService = new ReportService(
                _reportRepoMock.Object,
                Mock.Of<IBanRepository>(),
                _reportLoggerMock.Object);

            _service = new BanService(
                _banRepoMock.Object,
                reportService,
                _loggerMock.Object);
        }

        [Fact]
        public async Task CreatesBan_WithCorrectData() //правильное автозаполнение полей при создании бана
        {
            // Arrange
            var dto = new CreateBanDTO
            {
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Spam",
                IsActive = true,
                IsDeleted = false
            };

            Ban capturedBan = null!;

            _banRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Ban>()))
                .Callback<Ban>(b => capturedBan = b)
                .ReturnsAsync((Ban b) => b);

            _reportRepoMock
                .Setup(r => r.GetByReportedIdAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<UserReport>());

            // Act
            var result = await _service.AddAsync(dto);

            // Assert
            Assert.Equal("user1", capturedBan.BannedUserId);
            Assert.Equal("moder1", capturedBan.ModerId);
            Assert.Equal("Spam", capturedBan.BanReason);

            Assert.True(capturedBan.IsActive);
            Assert.False(capturedBan.IsDeleted);

            Assert.Equal(
                DateOnly.FromDateTime(DateTime.Today),
                capturedBan.CreatedAt);

            _banRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Ban>()),
                Times.Once);
        }

        [Fact]
        public async Task LogsAndRethrows_WhenBanCreationFails() // лог ошибки
        {
            // Arrange
            var dto = new CreateBanDTO
            {
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Spam"
            };

            _banRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Ban>()))
                .ThrowsAsync(new Exception("DB error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _service.AddAsync(dto));

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