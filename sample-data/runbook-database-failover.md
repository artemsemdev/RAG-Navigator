# Runbook: Azure SQL Database Failover

**Owner:** Platform Engineering
**Last Updated:** 2024-03-10
**Severity Level:** P1
**Estimated Recovery Time:** 5-15 minutes (automatic), 30 minutes (manual)

## Overview

This runbook covers failover procedures for our Azure SQL Database instances configured with auto-failover groups. We use the Business Critical tier with zone-redundant replicas across Availability Zones in East US 2 (primary) and West US 2 (secondary).

## When to Use This Runbook

- Azure portal shows the primary database as unavailable.
- Application logs show sustained `SqlException` with error codes 40613 (database unavailable) or 40197 (service error).
- Monitoring alerts fire: "SQL Connection Failure Rate > 5%" for more than 3 minutes.
- Microsoft posts an Azure status incident affecting your region.

## Automatic Failover

Auto-failover groups are configured with a 60-second grace period. If the primary becomes unreachable for more than 60 seconds, Azure automatically promotes the secondary.

### What Happens Automatically

1. Azure detects the primary is unhealthy.
2. After the grace period, the secondary is promoted to primary.
3. DNS records for the failover group endpoint update within 30 seconds.
4. Applications using the failover group listener endpoint (`ourfailovergroup.database.windows.net`) reconnect automatically.

### Verify Automatic Failover

1. Check the Azure Portal → SQL Database → Failover Groups blade.
2. Confirm the secondary region shows as "Primary."
3. Run health check query:
   ```sql
   SELECT @@SERVERNAME, DB_NAME(), DATABASEPROPERTYEX(DB_NAME(), 'Updateability')
   ```
4. Verify application health endpoints return HTTP 200.

## Manual Failover Procedure

Use manual failover when automatic failover has not triggered but you need to proactively move traffic (e.g., planned maintenance, degraded performance in primary region).

### Prerequisites
- Azure CLI installed and authenticated.
- Contributor role on the SQL Server resource.
- Notify the on-call Slack channel `#platform-oncall` before proceeding.

### Steps

1. **Assess the situation:**
   ```bash
   az sql failover-group show \
     --name ourfailovergroup \
     --server our-primary-sql \
     --resource-group rg-platform-prod
   ```

2. **Initiate failover (with data loss prevention):**
   ```bash
   az sql failover-group set-primary \
     --name ourfailovergroup \
     --server our-secondary-sql \
     --resource-group rg-platform-prod
   ```

3. **Monitor replication status:**
   ```sql
   SELECT partner_server, replication_state_desc, last_replication
   FROM sys.dm_geo_replication_link_status
   ```

4. **Validate:**
   - Confirm application connectivity through the failover group endpoint.
   - Verify no data loss by checking the most recent order IDs.
   - Monitor error rates in Application Insights for 10 minutes.

## Post-Failover Actions

1. Update the incident channel with failover status and new primary region.
2. Monitor replication lag as the old primary re-syncs.
3. Schedule a failback during the next maintenance window if the original region recovers.
4. File a post-incident review if the failover was unplanned.

## Connection String Configuration

Applications must use the failover group listener endpoint, never the direct server name:

```
Server=ourfailovergroup.database.windows.net;Database=OurDb;Authentication=Active Directory Managed Identity;
```

Do NOT hardcode individual server names (`our-primary-sql.database.windows.net`). This defeats the purpose of failover groups.

## Escalation

If failover does not complete within 15 minutes:
1. Open an Azure Support ticket with Severity A.
2. Page the VP of Engineering via PagerDuty.
3. Communicate status to stakeholders via the `#incidents` Slack channel every 10 minutes.
