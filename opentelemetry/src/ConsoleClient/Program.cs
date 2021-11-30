using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Trace;

var cfg = new ConfigurationBuilder().AddEnvironmentVariables().Build();

var client = new HttpClient();

client.BaseAddress = new Uri(cfg["ApiUrl"]);

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddHttpClientInstrumentation()
            .SetSampler(new AlwaysOnSampler())
            .AddSource("BeersApi")
            .AddConsoleExporter()
            .Build();


var url = "/beers"; 

while (true)
{
    var fore = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"Getting Beers from: {client.BaseAddress}{url}");
    Console.ForegroundColor = fore;
    var response = await client.GetAsync(url);
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();

    fore = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.ForegroundColor = fore;

    Console.WriteLine(content);

    await Task.Delay(5000);
}