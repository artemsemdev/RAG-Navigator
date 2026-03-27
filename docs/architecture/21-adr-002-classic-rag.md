# ADR-002: Code-First Classic RAG Over Orchestration Frameworks

**Status:** Accepted
**Date:** 2024-03-01
**Context:** RAG Navigator — choosing how to implement the RAG pipeline

## Context

Several frameworks exist to abstract the RAG pipeline in .NET:
- **Semantic Kernel** (Microsoft) — provides built-in connectors for embeddings, vector stores, and LLM orchestration.
- **Kernel Memory** (Microsoft) — higher-level abstraction specifically for RAG with document ingestion.
- **LangChain equivalents** — various .NET ports of the Python LangChain pattern.

These frameworks can accelerate development but introduce abstraction layers that hide the RAG mechanics.

## Decision

Implement the RAG pipeline using **direct Azure SDK calls** (`Azure.AI.OpenAI`, `Azure.Search.Documents`) with hand-written service classes and clear interfaces.

## Rationale

### The Purpose Is Demonstration, Not Acceleration

This project exists to demonstrate understanding of RAG mechanics to hiring managers and interviewers. Using a framework that abstracts away the core pipeline would be counterproductive:

- **With Semantic Kernel:** "I called `kernel.InvokeAsync<ChatResult>()` and it handled everything."
- **With direct SDK usage:** "I embedded the query, ran a hybrid search with RRF fusion, built a grounded prompt with the top-5 chunks, and sent it to GPT-4o at temperature 0.1 to minimize hallucination."

The second explanation demonstrates actual understanding.

### Every Step Is Visible

The current implementation makes each RAG stage explicit:

| Stage | Code Location | Direct SDK Call |
|-------|--------------|-----------------|
| Chunking | `MarkdownDocumentChunker` | Pure C# (no SDK) |
| Embedding | `AzureOpenAIEmbeddingService` | `EmbeddingClient.GenerateEmbeddingsAsync()` |
| Indexing | `AzureSearchIndexService` | `SearchClient.UploadDocumentsAsync()` |
| Retrieval | `AzureSearchRetrievalService` | `SearchClient.SearchAsync()` with `VectorizedQuery` |
| Prompting | `PromptBuilder` | Pure C# (no SDK) |
| Completion | `AzureOpenAIChatService` | `ChatClient.CompleteChatAsync()` |
| Citations | `PromptBuilder.ExtractCitations` | Pure C# regex parsing |

A reviewer can trace the entire pipeline by reading these six files.

### Interfaces Allow Future Framework Adoption

The interface-based design means a Semantic Kernel or Kernel Memory implementation could be added as an alternative Infrastructure layer without changing any Application code. The decision is not anti-framework — it is pro-transparency for this specific use case.

## Consequences

### Positive
- Full pipeline visibility — every SDK call is explicit.
- Interviewers can ask about any pipeline stage and get a concrete code-level answer.
- No framework-specific learning curve for reviewers.
- No framework version dependency risks.

### Negative
- More code than a framework-based approach (~200 lines of service code vs. ~30 lines with Semantic Kernel).
- No built-in retry policies beyond what the Azure SDKs provide.
- No built-in prompt template system (the static `PromptBuilder` handles this).
- Manual orchestration of the pipeline steps.

### Mitigations
- The Azure SDKs include built-in retry policies for transient errors.
- The explicit orchestration is an advantage, not a disadvantage, for a demo project.
- The total codebase is small enough that the additional code is not a maintenance burden.

## Alternatives Considered

| Alternative | Why Not |
|-------------|---------|
| **Semantic Kernel** | Hides the pipeline behind `kernel.InvokeAsync()`. Good for production, but reduces interview value. |
| **Kernel Memory** | Even higher abstraction. `memory.ImportDocumentAsync()` does chunking + embedding + indexing in one call — precisely what we want to show we understand. |
| **LlamaIndex (.NET port)** | Less mature in .NET ecosystem. Same abstraction concern. |
