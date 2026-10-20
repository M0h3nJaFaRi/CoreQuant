using CoreQuant.Root;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddConsumerDependency(builder.Configuration, typeof(CoreQuant.Consumer.AssemblyMarker).Assembly);

var host = builder.Build();

host.Services.ValidateAssembly();

host.Run();