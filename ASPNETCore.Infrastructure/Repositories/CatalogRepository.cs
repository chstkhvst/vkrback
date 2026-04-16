using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ASPNETCore.Infrastructure.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        private readonly ApplicationDbContext _context;
        public CatalogRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AttendanceStatus>> GetAttendanceStatusesAsync()
        {
            return await _context.AttendanceStatuses
                .AsNoTracking()
                .ToListAsync();
        } 
        public async Task<AttendanceStatus?> GetAttendanceStatusByIdAsync(int id)
        {
            return await _context.AttendanceStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<IEnumerable<EventCategory>> GetEventCategoriesAsync()
        {
            return await _context.EventCategories
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<EventCategory?> GetEventCategoryByIdAsync(int id)
        {
            return await _context.EventCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task<IEnumerable<EventStatus>> GetEventStatusesAsync()
        {
            return await _context.EventStatuses
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EventStatus?> GetEventStatusByIdAsync(int id)
        {
            return await _context.EventStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<IEnumerable<ReportStatus>> GetReportStatusesAsync()
        {
            return await _context.ReportStatuses
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<ReportStatus?> GetReportStatusByIdAsync(int id)
        {
            return await _context.ReportStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public async Task<IEnumerable<VolunteerRank>> GetVolunteerRanksAsync()
        {
            return await _context.VolunteerRanks
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<VolunteerRank?> GetVolunteerRankByIdAsync(int id)
        {
            return await _context.VolunteerRanks
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<VolunteerRank?> GetRankForPointsAsync(int points)
        {
            return await _context.VolunteerRanks
                .Where(r => r.PointsRequired <= points)
                .OrderByDescending(r => r.PointsRequired)
                .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<City?> GetCityByIdAsync(int id)
        {
            return await _context.Cities
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

    }
}