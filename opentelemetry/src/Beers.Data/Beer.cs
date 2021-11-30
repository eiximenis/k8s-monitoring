namespace Beers.Data
{
    public class Beer
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public double Abv { get; set; }

        public int BreweryId { get; set; }

        public Brewery Brewery { get; set; }

        public Beer()
        {
            Name = string.Empty;
        }
    }
}