<!--
Sync Impact Report
- Version change: uninitialized template -> 1.0.0
- Modified principles: all five template principles replaced with ContosoDashboard
	training, security, testing, integration, and simplicity rules
- Added sections: Training and Technology Constraints; Development Workflow and
	Quality Gates
- Removed sections: none
- Follow-up TODO: confirm the original ratification date
-->

# ContosoDashboard Constitution

## Core Principles

### I. Training-First Scope
ContosoDashboard MUST remain an educational, offline-first sample. Features and
architectural decisions MUST be understandable in a training context, MUST avoid
production claims, and MUST preserve the documented mock-authentication and local
data limitations. Production-only infrastructure MUST NOT be introduced without a
clear educational purpose and documented local fallback.

### II. Layered Design and Replaceable Infrastructure
Application behavior MUST remain separated across pages, services, models, and data
access. Infrastructure dependencies MUST be accessed through suitable abstractions
when a local implementation may later be replaced by a cloud implementation.
Business logic MUST NOT depend directly on Azure or other external providers.

### III. Security by Default
Every protected page and service operation MUST enforce authentication, authorization,
and user-data isolation at the appropriate boundary. Changes involving identity,
roles, object access, file handling, or user input MUST include a security-focused
test or documented verification. Mock authentication MUST remain explicitly labeled
as training-only and MUST NOT be presented as production identity security.

### IV. Testable Behavior and Integration Checks
New or changed behavior MUST have focused tests or an executable verification path.
Changes crossing page, service, database, authentication, or shared-model boundaries
MUST include an integration check covering the contract that changed. Tests MUST
prioritize authorization, user isolation, persistence, and user-visible workflows
because these are the highest-risk behaviors in this application.

### V. Simplicity and Traceable Change
The smallest design that satisfies the documented training goal MUST be preferred.
New dependencies, abstractions, and configuration MUST have a concrete rationale.
Changes MUST preserve existing public behavior unless the change explicitly documents
the affected contract, migration impact, and verification evidence.

## Training and Technology Constraints

The application MUST support local, offline development with the repository's
ASP.NET Core and Blazor Server stack. Documentation MUST distinguish training
shortcuts from production requirements. Security-sensitive documentation MUST call
out the need for a real identity provider, password hashing, MFA, TLS, audit
logging, and applicable accessibility and compliance work before production use.
Database and file-storage choices MUST retain a migration path through configuration
or replaceable implementations where practical.

## Development Workflow and Quality Gates

Each feature MUST begin with a clear specification of user-visible behavior and
acceptance checks. Implementation changes MUST be reviewed for constitution
compliance, authorization impact, data isolation, and regression risk. Before a
change is considered complete, the project MUST pass the narrowest relevant build,
test, or executable verification available; documentation MUST be updated when
behavior, limitations, or setup steps change.

## Governance
<!-- Example: Constitution supersedes all other practices; Amendments require documentation, approval, migration plan -->

This constitution governs project design and development decisions. Amendments MUST
be proposed with the reason, affected principles, compatibility impact, and required
follow-up work. A change requires maintainer approval through the repository review
process before it is adopted. Every review MUST check compliance with the principles
and quality gates, and any justified exception MUST be documented with an owner and
an expiry or removal condition.

Constitution versions use semantic versioning: MAJOR for incompatible governance
changes or removals, MINOR for new or materially expanded principles or sections,
and PATCH for clarifications and non-semantic corrections. The last amended date
MUST be updated for every adopted change. Compliance MUST be reviewed during feature
planning, implementation review, and release readiness checks.

**Version**: 1.0.0 | **Ratified**: TODO(RATIFICATION_DATE): confirm original adoption date | **Last Amended**: 2026-09-09
