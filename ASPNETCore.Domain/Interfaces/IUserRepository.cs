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
        Task<User?> GetByIdWithDetailsAsync(string id);
        Task<(List<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search);
    }
}
