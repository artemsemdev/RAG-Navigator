using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using RAGNavigator.Application.Interfaces;
using RAGNavigator.Infrastructure.Configuration;

namespace RAGNavigator.Infrastructure.AI;

public sealed class AzureOpenAIEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _client;
    private readonly ILogger<AzureOpenAIEmbeddingService> _logger;

    public AzureOpenAIEmbeddingService(
        AzureOpenAIClient openAIClient,
        IOptions<AzureOpenAIOptions> options,
        ILogger<AzureOpenAIEmbeddingService> logger)
    {
        _client = openAIClient.GetEmbeddingClient(options.Value.EmbeddingDeployment);
        _logger = logger;
    }

    public async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(
        string text, CancellationToken cancellationToken = default)
    {
        var result = await _client.GenerateEmbeddingAsync(text, cancellationToken: cancellationToken);
        return result.Value.ToFloats();
    }

    public async Task<IReadOnlyList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(
        IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Generating embeddings for {Count} texts", texts.Count);

        var result = await _client.GenerateEmbeddingsAsync(texts, cancellationToken: cancellationToken);

        return result.Value
            .OrderBy(e => e.Index)
            .Select(e => e.ToFloats())
            .ToList();
    }
}
