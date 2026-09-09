# Implementation Plan: Document Upload and Management

**Branch**: `001-document-upload-management` | **Date**: 2026-09-09 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-document-upload-management/spec.md`

## Summary

Add secure document upload, organization, sharing, retrieval, task/project integration,
dashboard visibility, notifications, and administrator audit reporting to the existing
offline Blazor Server application. Use EF Core entities and service-layer authorization,
with local filesystem storage and malware-scanning abstractions so business behavior can
later migrate to hosted storage without changing the UI or data contract.

## Technical Context

**Language/Version**: C# on .NET 10; Razor components with nullable reference types enabled  
**Primary Dependencies**: ASP.NET Core Blazor Server, EF Core SQL Server 10, existing cookie/mock authentication and notification services  
**Storage**: SQL Server LocalDB/SQL Server for metadata; local filesystem under application data for file content; replaceable storage interface for future object storage  
**Testing**: `dotnet build`; add focused unit/integration coverage for service authorization, storage cleanup, upload validation, search filtering, sharing, notifications, and audit retention  
**Target Platform**: Offline-capable web application running on the existing ASP.NET Core host  
**Project Type**: Single web project with Razor Pages/Blazor Server, EF Core data layer, and service layer  
**Performance Goals**: Uploads up to 25 MB within 30 seconds; lists and search within 2 seconds for up to 500 accessible documents; previews within 3 seconds  
**Constraints**: Files outside `wwwroot`; fail closed on unavailable/unsafe scans; dynamic authorization on every operation; integer IDs and text categories; 12-month audit retention; no cloud dependency  
**Scale/Scope**: Existing training users and projects; initial document lists up to 500 items; no version history, quotas, trash recovery, external storage, or mobile app

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Gate

- **Training-first scope**: PASS. The design remains offline-capable and documents mock authentication, local storage, scanner limitations, and production migration boundaries.
- **Layered design and replaceable infrastructure**: PASS. File storage and scanning are abstractions; document workflows remain in services rather than Razor components.
- **Security by default**: PASS. Authorization is applied in service queries and mutations, downloads are protected, paths are generated, and scan failures reject uploads.
- **Testable behavior and integration checks**: PASS. User stories have acceptance scenarios; the quickstart and planned focused tests cover authorization, persistence, and user-visible workflows.
- **Simplicity and traceable change**: PASS. The initial design uses EF Core and existing notification patterns without external search or cloud packages.

### Post-Design Gate

- **Training-first scope**: PASS. No production-only dependency is required for local operation.
- **Layered design and replaceable infrastructure**: PASS. Contracts define storage, scanning, notification, and document-service boundaries.
- **Security by default**: PASS. Membership is evaluated at operation time; direct shares are explicit; unauthorized results do not disclose document existence.
- **Testable behavior and integration checks**: PASS. `quickstart.md` defines runnable scenarios and the plan reserves tests for the highest-risk cross-layer contracts.
- **Simplicity and traceable change**: PASS. No constitution violation or complexity exception is required.

## Project Structure

### Documentation (this feature)

```text
specs/001-document-upload-management/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)
```text
ContosoDashboard/
├── Data/
│   └── ApplicationDbContext.cs                 # Document, share, and activity DbSets/configuration
├── Models/
│   ├── Document.cs
│   ├── DocumentShare.cs
│   ├── DocumentActivity.cs
│   └── ... existing User/Project/TaskItem models
├── Services/
│   ├── DocumentService.cs                       # Queries, authorization, workflows, audit
│   ├── FileStorageService.cs                    # IFileStorageService + local implementation
│   ├── FileScannerService.cs                    # IFileScanner + training implementation
│   ├── NotificationService.cs                  # Extend notification types/retry boundary
│   └── DashboardService.cs                      # Recent documents and count
├── Pages/
│   ├── Documents.razor                          # Browse, sort, filter, search, shared view
│   ├── DocumentDetails.razor                    # Metadata, preview, download, share, delete
│   ├── ProjectDetails.razor                     # Project document section
│   ├── Tasks.razor                              # Task document entry point
│   └── Index.razor                              # Recent documents widget and count
├── wwwroot/css/site.css                         # Feature styles only where needed
└── AppData/uploads/                             # Runtime-only storage; outside wwwroot

ContosoDashboard.Tests/
├── DocumentServiceTests.cs                      # Authorization and workflow tests
├── FileStorageServiceTests.cs                   # Path, cleanup, and replacement tests
└── DocumentIntegrationTests.cs                 # EF/query/notification contract checks
```

**Structure Decision**: Extend the existing single web project in its current
`Data`, `Models`, `Services`, and `Pages` layers. Add a focused test project only if
the repository has no suitable test harness, keeping business rules testable without
moving them into UI components. Runtime files live in `AppData/uploads`, which is
created/configured at startup and excluded from source control.

## Implementation Sequence

1. Add entities, relationships, indexes, and retention query configuration to the EF model.
2. Add storage/scanning contracts and local implementations with generated relative paths, size/type validation, cleanup, and fail-closed scan behavior.
3. Implement `DocumentService` queries and mutations with operation-time authorization, project/task associations, direct department/team shares, audit writes, and notification retry handling.
4. Register services and configuration in `Program.cs`; create runtime upload directories safely and do not expose them as static files.
5. Add document browsing/upload/detail UI with progress, per-file outcomes, sorting/filtering/search, preview/download, metadata editing, replacement, deletion, and sharing.
6. Integrate project details, task context, dashboard recent documents/count, and administrator reporting.
7. Add focused tests and execute the quickstart scenarios, then run `dotnet build` and review authorization/data-isolation evidence.

## Risk Controls

- **File/path risk**: generated GUID paths, relative-path validation, storage outside `wwwroot`, and cleanup on persistence failure.
- **IDOR risk**: every service operation receives actor context and applies authorization predicates before returning metadata or streams.
- **Scanner limitation**: unavailable or unsafe scan rejects immediately; training scanner limitations are documented.
- **Notification failure**: share persists independently and failed delivery is retryable; duplicate grants are prevented.
- **Performance risk**: indexed query fields, server-side filters, bounded results, and no unbounded file/list loading in Razor components.
- **Database initialization risk**: preserve the repository's `EnsureCreated` behavior for training, document any clean-database requirement, and avoid destructive migrations in the feature.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
No constitution violations. Complexity tracking is not applicable.
