namespace SupportHub.Features.Staff.Registration;

public class DummyStaffRegistrationValidator : IStaffRegistrationValidator
{
    // TODO: Move this to a database table or configuration
    private readonly HashSet<string> _allowedEmails = new(StringComparer.OrdinalIgnoreCase)
    {
        "staff1@example.com",
        "staff2@example.com",
        "test@test.com"
    };

    public Task<bool> IsRegistrationAllowedAsync(string userEmail)
    {
        return Task.FromResult(_allowedEmails.Contains(userEmail));
    }
}