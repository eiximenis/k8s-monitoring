// See https://aka.ms/new-console-template for more information

using Beers.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var cfg = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var constr = cfg["pgsql:constr"];
Console.WriteLine($"Seeding database {constr}");

var options = new DbContextOptionsBuilder<BeersContext>().UseNpgsql(constr).Options;
var context = new BeersContext(options);

await context.Database.MigrateAsync();

await SeedDatabase();


async Task SeedDatabase()
{
    if (context.Breweries.Any()) return;

    var espina = new Brewery()
    {
        Name = "Espina de Ferro"
    };

    var montseny = new Brewery()
    {
        Name = "Montseny"
    };

    context.Breweries.Add(espina);
    context.Breweries.Add(montseny);

    for (var idx = 1; idx <= 100; idx++)
    {
        context.Beers.Add(new Beer()
        {
            Name = $"LIMBO {idx}",
            Abv = 3 + idx / 10.0,
            Brewery = espina
        });
    }

    for (var idx = 1; idx < 100; idx++)
    {
        context.Beers.Add(new Beer()
        {
            Name = $"Collab {idx}",
            Abv = 4 + idx / 70.0,
            Brewery = montseny
        });
    }

    await context.SaveChangesAsync();


}
