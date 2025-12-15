var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.ProcureHub_WebApi>("api");

builder.Build().Run();
