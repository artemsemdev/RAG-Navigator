namespace RAGNavigator.Application.Models;

/// <summary>
/// Represents a document that has been indexed in Azure AI Search.
/// </summary>
public sealed class SourceDocument
{
    public required string DocumentId { get; init; }
    public required string FileName { get; init; }
    public required string Title { get; init; }
    public required int ChunkCount { get; init; }
}
