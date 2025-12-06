using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupportHub.Data;

namespace SupportHub.Models;

public class Staff
{
    [Key]
    public string UserId { get; set; }
    
    public ApplicationUser User { get; set; }
    
    public int DepartmentId { get; set; }

    public Department Department { get; set; }
    
    public DateTime EnabledAt {  get; set; }

    public DateTime CreatedAt {  get; set; }
    
    public DateTime UpdatedAt {  get; set; }
    
    public DateTime DeletedAt {  get; set; }
}

public class StaffEntityTypeConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder
            .HasOne(s => s.User)
            .WithOne()
            .HasForeignKey<Staff>(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne(s => s.Department)
            .WithMany(d => d.Staff)
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);;
    }
}