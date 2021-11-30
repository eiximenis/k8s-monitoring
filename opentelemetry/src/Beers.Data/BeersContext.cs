using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beers.Data
{
    public class BeersContext : DbContext
    {
        public BeersContext(DbContextOptions<BeersContext> opt) : base(opt)
        {
        }
        public DbSet<Beer> Beers { get; set; }
        public DbSet<Brewery> Breweries { get; set; }
    }
}
