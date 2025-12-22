using System.ComponentModel.DataAnnotations;

namespace ProcureHub.WebApi.Features.Auth;

public class User
{
    public required string Id { get; set; }

    public required string Email { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string[] Roles { get; set; }
}
