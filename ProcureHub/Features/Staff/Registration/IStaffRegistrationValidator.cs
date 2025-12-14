namespace ProcureHub.Features.Staff.Registration;

public interface IStaffRegistrationValidator
{
    Task<bool> IsRegistrationAllowedAsync(string userEmail);
}