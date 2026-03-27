using RAGNavigator.Application.Models;

namespace RAGNavigator.Application.Interfaces;

public interface IDocumentChunker
{
    IReadOnlyList<DocumentChunk> Chunk(string content, string fileName, string documentTitle);
}
