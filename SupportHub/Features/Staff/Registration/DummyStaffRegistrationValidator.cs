namespace SupportHub.Features.Staff.Registration;

public class DummyStaffRegistrationValidator : IStaffRegistrationValidator
{
    // TODO: Move this to a database table or configuration
    public static readonly HashSet<string> AllowedEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "staff1@example.com",
        "staff2@example.com",
        "test@test.com"
    };

    public Task<bool> IsRegistrationAllowedAsync(string userEmail)
    {
        return Task.FromResult(AllowedEmails.Contains(userEmail));
    }
}