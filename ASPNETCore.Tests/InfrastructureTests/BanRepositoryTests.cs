using ASPNETCore.Domain.Entities;
using ASPNETCore.Infrastructure.Data;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Tests.InfrastructureTests
{
    public class BanRepositoryTests
    {
        private ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task IsUserBannedAsync_ReturnsTrue_WhenActiveBanExists()
        {
            // Arrange
            var context = GetContext();

            context.Bans.Add(new Ban
            {
                Id = 1,
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Spam",
                IsActive = true,
                IsDeleted = false,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = await repo.IsUserBannedAsync("user1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserBannedAsync_ReturnsFalse_WhenBanInactiveOrDeleted()
        {
            // Arrange
            var context = GetContext();

            context.Bans.AddRange(
                new Ban
                {
                    Id = 1,
                    BannedUserId = "user1",
                    ModerId = "moder1",
                    BanReason = "Spam",
                    IsActive = false,
                    IsDeleted = false,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                },
                new Ban
                {
                    Id = 2,
                    BannedUserId = "user1",
                    ModerId = "moder1",
                    BanReason = "Spam",
                    IsActive = true,
                    IsDeleted = true,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = await repo.IsUserBannedAsync("user1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_FiltersByReason()
        {
            // Arrange
            var context = GetContext();

            var moder = new User
            {
                Id = "moder1",
                UserName = "moder"
            };

            var banned = new User
            {
                Id = "user1",
                UserName = "banned"
            };

            context.Users.AddRange(moder, banned);

            context.Bans.AddRange(
                new Ban
                {
                    Id = 1,
                    BannedUserId = banned.Id,
                    BannedUser = banned,
                    ModerId = moder.Id,
                    Moder = moder,
                    BanReason = "Spam messages",
                    IsActive = true,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                },
                new Ban
                {
                    Id = 2,
                    BannedUserId = banned.Id,
                    BannedUser = banned,
                    ModerId = moder.Id,
                    Moder = moder,
                    BanReason = "Offtopic",
                    IsActive = true,
                    CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
                });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = await repo.GetAllAsync("spam");

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task GetAllAsync_FiltersByModeratorUserName()
        {
            // Arrange
            var context = GetContext();

            var moder = new User
            {
                Id = "moder1",
                UserName = "supermoder"
            };

            var banned = new User
            {
                Id = "user1",
                UserName = "banned"
            };

            context.Users.AddRange(moder, banned);

            context.Bans.Add(new Ban
            {
                Id = 1,
                BannedUserId = banned.Id,
                BannedUser = banned,
                ModerId = moder.Id,
                Moder = moder,
                BanReason = "Spam",
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = await repo.GetAllAsync("super");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetAllAsync_FiltersByBannedUserName()
        {
            // Arrange
            var context = GetContext();

            var moder = new User
            {
                Id = "moder1",
                UserName = "moder"
            };

            var banned = new User
            {
                Id = "user1",
                UserName = "toxicuser"
            };

            context.Users.AddRange(moder, banned);

            context.Bans.Add(new Ban
            {
                Id = 1,
                BannedUserId = banned.Id,
                BannedUser = banned,
                ModerId = moder.Id,
                Moder = moder,
                BanReason = "Spam",
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = await repo.GetAllAsync("toxic");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyUserBans_OrderedByCreatedAtDescending()
        {
            // Arrange
            var context = GetContext();

            var moder = new User
            {
                Id = "moder1",
                UserName = "moder"
            };

            var banned = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.AddRange(moder, banned);

            context.Bans.AddRange(
                new Ban
                {
                    Id = 1,
                    BannedUserId = banned.Id,
                    ModerId = moder.Id,
                    BanReason = "Old",
                    CreatedAt = new DateOnly(2024, 1, 1),
                    IsActive = true
                },
                new Ban
                {
                    Id = 2,
                    BannedUserId = banned.Id,
                    ModerId = moder.Id,
                    BanReason = "New",
                    CreatedAt = new DateOnly(2025, 1, 1),
                    IsActive = true
                },
                new Ban
                {
                    Id = 3,
                    BannedUserId = "anotherUser",
                    ModerId = moder.Id,
                    BanReason = "Another",
                    CreatedAt = new DateOnly(2025, 1, 1),
                    IsActive = true
                });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = (await repo.GetByUserIdAsync("user1")).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[0].Id);
            Assert.Equal(1, result[1].Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsBanWithNavigationProperties()
        {
            // Arrange
            var context = GetContext();

            var moder = new User
            {
                Id = "moder1",
                UserName = "moder"
            };

            var banned = new User
            {
                Id = "user1",
                UserName = "banned",
                VolunteerProfile = new VolunteerProfile
                {
                    UserId = "user1",
                    RankId = 1,
                    Points = 100,
                    Rank = new VolunteerRank
                    {
                        Id = 1,
                        RankName = "Новичок",
                        PointsRequired = 0
                    }
                }
            };

            context.Users.AddRange(moder, banned);

            context.Bans.Add(new Ban
            {
                Id = 1,
                BannedUserId = banned.Id,
                BannedUser = banned,
                ModerId = moder.Id,
                Moder = moder,
                BanReason = "Spam",
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            // Act
            var result = await repo.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Moder);
            Assert.NotNull(result.BannedUser);
            Assert.NotNull(result.BannedUser.VolunteerProfile);
        }

        [Fact]
        public async Task AddAsync_SavesBan()
        {
            // Arrange
            var context = GetContext();

            var repo = new BanRepository(context);

            var ban = new Ban
            {
                Id = 1,
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Spam",
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Act
            await repo.AddAsync(ban);

            // Assert
            Assert.Single(context.Bans);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFields()
        {
            // Arrange
            var context = GetContext();

            context.Bans.Add(new Ban
            {
                Id = 1,
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Old",
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            });

            await context.SaveChangesAsync();

            var repo = new BanRepository(context);

            var updated = new Ban
            {
                Id = 1,
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Updated",
                IsActive = false,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Act
            await repo.UpdateAsync(updated);

            // Assert
            var result = await context.Bans.FindAsync(1);

            Assert.Equal("Updated", result.BanReason);
            Assert.False(result.IsActive);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenBanNotFound()
        {
            // Arrange
            var context = GetContext();

            var repo = new BanRepository(context);

            var ban = new Ban
            {
                Id = 999,
                BannedUserId = "user1",
                ModerId = "moder1",
                BanReason = "Spam",
                IsActive = true,
                CreatedAt = DateOnly.FromDateTime(DateTime.UtcNow)
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateAsync(ban));
        }
    }
}