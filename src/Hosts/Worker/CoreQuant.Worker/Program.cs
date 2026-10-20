using CoreQuant.Root;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWorkerDependency(builder.Configuration, typeof(CoreQuant.Worker.AssemblyMarker).Assembly);

var host = builder.Build();

host.Services.ValidateAssembly();

host.Run();