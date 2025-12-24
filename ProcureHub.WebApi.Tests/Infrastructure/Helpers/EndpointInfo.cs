namespace ProcureHub.WebApi.Tests.Infrastructure.Helpers;

public record EndpointInfo(string Path, string Method, string Name /*, Func<string, object>? MakeRequestBody = null!*/)
{
    public override string ToString()
    {
        return $"{Method} {Path} ({Name})";
    }
}
