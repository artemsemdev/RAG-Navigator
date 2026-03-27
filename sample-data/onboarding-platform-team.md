# Platform Engineering Team — Onboarding Guide

**Last Updated:** 2024-03-01
**Audience:** New engineers joining the Platform Engineering team

## Welcome

Welcome to Platform Engineering! Our team is responsible for the internal developer platform, CI/CD infrastructure, shared Azure services, and operational tooling. We serve ~60 engineers across 8 product teams.

## Team Mission

Build and maintain the infrastructure, tools, and services that enable product engineering teams to ship reliable software quickly, without needing to become infrastructure experts.

## Key Responsibilities

- **Azure Infrastructure:** Manage shared Azure resources (AKS clusters, Azure SQL, Service Bus, Key Vault, APIM) using Terraform.
- **CI/CD Pipelines:** Maintain Azure DevOps pipeline templates and GitHub Actions workflows.
- **Observability:** Run the centralized monitoring stack (Application Insights, Grafana, Azure Monitor).
- **Developer Experience:** Maintain internal CLI tools, project templates, and documentation.
- **On-Call Rotation:** Participate in the weekly on-call rotation covering production infrastructure.

## Architecture Overview

Our platform runs on Azure with the following core components:

- **Compute:** Azure Kubernetes Service (AKS) with 3 node pools (system, general, high-memory).
- **Data:** Azure SQL Database (Business Critical tier), Azure Cosmos DB (for event store), Redis Cache.
- **Messaging:** Azure Service Bus (Premium tier) for async workflows.
- **API Gateway:** Azure API Management for external-facing APIs.
- **Secrets:** Azure Key Vault with RBAC access policies.
- **DNS:** Azure DNS + Azure Front Door for global load balancing.

## First Week Checklist

### Day 1 — Access & Setup
- [ ] Get Azure Active Directory account with MFA enabled.
- [ ] Join Slack channels: `#platform-engineering`, `#platform-oncall`, `#incidents`.
- [ ] Clone the infrastructure repo: `github.com/our-org/platform-infra`.
- [ ] Install required tools: Azure CLI, Terraform, kubectl, Helm, Docker.
- [ ] Request access to Azure subscriptions (Dev, Staging, Prod) via the access request form.

### Day 2 — Understand the Landscape
- [ ] Read ADR-001 (Event-Driven Architecture) and ADR-002 (AKS Multi-Tenancy).
- [ ] Walk through the Terraform module structure with your buddy.
- [ ] Review the CI/CD pipeline architecture in the Wiki.
- [ ] Get read access to Grafana dashboards.

### Day 3 — Get Hands-On
- [ ] Deploy a test change to the Dev environment using Terraform.
- [ ] Trace a request through the stack: Front Door → APIM → AKS → Database.
- [ ] Review a recent PR in the infrastructure repo.

### Day 4-5 — Deeper Dives
- [ ] Shadow an on-call shift.
- [ ] Review the database failover runbook and incident postmortems.
- [ ] Pair with a teammate on a current sprint task.
- [ ] Set up your local development environment for the internal CLI tools.

## Communication Norms

- **Async by default.** Use Slack threads for discussions, not DMs.
- **PRs are conversations.** Explain the "why" in PR descriptions; comments are for learning, not gatekeeping.
- **Blameless culture.** Postmortems focus on systemic fixes, not individual blame.
- **Document decisions.** If a decision is worth discussing, it's worth an ADR.

## Key Contacts

| Role | Person | Slack |
|------|--------|-------|
| Team Lead | Marcus Rodriguez | @marcus |
| On-Call Coordinator | Sarah Chen | @sarah |
| Azure Admin | James Park | @james |
| DevOps Lead | Priya Nair | @priya |

## Useful Links

- Infrastructure Repo: `github.com/our-org/platform-infra`
- Internal Wiki: `wiki.internal/platform`
- Grafana Dashboards: `grafana.internal/dashboards`
- On-Call Schedule: PagerDuty → Platform Engineering
- Azure Portal: `portal.azure.com`
