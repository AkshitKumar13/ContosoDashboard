# Document Service Contract

The application exposes document behavior through a service boundary consumed by Blazor pages and integration points. Every operation receives the authenticated actor context or an equivalent actor identifier and applies authorization before returning data or touching storage.

## Operations

- `GetMyDocumentsAsync(actor, query)`: Returns the actor's accessible document page with sorting and filters.
- `SearchAsync(actor, query)`: Searches title, description, tags, uploader, and project using the same authorization predicate as browsing.
- `GetProjectDocumentsAsync(actor, projectId)`: Returns documents for a project only when the actor is currently a project member, manager, or administrator.
- `GetTaskDocumentsAsync(actor, taskId)`: Returns documents associated with an authorized task.
- `UploadAsync(actor, uploadRequest)`: Validates metadata/file, authorizes project/task scope, scans, stores, persists, audits, and sends project notifications.
- `UpdateMetadataAsync(actor, documentId, metadata)`: Updates owner-authorized metadata and audits the change.
- `ReplaceFileAsync(actor, documentId, file)`: Validates/scans/stores a replacement, switches only after success, and audits the replacement.
- `DownloadAsync(actor, documentId)`: Rechecks authorization, records a download activity, and returns a stream plus safe download metadata.
- `DeleteAsync(actor, documentId)`: Requires owner/project-manager/administrator permission, confirms at the UI boundary, removes metadata/file, and audits deletion.
- `ShareAsync(actor, documentId, target)`: Creates a direct-user or existing-department/team grant, audits it, and queues/retries recipient notification without rolling back the grant.
- `GetSharedWithMeAsync(actor, query)`: Returns active direct and department/team shares visible to the actor.
- `GetAdminReportAsync(actor, range)`: Requires administrator role and returns document type, uploader, and access-pattern aggregates within the retention window.

## Authorization Contract

- Employees can manage their own documents.
- Current project membership grants view/download for project documents.
- Project managers can manage documents associated with their projects.
- Team leads can manage documents within their existing team scope.
- Administrators can access all documents and reports.
- Project membership is evaluated at operation time; direct shares are independent and remain until revoked.
- Unauthorized operations return a not-found/denied result without disclosing document existence or storage details.
