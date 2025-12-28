namespace ProcureHub.WebApi.Tests.Infrastructure.Helpers;

public record EndpointInfo(string Path, string Method, string Name, EndpointTestOptions? Options = null)
{
    public override string ToString()
    {
        return $"{Method} {Path} ({Name})";
    }
}

public class EndpointTestOptions
{
    public bool RequiresAdmin { get; set; }
}
