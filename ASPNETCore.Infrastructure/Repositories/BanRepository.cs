using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Infrastructure.Repositories
{
    public class BanRepository : IBanRepository
    {
        private readonly ApplicationDbContext _context;
        public BanRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<bool> IsUserBannedAsync(string userId)
        {
            return await _context.Bans
                .AnyAsync(b => b.BannedUserId == userId &&
                               b.IsActive &&
                               !b.IsDeleted);
        }
        public async Task<IEnumerable<Ban>> GetAllAsync(string? search)
        {
            var query = _context.Bans
                .Include(b => b.Moder)
                .Include(b => b.BannedUser)
                .Where(b => !b.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lower = search.ToLower();

                query = query.Where(b =>
                    (b.BanReason != null && b.BanReason.ToLower().Contains(lower)) ||
                    (b.Moder != null && b.Moder.UserName.ToLower().Contains(lower)) ||
                    (b.BannedUser != null && b.BannedUser.UserName.ToLower().Contains(lower))
                );
            }

            return await query
                .OrderByDescending(b => b.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<Ban>> GetByUserIdAsync(string userId)
        {
            return await _context.Bans
                .Include(b => b.Moder)
                .Include(b => b.BannedUser)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(b => b.BannedUser)
                    .ThenInclude(u => u.OrganizerProfile)
                .Where(b => b.BannedUserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<Ban>> GetByModerIdAsync(string moderId)
        {
            return await _context.Bans
                .Include(b => b.BannedUser)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(b => b.BannedUser)
                    .ThenInclude(u => u.OrganizerProfile)
                .Include(b => b.Moder)
                .Where(b => b.ModerId == moderId && !b.IsDeleted)
                .OrderByDescending(b => b.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<Ban?> GetByIdAsync(int id)
        {
            return await _context.Bans
                .Include(b => b.Moder)
                .Include(b => b.BannedUser)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(b => b.BannedUser)
                    .ThenInclude(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        }
        public async Task<Ban> AddAsync(Ban ban)
        {
            _context.Bans.Add(ban);
            await _context.SaveChangesAsync();

            return ban;
        }
        public async Task<Ban> UpdateAsync(Ban ban)
        {
            var existing = await _context.Bans.FindAsync(ban.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Бан с ID {ban.Id} не найден.");

            _context.Entry(existing).CurrentValues.SetValues(ban);
            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task DeleteAsync(int id)
        {
            var ban = await _context.Bans.FindAsync(id);
            if (ban != null)
            {
                _context.Bans.Remove(ban);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SoftDeleteAsync(int id)
        {
            var ban = await _context.Bans
                .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (ban != null)
            {
                ban.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
