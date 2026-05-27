using ASPNETCore.Domain.Entities;
using ASPNETCore.Infrastructure.Data;
using ASPNETCore.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Tests.InfrastructureTests
{
    public class NotificationRepositoryTests
    {
        private ApplicationDbContext GetContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetByRecipientIdAsync_ReturnsNavigationProperties()
        {
            // Arrange
            var context = GetContext();

            var type = new NotificationType
            {
                Id = 1,
                NotificationTypeName = "Изменение"
            };

            var ev = new VolunteerEvent
            {
                Id = 1,
                Name = "Event",
                Description = "Desc",
                UserId = "user1",
                EventCategoryId = 1,
                EventStatusId = 1,
                CityId = 1
            };

            context.NotificationTypes.Add(type);
            context.VolunteerEvents.Add(ev);

            context.Notifications.Add(
                new Notification
                {
                    Id = 1,
                    RecipientId = "user1",
                    Message = "Changed",
                    TypeId = type.Id,
                    NotificationType = type,
                    EventId = ev.Id,
                    VolunteerEvent = ev,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repo = new NotificationRepository(context);

            // Act
            var result = (await repo.GetByRecipientIdAsync("user1")).First();

            // Assert
            Assert.NotNull(result.NotificationType);
            Assert.NotNull(result.VolunteerEvent);
        }

        [Fact]
        public async Task GetUnreadCountAsync_ReturnsCorrectCount()
        {
            // Arrange
            var context = GetContext();

            context.Notifications.AddRange(
                new Notification
                {
                    Id = 1,
                    RecipientId = "user1",
                    Message = "Unread1",
                    TypeId = 1,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    Id = 2,
                    RecipientId = "user1",
                    Message = "Unread2",
                    TypeId = 1,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    Id = 3,
                    RecipientId = "user1",
                    Message = "Read",
                    TypeId = 1,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repo = new NotificationRepository(context);

            // Act
            var result = await repo.GetUnreadCountAsync("user1");

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public async Task AddAsync_SavesNotification()
        {
            // Arrange
            var context = GetContext();

            var repo = new NotificationRepository(context);

            var notification = new Notification
            {
                Id = 1,
                RecipientId = "user1",
                Message = "Test",
                TypeId = 1,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            // Act
            await repo.AddAsync(notification);

            // Assert
            Assert.Single(context.Notifications);
        }

        [Fact]
        public async Task AddRangeAsync_SavesAllNotifications()
        {
            // Arrange
            var context = GetContext();

            var repo = new NotificationRepository(context);

            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = 1,
                    RecipientId = "user1",
                    Message = "One",
                    TypeId = 1,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    Id = 2,
                    RecipientId = "user1",
                    Message = "Two",
                    TypeId = 1,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Act
            await repo.AddRangeAsync(notifications);

            // Assert
            Assert.Equal(2, context.Notifications.Count());
        }

        [Fact]
        public async Task MarkAsReadAsync_MarksNotificationAsRead()
        {
            // Arrange
            var context = GetContext();

            context.Notifications.Add(
                new Notification
                {
                    Id = 1,
                    RecipientId = "user1",
                    Message = "Unread",
                    TypeId = 1,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repo = new NotificationRepository(context);

            // Act
            await repo.MarkAsReadAsync(1);

            // Assert
            var result = await context.Notifications.FindAsync(1);

            Assert.True(result.IsRead);
        }

        [Fact]
        public async Task MarkAsReadAsync_DoesNothing_WhenAlreadyRead()
        {
            // Arrange
            var context = GetContext();

            context.Notifications.Add(
                new Notification
                {
                    Id = 1,
                    RecipientId = "user1",
                    Message = "Read",
                    TypeId = 1,
                    IsRead = true,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repo = new NotificationRepository(context);

            // Act
            await repo.MarkAsReadAsync(1);

            // Assert
            var result = await context.Notifications.FindAsync(1);

            Assert.True(result.IsRead);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_MarksOnlyRecipientNotifications()
        {
            // Arrange
            var context = GetContext();

            context.Notifications.AddRange(
                new Notification
                {
                    Id = 1,
                    RecipientId = "user1",
                    Message = "Unread1",
                    TypeId = 1,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    Id = 2,
                    RecipientId = "user1",
                    Message = "Unread2",
                    TypeId = 1,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Notification
                {
                    Id = 3,
                    RecipientId = "user2",
                    Message = "Other user",
                    TypeId = 1,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });

            await context.SaveChangesAsync();

            var repo = new NotificationRepository(context);

            // Act
            await repo.MarkAllAsReadAsync("user1");

            // Assert
            var user1Notifications = context.Notifications
                .Where(n => n.RecipientId == "user1")
                .ToList();

            var user2Notification = await context.Notifications.FindAsync(3);

            Assert.All(user1Notifications, n => Assert.True(n.IsRead));

            Assert.False(user2Notification.IsRead);
        }
    }
}
