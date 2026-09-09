# Research: Document Upload and Management

## Decision: Use service-layer orchestration with replaceable storage and scanning boundaries

- **Decision**: Add `IDocumentService` for document authorization and business workflows, `IFileStorageService` for file persistence, and `IFileScanner` for malware checks. Keep the training implementation local and register the abstractions through dependency injection.
- **Rationale**: This matches the repository's existing service interface pattern, keeps business logic independent of filesystem and scanner details, and preserves the stakeholder requirement for future hosted-storage migration.
- **Alternatives considered**: Direct `System.IO` calls from Razor pages were rejected because they would duplicate authorization and make migration/testing harder. Adding Azure SDK packages now was rejected because the application must remain offline-first.

## Decision: Store files outside `wwwroot` with generated relative paths

- **Decision**: Store uploads under a configured application-data root such as `AppData/uploads/{userId}/{projectId-or-personal}/{guid}.{extension}`. Persist only a normalized relative storage path and never the original filename as a path component.
- **Rationale**: Files are not directly web-addressable, GUID names prevent traversal and collisions, and a relative path can map to local disk or future object-storage keys.
- **Alternatives considered**: Storing in `wwwroot` was rejected because it bypasses authorization. Persisting the full absolute path was rejected because it is not portable.

## Decision: Fail closed on malware scanning

- **Decision**: A file is unavailable until scanning succeeds; a failed scan, unavailable scanner, or unsafe result rejects the upload immediately and prevents metadata from being reported as successful.
- **Rationale**: This is the accepted clarification and satisfies the security-by-default constitution principle. The local training scanner must provide a deterministic, documented behavior without claiming production malware protection.
- **Alternatives considered**: Holding files for administrator review was rejected for the initial offline feature because it adds workflow state and operational complexity. Accepting files before scanning was rejected because it creates an unsafe accessible state.

## Decision: Use integer document keys and text categories

- **Decision**: `Document`, `DocumentShare`, and `DocumentActivity` use integer primary keys. Categories are stored as validated text values, not an enum column.
- **Rationale**: This follows the stakeholder constraint and existing model conventions while keeping the category list understandable and extensible.
- **Alternatives considered**: GUID document IDs were rejected for consistency. An integer enum category was rejected because the requirement explicitly calls for text values.

## Decision: Calculate authorization at operation time

- **Decision**: Document queries and mutations must apply authorization predicates in the service layer. Project membership grants project access only while membership exists; direct user/department shares are independent and remain until revoked.
- **Rationale**: Authorization is dynamic, prevents IDOR, and directly captures the clarification about membership loss and direct shares.
- **Alternatives considered**: Copying permissions into a static access list was rejected because membership changes would become stale. Relying only on page attributes was rejected because downloads and service calls also require protection.

## Decision: Preserve sharing on notification failure and retry delivery

- **Decision**: Persist a valid share before notification delivery; notification failure does not roll back access. Implement bounded retry behavior through the existing notification service boundary and record the failed delivery for retry/audit.
- **Rationale**: This is the accepted clarification and prevents notification transport from incorrectly revoking a permission that was explicitly granted.
- **Alternatives considered**: Transactionally rolling back the share was rejected because it couples access state to delivery reliability. Silent loss without retry was rejected because recipients would not know about a valid share.

## Decision: Use EF Core queries and indexes for the initial performance target

- **Decision**: Add indexes for uploader, project, upload date, category, content type, and activity timestamps; implement filtered/sorted queries in `DocumentService` and bound list results/paging for the 500-document target.
- **Rationale**: The existing application uses EF Core directly and the stated scale is modest. This avoids premature search infrastructure while preserving a clear upgrade point.
- **Alternatives considered**: External search was rejected because the feature must work offline. Loading all documents into Razor components was rejected because it undermines the 2-second target and leaks filtering responsibility into the UI.

## Decision: Treat audit records as append-oriented records with 12-month retention

- **Decision**: Record upload, download, deletion, replacement, metadata changes, and share actions with actor, document, action, timestamp, and relevant target details. Provide administrator-only reporting over the retained 12-month window.
- **Rationale**: This satisfies the clarified retention requirement and creates an attributable security trail without introducing a separate telemetry platform.
- **Alternatives considered**: Unstructured application logs were rejected because they are harder to query and report. Indefinite retention was rejected because it exceeds the clarified initial scope.
