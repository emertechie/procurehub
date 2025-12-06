using System.ComponentModel.DataAnnotations;

namespace SupportHub.Models;

public class Department
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string? Name { get; set; }
    
    public ICollection<Staff> Staff { get; set; } = new HashSet<Staff>();
    
    public DateTime CreatedAt {  get; set; }
    
    public DateTime UpdatedAt {  get; set; }
}
