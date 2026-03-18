using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Interfaces
{
    public interface IReportRepository
    {
        Task<UserReport?> GetByIdAsync(int id);
        Task<IEnumerable<UserReport>> GetAllAsync();
        Task<IEnumerable<UserReport>> GetBySenderIdAsync(string senderId);
        Task<IEnumerable<UserReport>> GetByReportedIdAsync(string reportedId);
        Task<UserReport> AddAsync(UserReport rep);
        Task<UserReport>UpdateAsync(UserReport rep);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
        Task<IEnumerable<UserReport>> GetByStatusIdAsync(int statusId);
        Task<IEnumerable<UserReport>> GetBySenderAndReportedAsync(string senderId, string reportedId);

        //можно добавить функционал типа если модер банит 1 чела то статус всех остальных 
        //жалоб на того же чела меняется на рассмотрена

        // жалобы в группе?
    }
}
