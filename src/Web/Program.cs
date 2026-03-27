using RAGNavigator.Application.Services;
using RAGNavigator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Map environment variables to configuration sections so either appsettings.json
// or env vars can be used. This makes local dev and CI/CD flexible.
builder.Configuration.AddEnvironmentVariables();
MapEnvironmentVariables(builder.Configuration);

builder.Services.AddRazorPages();
builder.Services.AddRAGNavigatorServices(builder.Configuration);

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

// --- API Endpoints ---

app.MapPost("/api/chat", async (
    ChatRequest request,
    RagOrchestrator orchestrator,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
        return Results.BadRequest(new { error = "Question is required." });

    if (request.Question.Length > 2000)
        return Results.BadRequest(new { error = "Question must be 2000 characters or fewer." });

    var response = await orchestrator.AskAsync(
        request.Question,
        request.DebugMode,
        cancellationToken);

    return Results.Ok(response);
});

app.MapPost("/api/index/reindex", async (
    DocumentProcessor processor,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var repoRoot = FindRepoRoot();
    var folders = new List<string>();

    // Primary sample data folder
    var sampleDataPath = configuration.GetValue<string>("SampleDataPath");
    if (!string.IsNullOrWhiteSpace(sampleDataPath))
        folders.Add(sampleDataPath);
    else if (repoRoot is not null)
        folders.Add(Path.Combine(repoRoot, "sample-data"));

    // Architecture docs (indexed as part of the RAG corpus — see ADR-004)
    if (repoRoot is not null)
    {
        var archDocsPath = Path.Combine(repoRoot, "docs", "architecture");
        if (Directory.Exists(archDocsPath))
            folders.Add(archDocsPath);
    }

    if (folders.Count == 0)
        return Results.BadRequest(new { error = "No document folders found. Set SampleDataPath or run from the repo directory." });

    var chunkCount = await processor.IngestDocumentsAsync(folders, cancellationToken);

    return Results.Ok(new { message = "Indexing complete.", chunksIndexed = chunkCount });
});

app.MapGet("/api/index/documents", async (
    RAGNavigator.Application.Interfaces.ISearchIndexService indexService,
    CancellationToken cancellationToken) =>
{
    var documents = await indexService.GetIndexedDocumentsAsync(cancellationToken);
    return Results.Ok(documents);
});

app.Run();

// --- Helpers ---

static void MapEnvironmentVariables(ConfigurationManager config)
{
    // Allow flat env vars like AZURE_OPENAI_ENDPOINT to map into structured config sections.
    // This pattern is common for Azure apps using App Service / Container Apps configuration.
    var envMappings = new Dictionary<string, string>
    {
        ["AZURE_OPENAI_ENDPOINT"] = "AzureOpenAI:Endpoint",
        ["AZURE_OPENAI_CHAT_DEPLOYMENT"] = "AzureOpenAI:ChatDeployment",
        ["AZURE_OPENAI_EMBEDDING_DEPLOYMENT"] = "AzureOpenAI:EmbeddingDeployment",
        ["AZURE_OPENAI_API_KEY"] = "AzureOpenAI:ApiKey",
        ["AZURE_SEARCH_ENDPOINT"] = "AzureSearch:Endpoint",
        ["AZURE_SEARCH_INDEX_NAME"] = "AzureSearch:IndexName",
        ["AZURE_SEARCH_API_KEY"] = "AzureSearch:ApiKey"
    };

    foreach (var (envVar, configKey) in envMappings)
    {
        var value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(value))
            config[configKey] = value;
    }
}

static string? FindRepoRoot()
{
    // Walk up from the working directory to find the repo root (contains RAGNavigator.sln).
    var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (dir is not null)
    {
        if (File.Exists(Path.Combine(dir.FullName, "RAGNavigator.sln")))
            return dir.FullName;
        dir = dir.Parent;
    }
    return null;
}

public record ChatRequest(string Question, bool DebugMode = false);
