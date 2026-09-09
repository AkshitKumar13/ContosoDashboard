# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/001-document-upload-management/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

## Phase 1: Setup

**Purpose**: Establish feature configuration and runtime boundaries without changing existing user behavior.

- [x] T001 Add document-storage configuration keys and local upload-root defaults in `ContosoDashboard/appsettings.json` and `ContosoDashboard/appsettings.Development.json`
- [x] T002 [P] Add `AppData/uploads/.gitkeep` and update `ContosoDashboard/.gitignore` so runtime uploads remain outside source control
- [x] T003 [P] Create the focused test project structure in `ContosoDashboard.Tests/ContosoDashboard.Tests.csproj` with references to `ContosoDashboard/ContosoDashboard.csproj`
- [x] T004 [P] Add shared test fixtures for temporary storage, seeded users, projects, and authenticated actors in `ContosoDashboard.Tests/DocumentTestFixture.cs`

---

## Phase 2: Foundational

**Purpose**: Implement blocking data, storage, scanning, authorization, and notification primitives before user-story work.

- [x] T005 Create `ContosoDashboard/Models/Document.cs` with integer key, metadata, generated relative path, owner, project, and task associations
- [x] T006 [P] Create `ContosoDashboard/Models/DocumentShare.cs` with mutually exclusive direct-user and department/team targets
- [x] T007 [P] Create `ContosoDashboard/Models/DocumentActivity.cs` with actor, action, document, details, and timestamp fields
- [x] T008 Extend `ContosoDashboard/Data/ApplicationDbContext.cs` with document DbSets, relationships, indexes, uniqueness rules, and 12-month activity query support
- [x] T009 Create `ContosoDashboard/Services/FileStorageService.cs` with `IFileStorageService` and local storage implementation outside `wwwroot`, including relative-path validation and cleanup
- [x] T010 [P] Create `ContosoDashboard/Services/FileScannerService.cs` with `IFileScanner` and explicit clean/unsafe/unavailable results that fail closed
- [x] T011 [P] Create `ContosoDashboard/Services/DocumentValidation.cs` with approved categories, extensions, MIME types, 25 MB limit, title/tag validation, and filename normalization
- [x] T012 Extend `ContosoDashboard/Models/Notification.cs` and `ContosoDashboard/Services/NotificationService.cs` with document notification types and retryable delivery-failure handling
- [x] T013 Register document services, storage-root configuration, scanner, and notification dependencies in `ContosoDashboard/Program.cs` without exposing `AppData/uploads` through static files
- [x] T014 [P] Add foundational storage, scanner, and validation tests in `ContosoDashboard.Tests/FileStorageServiceTests.cs` and `ContosoDashboard.Tests/DocumentValidationTests.cs`

**Checkpoint**: Data, storage, scanning, validation, authorization inputs, and notification primitives are ready for story implementation.

---

## Phase 3: User Story 1 - Upload and Securely Store Documents (Priority: P1) MVP

**Goal**: Authenticated employees can upload valid documents with metadata, receive per-file outcomes, and have unsafe or invalid files rejected.

**Independent Test**: Upload valid, unsupported, oversized, scan-failed, and multi-file selections and verify secure storage, metadata, cleanup, and user feedback without search or sharing.

### Tests for User Story 1

- [x] T015 [P] [US1] Add upload success, required metadata, supported-type, size-limit, scanner-failure, and per-file-result tests in `ContosoDashboard.Tests/DocumentUploadTests.cs`
- [x] T016 [P] [US1] Add upload authorization, project/task association, persistence-failure cleanup, and no-public-path tests in `ContosoDashboard.Tests/DocumentUploadAuthorizationTests.cs`

### Implementation for User Story 1

- [x] T017 [US1] Create `ContosoDashboard/Services/DocumentService.cs` with `IDocumentService` upload orchestration: validate, authorize, generate path, scan, store, persist metadata, audit, and notify
- [x] T018 [US1] Implement fail-closed upload cleanup and retry-safe outcomes in `ContosoDashboard/Services/DocumentService.cs`
- [x] T019 [US1] Create `ContosoDashboard/Pages/Documents.razor` with authenticated multi-file selection, metadata form, progress state, per-file results, and upload retry behavior
- [x] T020 [US1] Add upload navigation and document-management entry points to `ContosoDashboard/Shared/NavMenu.razor`
- [ ] T021 [US1] Run the User Story 1 independent validation from `specs/001-document-upload-management/quickstart.md` and record build/test evidence

**Checkpoint**: User Story 1 is independently usable as the MVP.

---

## Phase 4: User Story 2 - Find, View, Download, and Manage Accessible Documents (Priority: P1)

**Goal**: Users can browse, filter, search, preview, download, edit, replace, and delete only documents they are authorized to access.

**Independent Test**: Seed documents across users, categories, projects, and dates; verify all retrieval and management operations plus denied-access behavior for every role.

### Tests for User Story 2

- [x] T022 [P] [US2] Add authorized query, sorting, filtering, search, paging, and unauthorized-result exclusion tests in `ContosoDashboard.Tests/DocumentQueryTests.cs`
- [x] T023 [P] [US2] Add preview/download, metadata-edit, replacement rollback, permanent-delete, and audit tests in `ContosoDashboard.Tests/DocumentManagementTests.cs`

### Implementation for User Story 2

- [x] T024 [US2] Implement authorized browse, filter, sort, search, project, task, and Shared with Me queries in `ContosoDashboard/Services/DocumentService.cs`
- [x] T025 [US2] Implement authorized download and preview stream operations with download activity records in `ContosoDashboard/Services/DocumentService.cs`
- [x] T026 [US2] Implement metadata updates, replacement-file transaction/cleanup, confirmation-gated deletion, and activity retention queries in `ContosoDashboard/Services/DocumentService.cs`
- [x] T027 [US2] Complete `ContosoDashboard/Pages/Documents.razor` with server-side query controls, accessible document table, sorting, filtering, search, and Shared with Me view
- [x] T028 [US2] Create `ContosoDashboard/Pages/DocumentDetails.razor` with authorization-aware preview/download, metadata editing, replacement, deletion, and error states
- [x] T029 [US2] Add authorized document download/preview endpoint handling in `ContosoDashboard/Program.cs` without serving storage paths directly
- [ ] T030 [US2] Run the User Story 2 independent validation from `specs/001-document-upload-management/quickstart.md` and record performance and IDOR evidence

**Checkpoint**: User Stories 1 and 2 are independently functional and secure.

---

## Phase 5: User Story 3 - Collaborate Through Projects, Tasks, and Sharing (Priority: P2)

**Goal**: Authorized project/task users can access contextual documents, share with users or existing departments/teams, and receive retryable notifications.

**Independent Test**: Exercise project membership changes, task association, direct shares, department/team shares, notification failure, and management scopes for all roles.

### Tests for User Story 3

- [x] T031 [P] [US3] Add dynamic project-membership, direct-share precedence, department/team-share, and role-scope tests in `ContosoDashboard.Tests/DocumentAuthorizationTests.cs`
- [x] T032 [P] [US3] Add project/task association, notification creation, notification failure retry, and duplicate-share tests in `ContosoDashboard.Tests/DocumentSharingTests.cs`

### Implementation for User Story 3

- [x] T033 [US3] Implement operation-time authorization predicates and share management in `ContosoDashboard/Services/DocumentService.cs`, including membership removal and direct-share precedence
- [x] T034 [US3] Implement project-member notification fan-out and retryable share notification behavior in `ContosoDashboard/Services/NotificationService.cs`
- [x] T035 [US3] Add project document listing, upload entry point, and document summary to `ContosoDashboard/Pages/ProjectDetails.razor`
- [x] T036 [US3] Add task document listing and upload/attach entry point to `ContosoDashboard/Pages/Tasks.razor`
- [x] T037 [US3] Add share-user, share-department/team, revoke, recipient notification, and Shared with Me controls to `ContosoDashboard/Pages/DocumentDetails.razor` and `ContosoDashboard/Pages/Documents.razor`
- [ ] T038 [US3] Run the User Story 3 independent validation from `specs/001-document-upload-management/quickstart.md` and record authorization and retry evidence

**Checkpoint**: Project, task, sharing, and notification collaboration flows are independently testable.

---

## Phase 6: User Story 4 - Monitor Document Activity (Priority: P3)

**Goal**: Employees see recent uploads and document counts on the dashboard, while administrators can review activity and generate reports.

**Independent Test**: Seed recent documents and historical activity, verify dashboard data, audit events, reports, retention, and administrator-only access.

### Tests for User Story 4

- [x] T039 [P] [US4] Add dashboard recent-document/count and administrator-report authorization tests in `ContosoDashboard.Tests/DocumentDashboardTests.cs`
- [x] T040 [P] [US4] Add audit action coverage, 12-month retention boundary, and report aggregate tests in `ContosoDashboard.Tests/DocumentAuditTests.cs`

### Implementation for User Story 4

- [x] T041 [US4] Extend `ContosoDashboard/Services/DashboardService.cs` and `DashboardSummary` with the current user's document count and five latest uploads
- [x] T042 [US4] Add Recent Documents widget and document count summary card to `ContosoDashboard/Pages/Index.razor`
- [x] T043 [US4] Add administrator-only activity/report queries to `ContosoDashboard/Services/DocumentService.cs`
- [x] T044 [US4] Create `ContosoDashboard/Pages/DocumentReports.razor` with document-type, uploader, and access-pattern reports and denied non-administrator state
- [x] T045 [US4] Add administrator navigation for reports in `ContosoDashboard/Shared/NavMenu.razor`
- [ ] T046 [US4] Run the User Story 4 independent validation from `specs/001-document-upload-management/quickstart.md` and record audit/report evidence

**Checkpoint**: Dashboard visibility and administrator audit/reporting are complete.

---

## Phase 7: Polish and Cross-Cutting Validation

**Purpose**: Verify the whole feature against the specification, constitution, performance targets, and training limitations.

- [x] T047 [P] Add feature-specific styles and responsive states in `ContosoDashboard/wwwroot/css/site.css`
- [x] T048 [P] Add user-facing setup and training-only storage/scanner limitations to `README.md`
- [x] T049 Run the complete authorization/IDOR, cleanup, retry, and no-public-path test suite in `ContosoDashboard.Tests/`
- [x] T050 Run `dotnet build` and resolve feature-related compiler or analyzer errors in `ContosoDashboard/ContosoDashboard.csproj`
- [ ] T051 Execute all scenarios in `specs/001-document-upload-management/quickstart.md` against a clean local database and document results
- [x] T052 Review the implementation against `specs/001-document-upload-management/spec.md`, `plan.md`, and `constitution.md`; record any justified deviations before release

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies; T001-T004 can begin immediately in parallel where files do not overlap.
- **Foundational (Phase 2)**: Depends on Setup; T005-T014 block all user stories.
- **User Story 1 (Phase 3)**: Depends on Foundational; delivers the MVP upload slice.
- **User Story 2 (Phase 4)**: Depends on Foundational and the `IDocumentService` created in US1; retrieval can begin after T017, while UI tasks build on the shared service.
- **User Story 3 (Phase 5)**: Depends on Foundational and document records/service behavior from US1/US2.
- **User Story 4 (Phase 6)**: Depends on audit behavior from US1-US3 and dashboard integration points.
- **Polish (Phase 7)**: Depends on all selected user stories.

### User Story Dependencies

- **US1 (P1)**: Foundational only; independently delivers the MVP.
- **US2 (P1)**: Foundational plus the shared document service from US1; independently testable with seeded data.
- **US3 (P2)**: Depends on document persistence and authorization from US1/US2; adds collaboration behavior.
- **US4 (P3)**: Depends on audit records and document queries from US1-US3; adds reporting and dashboard visibility.

### Parallel Opportunities

- T002-T004 can run in parallel after setup scope is agreed.
- T006, T007, T010, T011, and T014 can run in parallel after the project structure exists.
- Within US1, T015 and T016 can run in parallel; UI work begins after the service contract is stable.
- Within US2, T022 and T023 can run in parallel; query, download, and UI work can be split after T017.
- Within US3, T031 and T032 can run in parallel; project, task, and sharing UI tasks can be split after service contracts stabilize.
- Within US4, T039 and T040 can run in parallel; dashboard and reporting UI can be developed separately.
- T047-T049 can run in parallel after feature behavior is complete.

## Parallel Example: User Story 1

```text
Task T015: Upload behavior tests in ContosoDashboard.Tests/DocumentUploadTests.cs
Task T016: Upload authorization tests in ContosoDashboard.Tests/DocumentUploadAuthorizationTests.cs
```

## Parallel Example: User Story 3

```text
Task T031: Authorization and share-scope tests in ContosoDashboard.Tests/DocumentAuthorizationTests.cs
Task T032: Sharing and notification tests in ContosoDashboard.Tests/DocumentSharingTests.cs
Task T035: Project document integration in ContosoDashboard/Pages/ProjectDetails.razor
Task T036: Task document integration in ContosoDashboard/Pages/Tasks.razor
```

## Implementation Strategy

### MVP First

1. Complete Phase 1 setup and Phase 2 foundational storage, scanning, validation, and data work.
2. Complete User Story 1, including focused upload and authorization tests.
3. Stop and validate the upload journey independently using `quickstart.md`.
4. Demo the secure offline upload slice before adding retrieval or collaboration.

### Incremental Delivery

1. Add User Story 2 for secure retrieval and document management.
2. Add User Story 3 for project/task context, sharing, and notifications.
3. Add User Story 4 for dashboard visibility, audit, and reporting.
4. Complete cross-cutting validation, performance checks, documentation, and release review.

### Parallel Team Strategy

1. Complete Setup and Foundational phases together because they define shared contracts.
2. After the foundational checkpoint, assign one developer to upload/storage, one to retrieval/UI, and one to authorization/sharing.
3. Start dashboard/reporting after audit contracts stabilize.
4. Integrate each story at its checkpoint and run its independent validation before proceeding.
