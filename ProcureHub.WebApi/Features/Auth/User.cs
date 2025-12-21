using System.ComponentModel.DataAnnotations;

namespace ProcureHub.WebApi.Features.Auth;

public class User
{
    [Required]
    public string Id { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string[] Roles { get; set; }
}
