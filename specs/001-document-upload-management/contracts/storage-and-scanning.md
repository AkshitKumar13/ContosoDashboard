# Storage and Scanning Contracts

## `IFileStorageService`

- `UploadAsync(Stream content, string relativePath, string contentType, CancellationToken cancellationToken)`: Saves content to the configured storage provider and returns the normalized relative key.
- `DeleteAsync(string relativePath, CancellationToken cancellationToken)`: Deletes a stored object if present.
- `DownloadAsync(string relativePath, CancellationToken cancellationToken)`: Opens content only after the caller has authorized the document.
- `ExistsAsync(string relativePath, CancellationToken cancellationToken)`: Supports cleanup and replacement checks.

The local implementation uses a configured application-data root outside `wwwroot`. It must reject path traversal, absolute paths, and paths whose generated segments do not match the expected user/project/GUID pattern.

## `IFileScanner`

- `ScanAsync(Stream content, string contentType, CancellationToken cancellationToken)`: Returns `Clean`, `Unsafe`, or `Unavailable`.

Only `Clean` permits an upload or replacement to proceed. `Unsafe` and `Unavailable` reject immediately. The training implementation must be explicit about its limitations and must not claim production malware protection.

## Notification Delivery

Document sharing and project-upload notifications use `INotificationService`. A persisted share or document remains valid when notification creation/delivery fails. The service records a retryable delivery failure and retries without duplicating the permission grant.
