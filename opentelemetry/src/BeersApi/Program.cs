using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Microsoft.EntityFrameworkCore;
using Beers.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<BeersContext>(opt => opt.UseNpgsql(builder.Configuration["pgsql:constr"]));
builder.Services.AddScoped<BeersQueries>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetryTracing(tpb => tpb
    .AddAspNetCoreInstrumentation()
    .AddZipkinExporter(opt => opt.Endpoint = new Uri(builder.Configuration["zipkin:url"]))
    .AddConsoleExporter()
    .AddSource("Beers.Data")
    .AddSource("BeersApi")
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("BeersApi")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
