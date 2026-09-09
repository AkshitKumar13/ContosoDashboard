# Quickstart Validation: Document Upload and Management

## Prerequisites

- .NET 10 SDK compatible with `ContosoDashboard.csproj`.
- SQL Server LocalDB available through the configured `DefaultConnection`.
- A clean local database for first-run validation; the application uses `EnsureCreated`.
- Run from the repository root with PowerShell.

## Start the Application

```powershell
cd ContosoDashboard
dotnet run
```

Open the HTTPS URL printed by the application and sign in through the training login page.

## Validation Scenarios

1. **Secure upload**: Sign in as an employee, upload a supported file under 25 MB with a title/category, and verify success, metadata, and a file outside `wwwroot`. Repeat with an unsupported extension, oversized file, and unavailable/unsafe scan; each must be rejected.
2. **Authorization**: Upload a project document as a project member, verify another current member can view/download it, then remove membership and verify project access is denied. Create a direct share and verify it remains available after membership removal.
3. **Browse and search**: Create documents across categories/projects, then verify sorting, filters, search fields, and that unauthorized documents never appear.
4. **Preview and download**: Verify authorized PDF/image preview and download for other supported files. Verify denied users receive no file or metadata.
5. **Metadata and replacement**: Edit metadata and replace a file with a valid upload. Force an invalid replacement and verify the original file remains available.
6. **Sharing and retry**: Share to a user and existing department/team, verify Shared with Me and in-app notification. Simulate notification failure and verify the share remains active and retryable.
7. **Task/project/dashboard integration**: Attach a document from a task, inspect the project view, and verify the dashboard shows the five latest uploads and document count.
8. **Audit/reporting**: As an administrator, verify upload, download, delete, replace, metadata, and share activities and report aggregates. Verify non-administrators cannot access reports.

## Automated Checks

```powershell
dotnet build
```

Run the focused test project when added by implementation tasks. The minimum acceptance evidence must cover upload validation, authorization/IDOR denial, persistence cleanup, search filtering, notification retry, and 12-month audit retention behavior.

See [data-model.md](data-model.md) for entity rules and [contracts/](contracts/) for service and infrastructure contracts.
