using Microsoft.AspNetCore.Identity;
using SupportHub.Common;

namespace SupportHub.Features.Staff;

public static class StaffErrors
{
    public static Error UserCreationFailed(IEnumerable<IdentityError> identityErrors)
    {
        var validationErrors = identityErrors
            .GroupBy(e => e.Code)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray()
            );

        return Error.Validation(
            "Staff.UserCreationFailed",
            "Failed to create staff user account",
            validationErrors
        );
    }
}