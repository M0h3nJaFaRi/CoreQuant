using Common.Scheduler;
using CoreQuant.Infrastructure;
using CoreQuant.Infrastructure.Persistence.EF;
using CoreQuant.Scheduler;
using Framework;

var builder = Host.CreateApplicationBuilder(args);

var applicationName = builder.Configuration["ApplicationName"] ?? "CoreQuant.Scheduler";

builder.Services.AddFramework(
    builder.Configuration,
    applicationName,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCommonScheduler(
    "CoreQuant.Scheduler",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantInfrastructure(builder.Configuration);
builder.Services.AddCoreQuantPersistenceEfMySql(builder.Configuration);
builder.Services.AddHostedService<OutboxRetrySchedulerHostedService>();

var host = builder.Build();
host.Run();
