var builder = DistributedApplication.CreateBuilder(args);

// Add the following line to configure the Azure App Container environment
builder.AddAzureContainerAppEnvironment("env");

var api = builder.AddProject<Projects.ProcureHub_WebApi>("api");

builder.AddViteApp("frontend", "../ProcureHub.WebApp")
    .WithReference(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
