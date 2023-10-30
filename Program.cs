using blitz_api.Controllers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Use(async (context, next) =>
{
    var endpoint = context.GetEndpoint();
    if (endpoint == null)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Bad Endpoint");
        return;
    }
    await next();
});

GtfsRealtimeController realtimeController = new();
GtfsStaticController staticController = new();

app.MapGet("/getBusNetwork", () =>
{
    var jsonFile = staticController.GetBusNetwork();

    if (File.Exists(jsonFile))
    {
        return Results.File(jsonFile, "application/json");
    }
    else
    {
        return Results.NotFound();
    }
});

app.MapGet("/updateBusNetwork", () => staticController.MakeBusNetwork());

app.MapGet("/horaires/{routeId}/{stopId}", async (string routeId, string stopId) =>
{
    List<string> arrivalTimes = await realtimeController.GetHoraireForStop(routeId, stopId);
    return Results.Ok(arrivalTimes);
});

app.Run();
