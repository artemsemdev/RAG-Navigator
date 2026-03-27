using RAGNavigator.Application.Models;

namespace RAGNavigator.Application.Interfaces;

public interface IChatCompletionService
{
    Task<string> GenerateAnswerAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
}
