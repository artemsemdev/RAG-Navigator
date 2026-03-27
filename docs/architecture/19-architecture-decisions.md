# Architecture Decisions

## Decision Summary

| ID | Decision | Status | Rationale |
|----|----------|--------|-----------|
| ADR-001 | [Modular monolith over microservices](20-adr-001-modular-monolith.md) | Accepted | Right-sized for demo scope; clean internal boundaries without distributed system complexity |
| ADR-002 | [Code-first classic RAG over orchestration frameworks](21-adr-002-classic-rag.md) | Accepted | Makes the RAG pipeline fully visible and explainable; avoids black-box abstraction |
| ADR-003 | [Hybrid retrieval (keyword + vector)](22-adr-003-hybrid-retrieval.md) | Accepted | Combines exact term matching with semantic understanding; RRF fusion requires no tuning |
| ADR-004 | [Architecture docs as part of the RAG corpus](23-adr-004-docs-as-corpus.md) | Accepted | Demonstrates the system's value on its own design artifacts; enables architecture Q&A |

## Decision Principles

These principles guided the architectural choices:

1. **Clarity over cleverness.** Code and architecture should be immediately understandable by someone reading it for the first time.
2. **Right-sized for scope.** Each decision matches the current needs — not hypothetical future requirements.
3. **Honest about trade-offs.** Every simplification is documented, and the production path is described.
4. **Interview-ready.** Every decision should withstand a "why did you choose this?" question from an experienced architect.

## Decisions Not Taken

| Decision | Reason Not Taken |
|----------|-----------------|
| Use Semantic Kernel for orchestration | Would hide the RAG pipeline behind framework abstractions; less demonstrative of actual understanding |
| Use Azure Cosmos DB for document storage | No need for a separate document store when Azure AI Search holds all indexed data |
| Use Azure Functions for ingestion | Adds an additional compute service; synchronous ingestion in-process is sufficient for demo corpus size |
| Use Azure Event Grid for reindex triggers | Over-engineered for a manual reindex button; would be appropriate if documents changed automatically |
| Use multiple indexes (per document type) | Adds operational complexity; a single index with metadata fields is sufficient and simpler to manage |
| Use LangChain / LlamaIndex (.NET equivalents) | Same rationale as Semantic Kernel — the goal is to show direct SDK usage |
