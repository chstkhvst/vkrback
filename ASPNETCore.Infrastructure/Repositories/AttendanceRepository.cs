using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Infrastructure.Repositories
{
    public class AttendanceRepository : IAttendanceRepositiory
    {
        private readonly ApplicationDbContext _context;

        public AttendanceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<EventAttendance?> GetByIdAsync(int id)
        {
            return await _context.EventAttendances
                .Include(a => a.VolunteerEvent)
                .Include(a => a.AttendanceStatus)
                .Include(a => a.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(a => a.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);
        }

        public async Task<IEnumerable<EventAttendance>> GetAllAsync()
        {
            return await _context.EventAttendances
                .Include(a => a.VolunteerEvent)
                .Include(a => a.AttendanceStatus)
                .Include(a => a.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(a => a.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .Where(a => !a.IsDeleted)
                .OrderByDescending(r => r.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<EventAttendance>> GetByUserIdAsync(string userId)
        {
            return await _context.EventAttendances
                .Include(a => a.VolunteerEvent)
                .Include(a => a.AttendanceStatus)
                .Where(a => a.UserId == userId && !a.IsDeleted)
                .OrderByDescending(a => a.VolunteerEvent.EventDateTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<EventAttendance?>> GetByEventIdAsync(int eventId)
        {
            return await _context.EventAttendances
                .Include(a => a.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(a => a.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .Include(a => a.AttendanceStatus)
                .Where(a => a.EventId == eventId && !a.IsDeleted)
                .OrderByDescending(a => a.Id)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EventAttendance?> GetByUserAndEventAsync(string userId, int eventId)
        {
            return await _context.EventAttendances
                .Include(a => a.AttendanceStatus)
                .FirstOrDefaultAsync(a => a.UserId == userId && a.EventId == eventId && !a.IsDeleted);
        }

        public async Task<int> CountParticipantsAsync(int eventId)
        {
            return await _context.EventAttendances
                .CountAsync(a => a.EventId == eventId && !a.IsDeleted);
        }

        public async Task<EventAttendance> AddAsync(EventAttendance att)
        {
            _context.EventAttendances.Add(att);
            await _context.SaveChangesAsync();

            return att;
        }

        public async Task<EventAttendance> UpdateAsync(EventAttendance att)
        {
            var existing = await _context.EventAttendances.FindAsync(att.Id);

            if (existing == null)
                throw new KeyNotFoundException($"Посещение c {att.Id} не найдено.");

            _context.Entry(existing).CurrentValues.SetValues(att);

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var att = await _context.EventAttendances.FindAsync(id);
            if (att != null)
            {
                _context.EventAttendances.Remove(att);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var att = await _context.EventAttendances.FindAsync(id);
            if (att != null)
            {
                att.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
