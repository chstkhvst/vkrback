using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ASPNETCore.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ApplicationDbContext _context;

        public ReportRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<UserReport?> GetByIdAsync(int id)
        {
            return await _context.UserReports
                .Include(r => r.Sender)
                    .ThenInclude(s => s.VolunteerProfile)
                .Include(r => r.Sender)
                    .ThenInclude(s => s.OrganizerProfile)
                .Include(r => r.Reported)
                    .ThenInclude(r => r.VolunteerProfile)
                .Include(r => r.Reported)
                    .ThenInclude(r => r.OrganizerProfile)
                .Include(r => r.ReportStatus)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        }
        public async Task<IEnumerable<UserReport>> GetAllAsync()
        {
            return await _context.UserReports
                .Include(r => r.Sender)
                .Include(r => r.Reported)
                .Include(r => r.ReportStatus)
                .Where(r => !r.IsDeleted)
                .OrderBy(r => r.ReportStatusId)
                .ThenBy(r => r.ReportedUserId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<UserReport>> GetBySenderIdAsync(string senderId)
        {
            return await _context.UserReports
                .Include(r => r.Reported)
                    .ThenInclude(r => r.VolunteerProfile)
                .Include(r => r.Reported)
                    .ThenInclude(r => r.OrganizerProfile)
                .Include(r => r.ReportStatus)
                .Where(r => r.SenderUserId == senderId && !r.IsDeleted)
                .OrderBy(r => r.ReportStatusId)
                .ThenBy(r => r.ReportedUserId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<UserReport>> GetByReportedIdAsync(string reportedId)
        {
            return await _context.UserReports
                .Include(r => r.Sender)
                    .ThenInclude(s => s.VolunteerProfile)
                .Include(r => r.Sender)
                    .ThenInclude(s => s.OrganizerProfile)
                .Include(r => r.ReportStatus)
                .Where(r => r.ReportedUserId == reportedId && !r.IsDeleted)
                .OrderBy(r => r.ReportStatusId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<UserReport>> GetByStatusIdAsync(int statusId)
        {
            return await _context.UserReports
                .Include(r => r.Sender)
                .Include(r => r.Reported)
                .Where(r => r.ReportStatusId == statusId && !r.IsDeleted)
                .OrderByDescending(r => r.ReportedUserId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<UserReport>> GetBySenderAndReportedAsync(string senderId, string reportedId)
        {
            return await _context.UserReports
                .Include(r => r.ReportStatus)
                .Include(r => r.Sender)
                .Include(r => r.Reported)
                .Where(r => r.SenderUserId == senderId &&
                           r.ReportedUserId == reportedId &&
                           !r.IsDeleted)
                .OrderBy(r => r.ReportStatusId)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<ReportGroup>> GetGroupedReportsAsync(int? statusId = null, string? keywords = null)
        {
            var query = _context.UserReports
                .Include(r => r.Sender)
                .Include(r => r.Reported)
                .Include(r => r.ReportStatus)
                .Where(r => !r.IsDeleted);

            if (statusId != null)
            {
                query = query.Where(r => r.ReportStatusId == statusId);
            }

            if (!string.IsNullOrWhiteSpace(keywords))
            {
                var lower = keywords.ToLower();

                query = query.Where(r =>
                    r.Reported.UserName.ToLower().Contains(lower) ||
                    r.Reported.FullName.ToLower().Contains(lower) 
                );
            }
            var reports = await query
                .AsNoTracking()
                .ToListAsync();

            var grouped = reports
                .GroupBy(r => r.ReportedUserId)
                .Select(g => new ReportGroup
                {
                    ReportedUserId = g.Key,
                    ReportedUser = g.First().Reported, 
                    Count = g.Count(),
                    Reports = g.ToList()
                })
                .OrderByDescending(g => g.Reports.Count(r => r.ReportStatusId == 1))
                .ThenByDescending(g => g.Count);

            return grouped;
        }
        public async Task<UserReport> AddAsync(UserReport rep)
        {
            _context.UserReports.Add(rep);
            int changes = await _context.SaveChangesAsync();
            return rep;
        }
        public async Task<UserReport> UpdateAsync(UserReport rep)
        {
            var existing = await _context.UserReports.FindAsync(rep.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Жалоба с ID {rep.Id} не найдена.");

            _context.Entry(existing).CurrentValues.SetValues(rep);
            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task DeleteAsync(int id)
        {
            var rep = await _context.UserReports.FindAsync(id);
            if (rep != null)
            {
                _context.UserReports.Remove(rep);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SoftDeleteAsync(int id)
        {
            var rep = await _context.UserReports
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (rep != null)
            {
                rep.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
