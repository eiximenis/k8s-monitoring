namespace Beers.Data
{
    public class Brewery
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Brewery()
        {
            Name = string.Empty;
        }
    }
}