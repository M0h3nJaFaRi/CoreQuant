using Common.Consumer;
using CoreQuant.Infrastructure;
using Framework;

var builder = Host.CreateApplicationBuilder(args);

var applicationName = builder.Configuration["ApplicationName"] ?? "CoreQuant.Consumer";

builder.Services.AddFramework(
    builder.Configuration,
    applicationName,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCommonConsumer(
    "CoreQuant.Consumer",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantInfrastructure(builder.Configuration);

var host = builder.Build();
host.Run();
