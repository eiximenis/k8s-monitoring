using HelloMonitor.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelloMonitor.Controllers;

[ApiController]
[Route("[controller]")]
public class ForecastsController : ControllerBase
{
    private readonly ILogger<ForecastsController> _logger;
    private readonly HelloContext _db;

    public ForecastsController(ILogger<ForecastsController> logger, HelloContext db)
    {
        _logger = logger;
        _db = db;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> Get()
    {
        _logger.LogInformation("Fetching forecasts data.");
        var data = await _db.Forecasts.ToListAsync();
        _logger.LogInformation("Forecasts data fetched.");
        return Ok(data);
    }
}
