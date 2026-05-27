using ASPNETCore.Domain.Entities;
using ASPNETCore.Infrastructure.Data;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Tests.InfrastructureTests
{
    public class EventRepositoryTests
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
            context.EventCategories.AddRange(
                new EventCategory
                {
                    Id = 1,
                    CategoryName = "Category1"
                },
                new EventCategory
                {
                    Id = 2,
                    CategoryName = "Category2"
                });

            context.EventStatuses.AddRange(
                new EventStatus
                {
                    Id = 1,
                    EventStatusName = "На модерации"
                },
                new EventStatus
                {
                    Id = 2,
                    EventStatusName = "Одобрено"
                });

            context.Cities.AddRange(
                new City
                {
                    Id = 1,
                    Name = "Moscow"
                },
                new City
                {
                    Id = 2,
                    Name = "SPB"
                });

            context.AttendanceStatuses.Add(
                new AttendanceStatus
                {
                    Id = 1,
                    AttendanceStatusName = "Запланировано"
                });

            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsEventWithNavigationProperties() //навигационные свойства
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "testuser"
            };

            var organizerProfile = new OrganizerProfile
            {
                UserId = "user1",
                OrganizationName = "Org",
                Ogrn = "123"
            };

            var attendee = new User
            {
                Id = "user2",
                UserName = "attendee"
            };

            context.Users.AddRange(user, attendee);
            context.OrganizerProfiles.Add(organizerProfile);

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Test Event",
                Description = "Description",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1,
                Attendees = new List<EventAttendance>
                {
                    new EventAttendance
                    {
                        Id = 1,
                        UserId = attendee.Id,
                        AttendanceStatusId = 1
                    }
                }
            };

            context.VolunteerEvents.Add(ev);

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.NotNull(result.User);
            Assert.NotNull(result.EventCategory);
            Assert.NotNull(result.EventStatus);
            Assert.NotNull(result.City);

            Assert.Single(result.Attendees);

            Assert.NotNull(result.Attendees.First().User);
            Assert.NotNull(result.Attendees.First().AttendanceStatus);
        }

        [Fact]
        public async Task GetEventsByUserIdAsync_ReturnsOnlyUserEvents() // возвращает корректно ивенты 1 пользователя
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

            context.VolunteerEvents.AddRange(
                new VolunteerEvent
                {
                    Id = 1,
                    UserId = user1.Id,
                    Name = "Event1",
                    Description = "Desc",
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                },
                new VolunteerEvent
                {
                    Id = 2,
                    UserId = user2.Id,
                    Name = "Event2",
                    Description = "Desc",
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetEventsByUserIdAsync("user1");

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task GetAllAsync_DoesNotReturnDeletedEvents() //софтделитнутые не инклюдает
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            context.VolunteerEvents.AddRange(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Active",
                    Description = "Desc",
                    UserId = user.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1,
                    IsDeleted = false
                },
                new VolunteerEvent
                {
                    Id = 2,
                    Name = "Deleted",
                    Description = "Desc",
                    UserId = user.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1,
                    IsDeleted = true
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetAllAsync();

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Fact]
        public async Task GetFilteredAsync_FiltersByCategory() //проверка фильтров по категории
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            context.VolunteerEvents.AddRange(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Event1",
                    Description = "Desc",
                    UserId = user.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                },
                new VolunteerEvent
                {
                    Id = 2,
                    Name = "Event2",
                    Description = "Desc",
                    UserId = user.Id,
                    EventCategoryId = 2,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetFilteredAsync(1, null, null, null);

            Assert.Single(result);
            Assert.Equal(1, result.First().EventCategoryId);
        }

        [Fact]
        public async Task GetFilteredAsync_FiltersByCity() // по городам
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            context.VolunteerEvents.AddRange(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Event1",
                    Description = "Desc",
                    UserId = user.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                },
                new VolunteerEvent
                {
                    Id = 2,
                    Name = "Event2",
                    Description = "Desc",
                    UserId = user.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 2
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetFilteredAsync(null, 1, null, null);

            Assert.Single(result);
            Assert.Equal(1, result.First().CityId);
        }

        [Fact]
        public async Task GetFilteredAsync_SearchesByName() // по названию
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            context.VolunteerEvents.Add(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Clean City",
                    Description = "Desc",
                    UserId = "user1",
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetFilteredAsync(null, null, "clean", null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetFilteredAsync_SearchesByDescription() // включает слова из описания в поиск
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            context.VolunteerEvents.Add(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Event",
                    Description = "Save nature",
                    UserId = "user1",
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetFilteredAsync(null, null, "nature", null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetFilteredAsync_SearchesByUserName() // по автору ивента
        {
            var context = GetContext();

            await SeedRequiredData(context);

            context.Users.Add(new User
            {
                Id = "user1",
                UserName = "volunteer123"
            });

            context.VolunteerEvents.Add(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Event",
                    Description = "Desc",
                    UserId = "user1",
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetFilteredAsync(null, null, "volunteer", null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetFilteredAsync_KeywordSearch() 
        {
            var context = GetContext();

            await SeedRequiredData(context);

            context.Users.Add(new User
            {
                Id = "user1",
                UserName = "user"
            });

            context.VolunteerEvents.Add(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Clean City",
                    Description = "Desc",
                    UserId = "user1",
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetFilteredAsync(null, null, "  CLEAN  ", null);

            Assert.Single(result);
        }

        [Fact]
        public async Task GetPagedOrgAsync_ReturnsOnlyOrganizerEvents() // только ивенты организаций
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var organizer = new User
            {
                Id = "org",
                UserName = "org"
            };

            var volunteer = new User
            {
                Id = "vol",
                UserName = "vol"
            };

            context.Users.AddRange(organizer, volunteer);

            context.OrganizerProfiles.Add(
                new OrganizerProfile
                {
                    UserId = organizer.Id,
                    OrganizationName = "Org",
                    Ogrn = "123"
                });

            context.VolunteerEvents.AddRange(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Org Event",
                    Description = "Desc",
                    UserId = organizer.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                },
                new VolunteerEvent
                {
                    Id = 2,
                    Name = "Community Event",
                    Description = "Desc",
                    UserId = volunteer.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetPagedOrgAsync(
                1,
                10,
                null,
                null,
                null,
                null,
                null);

            Assert.Single(result.Items);
            Assert.Equal(1, result.Items.First().Id);
        }

        [Fact]
        public async Task GetPagedOrgAsync_ReturnsCorrectPagination() // проверка пагинации
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var organizer = new User
            {
                Id = "org",
                UserName = "org"
            };

            context.Users.Add(organizer);

            context.OrganizerProfiles.Add(
                new OrganizerProfile
                {
                    UserId = organizer.Id,
                    OrganizationName = "Org",
                    Ogrn = "123"
                });

            for (int i = 1; i <= 5; i++)
            {
                context.VolunteerEvents.Add(
                    new VolunteerEvent
                    {
                        Id = i,
                        Name = $"Event{i}",
                        Description = "Desc",
                        UserId = organizer.Id,
                        EventCategoryId = 1,
                        EventStatusId = 1,
                        CityId = 1
                    });
            }

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var result = await repo.GetPagedOrgAsync(
                2,
                2,
                null,
                null,
                null,
                null,
                null);

            Assert.Equal(2, result.Items.Count);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(2, result.CurrentPage);
        }

        [Fact]
        public async Task AddAsync_SavesEvent() // сохраняет ивент
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

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

            await repo.AddAsync(ev);

            Assert.Single(context.VolunteerEvents);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesEventFields() // апдейт
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var user = new User
            {
                Id = "user1",
                UserName = "user"
            };

            context.Users.Add(user);

            context.VolunteerEvents.Add(
                new VolunteerEvent
                {
                    Id = 1,
                    Name = "Old",
                    Description = "Old desc",
                    UserId = user.Id,
                    EventCategoryId = 1,
                    EventStatusId = 1,
                    CityId = 1
                });

            await context.SaveChangesAsync();

            var repo = new EventRepository(context);

            var updated = new VolunteerEvent
            {
                Id = 1,
                Name = "New",
                Description = "New desc",
                UserId = user.Id,
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            await repo.UpdateAsync(updated);

            var result = await context.VolunteerEvents.FindAsync(1);

            Assert.Equal("New", result.Name);
            Assert.Equal("New desc", result.Description);
        }

        [Fact]
        public async Task UpdateAsync_Throws_WhenEventDoesNotExist() // обработка если не существует
        {
            var context = GetContext();

            await SeedRequiredData(context);

            var repo = new EventRepository(context);

            var ev = new VolunteerEvent
            {
                Id = 999,
                Name = "Event",
                Description = "Desc",
                UserId = "user1",
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                repo.UpdateAsync(ev));
        }
    }
}