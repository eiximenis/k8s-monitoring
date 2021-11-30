using Beers.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BeersApi.Controllers;

[ApiController]
[Route("[controller]")]
public class BeersController : ControllerBase
{

    private readonly ILogger<BeersController> _logger;
    private readonly BeersQueries _queries;

    public BeersController(ILogger<BeersController> logger, BeersQueries queries)
    {
        _logger = logger;
        _queries = queries;
    }

    [HttpGet(Name = "GetAllBeers")]
    public async Task<ActionResult<IEnumerable<Beer>>> Get()
    {
        var activity = Activity.Current;

        activity?.SetTag("BeersApi.Controller", nameof(BeersController));
        activity?.SetTag("BeersApi.ActionName", "GetAllBeers");

        var random = new Random().Next(200, 1000);

        var beers = await _queries.GetAll();

        using var delayActivity = Telemetry.ActivitySource.StartActivity("StupidWait");
        await Task.Delay(random);
        return Ok(beers);
    }
}
