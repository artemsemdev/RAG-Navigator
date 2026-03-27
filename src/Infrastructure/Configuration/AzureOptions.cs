using System.ComponentModel.DataAnnotations;

namespace RAGNavigator.Infrastructure.Configuration;

public sealed class AzureOpenAIOptions
{
    public const string SectionName = "AzureOpenAI";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string ChatDeployment { get; set; } = string.Empty;

    [Required]
    public string EmbeddingDeployment { get; set; } = string.Empty;

    /// <summary>
    /// Optional API key for local development. When empty, DefaultAzureCredential is used.
    /// In production, prefer managed identity — no key needed.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}

public sealed class AzureSearchOptions
{
    public const string SectionName = "AzureSearch";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    [Required]
    public string IndexName { get; set; } = "rag-navigator-index";

    /// <summary>
    /// Optional API key for local development. When empty, DefaultAzureCredential is used.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
