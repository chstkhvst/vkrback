using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Infrastructure.Repositories
{
    public class UserWithMonthlyPoints
    {
        public User User { get; set; }
        public int MonthlyPoints { get; set; }
    }
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;

        public UserRepository(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        //для логина
        public async Task<User?> GetByUserNameWithProfilesAsync(string userName)
        {
            return await _userManager.Users
                .Include(u => u.VolunteerProfile)
                .Include(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }

        //для current user и update profile
        public async Task<User?> GetByIdWithProfilesAsync(string id)
        {
            return await _userManager.Users
                .Include(u => u.VolunteerProfile)
                    .ThenInclude(vp => vp.Rank)
                .Include(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        //для модера
        public async Task<User?> GetByIdWithFullDetailsAsync(string id)
        {
            return await _userManager.Users
                .Include(u => u.VolunteerProfile)
                .Include(u => u.OrganizerProfile)
                .Include(u => u.Bans.OrderByDescending(b => b.CreatedAt))
                .Include(u => u.UserReports)
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        //пагинация
        public async Task<(List<User> Users, int TotalCount)> GetPagedAsync(
            int page,
            int pageSize,
            string? search)
        {
            var query = _userManager.Users
                .Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.ToLower();

                query = query.Where(u =>
                    (u.UserName != null && u.UserName.ToLower().Contains(normalized)) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(normalized))
                );
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .Include(u => u.VolunteerProfile)
                .Include(u => u.OrganizerProfile)
                .OrderBy(u => u.UserName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
        public async Task<List<User>> GetForRatingAllAsync()
        {
            return await _userManager.Users
                .Where(u => !u.IsDeleted && u.VolunteerProfile != null)
                .Include(u => u.VolunteerProfile)
                    .ThenInclude(vp => vp.Rank)
                .OrderByDescending(u => u.VolunteerProfile.Points)
                .Take(50)
                .ToListAsync();
        }
        public async Task<List<(User User, int MonthlyPoints)>> GetForRatingMonthlyAsync(int months = 1)
        {
            var startDate = DateTime.UtcNow.AddMonths(-months);

            var userPoints = await _userManager.Users
                .Where(u => !u.IsDeleted && u.VolunteerProfile != null)
                .Select(u => new
                {
                    UserId = u.Id,
                    MonthlyPoints = u.EventAttendance
                        .Where(a => a.AttendanceStatusId == 3 &&
                                    a.VolunteerEvent.EventDateTime >= startDate)
                        .Sum(a => a.VolunteerEvent.EventPoints)
                })
                .Where(x => x.MonthlyPoints > 0)
                .OrderByDescending(x => x.MonthlyPoints)
                .Take(20)
                .ToListAsync();

            var userIds = userPoints.Select(x => x.UserId).ToList();

            var users = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.VolunteerProfile)
                    .ThenInclude(vp => vp.Rank)
                .ToListAsync();

            var result = userPoints
                .Join(users, up => up.UserId, u => u.Id, (up, u) => (User: u, MonthlyPoints: up.MonthlyPoints))
                .OrderByDescending(x => x.MonthlyPoints)
                .ToList();

            return result;
        }
        public async Task UpdateAsync(User user)
        {
            await _userManager.UpdateAsync(user);
        }
    }
}
