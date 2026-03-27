using RAGNavigator.Application.Models;

namespace RAGNavigator.Application.Interfaces;

public interface IRetrievalService
{
    Task<IReadOnlyList<RetrievalResult>> SearchAsync(
        string query,
        ReadOnlyMemory<float> queryEmbedding,
        int topK = 5,
        CancellationToken cancellationToken = default);
}
