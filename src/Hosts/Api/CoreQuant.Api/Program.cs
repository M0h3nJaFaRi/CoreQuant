using Common.Api;
using CoreQuant.Infrastructure;
using CoreQuant.Infrastructure.Acl;
using CoreQuant.Infrastructure.Persistence.EF;
using Framework;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var applicationName = builder.Configuration["ApplicationName"] ?? "CoreQuant.Api";

builder.Services.AddFramework(
    builder.Configuration,
    applicationName,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCommonApi(
    "CoreQuant.Api",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantInfrastructure(builder.Configuration);
builder.Services.AddCoreQuantAcl(builder.Configuration);
builder.Services.AddCoreQuantPersistenceEf(builder.Configuration);

var app = builder.Build();

app.UseCommonApi();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
