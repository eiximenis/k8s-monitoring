using HelloMonitor.Data;
using Microsoft.EntityFrameworkCore;

namespace HelloMonitor
{
    public static class Seeder
    {
        public static async Task DoSeed(string constr)
        {

            Console.WriteLine($"Seeding {constr}");
            var options = new DbContextOptionsBuilder<HelloContext>().UseNpgsql(constr).Options;
            var ctx = new HelloContext(options);
            await ctx.Database.MigrateAsync();
            if (ctx.Forecasts.Any())
            {
                return;
            }

            var rnd = new Random();
            for (var idx = 1; idx <= 100; idx++)
            {
                ctx.Forecasts.Add(new WeatherForecast()
                {
                    Date = DateTime.UtcNow.AddDays(-idx),
                    Summary = $"Some weather data #{idx}",
                    TemperatureC = -10 + rnd.Next(1, 50)
                }); ;
            }

            await ctx.SaveChangesAsync();
            Console.WriteLine("Seed done!");
        }
    }
}
