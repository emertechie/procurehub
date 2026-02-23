using Microsoft.AspNetCore.Identity;
using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.Users.Validation;

/// <summary>
/// Maps ASP.NET Identity error codes to form field names for validation errors.
/// </summary>
public static class IdentityErrorMapper
{
    /// <summary>
    /// Converts Identity errors to a validation Error with field-mapped keys.
    /// </summary>
    public static Error ToValidationError(
        IEnumerable<IdentityError> identityErrors,
        string errorCode,
        string title)
    {
        var validationErrors = identityErrors
            .GroupBy(e => MapErrorCodeToFieldName(e.Code))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.Description).ToArray()
            );

        return Error.Validation(errorCode, title, validationErrors);
    }

    /// <summary>
    /// Maps an Identity error code to the corresponding form field name.
    /// </summary>
    public static string MapErrorCodeToFieldName(string errorCode) => errorCode switch
    {
        "PasswordRequiresNonAlphanumeric" or
        "PasswordRequiresDigit" or
        "PasswordRequiresLower" or
        "PasswordRequiresUpper" or
        "PasswordTooShort" or
        "PasswordRequiresUniqueChars" => "Password",

        "DuplicateUserName" or
        "DuplicateEmail" or
        "InvalidEmail" or
        "InvalidUserName" => "Email",

        _ => errorCode
    };
}
