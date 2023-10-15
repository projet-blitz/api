using blitz_api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
GTFSBinding gtfsBinding = new();
StaticCsv staticCsv = new();
STMGetter stmGetter = new();

app.MapGet("/routes", () => staticCsv.GetAllRoutes());

app.MapGet("/directions/{routeId}", (string routeId) => staticCsv.GetDirectionsForRouteId(routeId));

app.MapGet("/stops/{routeId}/{direction}", (string routeId, string direction) => staticCsv.GetStopIdsForRouteAndDirection(routeId , direction));

app.MapGet("/horaires/{routeId}/{stopId}", (string routeId, string stopId) => gtfsBinding.GetHoraireForStop(routeId, stopId));

app.MapGet("/all", () => gtfsBinding.GiveMeData());

app.Run();
