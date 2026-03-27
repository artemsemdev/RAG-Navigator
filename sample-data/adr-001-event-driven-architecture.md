# ADR-001: Adopt Event-Driven Architecture for Order Processing

**Status:** Accepted
**Date:** 2024-01-15
**Decision Makers:** Platform Engineering, Backend Team, Product Engineering

## Context

Our current order processing pipeline is synchronous and tightly coupled. When a customer places an order, the API gateway calls the order service, which calls inventory, payment, notification, and fulfillment services in sequence. This creates several problems:

- **Latency:** End-to-end processing takes 3-5 seconds because every downstream call is blocking.
- **Reliability:** If the notification service is down, the entire order fails even though notification is not critical to order placement.
- **Scalability:** The order service cannot scale independently from downstream services. Black Friday traffic caused cascading failures in 2023.
- **Coupling:** Every new downstream consumer requires changes to the order service.

## Decision

We will adopt an event-driven architecture using Azure Service Bus as the message broker for order processing and related workflows.

### Key Design Choices

1. **Azure Service Bus Topics:** Each domain event (e.g., `OrderPlaced`, `PaymentCompleted`, `InventoryReserved`) is published to a dedicated topic. Consumers subscribe independently.

2. **Event Schema:** All events follow a CloudEvents-compatible envelope with `type`, `source`, `subject`, `time`, and `data` fields. We use JSON serialization with a schema registry for backward compatibility.

3. **Idempotency:** Every consumer must be idempotent. We enforce this using a deduplication table keyed by event ID with a 7-day TTL.

4. **Dead Letter Handling:** Failed messages are routed to dead-letter queues after 5 retry attempts with exponential backoff. An alert fires on DLQ depth > 10.

5. **Ordering Guarantees:** We use Service Bus sessions (keyed by Order ID) where strict ordering is required (e.g., payment before fulfillment).

## Consequences

### Positive
- Order placement latency drops to ~200ms (publish event and return).
- Services can be deployed and scaled independently.
- New consumers (analytics, fraud detection) can subscribe without modifying the producer.
- Partial system outages no longer fail the entire order flow.

### Negative
- Increased operational complexity: distributed tracing, message monitoring, DLQ management.
- Eventual consistency: inventory counts may briefly lag behind actual state.
- Debugging is harder — must correlate events across services using correlation IDs.

### Mitigations
- We will adopt Azure Application Insights with W3C trace context propagation for end-to-end tracing.
- A dedicated Grafana dashboard will monitor event throughput, consumer lag, and DLQ depth.
- Runbooks will document DLQ triage and replay procedures.

## Alternatives Considered

1. **gRPC with circuit breakers:** Reduces latency but retains tight coupling. Does not solve the "add new consumer" problem.
2. **Azure Event Grid:** Lightweight but lacks the delivery guarantees and session support we need for financial transactions.
3. **Kafka on AKS:** More powerful but significant operational overhead for a team of our size. Service Bus is fully managed.
