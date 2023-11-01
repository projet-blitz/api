using blitz_api.Config;
using blitz_api.Controllers;
using blitz_api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHostedService<GtfsStaticUpdateService>();
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

app.MapGet("/getBusNetwork", async context =>
{
    /*while (Config.IsUpdating)
    {
        await Task.Delay(TimeSpan.FromSeconds(2));
    }*/
    
    var jsonFile = staticController.GetBusNetwork();

    if (File.Exists(jsonFile))
    {
        var json = Results.File(jsonFile, "application/json");
        context.Response.StatusCode = 200;
        await context.Response.WriteAsJsonAsync(json);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Bus Network not found");
    }
});

app.MapGet("/updateBusNetwork", () => 
{
    //if (!Config.IsUpdating)
    staticController.MakeBusNetwork();
   
});

app.MapGet("/horaires/{routeId}/{stopId}", async (string routeId, string stopId) =>
{
    List<string> arrivalTimes = await realtimeController.GetHoraireForStop(routeId, stopId);
    return Results.Ok(arrivalTimes);
});

app.Run();
