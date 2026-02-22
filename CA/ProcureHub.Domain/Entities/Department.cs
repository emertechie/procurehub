namespace ProcureHub.Domain.Entities;

public class Department
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public ICollection<User> Users { get; init; } = new HashSet<User>();

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}


