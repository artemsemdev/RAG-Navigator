using RAGNavigator.Application.Services;
using Xunit;

namespace RAGNavigator.Tests;

public class MarkdownDocumentChunkerTests
{
    private readonly MarkdownDocumentChunker _chunker = new();

    [Fact]
    public void Chunk_EmptyContent_ReturnsEmpty()
    {
        var result = _chunker.Chunk("", "test.md", "Test");
        Assert.Empty(result);
    }

    [Fact]
    public void Chunk_WhitespaceContent_ReturnsEmpty()
    {
        var result = _chunker.Chunk("   \n\n   ", "test.md", "Test");
        Assert.Empty(result);
    }

    [Fact]
    public void Chunk_ShortDocument_ReturnsOneChunk()
    {
        var content = """
            # My Document

            This is a paragraph with enough text to meet the minimum chunk size requirement.
            It contains meaningful content that should be preserved as a single chunk in the output.
            """;

        var result = _chunker.Chunk(content, "test.md", "My Document");

        Assert.Single(result);
        Assert.Equal("My Document", result[0].DocumentTitle);
        Assert.Equal("test.md", result[0].FileName);
        Assert.Equal(0, result[0].ChunkIndex);
        Assert.Contains("paragraph", result[0].Content);
    }

    [Fact]
    public void Chunk_MultipleHeadings_SplitsBySections()
    {
        var content = """
            # Main Title

            ## Section One

            This section contains enough text to meet the minimum chunk size.
            It discusses the first topic at length with sufficient detail to be meaningful.
            We need a reasonable amount of text to pass the minimum threshold.

            ## Section Two

            This is a different section with its own unique content and context.
            Again providing enough text to ensure this section is captured as a separate chunk.
            The chunker should recognize the heading boundary and split accordingly.

            ## Section Three

            A third section with additional content that provides more information.
            This ensures we have multiple chunks in our test and can verify correct splitting.
            The content here is distinct from the other sections for testing purposes.
            """;

        var result = _chunker.Chunk(content, "test.md", "Main Title");

        Assert.True(result.Count >= 3, $"Expected at least 3 chunks, got {result.Count}");

        // Verify sections are captured
        Assert.Contains(result, c => c.Section == "Section One");
        Assert.Contains(result, c => c.Section == "Section Two");
        Assert.Contains(result, c => c.Section == "Section Three");
    }

    [Fact]
    public void Chunk_AssignsUniqueChunkIds()
    {
        var content = """
            ## First Section

            Content for the first section with enough text to meet minimum requirements.
            This content discusses the first topic in the document being chunked.
            Additional sentences to ensure we have enough content for a valid chunk.

            ## Second Section

            Content for the second section with different text and different context.
            This is an entirely separate section that should get its own chunk ID.
            More text to make sure we pass the minimum chunk size threshold.
            """;

        var result = _chunker.Chunk(content, "test.md", "Test Doc");

        var ids = result.Select(c => c.ChunkId).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void Chunk_ConsistentDocumentId_ForSameFileName()
    {
        var content = """
            ## Section

            Enough content here to be a valid chunk with text that meets the minimum size.
            Additional lines to make sure we have enough characters in this chunk.
            """;

        var result1 = _chunker.Chunk(content, "same-file.md", "Title");
        var result2 = _chunker.Chunk(content, "same-file.md", "Title");

        Assert.Equal(result1[0].DocumentId, result2[0].DocumentId);
    }

    [Fact]
    public void Chunk_DifferentDocumentId_ForDifferentFileNames()
    {
        var content = """
            ## Section

            Content that meets minimum chunk size requirements for testing purposes.
            Additional text to ensure the chunk is large enough to be included.
            """;

        var result1 = _chunker.Chunk(content, "file-a.md", "Title A");
        var result2 = _chunker.Chunk(content, "file-b.md", "Title B");

        Assert.NotEqual(result1[0].DocumentId, result2[0].DocumentId);
    }

    [Fact]
    public void Chunk_PreservesFileName_AndTitle()
    {
        var content = """
            ## Overview

            This is a document overview section with enough content to form a valid chunk.
            The chunker must preserve the original file name and title in every chunk it produces.
            """;

        var result = _chunker.Chunk(content, "my-doc.md", "My Special Document");

        Assert.All(result, chunk =>
        {
            Assert.Equal("my-doc.md", chunk.FileName);
            Assert.Equal("My Special Document", chunk.DocumentTitle);
        });
    }

    [Fact]
    public void Chunk_LargeSection_SplitsWithinSection()
    {
        // Create content larger than MaxChunkSize (1500 chars)
        var paragraph = string.Join(" ", Enumerable.Repeat(
            "This is a meaningful sentence in a large document section.", 20));

        var content = $"""
            ## Large Section

            {paragraph}

            {paragraph}

            {paragraph}
            """;

        var result = _chunker.Chunk(content, "large.md", "Large Doc");

        Assert.True(result.Count > 1, "Large sections should be split into multiple chunks");
        Assert.All(result, chunk => Assert.Equal("Large Section", chunk.Section));
    }

    [Fact]
    public void Chunk_ChunkIndexesAreSequential()
    {
        var content = """
            ## Section A

            First section with enough content for a valid chunk. This text provides
            context about section A and its role in the overall document structure.

            ## Section B

            Second section with different content that is also long enough to form a chunk.
            This section discusses a different topic for testing purposes.

            ## Section C

            Third section rounding out the document with additional information.
            Enough text to ensure this also meets the minimum chunk size requirement.
            """;

        var result = _chunker.Chunk(content, "test.md", "Test");

        for (var i = 0; i < result.Count; i++)
        {
            Assert.Equal(i, result[i].ChunkIndex);
        }
    }
}
