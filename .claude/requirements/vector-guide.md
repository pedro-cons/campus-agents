# This is a guide to implement the vector search in minimal api

# How Vector Search Works

Vector search operates on a fundamentally different principle than traditional search.

Instead of matching keywords using = or “like”, it converts your data into numerical representations (vectors) and calculates mathematical distances between them. Documents that are semantically similar end up close together in vector space, even if they use completely different words.

# Implementing Vector Insert in .NET

We’ll build a vector search API using:

SQL Vector Search in .NET 10 EF Core 10
Ollama (local embeddings) - mxbai-embed-large
Entity Framework Core
ASP.NET Core Minimal APIs and Semantic Kernel

# Packages

#:sdk Microsoft.NET.Sdk.Web

#:property PublishAot=false
#:property JsonSerializerIsReflectionEnabledByDefault=true

#:package Microsoft.AspNetCore.OpenApi@9.*
#:package Scalar.AspNetCore@2.*
#:package Microsoft.EntityFrameworkCore.Design@10.0.0-rc.1.*
#:package Microsoft.EntityFrameworkCore.SqlServer@10.0.0-rc.1.*
#:package Microsoft.Data.SqlClient@6.*
#:package Microsoft.SemanticKernel.Connectors.Ollama@1.66.0-alpha
#:package OllamaSharp@5.4.8

using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.SemanticKernel.Embeddings;
using Scalar.AspNetCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization.Metadata;
using OllamaSharp;

# Configure Services

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddDbContext<VectorDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddTransient<OllamaApiClient>(c => new OllamaApiClient(
    "http://localhost:11434",
    "mxbai-embed-large"));

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new DefaultJsonTypeInfoResolver());
});

# Endpoints


// Endpoint to generate embeddings
app.MapPost("/api/embeddings", async (
    EmbeddingRequest request,
    OllamaApiClient client,
    VectorDbContext dbContext) =>
{
    var service = client.AsTextEmbeddingGenerationService();
    var embeddings = await service.GenerateEmbeddingAsync(request.Text);

    var textEmbedding = new TextEmbedding()
    {
        Text = request.Text,
        Embedding = new SqlVector<float>(embeddings)
    };

    await dbContext.TextEmbeddings.AddRangeAsync(textEmbedding);
    await dbContext.SaveChangesAsync();

    return Results.Ok(new EmbeddingResponse
    {
        Message = $"Successfully created the embedding for '{request.Text}'",
        Id = textEmbedding.Id,
        Text = textEmbedding.Text,
        CreatedAt = textEmbedding.CreatedAt
    });
})
.WithName("CreateEmbeddings");

// Endpoint to search semantically
app.MapPost("/api/search", async (
    SearchRequest request,
    OllamaApiClient client,
    VectorDbContext dbContext) =>
{
    // Convert query to embedding
    var service = client.AsTextEmbeddingGenerationService();
    var queryEmbedding = await service.GenerateEmbeddingAsync(request.Query);
    var queryVector = new SqlVector<float>(queryEmbedding);

    // SQL Server vector similarity search using EF Core 10 native support
    var results = await dbContext.TextEmbeddings
        .Select(e => new SearchResult
        {
            Id = e.Id,
            Text = e.Text,
            Distance = EF.Functions.VectorDistance("cosine", e.Embedding, queryVector)
        })
        .OrderBy(x => x.Distance)
        .Take(request.Limit)
        .ToListAsync();

    return Results.Ok(results);
})
.WithName("Search");

# Db Context


// Design-time factory for EF Core migrations
public class VectorDbContextFactory : IDesignTimeDbContextFactory<VectorDbContext>
{
    public VectorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VectorDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost,1433;Database=vectorsearch;User Id=sa;Password=VectorSearch123!;TrustServerCertificate=True");
        return new VectorDbContext(optionsBuilder.Options);
    }
}

// Db Context
public class VectorDbContext : DbContext
{
    public VectorDbContext(DbContextOptions<VectorDbContext> options) : base(options)
    {
    }

    public DbSet<TextEmbedding> TextEmbeddings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TextEmbedding>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Text).IsRequired();
            entity.Property(e => e.Embedding).HasColumnType("vector(1024)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}

# Class

public class TextEmbedding
{
    public int Id { get; set; }
    public required string Text { get; set; }

    [Column(TypeName = "vector(1024)")]
    public required SqlVector<float> Embedding { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

# Settings

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=vectorsearch;User Id=sa;Password=VectorSearch123!;TrustServerCertificate=True"
  }
}

# VERY IMPORTANT

- The table already exist do not need migration
- Max 2 endpoints
- Follow file-based approach
- use classes instead of records