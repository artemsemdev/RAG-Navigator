using RAGNavigator.Application.Models;

namespace RAGNavigator.Application.Interfaces;

public interface ISearchIndexService
{
    Task CreateOrUpdateIndexAsync(CancellationToken cancellationToken = default);
    Task UploadChunksAsync(IReadOnlyList<DocumentChunk> chunks, CancellationToken cancellationToken = default);
    Task DeleteAllDocumentsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SourceDocument>> GetIndexedDocumentsAsync(CancellationToken cancellationToken = default);
}
