# Changelog

## 2026-07-22

### Sprint 3 Added

- Added lecture scheduling domain entities, session lifecycle entities, and enums.
- Added application contracts, DTOs, commands, queries, conflict results, session results, time-zone contracts, and code-security services for lecture scheduling.
- Added EF Core configurations and migration `20260722160127_AddLectureSchedulingAndSessions` for `LectureSchedules`, `LectureSessions`, and `LectureSessionEvents`.
- Added Admin lecture schedule CRUD, conflict validation, classroom/section timetable pages, active-session monitoring, session details, force-end, and stale-expiration actions.
- Added Instructor schedule and active-session workflows with start, temporary code display, copy code, regenerate, end, and cancel actions.
- Added Student schedule and active-session views that hide session-code material.
- Added secure temporary code generation, hashing, Data Protection storage, expiration, regeneration, invalidation, and lifecycle audit events.
- Added Sprint 3 unit and integration tests for lecture domain rules, time-zone behavior, code security, scheduling conflicts, authorization, session lifecycle, and Student code exclusion.
- Added Sprint 3 documentation: plan, lecture scheduling domain model, test plan, review, and session-code security.

### Sprint 3 Changed

- Updated README, SRS, architecture, ERD, use cases, UI design-system notes, localization/theme documentation, traceability matrix, and data dictionary for lecture scheduling/session management.
- Updated layout navigation and Admin/Instructor/Student dashboards with lecture schedule/session links.
- Updated localization resources for English LTR and Arabic RTL lecture workflows.
- Updated CSS and JavaScript with timetable, responsive lecture identifiers, countdown, and copy-code support.
- Closed the Arabic RTL dark mobile clipping gap by replacing the hidden mobile-sidebar translate transform with an opacity/visibility closed state and by wrapping long lecture identifiers, action rows, and session-code content safely.

### Sprint 3 Verified

- Verified SQL Server LocalDB database `AttendAI_Sprint3Verification_20260722` with all migrations through `AddLectureSchedulingAndSessions`.
- Verified runtime Admin schedule creation and conflict rejection, Instructor session start/regenerate/end flow, Student active-session code exclusion, and cross-role access denial.
- Verified SQL evidence for lecture tables, Admin/Instructor/Student roles, lifecycle event persistence, and terminal code invalidation.
- Verified live Playwright responsive/layout coverage across 1440x900, 1024x768, 768x1024, 390x844, and 320x568; 134 checks reported 0 page-level overflow failures.
- Verified Arabic RTL dark desktop/tablet/mobile, including 390px and 320px, plus English LTR light/dark regressions.
- Verified keyboard walkthrough for mobile navigation, filters/forms, timetable/session links, active-session actions, language selector, theme selector, and Escape close behavior.
- Verified automated restore/build/test/format gates; final command evidence is recorded in Sprint 3 review.
- Verified no Sprint 4 attendance records, student code submission, face enrollment, reports, notifications, or location validation were implemented.

### Sprint 2 Added

- Added academic domain entities for departments, students, instructors, courses, sections, classrooms, student enrollments, and instructor assignments.
- Added application contracts, DTOs, commands, paging/query models, dashboard summaries, and academic lookup services.
- Added EF Core configurations and migration `20260722140818_AddAcademicManagement` with SQL Server `rowversion`, unique indexes, foreign keys, check constraints, and restrict delete behavior.
- Added Admin area CRUD workflows for academic setup, account creation, activation/deactivation, temporary password reset, enrollment status changes, and primary instructor assignment.
- Added administrator-created Student and Instructor account flows with role assignment and required first-login password change.
- Added academic data to Admin, Instructor, and Student dashboards.
- Added Sprint 2 unit and integration tests for domain rules, authorization, account creation, first-login change password, account deactivation, enrollment capacity/duplicates, and primary instructor uniqueness.
- Added Sprint 2 documentation: plan, academic domain model, test plan, review, and data dictionary.

### Sprint 2 Changed

- Updated localization resources and layout navigation for academic management in English LTR and Arabic RTL.
- Tightened Admin area POST security with inherited antiforgery validation and explicit tokens in Admin forms.
- Tightened `.gitignore` so root face-recognition model assets remain ignored without hiding source `Models` folders.
- Updated README, SRS, architecture, ERD, use cases, UI design-system notes, and traceability matrix for Sprint 2.

### Sprint 2 Verified

- Verified SQL Server LocalDB database `AttendAI_Sprint2Closure_20260722` with both migrations, all academic tables, Identity roles, key indexes, foreign keys, and check constraints.
- Verified runtime Admin CRUD setup and Student/Instructor authentication flows against SQL Server over HTTPS.
- Verified English/Arabic, LTR/RTL, light/dark, desktop/mobile pages with browser screenshots and no horizontal overflow in checked viewports.
- Verified `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet format --verify-no-changes`.
- Verified no non-ignored credential, private key, SDK license, biometric sample, model file, database file, or build output is tracked.

### Changed

- Closed Sprint 1 verification with SQL Server LocalDB migration, runtime authentication, role dashboard, localization, theme, responsive UI, security, and documentation evidence.
- Updated the face-recognition technical spike to clarify that ONNX Runtime is an inference runtime and requires separate face-detection and embedding components.
- Replaced the README demo password example with a placeholder and expanded closure instructions.
- Tightened `.gitignore` so generated third-party license files are not accidentally staged.

### Added

- Created AttendAI Clean Architecture solution with Domain, Application, Infrastructure, Web, UnitTests, IntegrationTests, and FaceRecognition.Poc projects.
- Added ASP.NET Core Identity, SQL Server EF Core persistence, role constants, role seeding, and optional demo admin seeding through configuration.
- Added role-based Admin, Instructor, and Student dashboards with authorization boundaries.
- Added login, logout, change password, access denied, profile summary, privacy notice, error, and 404 pages.
- Added English and Arabic localization with RTL/LTR-aware layout.
- Added light, dark, and system theme support with localStorage persistence.
- Added `IFaceRecognitionEngine`, deterministic fake engine, and separate POC console harness.
- Added initial EF Core migration `InitialIdentityFoundation`.
- Added unit and integration tests for Sprint 1 foundation behavior.
- Added Sprint 1 documentation set and repository hygiene files.
