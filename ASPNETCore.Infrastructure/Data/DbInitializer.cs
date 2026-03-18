using ASPNETCore.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace ASPNETCore.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Проверяем наличие ролей
            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole("moderator"));
                await roleManager.CreateAsync(new IdentityRole("organizer"));
                await roleManager.CreateAsync(new IdentityRole("volunteer"));
            }

            // Создание администратора
            if (await userManager.FindByNameAsync("admin") == null)
            {
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin@admin.com",
                    EmailConfirmed = true,
                    FullName = "Admin Admin"
                };

                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "moderator");
            }

            // AttendanceStatuses
            if (!context.AttendanceStatuses.Any())
            {
                context.AttendanceStatuses.AddRange(
                    new AttendanceStatus { AttendanceStatusName = "Запланировано" },
                    new AttendanceStatus { AttendanceStatusName = "Отменено" },
                    new AttendanceStatus { AttendanceStatusName = "Состоялось" },
                    new AttendanceStatus { AttendanceStatusName = "Неявка" }
                );
            }

            // EventStatuses
            if (!context.EventStatuses.Any())
            {
                context.EventStatuses.AddRange(
                    new EventStatus { EventStatusName = "На модерации" },
                    new EventStatus { EventStatusName = "Одобрено" },
                    new EventStatus { EventStatusName = "Отклонено" },
                    new EventStatus { EventStatusName = "Отменено" },
                    new EventStatus { EventStatusName = "Завершено" }
                );
            }

            // ReportStatuses
            if (!context.ReportStatuses.Any())
            {
                context.ReportStatuses.AddRange(
                    new ReportStatus { ReportStatusName = "На рассмотрении" },
                    new ReportStatus { ReportStatusName = "Одобрена" },
                    new ReportStatus { ReportStatusName = "Отклонена" }
                );
            }

            // VolunteerRanks
            if (!context.VolunteerRanks.Any())
            {
                context.VolunteerRanks.AddRange(
                    new VolunteerRank { RankName = "Новичок", PointsRequired = 0},
                    new VolunteerRank { RankName = "Любитель", PointsRequired = 100},
                    new VolunteerRank { RankName = "Активный волонтер", PointsRequired = 500 },
                    new VolunteerRank { RankName = "Опытный волонтер", PointsRequired = 1000 }
                );
            }

            // EventCategories
            if (!context.EventCategories.Any())
            {
                context.EventCategories.AddRange(
                    new EventCategory { CategoryName = "Экология и благоустройство" },
                    new EventCategory { CategoryName = "Социальная помощь" },
                    new EventCategory { CategoryName = "Здравоохранение" },
                    new EventCategory { CategoryName = "Культура" },
                    new EventCategory { CategoryName = "Спорт" },
                    new EventCategory { CategoryName = "Животные" },
                    new EventCategory { CategoryName = "Образование" }
                );
            }

            // Cities
            if (!context.Cities.Any())
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "russian-cities.json");

                var json = await File.ReadAllTextAsync(filePath);

                var cities = JsonSerializer.Deserialize<List<CitySeedModel>>(json);
                if (cities != null)
                {
                    var cityEntities = cities
                    .GroupBy(c => new { c.name, c.subject })
                    .Select(g => new City
                    {
                        Name = g.Key.name,
                        Subject = g.Key.subject
                    });
                    await context.Cities.AddRangeAsync(cityEntities);
                }
                await context.SaveChangesAsync();
            }
            if (!userManager.Users.Any(u => u.UserName == "user1"))
            {
                var user1 = new User
                {
                    UserName = "user1",
                    Email = "user1@test.com",
                    EmailConfirmed = true,
                    FullName = "User One"
                };

                await userManager.CreateAsync(user1, "User123!");
                await userManager.AddToRoleAsync(user1, "volunteer");
            }

            if (!userManager.Users.Any(u => u.UserName == "user2"))
            {
                var user2 = new User
                {
                    UserName = "user2",
                    Email = "user2@test.com",
                    EmailConfirmed = true,
                    FullName = "User Two"
                };

                await userManager.CreateAsync(user2, "User123!");
                await userManager.AddToRoleAsync(user2, "organizer");
            }

            await context.SaveChangesAsync();

            // Берем пользователей
            var userOne = await userManager.FindByNameAsync("user1");
            var userTwo = await userManager.FindByNameAsync("user2");
            var adminUser = await userManager.FindByNameAsync("admin");

            // VolunteerProfile
            if (!context.VolunteerProfiles.Any())
            {
                var rank = context.VolunteerRanks.First();

                context.VolunteerProfiles.Add(new VolunteerProfile
                {
                    UserId = userOne.Id,
                    RankId = rank.Id,
                    Points = 0
                });
            }

            // OrganizerProfile
            if (!context.OrganizerProfiles.Any())
            {
                context.OrganizerProfiles.Add(new OrganizerProfile
                {
                    UserId = userTwo.Id,
                    OrganizationName = "Test Org",
                    Ogrn = "1234567890"
                });
            }

            await context.SaveChangesAsync();

            // Event
            if (!context.VolunteerEvents.Any())
            {
                var category = context.EventCategories.First();
                var status = context.EventStatuses.First();
                var city = context.Cities.First();

                context.VolunteerEvents.Add(new VolunteerEvent
                {
                    Name = "Тестовое событие",
                    Description = "Описание",
                    EventDateTime = DateTime.UtcNow.AddDays(5),
                    EventPoints = 10,
                    ParticipantsLimit = 10,
                    EventCategoryId = category.Id,
                    EventStatusId = status.Id,
                    CityId = city.Id,
                    UserId = userTwo.Id, // организатор
                    ModeratedByUserId = adminUser.Id
                });
            }

            await context.SaveChangesAsync();

            // Attendance
            if (!context.EventAttendances.Any())
            {
                var ev = context.VolunteerEvents.First();
                var status = context.AttendanceStatuses.First();

                context.EventAttendances.Add(new EventAttendance
                {
                    EventId = ev.Id,
                    UserId = userOne.Id,
                    AttendanceStatusId = status.Id,
                    IsDeleted = false
                });
            }

            // UserReport 
            if (!context.UserReports.Any())
            {
                var reportStatus = context.ReportStatuses.First();

                context.UserReports.Add(new UserReport
                {
                    SenderUserId = userOne.Id,
                    ReportedUserId = userTwo.Id,
                    ReportReason = "Тестовая жалоба",
                    ReportStatusId = reportStatus.Id,
                    IsDeleted = false
                });
            }
            await context.SaveChangesAsync();
        }
    }
}
