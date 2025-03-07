using Google.Api;
using Ichiba.Shipment.Application;
using Ichiba.Shipment.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


// Add services to the container.
//builder.Services.AddDaprClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ichiba Shipment API",
        Version = "v1",
        Description = "API for managing shipments"
    });
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ichiba Shipment API v1");
        c.RoutePrefix = "swagger";
    });
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseCors("AllowLocalhost");


app.UseCloudEvents();

app.UseAuthorization();

app.MapControllers();

app.Run();
