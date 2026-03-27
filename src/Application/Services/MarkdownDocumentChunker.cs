using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using RAGNavigator.Application.Interfaces;
using RAGNavigator.Application.Models;

namespace RAGNavigator.Application.Services;

/// <summary>
/// Splits markdown documents into semantic chunks based on headings.
///
/// Strategy:
/// 1. Split on markdown headings (## or ###) to preserve semantic boundaries
/// 2. If a section exceeds MaxChunkSize, split further on paragraph boundaries
/// 3. Apply overlap between chunks to preserve context across boundaries
/// 4. Each chunk retains its section heading as metadata
/// </summary>
public sealed partial class MarkdownDocumentChunker : IDocumentChunker
{
    private const int MaxChunkSize = 1500;
    private const int ChunkOverlap = 200;
    private const int MinChunkSize = 100;

    public IReadOnlyList<DocumentChunk> Chunk(string content, string fileName, string documentTitle)
    {
        if (string.IsNullOrWhiteSpace(content))
            return [];

        var documentId = GenerateDocumentId(fileName);
        var sections = SplitBySections(content);
        var chunks = new List<DocumentChunk>();
        var chunkIndex = 0;

        foreach (var (heading, body) in sections)
        {
            var sectionChunks = SplitSectionIntoChunks(body);

            foreach (var chunkText in sectionChunks)
            {
                if (chunkText.Trim().Length < MinChunkSize)
                    continue;

                chunks.Add(new DocumentChunk
                {
                    ChunkId = $"{documentId}_chunk_{chunkIndex}",
                    DocumentId = documentId,
                    DocumentTitle = documentTitle,
                    FileName = fileName,
                    Section = heading,
                    ChunkIndex = chunkIndex,
                    Content = chunkText.Trim()
                });
                chunkIndex++;
            }
        }

        // If no chunks were produced (e.g., very short document), create a single chunk
        if (chunks.Count == 0 && content.Trim().Length >= MinChunkSize)
        {
            chunks.Add(new DocumentChunk
            {
                ChunkId = $"{documentId}_chunk_0",
                DocumentId = documentId,
                DocumentTitle = documentTitle,
                FileName = fileName,
                Section = documentTitle,
                ChunkIndex = 0,
                Content = content.Trim()
            });
        }

        return chunks;
    }

    private static List<(string Heading, string Body)> SplitBySections(string content)
    {
        var sections = new List<(string Heading, string Body)>();
        var headingPattern = HeadingRegex();
        var matches = headingPattern.Matches(content);

        if (matches.Count == 0)
        {
            // No headings found — extract title from first line or use "Introduction"
            var title = ExtractTitleFromContent(content);
            sections.Add((title, content));
            return sections;
        }

        // Content before the first heading
        var preHeading = content[..matches[0].Index].Trim();
        if (!string.IsNullOrWhiteSpace(preHeading))
        {
            var title = ExtractTitleFromContent(preHeading);
            sections.Add((title, preHeading));
        }

        for (var i = 0; i < matches.Count; i++)
        {
            var heading = matches[i].Groups[1].Value.Trim();
            var start = matches[i].Index + matches[i].Length;
            var end = i + 1 < matches.Count ? matches[i + 1].Index : content.Length;
            var body = content[start..end].Trim();

            if (!string.IsNullOrWhiteSpace(body))
                sections.Add((heading, body));
        }

        return sections;
    }

    private static string ExtractTitleFromContent(string content)
    {
        // Try to find a # Title line
        var titleMatch = TitleRegex().Match(content);
        if (titleMatch.Success)
            return titleMatch.Groups[1].Value.Trim();

        // Use first non-empty line
        var firstLine = content.Split('\n', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();
        return string.IsNullOrEmpty(firstLine) ? "Introduction" : Truncate(firstLine, 80);
    }

    private static List<string> SplitSectionIntoChunks(string text)
    {
        if (text.Length <= MaxChunkSize)
            return [text];

        var chunks = new List<string>();
        var paragraphs = text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries);
        var current = new StringBuilder();

        foreach (var paragraph in paragraphs)
        {
            if (current.Length + paragraph.Length + 2 > MaxChunkSize && current.Length > 0)
            {
                chunks.Add(current.ToString());

                // Apply overlap: keep the tail of the current chunk
                var overlap = GetOverlapText(current.ToString());
                current.Clear();
                if (!string.IsNullOrWhiteSpace(overlap))
                    current.Append(overlap).Append("\n\n");
            }

            if (current.Length > 0)
                current.Append("\n\n");
            current.Append(paragraph);
        }

        if (current.Length > 0)
            chunks.Add(current.ToString());

        return chunks;
    }

    private static string GetOverlapText(string text)
    {
        if (text.Length <= ChunkOverlap)
            return text;

        var tail = text[^ChunkOverlap..];
        // Start at a sentence or paragraph boundary if possible
        var sentenceBreak = tail.IndexOfAny(['.', '!', '?']);
        return sentenceBreak >= 0 ? tail[(sentenceBreak + 1)..].TrimStart() : tail;
    }

    private static string GenerateDocumentId(string fileName)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(fileName));
        return Convert.ToHexString(hash)[..12].ToLowerInvariant();
    }

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];

    [GeneratedRegex(@"^#{2,3}\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^#\s+(.+)$", RegexOptions.Multiline)]
    private static partial Regex TitleRegex();
}
