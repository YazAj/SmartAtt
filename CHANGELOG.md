# Changelog

## 2026-07-26

### Sprint 7 Added

- Added role-scoped attendance reporting for Students, Instructors, and Admin users.
- Added server-side report filters, sorting, pagination, summary metrics, and the authoritative attendance percentage calculation.
- Added derived `Missed` report rows for eligible completed attendance-enabled sessions without persisted absence records.
- Added safe CSV export for Student, Instructor, Admin attendance reports, and Admin audit search.
- Added CSV formula-injection neutralization, delimiter/quote/newline escaping, UTF-8 BOM output, safe export filenames, and no-store/private export response headers.
- Added Admin safe audit search over existing lecture-session events, attendance attempts, face-verification attempts, and face-enrollment events.
- Added English and Arabic localization resources and report UI styles using the existing RTL/LTR and dark/light design tokens.
- Added Sprint 7 unit and integration tests for reporting semantics, export safety, audit safety, and route authorization.
- Added Sprint 7 documentation files `docs/47` through `docs/53`.

### Sprint 7 Verification

- Marked Sprint 7 Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence.
- Verified no Sprint 7 migration was required; reports project existing Sprint 2-6 tables.
- Verified SQL Server LocalDB database `AttendAI_Sprint7Verification_20260726` with all six existing migrations through `AddSecureAttendanceCheckIn`.
- Verified seeded Admin, Instructor, and Student roles in SQL Server.
- Verified runtime Admin smoke with environment-only demo Admin credentials: startup, home, login, Admin dashboard, Admin reports, Admin audit, Admin CSV export no-store headers, Admin denial from Student report, privacy, 404, English LTR HTML attributes, and Arabic RTL HTML attributes.
- Verified automated tests after Sprint 7 implementation: 158 total, 158 passed, 0 failed, 0 skipped.
- Verified build after Sprint 7 implementation: `dotnet build AttendAI.sln` exited 0 with 0 warnings and 0 errors.
- Verified format check passed.
- Recorded project-owner-confirmed authorized manual runtime evidence for Admin, Instructor, and Student reports; cross-role isolation; filters; pagination; report calculations; safe audit search; CSV export; print-friendly views; English/Arabic; LTR/RTL; light/dark themes; desktop/tablet/390px/320px layouts; keyboard navigation; visible focus; semantic report tables; no horizontal overflow; readable dark-mode text; long Arabic wrapping; and Sprint 1-6 regression flows.
- Recorded project-owner-confirmed CSV evidence for Arabic Unicode, English text, comma/quote/newline escaping, formula-injection protection for `=`, `+`, `-`, `@`, tab, and carriage return, private/no-store behavior, and no committed generated CSV files.
- Security scan found no tracked real credential, private key, SDK license, biometric image/sample, model binary, database file, user secret, generated report export, or build output.
- AttendAI is ready for final academic graduation-project acceptance, subject to the documented academic and security limitations.
- Liveness, anti-spoofing, tamper-proof browser location, device attestation, GPS anti-spoofing, one-to-many identification, immutable audit ledger, public report links, generated server PDF export, cloud deployment, and production biometric certification remain not implemented and not claimed.

### Sprint 6 Added

- Added secure Student attendance check-in for active enrolled lecture sessions.
- Added `AttendanceChallenge`, `AttendanceAttempt`, and `AttendanceRecord` domain entities, enums, EF Core mappings, and migration `20260726173403_AddSecureAttendanceCheckIn`.
- Added lecture-session attendance policy snapshots for coordinates, radius, maximum accepted browser accuracy, and attendance verification requirements.
- Added one-time hashed challenge tokens, hashed idempotency keys, server-side duplicate protection, replay handling, and transactional attendance persistence.
- Added server-side Haversine geofence validation using browser latitude/longitude/accuracy without retaining exact submitted Student coordinates.
- Added Real face-engine gate for attendance and one-to-one verification purpose `FutureAttendance`.
- Added Student attendance index/check-in/result pages and Instructor attendance roster.
- Added browser geolocation capture helper and localized English/Arabic attendance resources.
- Added unit and integration tests for attendance domain rules, location policy, successful check-in, duplicate prevention, different-person rejection, outside-geofence rejection, challenge replay, real-engine gating, and endpoint authorization.
- Added Sprint 6 documentation files `docs/41` through `docs/46`.

### Sprint 6 Verification

- Marked Sprint 6 Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence.
- Verified implementation build: `dotnet build AttendAI.sln` exited 0 with 0 warnings and 0 errors.
- Verified automated tests during closure: `dotnet test AttendAI.sln` exited 0 with 142 total, 142 passed, 0 failed, 0 skipped.
- Verified formatting: `dotnet format AttendAI.sln --verify-no-changes` exited 0.
- Verified SQL Server LocalDB database `AttendAI_Sprint6Verification_20260726` with all 6 migrations through `AddSecureAttendanceCheckIn`, attendance tables, unique attendance indexes, lecture-session policy snapshot columns, and seeded Admin/Instructor/Student roles.
- Verified runtime smoke against SQL Server using environment-only demo Admin credentials: app startup, HTTPS home/login/privacy/access-denied/404/error routes, Admin login, Admin dashboard, Admin blocked from Student dashboard, change password, and profile.
- Recorded user-confirmed authorized manual verification: successful real same-person attendance, different-person/no-face/multiple-face/poor-quality rejections, inside-geofence acceptance, outside-geofence/inaccurate-location/permission-denied handling, closed-session rejection, duplicate and repeated-submit handling, challenge and idempotency behavior, restart persistence, Instructor roster update, camera cleanup, client-side location-state cleanup, no raw-image retention, no exact-coordinate retention by default, and no attendance side effect from Sprint 5 standalone verification.
- Recorded user-confirmed UI verification: English, Arabic, LTR, RTL, light mode, dark mode, desktop, mobile/responsive layout, usable camera controls, readable long validation messages, no page-level horizontal overflow, visible dark-mode text, and loading state preventing accidental duplicate submission.
- Recorded location-evidence limitation: physical-versus-simulated/manual browser location method was not recorded in closure evidence.
- Security scan found no tracked real credential, private key, SDK license, biometric image/sample, model binary, database file, user secret, or build output.
- Remaining limitations: liveness detection, anti-spoofing certification, complete replay-attack prevention, tamper-proof browser geolocation, device attestation, GPS anti-spoofing, production biometric certification, production fraud prevention, population-wide face-threshold calibration, one-to-many face identification, and classroom surveillance are not implemented or claimed.

### Real Face Engine Added

- Added `OpenCvSFaceRecognitionEngine` for `FaceRecognition:Provider=Real`.
- Added OpenCvSharp YuNet detection with five-landmark validation, SFace-compatible alignment, OpenCV-style blob preprocessing, ONNX Runtime SFace embedding inference, L2-normalized float32 little-endian templates, and cosine comparison.
- Added `RealFaceRecognitionModelStore`, safe model path resolution, SHA-256 validation, Git LFS pointer rejection, native/model readiness checks, and actual ONNX input/output metadata inspection.
- Added `scripts/setup-face-models.ps1` for official OpenCV Zoo model provisioning.
- Added `experiments/AttendAI.RealFaceEngine.Poc` for model-only readiness and approved local sample evaluation.
- Added unit tests for model path safety, missing/corrupt/pointer/hash model failures, invalid image handling, YuNet row interpretation, template codec safety, Fake-to-Real incompatibility, real metadata compatibility, threshold equality, and DI mode selection.
- Added docs 35 through 40 for the real-engine plan, architecture, test plan, review, model provisioning/licensing, and threshold evaluation.

### Real Face Engine Verified

- Verified model provisioning from official OpenCV Zoo URLs.
- Verified YuNet SHA-256 `8f2383e4dd3cfbb4553ea8718107fc0423210dc964f9f4280604804ed2552fa4`.
- Verified SFace SHA-256 `0ba9fbfa01b5270c96627c4ef784da859931e02f04419c829e83484087c34e79`.
- Verified model-only POC readiness on Windows x64: OpenCV `4.13.0`, SFace input `System.Single [1, 3, 112, 112]`, SFace output `System.Single [1, 128]`.
- Verified closure build after manual-evidence documentation update: `dotnet build AttendAI.sln` exited 0 with 0 warnings and 0 errors.
- Verified closure automated test run: `dotnet test AttendAI.sln` exited 0 with 119 total, 119 passed, 0 failed, 0 skipped.
- Verified real biometric re-enrollment was completed with Real mode.
- Verified the previous Fake template was replaced.
- Verified a protected OpenCV-SFace template was created.
- Verified same-person real verification returned Match with cosine similarity `0.950795`.
- Verified no raw biometric images were committed or intentionally retained.

### Real Face Engine Remaining Limitations

- Sprint 6 manual attendance closure now records project-owner-confirmed different-person, no-face, multiple-face, poor-quality, restart, camera-cleanup, and no-attendance-side-effect evidence for the secure attendance workflow.
- Threshold calibration beyond the documented `DevelopmentDefault` value remains a production-wide future gate.
- Liveness and anti-spoofing are not implemented and must not be claimed.

## 2026-07-22

### Sprint 5 Added

- Added one-to-one Student face verification against only the authenticated Student's active compatible template.
- Added append-only `FaceVerificationAttempt` safe metadata persistence and EF Core migration `20260722221335_AddFaceVerification`.
- Added verification outcomes, purpose, score metric policy, strongly typed options, eligibility, rate limiting, idempotency, diagnostics, template compatibility, and one-to-one verifier contracts.
- Added Infrastructure services for verification policy, diagnostics, compatibility, rate limiting, eligibility, query projections, attempt persistence, and template unprotection inside Infrastructure only.
- Added Student verification status, capture, result, and history pages with fake-mode demo warning.
- Added Admin face-verification overview, filters, details, diagnostics, and dashboard metrics.
- Added Sprint 5 unit and integration tests for domain rules, threshold decisions, compatibility checks, authorization, idempotency, match/no-match, and incompatible-template re-enrollment marking.
- Added Sprint 5 documentation set: plan, domain model, test plan, review, security, threshold calibration, and real-engine integration guide.

### Sprint 5 Changed

- Updated README, SRS, architecture, ERD, use cases, UI design notes, localization/theme documentation, face technical spike, traceability matrix, data dictionary, Sprint 4 review, and face-engine readiness gate.
- Updated navigation and dashboards with Student verification and Admin verification oversight links while leaving Instructor without verification management controls.
- Updated English and Arabic resources for verification statuses, diagnostics, validation messages, enum labels, page titles, buttons, table labels, and warnings.
- Updated shared camera JavaScript so verification capture stops camera tracks on submit and page unload.

### Sprint 5 Verified

- Verified SQL Server LocalDB database `AttendAI_Sprint5Verification_20260723` through all migrations including `AddFaceVerification`.
- Verified fake-development runtime smoke: Admin login, Student creation, Student forced password change, biometric consent, fake enrollment, match verification, no-match verification, rate-limit outcome, Student denial from Admin verification routes, and Admin safe attempt details.
- Verified automated tests: 94 total, 94 passed, 0 failed, 0 skipped during implementation after Sprint 5 coverage was added.
- Verified no attendance registration, location validation, attendance reports, exports, notifications, or one-to-many identification were implemented.
- Real face-engine runtime remains Not Verified because approved biometric samples, licensed models, production adapter, and native dependencies are not available.

### Sprint 4 Added

- Added biometric consent, protected face-template, and face-enrollment event domain entities and enums.
- Added biometric application DTOs, commands, queries, result models, and service interfaces.
- Added EF Core configurations and migration `20260722190900_AddBiometricEnrollment` for `BiometricConsents`, `StudentFaceTemplates`, and `FaceEnrollmentEvents`.
- Added fake/disabled/real engine mode readiness, production fake-engine blocking, capture validation, enrollment processing, Data Protection template protection, and enrollment rate limiting.
- Added Student biometric privacy notice, consent, enrollment status, camera capture wizard with upload fallback, re-enrollment, consent withdrawal, and safe history pages.
- Added Admin biometric oversight, filters, details, safe metadata, revoke, require re-enrollment, and audit history pages.
- Added Sprint 4 unit and integration tests for biometric domain rules, capture validation, template protection, enrollment processing, and authorization.
- Added Sprint 4 documentation: plan, domain model, test plan, review, biometric privacy/security, and face-engine readiness gate.

### Sprint 4 Changed

- Updated README, SRS, architecture, ERD, use cases, UI design-system notes, localization/theme documentation, face technical spike, traceability matrix, and data dictionary for biometric enrollment/profile management.
- Updated layout navigation and Admin/Student dashboards with biometric links and metrics.
- Updated localization resources for English LTR and Arabic RTL biometric workflows.
- Updated CSS and JavaScript with responsive camera capture, preview, capture counter, thumbnail strip, file fallback, and camera-track cleanup.

### Sprint 4 Verified

- Verified automated build with 0 warnings and 0 errors during implementation.
- Verified automated tests: 79 total, 79 passed, 0 failed, 0 skipped during implementation.
- Verified no Sprint 5 attendance submission, attendance decisions, face verification UI, or location validation were implemented.
- Real face-engine runtime remains Not Verified because approved biometric samples, licensed models, adapter, and native dependencies are not available.

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
