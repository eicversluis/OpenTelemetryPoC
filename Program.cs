using System.Globalization;

using Microsoft.AspNetCore.Mvc;

using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

const string serviceName = "roll-dice";
const string serviceVersion = "1.0.0";

builder.Logging.ClearProviders();

//builder.Logging.AddOpenTelemetry(options =>
//{
//    options.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
//        .AddOtlpExporter(otlpOptions =>
//        {
//            otlpOptions.Endpoint = new Uri("http://localhost:4317");
//        });
//});

//builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
//{
//    tracerProviderBuilder
//        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
//        .AddAspNetCoreInstrumentation()
//        .AddHttpClientInstrumentation()
//        .AddOtlpExporter(otlpOptions =>
//        {
//            otlpOptions.Endpoint = new Uri("http://localhost:4317"); 
//        });
//});

builder.Services.AddOpenTelemetry().WithTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion))
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri("http://localhost:4317");
        });
});

//builder.Services.AddOpenTelemetry().WithMetrics(metricProviderBuilder =>
//{
//    metricProviderBuilder
//        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion))
//        .AddAspNetCoreInstrumentation()
//        .AddOtlpExporter(otlpOptions =>
//        {
//            otlpOptions.Endpoint = new Uri("http://localhost:4317");
//        });
//});

var app = builder.Build();

app.MapGet("/", () => "Hello, OpenTelemetry!");

string HandleRollDice([FromServices] ILogger<Program> logger, string? player)
{
    var result = RollDice();

    if (string.IsNullOrEmpty(player))
    {
        logger.LogInformation("Anonymous player is rolling the dice: {result}", result);
    }
    else
    {
        logger.LogInformation("{player} is rolling the dice: {result}", player, result);
    }

    return result.ToString(CultureInfo.InvariantCulture);
}

int RollDice()
{
    return Random.Shared.Next(1, 7);
}

app.MapGet("/rolldice/{player?}", HandleRollDice);

app.Run();