using blitz_api.Controllers;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

GTFSBinding gtfsBinding = new();
StaticCsv staticCsv = new();
STMGetter stmGetter = new();
CsvWorker csvWorker = new();

app.MapGet("/routes", () => staticCsv.GetAllRoutes());

app.MapGet("/directions/{routeId}", (string routeId) => staticCsv.GetDirectionsForRouteId(routeId));

app.MapGet("/stops/{routeId}/{direction}", (string routeId, string direction) => staticCsv.GetStopIdsForRouteAndDirection(routeId , direction));

app.MapGet("/horaires/{routeId}/{stopId}", (string routeId, string stopId) => gtfsBinding.GetHoraireForStop(routeId, stopId));

app.MapGet("/all", () => gtfsBinding.GiveMeData());

app.MapGet("/gtfs", () => stmGetter.GetGtfsStatic());

app.MapGet("/test", () => csvWorker.GetBusSystem());

app.Run();
