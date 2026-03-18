using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Interfaces
{
    public interface IAttendanceRepositiory
    {
        Task<EventAttendance?> GetByIdAsync(int id);
        Task<IEnumerable<EventAttendance>> GetAllAsync();
        Task<IEnumerable<EventAttendance>> GetByUserIdAsync(string userId);
        Task<IEnumerable<EventAttendance>> GetByEventIdAsync(int eventId);
        Task<EventAttendance> AddAsync(EventAttendance att);
        Task<EventAttendance> UpdateAsync(EventAttendance att);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task<int> CountParticipantsAsync(int eventId);
        Task<EventAttendance?> GetByUserAndEventAsync(string userId, int eventId);
    }
}
