using ASPNETCore.Domain.Entities;
using ASPNETCore.Infrastructure.Data;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Tests.InfrastructureTests
{
    public class AttendanceRepositoryTests
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
            context.EventStatuses.Add(
                new EventStatus
                {
                    Id = 1,
                    EventStatusName = "Одобрено"
                });

            context.EventCategories.Add(
                new EventCategory
                {
                    Id = 1,
                    CategoryName = "Category"
                });

            context.Cities.Add(
                new City
                {
                    Id = 1,
                    Name = "Moscow"
                });

            context.AttendanceStatuses.AddRange(
                new AttendanceStatus
                {
                    Id = 1,
                    AttendanceStatusName = "Запланировано"
                },
                new AttendanceStatus
                {
                    Id = 2,
                    AttendanceStatusName = "Отменено"
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
        public async Task GetByIdAsync_ReturnsAttendanceWithNavigationProperties() // корректно инклюдает навигационные свойства
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            var profile = new VolunteerProfile
            {
                UserId = user.Id,
                RankId = 1,
                Points = 0
            };

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.Users.Add(user);
            context.VolunteerProfiles.Add(profile);
            context.VolunteerEvents.Add(ev);

            context.EventAttendances.Add(
                new EventAttendance
                {
                    Id = 1,
                    EventId = ev.Id,
                    UserId = user.Id,
                    AttendanceStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);

            Assert.NotNull(result.VolunteerEvent);
            Assert.NotNull(result.AttendanceStatus);

            Assert.NotNull(result.User);
            Assert.NotNull(result.User.VolunteerProfile);
            Assert.NotNull(result.User.VolunteerProfile.Rank);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsOnlyUserAttendances() // корректно возвращает посещения пользователя
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user1 = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            var user2 = new User
            {
                Id = "user2",
                UserName = "user2"
            };

            context.Users.AddRange(user1, user2);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user1.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.VolunteerEvents.Add(ev);

            context.EventAttendances.AddRange(
                new EventAttendance
                {
                    Id = 1,
                    EventId = 1,
                    UserId = user1.Id,
                    AttendanceStatusId = 1
                },
                new EventAttendance
                {
                    Id = 2,
                    EventId = 1,
                    UserId = user2.Id,
                    AttendanceStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var result = await repo.GetByUserIdAsync(user1.Id);

            Assert.Single(result);
            Assert.Equal(user1.Id, result.First().UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_OrdersByEventDate() // возвращает в порядке убывания даты
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            context.Users.Add(user);

            var ev1 = new VolunteerEvent
            {
                Id = 1,
                Name = "Late",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1,
                EventDateTime = new DateTime(2026, 5, 10)
            };

            var ev2 = new VolunteerEvent
            {
                Id = 2,
                Name = "Early",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1,
                EventDateTime = new DateTime(2026, 5, 1)
            };

            context.VolunteerEvents.AddRange(ev1, ev2);

            context.EventAttendances.AddRange(
                new EventAttendance
                {
                    Id = 1,
                    EventId = 1,
                    UserId = user.Id,
                    AttendanceStatusId = 1
                },
                new EventAttendance
                {
                    Id = 2,
                    EventId = 2,
                    UserId = user.Id,
                    AttendanceStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var result = (await repo.GetByUserIdAsync(user.Id)).ToList();

            Assert.Equal(2, result.Count);

            Assert.Equal(2, result[0].EventId);
            Assert.Equal(1, result[1].EventId);
        }

        [Fact]
        public async Task GetByUserAndEventAsync_ReturnsCorrectAttendance() //корректно возвращает посещение пользователя на ивент 
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            context.Users.Add(user);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.VolunteerEvents.Add(ev);

            context.EventAttendances.Add(
                new EventAttendance
                {
                    Id = 1,
                    EventId = 1,
                    UserId = user.Id,
                    AttendanceStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var result = await repo.GetByUserAndEventAsync(user.Id, 1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task CountParticipantsAsync_DoesNotCountCancelledAttendances() //не включает в подсчет отмененные
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            context.Users.Add(user);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.VolunteerEvents.Add(ev);

            context.EventAttendances.AddRange(
                new EventAttendance
                {
                    Id = 1,
                    EventId = 1,
                    UserId = user.Id,
                    AttendanceStatusId = 1
                },
                new EventAttendance
                {
                    Id = 2,
                    EventId = 1,
                    UserId = "user2",
                    AttendanceStatusId = 2
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var result = await repo.CountParticipantsAsync(1);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task CountParticipantsAsync_DoesNotCountDeletedAttendances()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            context.Users.Add(user);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.VolunteerEvents.Add(ev);

            context.EventAttendances.AddRange(
                new EventAttendance
                {
                    Id = 1,
                    EventId = 1,
                    UserId = user.Id,
                    AttendanceStatusId = 1,
                    IsDeleted = false
                },
                new EventAttendance
                {
                    Id = 2,
                    EventId = 1,
                    UserId = "user2",
                    AttendanceStatusId = 1,
                    IsDeleted = true
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var result = await repo.CountParticipantsAsync(1);

            Assert.Equal(1, result);
        }

        [Fact]
        public async Task AddAsync_SavesAttendance()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            context.Users.Add(user);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.VolunteerEvents.Add(ev);

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var att = new EventAttendance
            {
                Id = 1,
                EventId = 1,
                UserId = user.Id,
                AttendanceStatusId = 1
            };

            await repo.AddAsync(att);

            Assert.Single(context.EventAttendances);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesFields()
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user1"
            };

            context.Users.Add(user);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.VolunteerEvents.Add(ev);

            context.EventAttendances.Add(
                new EventAttendance
                {
                    Id = 1,
                    EventId = 1,
                    UserId = user.Id,
                    AttendanceStatusId = 1
                });

            await context.SaveChangesAsync();

            var repo = new AttendanceRepository(context);

            var updated = new EventAttendance
            {
                Id = 1,
                EventId = 1,
                UserId = user.Id,
                AttendanceStatusId = 2
            };

            await repo.UpdateAsync(updated);

            var result = await context.EventAttendances.FindAsync(1);

            Assert.Equal(2, result.AttendanceStatusId);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenAttendanceNotFound() //обработка исключений
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var repo = new AttendanceRepository(context);

            var att = new EventAttendance
            {
                Id = 999,
                EventId = 1,
                UserId = "user1",
                AttendanceStatusId = 1
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateAsync(att));
        }
    }
}