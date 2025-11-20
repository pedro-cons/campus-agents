# This is a guide to implement the server-sent events in minimal api

Server-Sent Events (SSE) is a one-way streaming technology that allows the server to push real-time updates to the client over a single, long-lived HTTP connection.

# Example minimal api endpoint

app.MapGet("/orders/live", (OrderService service, CancellationToken ct) =>  
{  
    return TypedResults.ServerSentEvents(  
        service.GenerateOrders(ct),  
        eventType: "order"  
    );  
}); 

# Service Example

// At the end of the file
public record OrderEvent(string Id, string Status, DateTime Timestamp);

// At the end of the file
public class OrderService  
{  
    public async IAsyncEnumerable<OrderEvent> GenerateOrders([EnumeratorCancellation] CancellationToken ct)  
    {  
        var rnd = Random.Shared;  
        while (!ct.IsCancellationRequested)  
        {  
            var order = new OrderEvent(  
                Id: Guid.NewGuid().ToString(),  
                Status: rnd.Next(0, 2) == 0 ? "created" : "processed",  
                Timestamp: DateTime.UtcNow  
            );  

            yield return order;  
            await Task.Delay(TimeSpan.FromSeconds(2), ct);  
        }  
    }  
}

In program.cs:

builder.Services.AddSingleton<OrderService>();  