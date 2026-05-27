using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Tests.InfrastructureTests
{
    public class ReportRepositoryTests
    {
        private ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private async Task SeedRequiredData(ApplicationDbContext context)
        {
            context.ReportStatuses.AddRange(
                new ReportStatus
                {
                    Id = 1,
                    ReportStatusName = "На рассмотрении"
                },
                new ReportStatus
                {
                    Id = 2,
                    ReportStatusName = "Рассмотрена"
                });

            context.VolunteerRanks.Add(
                new VolunteerRank
                {
                    Id = 1,
                    RankName = "Новичок",
                    PointsRequired = 0
                });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsReportWithNavigationProperties() // правильно грузит навигационные свойства
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender, reported);

            context.VolunteerProfiles.Add(
                new VolunteerProfile
                {
                    UserId = sender.Id,
                    RankId = 1,
                    Points = 0
                });

            context.OrganizerProfiles.Add(
                new OrganizerProfile
                {
                    UserId = reported.Id,
                    OrganizationName = "Org",
                    Ogrn = "123"
                });

            context.UserReports.Add(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Spam",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);

            Assert.NotNull(result.Sender);
            Assert.NotNull(result.Reported);
            Assert.NotNull(result.ReportStatus);

            Assert.NotNull(result.Sender.VolunteerProfile);
            Assert.NotNull(result.Reported.OrganizerProfile);
        }

        [Fact]
        public async Task GetBySenderIdAsync_ReturnsOnlySenderReports()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender1 = new User
            {
                Id = "sender1",
                UserName = "sender1"
            };

            var sender2 = new User
            {
                Id = "sender2",
                UserName = "sender2"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender1, sender2, reported);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender1.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason1",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender2.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason2",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = await repo.GetBySenderIdAsync(sender1.Id);

            Assert.Single(result);
            Assert.Equal(sender1.Id, result.First().SenderUserId);
        }

        [Fact]
        public async Task GetBySenderIdAsync_OrdersByStatusThenReportedUser() //правильно упорядочивает 
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reportedA = new User
            {
                Id = "a",
                UserName = "a"
            };

            var reportedB = new User
            {
                Id = "b",
                UserName = "b"
            };

            context.Users.AddRange(sender, reportedA, reportedB);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = "b",
                    ReportReason = "Reason",
                    ReportStatusId = 2
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender.Id,
                    ReportedUserId = "a",
                    ReportReason = "Reason",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = (await repo.GetBySenderIdAsync(sender.Id)).ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal(1, result[0].ReportStatusId);
            Assert.Equal(2, result[1].ReportStatusId);
        }

        [Fact]
        public async Task GetByReportedIdAsync_ReturnsOnlyReportedUserReports()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported1 = new User
            {
                Id = "reported1",
                UserName = "reported1"
            };

            var reported2 = new User
            {
                Id = "reported2",
                UserName = "reported2"
            };

            context.Users.AddRange(sender, reported1, reported2);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported1.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported2.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = await repo.GetByReportedIdAsync(reported1.Id);

            Assert.Single(result);
            Assert.Equal(reported1.Id, result.First().ReportedUserId);
        }

        [Fact]
        public async Task GetByStatusIdAsync_ReturnsOnlyReportsWithGivenStatus() //фильтр
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender, reported);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 2
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = await repo.GetByStatusIdAsync(1);

            Assert.Single(result);
            Assert.Equal(1, result.First().ReportStatusId);
        }

        [Fact]
        public async Task GetBySenderAndReportedAsync_ReturnsCorrectReports() 
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender1 = new User
            {
                Id = "sender1",
                UserName = "sender1"
            };

            var sender2 = new User
            {
                Id = "sender2",
                UserName = "sender2"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender1, sender2, reported);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender1.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender2.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = await repo.GetBySenderAndReportedAsync(
                sender1.Id,
                reported.Id);

            Assert.Single(result);
            Assert.Equal(sender1.Id, result.First().SenderUserId);
        }

        [Fact]
        public async Task GetGroupedReportsAsync_GroupsReportsByReportedUser() //правильная группировка
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender1 = new User
            {
                Id = "sender1",
                UserName = "sender1"
            };

            var sender2 = new User
            {
                Id = "sender2",
                UserName = "sender2"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender1, sender2, reported);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender1.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Spam",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender2.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Abuse",
                    ReportStatusId = 2
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = (await repo.GetGroupedReportsAsync()).ToList();

            Assert.Single(result);

            Assert.Equal(reported.Id, result[0].ReportedUserId);
            Assert.Equal(2, result[0].Count);
        }

        [Fact]
        public async Task GetGroupedReportsAsync_FiltersByStatus()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender, reported);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Spam",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Abuse",
                    ReportStatusId = 2
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = (await repo.GetGroupedReportsAsync(1)).ToList();

            Assert.Single(result);
            Assert.Single(result[0].Reports);

            Assert.Equal(1, result[0].Reports[0].ReportStatusId);
        }

        [Fact]
        public async Task GetGroupedReportsAsync_FiltersByKeywords()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported1 = new User
            {
                Id = "reported1",
                UserName = "toxicUser",
                FullName = "John Smith"
            };

            var reported2 = new User
            {
                Id = "reported2",
                UserName = "normalUser",
                FullName = "Jane Doe"
            };

            context.Users.AddRange(sender, reported1, reported2);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported1.Id,
                    ReportReason = "Spam",
                    ReportStatusId = 1
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported2.Id,
                    ReportReason = "Spam",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = (await repo.GetGroupedReportsAsync(
                null,
                "toxic")).ToList();

            Assert.Single(result);

            Assert.Equal(reported1.Id, result[0].ReportedUserId);
        }

        [Fact]
        public async Task GetGroupedReportsAsync_OrdersReportsInsideGroup() //фильтрует жалобы внутри группы по не рассмотренным
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender, reported);

            context.UserReports.AddRange(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 2
                },
                new UserReport
                {
                    Id = 2,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Reason",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var result = (await repo.GetGroupedReportsAsync()).First();

            Assert.Equal(1, result.Reports[0].ReportStatusId);
            Assert.Equal(2, result.Reports[1].ReportStatusId);
        }

        [Fact]
        public async Task AddAsync_SavesReport()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender, reported);

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var report = new UserReport
            {
                Id = 1,
                SenderUserId = sender.Id,
                ReportedUserId = reported.Id,
                ReportReason = "Spam",
                ReportStatusId = 1
            };

            await repo.AddAsync(report);

            Assert.Single(context.UserReports);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFields()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var sender = new User
            {
                Id = "sender",
                UserName = "sender"
            };

            var reported = new User
            {
                Id = "reported",
                UserName = "reported"
            };

            context.Users.AddRange(sender, reported);

            context.UserReports.Add(
                new UserReport
                {
                    Id = 1,
                    SenderUserId = sender.Id,
                    ReportedUserId = reported.Id,
                    ReportReason = "Old",
                    ReportStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new ReportRepository(context);

            var updated = new UserReport
            {
                Id = 1,
                SenderUserId = sender.Id,
                ReportedUserId = reported.Id,
                ReportReason = "New",
                ReportStatusId = 2
            };

            await repo.UpdateAsync(updated);

            var result = await context.UserReports.FindAsync(1);

            Assert.Equal("New", result.ReportReason);
            Assert.Equal(2, result.ReportStatusId);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenReportNotFound()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var repo = new ReportRepository(context);

            var report = new UserReport
            {
                Id = 999,
                SenderUserId = "sender",
                ReportedUserId = "reported",
                ReportReason = "Reason",
                ReportStatusId = 1
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateAsync(report));
        }
    }
}