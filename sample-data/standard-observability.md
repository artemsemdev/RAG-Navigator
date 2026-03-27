# Observability Standards

**Version:** 1.3
**Last Updated:** 2024-02-28
**Owner:** Platform Engineering

## Overview

Observability is a core operational requirement for all production services. Every service must implement structured logging, distributed tracing, and metrics collection following the standards defined in this document.

## Three Pillars

### 1. Structured Logging

All services must use structured (JSON) logging with consistent field names.

**Required fields for every log entry:**

| Field | Description | Example |
|-------|-------------|---------|
| `timestamp` | ISO 8601 UTC timestamp | `2024-01-15T14:30:00.123Z` |
| `level` | Log level | `Information`, `Warning`, `Error` |
| `message` | Human-readable description | `"Order processed successfully"` |
| `service` | Service name | `"order-service"` |
| `correlationId` | W3C trace context trace ID | `"abc123def456"` |
| `environment` | Deployment environment | `"production"` |

**Contextual fields (where applicable):**
- `userId` — the authenticated user ID (never log PII like email or name).
- `orderId`, `transactionId` — domain-specific identifiers.
- `duration` — operation duration in milliseconds.
- `statusCode` — HTTP status code for request logs.

**Logging Rules:**
- Use `Information` for normal operations (request served, job completed).
- Use `Warning` for recoverable issues (retry succeeded, fallback used).
- Use `Error` for failures that require investigation.
- Never log at `Debug` or `Trace` level in production (use dynamic log levels if needed).
- Never log sensitive data: passwords, tokens, credit card numbers, personal data.
- Log the full exception stack trace for errors, but only at the origin — do not re-log caught exceptions at every call level.

### 2. Distributed Tracing

We use the W3C Trace Context standard propagated via the `traceparent` HTTP header. Azure Application Insights is our tracing backend.

**Requirements:**
- Every incoming HTTP request must create or continue a trace.
- All outgoing HTTP calls, Service Bus messages, and database queries must propagate the trace context.
- Use the `Activity` API in .NET for creating spans. Do not build custom correlation mechanisms.
- Name spans descriptively: `"ProcessOrder"`, `"ValidateInventory"`, not `"Step1"`, `"DoWork"`.

**Configuration:**
```csharp
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, options) =>
{
    module.EnableSqlCommandTextInstrumentation = true;
});
```

### 3. Metrics

Services must expose the following standard metrics. We use Azure Monitor / Application Insights for collection.

**Required Metrics:**

| Metric | Type | Description |
|--------|------|-------------|
| `http_request_duration_seconds` | Histogram | Request latency by endpoint and status code |
| `http_requests_total` | Counter | Total request count by endpoint and status code |
| `active_connections` | Gauge | Current active HTTP connections |
| `dependency_call_duration_seconds` | Histogram | Downstream dependency latency |

**Custom Metrics:**
- Define domain-specific metrics for key business operations: `orders_processed_total`, `payment_failures_total`, `inventory_reservations_active`.
- Use tags/dimensions for breakdowns: `region`, `customer_tier`, `payment_method`.

## Dashboards

Every service must have a Grafana dashboard with:

1. **Request Rate:** Requests per second over time.
2. **Error Rate:** 4xx and 5xx rates as percentages.
3. **Latency Percentiles:** P50, P95, P99 response times.
4. **Dependency Health:** Latency and error rates for all downstream dependencies.
5. **Resource Utilization:** CPU, memory, pod count (for AKS services).

Dashboard templates are available in the `grafana-dashboards` repo. Clone and customize for your service.

## Alerting

### Standard Alert Rules

| Alert | Condition | Severity | Action |
|-------|-----------|----------|--------|
| High Error Rate | 5xx rate > 5% for 5 minutes | P2 | Page on-call |
| Critical Error Rate | 5xx rate > 25% for 2 minutes | P1 | Page on-call + escalate |
| High Latency | P95 > 2s for 10 minutes | P3 | Notify Slack |
| Dependency Failure | Dependency error rate > 10% for 5 minutes | P2 | Page on-call |
| Pod Restarts | > 3 restarts in 10 minutes | P2 | Page on-call |

### Alert Hygiene
- Every alert must have a runbook linked in the alert description.
- Review and tune alert thresholds quarterly.
- Delete alerts that have not fired in 6 months — they are either too conservative or for decommissioned components.
- Alerts must fire in staging before they fire in production (test your alerting).

## Implementation Checklist

Before going to production, verify:
- [ ] Structured JSON logging is configured with all required fields.
- [ ] Application Insights SDK is wired up with trace context propagation.
- [ ] A Grafana dashboard exists with the standard panels.
- [ ] Alert rules are configured with linked runbooks.
- [ ] No sensitive data appears in logs (verify with a security review).
- [ ] Health check endpoints (`/health`, `/health/ready`) are implemented.
