# ADR-002: AKS Multi-Tenancy Strategy Using Namespaces

**Status:** Accepted
**Date:** 2024-02-01
**Decision Makers:** Platform Engineering, Security, Architecture Guild

## Context

We currently run a single shared AKS cluster for all product teams. As the number of teams and services grows (8 teams, 35+ microservices), we need a clear multi-tenancy strategy to provide:

- Resource isolation between teams (noisy neighbor prevention).
- Network segmentation for security compliance.
- Independent scaling and cost attribution per team.
- Self-service deployment without stepping on other teams' resources.

## Decision

We will implement namespace-based multi-tenancy within a single AKS cluster, with resource quotas, network policies, and RBAC boundaries per team namespace.

### Implementation Details

1. **Namespace per Team:** Each product team gets a dedicated namespace (e.g., `team-orders`, `team-payments`, `team-catalog`). Shared services (ingress, monitoring agents) run in `platform-system`.

2. **Resource Quotas:**
   - Each namespace has CPU and memory limits enforced via `ResourceQuota`.
   - Default: 8 CPU cores, 16 GiB memory per namespace.
   - Teams can request increases through a PR to the platform-infra repo.

3. **Network Policies:**
   - Default deny all ingress/egress per namespace.
   - Explicit policies allow traffic from the ingress controller and between approved namespaces.
   - Egress to Azure services (SQL, Service Bus, Key Vault) is allowed via Azure Private Link.

4. **RBAC:**
   - Team members get `edit` role within their namespace only.
   - Platform Engineering has `cluster-admin`.
   - Service accounts use Workload Identity to access Azure resources with least-privilege roles.

5. **Cost Attribution:**
   - We tag AKS node pools and use namespace-level resource consumption from Azure Cost Management.
   - Monthly cost reports are generated per team namespace.

## Consequences

### Positive
- Clear blast radius: a misconfigured deployment affects only one namespace.
- Teams can deploy independently without cross-team coordination.
- Cost visibility per team enables informed capacity planning.
- Network policies satisfy SOC 2 segmentation requirements.

### Negative
- Cluster-level issues (node pool exhaustion, control plane problems) affect all teams.
- Namespace quotas require tuning as workloads evolve.
- Network policies add complexity to cross-team service calls.

## Alternatives Considered

1. **Cluster per Team:** Strongest isolation but much higher cost and operational burden (~$2,000/month per additional cluster minimum).
2. **Virtual Clusters (vCluster):** Promising technology but not yet mature enough for our compliance requirements.
3. **No Isolation:** Simpler but does not meet security requirements and creates noisy-neighbor risks.
