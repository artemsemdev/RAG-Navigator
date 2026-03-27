using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using RAGNavigator.Application.Interfaces;
using RAGNavigator.Infrastructure.Configuration;

namespace RAGNavigator.Infrastructure.AI;

public sealed class AzureOpenAIChatService : IChatCompletionService
{
    private readonly ChatClient _client;
    private readonly ILogger<AzureOpenAIChatService> _logger;

    public AzureOpenAIChatService(
        AzureOpenAIClient openAIClient,
        IOptions<AzureOpenAIOptions> options,
        ILogger<AzureOpenAIChatService> logger)
    {
        _client = openAIClient.GetChatClient(options.Value.ChatDeployment);
        _logger = logger;
    }

    public async Task<string> GenerateAnswerAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Sending chat completion request ({PromptLength} chars)", userPrompt.Length);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.1f, // Low temperature for factual, grounded answers
            MaxOutputTokenCount = 2048
        };

        var response = await _client.CompleteChatAsync(messages, options, cancellationToken);

        if (response.Value.Content.Count == 0)
        {
            _logger.LogWarning("Azure OpenAI returned empty content (finish reason: {FinishReason})",
                response.Value.FinishReason);
            return "The model was unable to generate a response. This may be due to content filtering. Please try rephrasing your question.";
        }

        var content = response.Value.Content[0].Text;
        _logger.LogDebug("Received chat response ({ResponseLength} chars)", content.Length);
        return content;
    }
}
