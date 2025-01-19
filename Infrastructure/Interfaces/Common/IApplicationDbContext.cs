
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Interfaces.Common
{
    public interface IApplicationDbContext
    { 
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}