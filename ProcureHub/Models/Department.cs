using System.ComponentModel.DataAnnotations;

namespace ProcureHub.Models;

public class Department
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
