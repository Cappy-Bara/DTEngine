using DTEngine;
using DTEngine.Contracts;
using DTEngine.Socket;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR(opt => opt.EnableDetailedErrors = true);

builder.Services.AddSingleton<SimulationContext>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()
        );
});

var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.MapPost("/init", (InputData input,SimulationContext sc) => sc.Initialize(input));
app.MapHub<SimulationHub>("/simulationHub");

app.Run();