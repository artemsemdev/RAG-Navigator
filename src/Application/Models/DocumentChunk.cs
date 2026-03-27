namespace RAGNavigator.Application.Models;

/// <summary>
/// Represents a chunk of a document with its metadata and optional embedding vector.
/// This is the core unit of storage and retrieval in the RAG pipeline.
/// </summary>
public sealed class DocumentChunk
{
    public required string ChunkId { get; init; }
    public required string DocumentId { get; init; }
    public required string DocumentTitle { get; init; }
    public required string FileName { get; init; }
    public required string Section { get; init; }
    public required int ChunkIndex { get; init; }
    public required string Content { get; init; }
    public ReadOnlyMemory<float>? Embedding { get; set; }
}
