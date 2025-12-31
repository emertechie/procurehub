namespace ProcureHub.Common;

public record Error
{
    public string Code { get; init; }
    public string Message { get; init; }
    public ErrorType Type { get; init; }
    public Dictionary<string, string[]>? ValidationErrors { get; init; }

    private Error(string code, string message, ErrorType type, Dictionary<string, string[]>? validationErrors = null)
    {
        Code = code;
        Message = message;
        Type = type;
        ValidationErrors = validationErrors;
    }

    public static Error Validation(string code, string message, Dictionary<string, string[]>? errors = null)
        => new(code, message, ErrorType.Validation, errors);

    public static Error Validation(string message)
        => new("Validation.Error", message, ErrorType.Validation);

    public static Error NotFound(string code, string message)
        => new(code, message, ErrorType.NotFound);

    public static Error NotFound(string message)
        => new("NotFound", message, ErrorType.NotFound);

    public static Error Conflict(string code, string message)
        => new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message)
        => new(code, message, ErrorType.Failure);

    public static Error Unauthorized(string message)
        => new("Unauthorized", message, ErrorType.Unauthorized);

    public static Error Unauthorized(string code, string message)
        => new(code, message, ErrorType.Unauthorized);

    public static Error Unauthorized()
        => new("Unauthorized", "User is not authorized.", ErrorType.Unauthorized);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
    Unauthorized
}
