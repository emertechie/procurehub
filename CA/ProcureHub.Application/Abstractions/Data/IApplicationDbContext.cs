using Microsoft.EntityFrameworkCore;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    
    DbSet<Department> Departments { get; } 
    
    DbSet<Category> Categories { get; }
    
    DbSet<PurchaseRequest> PurchaseRequests { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
