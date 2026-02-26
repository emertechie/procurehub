namespace ProcureHub.Application.Common.Authorization;

public sealed class RequestUnauthenticatedException(string requestName)
    : Exception($"Request '{requestName}' requires authenticated user.")
{
}

public sealed class RequestForbiddenException(string requestName, IReadOnlyList<string> requiredRoles)
    : Exception($"Request '{requestName}' requires one of roles: {string.Join(", ", requiredRoles)}")
{
    public IReadOnlyList<string> RequiredRoles { get; } = requiredRoles;
}
