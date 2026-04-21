using ASPNETCore.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASPNETCore.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUserNameWithProfilesAsync(string userName);

        Task<User?> GetByIdWithProfilesAsync(string id);

        Task<User?> GetByIdWithFullDetailsAsync(string id);

        Task<(List<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search);
        Task<List<User>> GetForRatingAsync();

        Task UpdateAsync(User user);
    }
}
