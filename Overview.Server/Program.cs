using Overview.Server.Application.DependencyInjection;
using Overview.Server.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddServerApplication();
builder.Services.AddServerInfrastructure(builder.Configuration);
builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
