namespace SupportHub.Features.Staff.Registration;

public interface IStaffRegistrationValidator
{
    Task<bool> IsRegistrationAllowedAsync(string userEmail);
}