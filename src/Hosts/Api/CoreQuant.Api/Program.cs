using CoreQuant.Root;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiDependency(builder.Configuration, typeof(CoreQuant.Api.AssemblyMarker).Assembly)
                .AddControllers();

var app = builder.Build();

app.Services.ValidateAssembly();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
