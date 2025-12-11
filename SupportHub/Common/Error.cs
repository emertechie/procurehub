namespace SupportHub.Common;

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

    public static Error Validation(string code, string message, Dictionary<string, string[]> errors) 
        => new(code, message, ErrorType.Validation, errors);

    public static Error NotFound(string code, string message) 
        => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) 
        => new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) 
        => new(code, message, ErrorType.Failure);

    public static Error Unauthorized(string code, string message) 
        => new(code, message, ErrorType.Unauthorized);
}

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Failure,
    Unauthorized
}