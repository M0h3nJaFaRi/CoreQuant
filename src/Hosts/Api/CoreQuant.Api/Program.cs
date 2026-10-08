using Common.Api;
using CoreQuant.Infrastructure.Acl;
using CoreQuant.Infrastructure.Persistence.EF;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCommonApi(
    "CoreQuant.Api",
    builder.Configuration,
    typeof(CoreQuant.Application.AssemblyMarker).Assembly);

builder.Services.AddCoreQuantAcl(builder.Configuration);
builder.Services.AddCoreQuantPersistenceEfMySql(builder.Configuration);

var app = builder.Build();

app.UseCommonApi();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
