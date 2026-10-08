using Common.Worker;
using CoreQuant.Infrastructure.Persistence.EF;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCommonWorker(
    "CoreQuant.Worker",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantPersistenceEfMySql(builder.Configuration);

var host = builder.Build();
host.Run();
