# Solution Overview

## Business Context

Engineering teams accumulate critical operational knowledge across architecture decision records, runbooks, postmortems, onboarding guides, and platform standards. This knowledge is typically scattered across wikis, repos, and shared drives. Engineers spend significant time searching for answers that already exist in these documents, and new team members face a steep ramp-up curve navigating fragmented sources.

RAG Navigator addresses this by providing a conversational interface over internal engineering documents, grounded in Azure AI services.

## Problem Statement

There is no single, searchable, AI-assisted interface for engineers to query their own operational knowledge base. Traditional keyword search fails when engineers don't know the exact terminology used in the documents, and full-text search cannot understand intent or synthesize answers from multiple sources.

## Target Users

- **Platform engineers** seeking runbook steps during incidents.
- **New team members** onboarding and learning team standards.
- **Engineering leads** reviewing past architecture decisions.
- **Interviewers / reviewers** evaluating Azure GenAI and RAG skills demonstrated by this project.

## Architecture Goals

| Goal | Rationale |
|------|-----------|
| **Transparency** | The RAG pipeline must be fully visible in code — no black-box orchestration frameworks |
| **Correctness** | Answers must be grounded in retrieved documents with verifiable citations |
| **Simplicity** | A small, working end-to-end system is more valuable than a large unfinished one |
| **Portability** | Local development with env vars; production-ready with managed identity |
| **Interview readiness** | Code and docs should withstand scrutiny from senior engineers and architects |

## Scope

### In Scope

- Document ingestion from local markdown/text files
- Semantic chunking with metadata preservation
- Embedding generation via Azure OpenAI
- Programmatic Azure AI Search index management
- Hybrid retrieval (keyword + vector search)
- Grounded LLM answer generation with citations
- Debug mode for retrieval inspection
- Clean web UI with chat interface
- Architecture documentation as part of the searchable corpus

### Non-Goals

- End-user authentication or multi-tenancy
- Document upload through the UI
- Conversation memory or multi-turn context
- Production-grade deployment automation
- Enterprise-scale document processing
- Real-time document synchronization

## Major Trade-offs

| Decision | Trade-off | Rationale |
|----------|-----------|-----------|
| Modular monolith | Less scalable than microservices | Right-sized for a demo; avoids distributed system complexity without sacrificing clean architecture |
| Direct SDK usage | More code than using Semantic Kernel or similar | RAG pipeline logic is fully visible and explainable in interviews |
| Fixed chunk parameters | Less flexible than adaptive chunking | Simpler implementation; parameters are well-tuned for the document types in this corpus |
| API key fallback | Less secure than managed-identity-only | Enables immediate local development without Azure AD configuration |
| Single search index | No multi-tenant isolation | Sufficient for demo scope; the schema supports future partitioning by adding filter fields |
| Synchronous indexing | Blocks the caller during reindex | Acceptable for a small corpus; production would use background jobs |
