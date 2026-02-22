using Microsoft.EntityFrameworkCore;
using ProcureHub.Domain.Entities;

namespace ProcureHub.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    public DbSet<Department> Departments { get; set; } 
    
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<PurchaseRequest> PurchaseRequests { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
