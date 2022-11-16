using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.RequestQuery;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/status-code/{statusCode:int}", ([Required][FromRoute]int statusCode) =>
{
    return Results.StatusCode(statusCode);
});

app.MapGet("/delay/{millisecondsDelay:int}", async ([Required][FromRoute]int millisecondsDelay) =>
{
    await Task.Delay(millisecondsDelay);
    return Results.Ok();
});

app.MapGet("/guid", () => Results.Ok(Guid.NewGuid()));

app.Run();
