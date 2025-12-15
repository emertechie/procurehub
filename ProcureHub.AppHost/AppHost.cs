var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.ProcureHub_WebApi>("api");

builder.AddViteApp("frontend", "../ProcureHub.WebApp")
    .WithPnpm()
    .WithReference(api);

builder.Build().Run();
