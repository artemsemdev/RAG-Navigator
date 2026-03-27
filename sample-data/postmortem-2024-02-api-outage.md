# Post-Incident Review: API Gateway Outage — February 8, 2024

**Incident ID:** INC-2024-0208
**Severity:** P1
**Duration:** 47 minutes (14:23 UTC – 15:10 UTC)
**Impact:** 100% of API traffic returned HTTP 503 for 47 minutes. Affected ~12,000 active users.
**Incident Commander:** Sarah Chen
**Author:** Platform Engineering Team

## Summary

On February 8, 2024, a misconfigured Azure API Management (APIM) policy deployment caused all API traffic to fail with 503 errors. The root cause was a Terraform change that removed the backend service URL from the APIM configuration, which was deployed automatically through our CI/CD pipeline without adequate validation.

## Timeline (UTC)

| Time | Event |
|------|-------|
| 13:45 | PR #892 merged: Terraform change to update APIM rate limiting policy |
| 14:00 | CI/CD pipeline deploys Terraform changes to production |
| 14:23 | Monitoring alert: "API Error Rate > 50%" fires |
| 14:25 | On-call engineer acknowledges alert, begins investigation |
| 14:30 | APIM blade in Azure Portal shows all operations returning 503 |
| 14:35 | Root cause identified: backend URL field is empty in APIM config |
| 14:40 | Decision to rollback Terraform state rather than manual fix |
| 14:42 | Rollback initiated: `terraform apply` with previous known-good state |
| 14:55 | Terraform rollback completes, APIM policies restored |
| 15:05 | Error rate drops below 1% |
| 15:10 | Incident resolved. All-clear posted to #incidents |

## Root Cause

The Terraform module for APIM was refactored in PR #892 to support per-operation rate limiting. During the refactor, the developer moved the `backend_url` variable from the `api_management_api` resource to a new module but accidentally used a variable reference that evaluated to an empty string in the production `tfvars` file.

The Terraform plan showed the change, but the diff was buried in a 400-line plan output. The reviewer approved the PR without catching the empty backend URL.

## Contributing Factors

1. **No integration test for APIM connectivity.** The pipeline deployed Terraform without validating that the APIM instance could reach its backend after the change.
2. **Large Terraform plan diffs.** The refactor touched multiple resources, producing a plan output that was difficult to review manually.
3. **No canary or staged rollout for APIM changes.** The change went directly to production.
4. **Monitoring gap.** The 503 alert threshold was 50%, so it took ~23 minutes of full outage before the alert fired (the metric uses a 5-minute aggregation window).

## Corrective Actions

| Action | Owner | Status | Due Date |
|--------|-------|--------|----------|
| Add post-deploy health check to APIM pipeline: call a synthetic endpoint and verify 200 response | DevOps | Completed | 2024-02-15 |
| Reduce API error rate alert threshold from 50% to 10% with 1-minute aggregation | Platform Eng | Completed | 2024-02-12 |
| Implement `terraform plan` output parser that flags removals of critical fields (backend_url, certificate, etc.) | DevOps | In Progress | 2024-03-01 |
| Add APIM smoke test stage to release pipeline before full traffic shift | Platform Eng | Planned | 2024-03-15 |
| Establish Terraform module review checklist for infrastructure PRs | Engineering Leads | Planned | 2024-03-01 |

## Lessons Learned

1. **Automated validation beats human review for infrastructure changes.** A simple post-deploy curl would have caught this in seconds.
2. **Alert thresholds must match business SLAs.** A 50% error rate threshold is too high when the SLA is 99.9%.
3. **Terraform refactors are high-risk changes** and should be treated with the same rigor as database migrations: staged rollout, automated testing, and explicit rollback plans.

## Impact Assessment

- **Customer Impact:** ~12,000 users experienced full API unavailability for 47 minutes.
- **Revenue Impact:** Estimated $18,000 in lost transactions.
- **SLA Impact:** Monthly uptime dropped to 99.89%, breaching the 99.9% SLA.
- **Reputation:** 23 support tickets filed, 4 social media mentions.
