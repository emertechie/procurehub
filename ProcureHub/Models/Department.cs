using System.ComponentModel.DataAnnotations;

namespace ProcureHub.Models;

public class Department
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string? Name { get; set; }

    public ICollection<User> Users { get; set; } = new HashSet<User>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
