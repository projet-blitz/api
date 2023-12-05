using blitz_api.Helpers;
using blitz_api.Services;
using System.Collections.Concurrent;
using static blitz_api.Config.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHostedService<GtfsStaticUpdateService>();

GlobalStore.GlobalVar = new ConcurrentDictionary<string, bool>();
GlobalStore.GlobalVar[UpdateFlagKey] = false;

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment()) { }

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
