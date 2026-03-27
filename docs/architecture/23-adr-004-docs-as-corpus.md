# ADR-004: Architecture Documents as Part of the RAG Corpus

**Status:** Accepted
**Date:** 2024-03-15
**Context:** RAG Navigator — choosing what documents to index

## Context

RAG Navigator needs a realistic document corpus to demonstrate its capabilities. The sample data folder already contains engineering knowledge documents (ADRs, runbooks, postmortems, standards). Separately, the project includes a comprehensive architecture documentation set in `docs/architecture/`.

## Decision

Include the project's own architecture documents (`docs/architecture/*.md`) in the indexed RAG corpus alongside the sample engineering documents.

## Rationale

### Demonstrates the System on Itself

The most compelling demo of a RAG system is one that can answer questions about its own design. This creates a compelling self-referential loop:

- **Interviewer:** "Why did you choose hybrid retrieval?"
- **System:** Answers by retrieving and citing ADR-003 from the indexed architecture docs.
- **Effect:** The system demonstrates both its retrieval capability and its architectural reasoning simultaneously.

### Enriches the Demo Corpus

The architecture documents add 20+ detailed markdown files to the corpus, increasing:
- Total chunk count (more retrieval diversity).
- Topic coverage (security, cost, deployment, reliability — beyond just engineering ops).
- Question variety (users can ask architecture questions, not just operational ones).

### Validates Chunking on Real-World Content

Architecture documents have different structures than runbooks or postmortems:
- More tables and diagrams (Mermaid).
- Longer analytical sections.
- Cross-references between documents.
- Mixed heading depths.

Indexing these documents validates that the chunker handles diverse markdown structures well.

### No Additional Maintenance Cost

The architecture documents exist regardless of this decision. Indexing them requires only adding the `docs/architecture/` folder path to the ingestion scan — a one-line configuration change.

## Consequences

### Positive
- Richer demo — more documents, more question types, more impressive results.
- Self-documenting system — the system can explain its own design.
- Architecture Q&A capability for interviews.
- Validates chunker robustness on diverse document types.

### Negative
- Architecture docs may change as the project evolves, requiring re-indexing.
- Some documents contain Mermaid diagram syntax, which is not meaningful text for search.
- Cross-references between architecture docs (relative links) are not resolved in chunks.

### Mitigations
- Mermaid diagram blocks are typically small relative to surrounding prose and don't significantly impact retrieval quality.
- Cross-references in chunks provide enough context (document name, section) for the user to find the referenced document.
- Reindexing is fast (~5-10 seconds) and can be triggered with one click.

## Example Questions Enabled

With architecture docs in the corpus, users can ask:

| Question | Retrieves From |
|----------|---------------|
| "Why is this a modular monolith?" | ADR-001 |
| "What are the security risks?" | Security and Threat Model |
| "How does authentication work?" | Identity and Access |
| "What would you change for production?" | Non-Functional Requirements, Reliability |
| "What are the cost drivers?" | Cost Considerations |
| "How does the deployment work?" | Deployment Topology |
| "What does the CI/CD pipeline look like?" | CI/CD and Release Strategy |
| "How do I troubleshoot poor answers?" | Operations Runbook |
