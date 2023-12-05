using blitz_api.Helpers;
using blitz_api.Services;
using System.Collections.Concurrent;
using static blitz_api.Config.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<GtfsStaticUpdateService>();

GlobalStore.GlobalVar = new ConcurrentDictionary<string, bool>();
GlobalStore.GlobalVar[UpdateFlagKey] = false;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
