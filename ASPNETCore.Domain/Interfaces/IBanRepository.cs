using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Interfaces
{
    public interface IBanRepository
    {
        Task<bool> IsUserBannedAsync(string userId);
        Task<Ban?> GetByIdAsync(int id);
        Task<IEnumerable<Ban>> GetAllAsync(string? search);
        Task<IEnumerable<Ban>> GetByUserIdAsync(string userId);
        Task<IEnumerable<Ban>> GetByModerIdAsync(string moderId);
        Task<Ban> AddAsync(Ban ban);
        Task<Ban> UpdateAsync(Ban ban);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
    }
}
