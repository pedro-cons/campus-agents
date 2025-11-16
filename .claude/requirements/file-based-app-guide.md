# This is a guide to implement the minimal api using the file based app feature

# Minimal api basic structure

#:sdk Microsoft.NET.Sdk.Web //MANDATORY

var builder = WebApplication.CreateBuilder();

var app = builder.Build();

app.MapGet("/api/test", () => new
{
    message = "Hello from Boosting API!",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})
.WithName("GetTest");

app.Run();

Here’s an example using Humanizer:

#:package Humanizer@2.*

using Humanizer;

Console.WriteLine(1234.ToWords());

# How to add Scalar

#:package Scalar.AspNetCore@2.*
#:package Microsoft.AspNetCore.OpenApi@9.*

using Scalar.AspNetCore;

Then:

builder.Services.AddOpenApi();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.WithTitle("MyApi - Api")
        .WithTheme(ScalarTheme.BluePlanet)
        .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.Curl)
        .HideModels()
        .WithSearchHotKey("k");
});

# Records

record NumberRequest(long Number);
record NumberResponse(long Input, string Output);

return Results.Ok(new NumberResponse(request.Number, result));

# ConfigureHttpJsonOptions

using System.Text.Json.Serialization.Metadata;

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new DefaultJsonTypeInfoResolver());
});

# Important points

- Don't need to create a solution
- Don't need to create a project
- just need to execute this command to run the project dotnet run app.cs
- Records are at the end of the file
- Do not use nothing that needs ApiSerializerContext or .Produces<NumberResponse>(StatusCodes.Status200OK)