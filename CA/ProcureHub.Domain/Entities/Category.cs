namespace ProcureHub.Domain.Entities;

public class Category
{
    // TODO: where does this belong?
    public const int NameMaxLength = 100;
    
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
