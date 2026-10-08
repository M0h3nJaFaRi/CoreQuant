using Common.Worker;
using CoreQuant.Infrastructure;
using CoreQuant.Infrastructure.Persistence.EF;
using Framework;

var builder = Host.CreateApplicationBuilder(args);

var applicationName = builder.Configuration["ApplicationName"] ?? "CoreQuant.Worker";

builder.Services.AddFramework(
    builder.Configuration,
    applicationName,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCommonWorker(
    "CoreQuant.Worker",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantInfrastructure(builder.Configuration);
builder.Services.AddCoreQuantPersistenceEfMySql(builder.Configuration);

var host = builder.Build();
host.Run();
