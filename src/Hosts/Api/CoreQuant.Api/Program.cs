using CoreQuant.Root;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseDefaultServiceProvider(spo =>
{
    spo.ValidateScopes = true;
    spo.ValidateOnBuild = true;
});
builder.Services.AddApiDependency(builder.Configuration, typeof(CoreQuant.Api.AssemblyMarker).Assembly);
var app = builder.Build();
app.Services.ValidateAssembly();

app.UseApiPipeline()
   .MapApiEndpoints();


app.Run();