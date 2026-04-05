using ASPNETCore.Domain.Entities;
using ASPNETCore.Domain.Interfaces;
using ASPNETCore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


namespace ASPNETCore.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly ApplicationDbContext _context;
        public EventRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        private IQueryable<VolunteerEvent> ApplyFilters(
            IQueryable<VolunteerEvent> query,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            if (catId.HasValue)
                query = query.Where(e => e.EventCategoryId == catId.Value);

            if (cityId.HasValue)
                query = query.Where(e => e.CityId == cityId.Value);

            if (dateTime.HasValue)
            {
                var date = dateTime.Value.Date;
                query = query.Where(e => e.EventDateTime.HasValue &&
                    e.EventDateTime.Value.Date == date);
            }

            if (!string.IsNullOrWhiteSpace(keyWords))
            {
                var searchTerm = keyWords.Trim().ToLower();
                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchTerm) ||
                    e.Description.ToLower().Contains(searchTerm));
            }

            return query;
        }
        public async Task<VolunteerEvent?> GetByIdAsync(int id)
        {
            return await _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)

                .Include(e => e.User)
                    .ThenInclude(u => u.VolunteerProfile)

                .Include(e => e.User)
                    .ThenInclude(u => u.OrganizerProfile)

                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)

                .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)

                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
        }

        public async Task<IEnumerable<VolunteerEvent>> GetEventsByUserIdAsync(string userId)
        {
            return await _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted && e.UserId == userId)
                .OrderBy(e => e.EventDateTime)
                .AsSplitQuery()
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<VolunteerEvent>> GetAllAsync()
        {
            return await _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(a => a.EventStatus)
                .Include(a => a.City)
                .Include(a => a.User)
                    .ThenInclude(u => u.VolunteerProfile)
                 .Include(a => a.User)
                    .ThenInclude(u => u.OrganizerProfile)
                 .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)
                 .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.Id)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<VolunteerEvent>> GetFilteredAsync(
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var query = _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)
                .Include(a => a.User)
                    .ThenInclude(u => u.VolunteerProfile)
                 .Include(a => a.User)
                    .ThenInclude(u => u.OrganizerProfile)
                 .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted)
                .AsNoTracking();
            query = ApplyFilters(query, catId, cityId, keyWords, dateTime);

            return await query
                .OrderBy(e => e.EventDateTime)
                .ToListAsync();
        }
        // все ивенты для модера, созданные организаторами
        public async Task<PaginatedResponse<VolunteerEvent>> GetPagedOrgAsync(
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var query = _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)
                .Include(e => e.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(e => e.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted)
                .Where(e => e.User.OrganizerProfile != null)
                .AsNoTracking();
            query = ApplyFilters(query, catId, cityId, keyWords, dateTime);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EventDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<VolunteerEvent>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };
        }
        // все ивенты для юзеров, созданные организациями
        public async Task<PaginatedResponse<VolunteerEvent>> GetPagedOrgForUserAsync( 
            string userId,
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var query = _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)
                .Include(e => e.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(e => e.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted)
                .Where(e => e.EventStatusId == 2)
                .Where(e => e.User.OrganizerProfile!= null)
                .AsNoTracking();

            query = ApplyFilters(query, catId, cityId, keyWords, dateTime);

            query = query.Where(e =>
                !e.Attendees.Any(a =>
                    a.UserId == userId &&
                    !a.IsDeleted &&
                    (a.AttendanceStatusId == 1 || a.AttendanceStatusId == 3)
                ) && //лимит участников
                (e.ParticipantsLimit == null ||
                e.Attendees.Count(a => !a.IsDeleted && a.AttendanceStatusId == 1) < e.ParticipantsLimit)
            );

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EventDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<VolunteerEvent>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };
        }
        // все ивенты для модера, созданные волонтерами
        public async Task<PaginatedResponse<VolunteerEvent>> GetPagedCommunityEventsAsync(
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var query = _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)
                .Include(e => e.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(e => e.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted)
                .Where(e => e.User.OrganizerProfile == null)
                .AsNoTracking();
            query = ApplyFilters(query, catId, cityId, keyWords, dateTime);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EventDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<VolunteerEvent>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };
        }
        // все ивенты созданные волонтерами для отображения волонтерам
        public async Task<PaginatedResponse<VolunteerEvent>> GetPagedCommunityEventsForUserAsync(
            string userId,
            int pageNumber,
            int pageSize,
            int? catId,
            int? cityId,
            string? keyWords,
            DateTime? dateTime)
        {
            var query = _context.VolunteerEvents
                .Include(e => e.EventCategory)
                .Include(e => e.EventStatus)
                .Include(e => e.City)
                .Include(e => e.User)
                    .ThenInclude(u => u.VolunteerProfile)
                .Include(e => e.User)
                    .ThenInclude(u => u.OrganizerProfile)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.AttendanceStatus)
                .Include(e => e.Attendees)
                    .ThenInclude(a => a.User)
                .Where(e => !e.IsDeleted)
                .Where(e => e.EventStatusId == 2)
                .Where(e => e.User.OrganizerProfile == null)
                .AsNoTracking();

            query = ApplyFilters(query, catId, cityId, keyWords, dateTime);

            query = query.Where(e =>
                !e.Attendees.Any(a =>
                    a.UserId == userId &&
                    !a.IsDeleted &&
                    (a.AttendanceStatusId == 1 || a.AttendanceStatusId == 3)
                ) && //лимит участников
                (e.ParticipantsLimit == null ||
                e.Attendees.Count(a => !a.IsDeleted && a.AttendanceStatusId == 1) < e.ParticipantsLimit)
            );

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(e => e.EventDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginatedResponse<VolunteerEvent>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                TotalPages = totalPages
            };
        }
        public async Task<VolunteerEvent> AddAsync(VolunteerEvent volunteerEvent)
        {
            _context.VolunteerEvents.Add(volunteerEvent);
            int changes = await _context.SaveChangesAsync();
            return volunteerEvent;
        }
        public async Task<VolunteerEvent> UpdateAsync(VolunteerEvent ev)
        {
            var existing = await _context.VolunteerEvents.FindAsync(ev.Id);
            if (existing == null)
                throw new KeyNotFoundException($"Мероприятие с {ev.Id} не найдена.");

            _context.Entry(existing).CurrentValues.SetValues(ev);
            await _context.SaveChangesAsync();
            return existing;
        }
        public async Task DeleteAsync(int id)
        {
            var ev = await _context.VolunteerEvents.FindAsync(id);
            if (ev != null)
            {
                _context.VolunteerEvents.Remove(ev);
                await _context.SaveChangesAsync();
            }
        }
        public async Task SoftDeleteAsync(int id)
        {
            var eventToDelete = await _context.VolunteerEvents
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (eventToDelete != null)
            {
                eventToDelete.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
