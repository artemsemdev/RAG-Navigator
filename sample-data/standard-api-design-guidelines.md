# API Design Guidelines

**Version:** 2.1
**Last Updated:** 2024-02-20
**Owner:** Architecture Guild

## Purpose

These guidelines establish consistent conventions for designing HTTP APIs across all product teams. Following these standards ensures our APIs are predictable, discoverable, and easy to integrate with.

## URL Structure

### Resource Naming
- Use plural nouns for collections: `/orders`, `/users`, `/products`.
- Use kebab-case for multi-word resources: `/order-items`, `/shipping-labels`.
- Nest resources to express containment: `/orders/{orderId}/items`.
- Limit nesting to two levels maximum. Beyond that, promote the resource to a top-level endpoint.

### Versioning
- Version in the URL path: `/api/v1/orders`.
- Increment the major version only for breaking changes.
- Breaking changes include: removing a field, changing a field type, altering error formats.
- Non-breaking changes (adding fields, new endpoints) do not require a version bump.

## HTTP Methods

| Method | Usage | Idempotent | Example |
|--------|-------|------------|---------|
| GET | Retrieve a resource or collection | Yes | `GET /api/v1/orders/123` |
| POST | Create a new resource | No | `POST /api/v1/orders` |
| PUT | Full replacement of a resource | Yes | `PUT /api/v1/orders/123` |
| PATCH | Partial update of a resource | No | `PATCH /api/v1/orders/123` |
| DELETE | Remove a resource | Yes | `DELETE /api/v1/orders/123` |

## Request and Response Format

### Content Type
- All endpoints accept and return `application/json`.
- Use camelCase for JSON property names.
- Use ISO 8601 for dates and times: `2024-01-15T14:30:00Z`.
- Use strings for monetary amounts to avoid floating-point issues.

### Pagination
Use cursor-based pagination for large collections:

```json
{
  "data": [...],
  "pagination": {
    "nextCursor": "eyJpZCI6MTIzfQ==",
    "hasMore": true,
    "pageSize": 25
  }
}
```

Do not use offset-based pagination for datasets that change frequently — it leads to skipped or duplicated items.

### Filtering and Sorting
- Filter with query parameters: `GET /orders?status=pending&createdAfter=2024-01-01`.
- Sort with the `sort` parameter: `GET /orders?sort=-createdAt` (prefix with `-` for descending).

## Error Handling

### Error Response Format

All errors must follow this structure:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Human-readable description of the error.",
    "details": [
      {
        "field": "email",
        "message": "Must be a valid email address."
      }
    ],
    "traceId": "abc123-def456"
  }
}
```

### HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 | Successful GET, PUT, PATCH |
| 201 | Successful POST (resource created) |
| 204 | Successful DELETE (no content) |
| 400 | Validation error or malformed request |
| 401 | Authentication required |
| 403 | Authenticated but not authorized |
| 404 | Resource not found |
| 409 | Conflict (e.g., duplicate resource) |
| 429 | Rate limit exceeded |
| 500 | Internal server error (never expose stack traces) |

### Error Codes
- Define domain-specific error codes like `ORDER_ALREADY_SHIPPED`, `INSUFFICIENT_INVENTORY`.
- Include a `traceId` in every error response to enable log correlation.
- Log all 5xx errors; log 4xx errors at debug level.

## Authentication and Authorization

- All APIs require authentication via Azure AD OAuth 2.0 bearer tokens.
- Use the `Authorization: Bearer <token>` header.
- Validate tokens using the Azure AD JWKS endpoint.
- Implement role-based access control (RBAC) for sensitive endpoints.
- Never pass credentials as query parameters.

## Rate Limiting

- Apply rate limiting at the APIM gateway level.
- Default rate: 100 requests per minute per client.
- Return `429 Too Many Requests` with a `Retry-After` header.
- Critical endpoints (payments, auth) may have stricter limits.

## Health Checks

Every service must expose:
- `GET /health` — basic liveness check (returns 200 if the process is running).
- `GET /health/ready` — readiness check (validates database connectivity, downstream dependencies).

The readiness endpoint must not cache results. It should make real calls to dependencies.
