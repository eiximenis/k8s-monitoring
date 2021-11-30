using HelloMonitor;
using HelloMonitor.Data;
using Microsoft.EntityFrameworkCore;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

if (args.Length > 0)
{
    if (args[0] == "/seed")
    {
        await Seeder.DoSeed(builder.Configuration["pgsql:constr"]);
        return 0;
    }
}


// Add services to the container.
builder.Services.AddDbContext<HelloContext>(opt => opt.UseNpgsql(builder.Configuration["pgsql:constr"]));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpMetrics();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.UseEndpoints(endpoints => endpoints.MapMetrics());

app.Run();

return 0;