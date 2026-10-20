using CoreQuant.Root;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSchedulerDependency(builder.Configuration, typeof(CoreQuant.Scheduler.AssemblyMarker).Assembly);

var host = builder.Build();

host.Services.ValidateAssembly();

host.Run();