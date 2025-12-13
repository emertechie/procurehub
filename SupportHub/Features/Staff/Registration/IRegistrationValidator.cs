namespace SupportHub.Features.Staff.Registration;

public interface IRegistrationValidator
{
    Task<bool> IsRegistrationAllowed(string userEmail);
}