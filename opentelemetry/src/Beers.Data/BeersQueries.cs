using Microsoft.EntityFrameworkCore;

namespace Beers.Data;
public class BeersQueries
{

    private readonly BeersContext _db;
    public BeersQueries(BeersContext db) => _db = db;

    public async Task<IEnumerable<Beer>> GetAll()
    {

        using var activity = Telemetry.ActivitySource.StartActivity("GetAllBeers");

        var data = await _db.Beers.ToListAsync();
        return data;

    }

}
