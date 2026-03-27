# ADR-001: Modular Monolith Over Microservices

**Status:** Accepted
**Date:** 2024-03-01
**Context:** RAG Navigator — choosing the deployment architecture

## Context

RAG Navigator has three logical concerns: document ingestion, search/retrieval, and answer generation. These could be deployed as separate services (microservices) or as a single application with clean internal boundaries (modular monolith).

## Decision

Deploy as a **single ASP.NET Core application** with a three-layer internal architecture:
- **Application layer:** Domain logic, interfaces, models — zero external dependencies.
- **Infrastructure layer:** Azure SDK implementations — isolated behind interfaces.
- **Web layer:** HTTP endpoints and UI — depends on both layers above.

## Rationale

### Right-Sized for Scope

This application has:
- One team (one developer).
- One deployment target.
- One scaling requirement (handle a few concurrent users).
- A small corpus (~30-80 documents).

Microservices would add inter-service communication, service discovery, distributed tracing setup, multiple CI/CD pipelines, and container orchestration — all for zero architectural benefit at this scale.

### Clean Boundaries Without Network Overhead

The three project references enforce the same dependency rules as separate services:
- The Application project cannot reference Infrastructure (it defines interfaces, Infrastructure implements them).
- Business logic is testable without Azure SDK dependencies.
- Replacing an implementation (e.g., swapping Azure AI Search for Elasticsearch) requires changes only in Infrastructure.

These are the same architectural benefits of microservices, achieved through project boundaries instead of network boundaries.

### Straightforward Extraction Path

If a component needed independent scaling in the future (e.g., moving ingestion to a background worker), the interface boundaries make extraction straightforward:
1. Create a new project for the worker service.
2. Reference the existing Application and Infrastructure projects.
3. Move the `DocumentProcessor` orchestration to the worker.
4. No refactoring of business logic required.

## Consequences

### Positive
- Single deployment unit — simple CI/CD, simple operations.
- No inter-service latency — all calls are in-process.
- Easier debugging — one process, one log stream.
- Lower Azure costs — one compute instance.

### Negative
- Cannot scale ingestion independently from querying (acceptable at demo scale).
- A bug in ingestion could theoretically affect the query path (mitigated by clean separation).
- Does not demonstrate distributed systems skills (but demonstrates architectural judgment about when not to distribute).

## Alternatives Considered

| Alternative | Why Not |
|-------------|---------|
| **Microservices (separate services for ingestion, query, API)** | Over-engineered for the scope. Adds operational complexity without proportional benefit. |
| **Azure Functions (serverless)** | Good for event-driven ingestion, but fragments the codebase and makes local development harder. |
| **Single project (no internal layers)** | Too simple — does not demonstrate separation of concerns or DI patterns. |
