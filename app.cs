#:sdk Microsoft.NET.Sdk.Web
#:package Scalar.AspNetCore@2.*
#:package Microsoft.AspNetCore.OpenApi@9.*

using Scalar.AspNetCore;
using System.Text.Json.Serialization.Metadata;

var builder = WebApplication.CreateBuilder();

builder.Services.AddOpenApi();
builder.Services.AddSingleton<OrderService>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new DefaultJsonTypeInfoResolver());
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("Orders API - SSE Demo")
        .WithTheme(ScalarTheme.BluePlanet)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Curl)
        .HideModels()
        .WithSearchHotKey("k");
});

app.MapGet("/orders", (OrderService service, CancellationToken ct) =>
{
    return TypedResults.ServerSentEvents(
        service.GenerateOrders(ct),
        eventType: "order"
    );
})
.WithName("GetOrders")
.WithSummary("Get orders stream")
.WithDescription("Streams orders in real-time using Server-Sent Events");

app.Run();

// REPR Pattern - Response class (not using records)
public class OrderEvent
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class OrderService
{
    public async IAsyncEnumerable<OrderEvent> GenerateOrders([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        var rnd = Random.Shared;
        while (!ct.IsCancellationRequested)
        {
            var order = new OrderEvent
            {
                Id = Guid.NewGuid().ToString(),
                Status = rnd.Next(0, 2) == 0 ? "created" : "processed",
                Timestamp = DateTime.UtcNow
            };

            yield return order;
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }
    }
}

