using Microsoft.EntityFrameworkCore;

namespace HelloMonitor.Data
{
    public class HelloContext : DbContext
    {

        public HelloContext(DbContextOptions<HelloContext> opt) : base(opt)
        {
        }

        public DbSet<WeatherForecast> Forecasts { get; set; }
    }
}
