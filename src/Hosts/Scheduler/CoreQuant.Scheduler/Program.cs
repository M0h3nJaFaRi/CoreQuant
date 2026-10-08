using Common.Scheduler;
using CoreQuant.Infrastructure;
using CoreQuant.Infrastructure.Persistence.EF;
using CoreQuant.Scheduler;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCommonScheduler(
    "CoreQuant.Scheduler",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantInfrastructure(builder.Configuration);
builder.Services.AddCoreQuantPersistenceEfMySql(builder.Configuration);
builder.Services.AddHostedService<OutboxRetrySchedulerHostedService>();

var host = builder.Build();
host.Run();
