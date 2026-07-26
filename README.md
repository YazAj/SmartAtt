# AttendAI

AttendAI is a responsive university attendance management web platform for the graduation project **Smart Attendance Management System Using Face Recognition**.

Sprint 1 established the buildable foundation. Sprint 2 adds academic and account management: departments, students, instructors, courses, sections, classrooms, enrollments, instructor assignments, administrator-created accounts, first-login password changes, and role-aware academic dashboards. Sprint 3 adds weekly lecture schedules, schedule conflict detection, instructor/student timetables, live lecture sessions, secure temporary session codes, and session lifecycle audit history. Sprint 4 adds biometric consent, face enrollment profile management, protected template storage, Admin biometric oversight, and the real-engine readiness gate. Sprint 5 adds secure one-to-one Student self-verification against the authenticated Student's active compatible template, safe attempt metadata, Admin verification oversight, diagnostics, rate limiting, and threshold policy. Sprint 6 adds secure biometric attendance check-in for active lecture sessions using real-mode one-to-one face verification, browser geolocation, server-side geofence validation, idempotency, one-time challenges, duplicate protection, safe attempt history, and Instructor roster visibility. Sprint 7 adds role-scoped attendance reports, server-side analytics summaries, safe audit search, CSV export, and final release-hardening documentation.

## Current Sprint Status

Sprint 7 is Partially completed. The reporting, analytics, safe audit, CSV export, SQL Server startup, Admin runtime smoke, documentation, and automated regression suite are implemented and verified. Sprint 7 is not marked Completed because the full manual visual/accessibility matrix and live SQL-backed Student/Instructor report walkthroughs still require project-owner evidence.

Sprint 6 remains Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence. Secure biometric attendance check-in is available for active enrolled lecture sessions using real one-to-one OpenCV YuNet/SFace verification, server-side geofence validation, attendance challenges, idempotency, database duplicate protection, safe attempt history, and Instructor roster visibility. Fake mode remains a deterministic development/demo engine only and cannot create Sprint 6 attendance records. Liveness, anti-spoofing, device attestation, tamper-proof browser geolocation, and complete replay-attack prevention are not implemented or claimed.

## Technology Stack

- .NET 8, ASP.NET Core 8 MVC, Razor Views
- ASP.NET Core Identity with cookie authentication
- Entity Framework Core with SQL Server provider
- Bootstrap 5, CSS custom properties, JavaScript
- Built-in localization for `en-US` and `ar-JO`
- xUnit, ASP.NET Core integration testing, SQLite in-memory test database
- OpenCvSharp4 and ONNX Runtime for optional local Real face-engine mode

## Architecture Summary

```text
src/AttendAI.Domain          Core domain abstractions, academic/lecture/biometric/verification/attendance entities, and enums
src/AttendAI.Application     Contracts, academic/lecture/biometric/verification/attendance/reporting DTOs/services, role routing, safe redirects, face engine abstraction
src/AttendAI.Infrastructure  EF Core, Identity persistence, academic/lecture/biometric/verification/attendance/reporting services, seeding, fake/real/disabled face engines
src/AttendAI.Web             MVC controllers, Admin area, lecture/biometric/verification/attendance/reporting UI, Razor views, localization, theming
tests/                       Unit and integration tests
experiments/                 Face-recognition POC console harness
docs/                        Sprint documentation and verification evidence
```

Dependencies point inward: Web uses Infrastructure and Application; Infrastructure uses Application and Domain; Application uses Domain; Domain has no framework dependency.

## Prerequisites

- .NET SDK `8.0.423` or compatible .NET 8 SDK
- SQL Server LocalDB or SQL Server for local development
- Optional: approved local face images for manual POC verification

Restore local tools and packages:

```bash
dotnet tool restore
dotnet restore AttendAI.sln
```

## Database Setup

The default development connection string targets LocalDB:

```json
"Server=(localdb)\\MSSQLLocalDB;Database=AttendAI;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

Override it with User Secrets or environment variables for local machines:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=AttendAI;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project src/AttendAI.Web
```

Apply migrations:

```bash
dotnet tool run dotnet-ef database update --project src/AttendAI.Infrastructure --startup-project src/AttendAI.Web --context ApplicationDbContext
```

The current migration set is:

- `20260722120650_InitialIdentityFoundation`
- `20260722140818_AddAcademicManagement`
- `20260722160127_AddLectureSchedulingAndSessions`
- `20260722190900_AddBiometricEnrollment`
- `20260722221335_AddFaceVerification`
- `20260726173403_AddSecureAttendanceCheckIn`

Sprint 7 did not add a migration.

## Demo Admin Setup

Roles are seeded by the application initializer. A demo administrator is created only when configured through User Secrets or environment variables:

```bash
dotnet user-secrets set "DemoAdmin:Email" "admin@example.edu" --project src/AttendAI.Web
dotnet user-secrets set "DemoAdmin:Password" "<generate-a-strong-local-password>" --project src/AttendAI.Web
dotnet user-secrets set "DemoAdmin:FullName" "Demo Administrator" --project src/AttendAI.Web
```

Do not commit real credentials.

## Run

```bash
dotnet run --project src/AttendAI.Web
```

The first non-test startup applies migrations and seeds Identity roles. Use the HTTPS URL printed by ASP.NET Core. Admin users can create Student and Instructor accounts with temporary passwords; those users must change password on first login. Admins can then create lecture schedules from active sections, assigned instructors, and classrooms. Instructors can start eligible lecture sessions and receive temporary codes only in the authorized response. Students can view active session state for enrolled sections but cannot see session codes. Students can also manage biometric consent, enrollment status, one-to-one self-verification, and attendance check-in for active enrolled sessions. Instructors can view the safe attendance roster for their active sessions. Admins can review biometric status, verification diagnostics, safe verification metadata, attendance reports, and safe audit search.

## Biometric Enrollment

Sprint 4 supports biometric profile management only:

- Student privacy notice and explicit consent.
- Camera capture wizard with approved file-upload fallback.
- Configurable capture count, MIME, size, dimension, quality, and rate-limit settings under `BiometricEnrollment`.
- Protected template storage through ASP.NET Core Data Protection.
- Student re-enrollment and consent withdrawal.
- Admin safe metadata view, revocation, and require re-enrollment.

The default `FaceRecognition:Provider` is `Fake` for development and automated tests. Fake mode is blocked in Production by the readiness service. `Real` mode resolves `OpenCvSFaceRecognitionEngine`, requires local ignored OpenCV Zoo YuNet/SFace models, rejects Fake templates as incompatible, and was used in project-owner-confirmed Sprint 6 one-to-one attendance verification. Broader production threshold calibration remains a future gate.

## Face Verification

Sprint 5 supports Student self-verification only:

- The Student is resolved from the authenticated Identity user; no browser-posted StudentId is trusted.
- Active account, completed first password change, active Student profile, active biometric consent, active compatible template, enabled engine, request validation, cooldown, and rate limits are required.
- Capture validation checks MIME, field name, file size, request size, PNG/JPEG signature, image dimensions, exactly-one-face outcome, and quality outcome through the configured engine path.
- Infrastructure loads and unprotects only the current Student's active template, compares the capture one-to-one, applies the configured score metric and threshold, zeroes unprotected template buffers where practical, and persists a safe `FaceVerificationAttempt`.
- Student pages display status, camera capture, result, and paged history. Admin pages display overview, filters, safe attempt details, diagnostics, and real-engine readiness status.
- Captured verification images, embeddings, protected templates, template fingerprints, session codes, attendance status, and location data are not stored in verification attempts.

Fake mode is visibly labeled as development/demo verification. Real mode can load the configured local models but remains a partial gate until `docs/34-Real-Face-Engine-Integration.md`, `docs/37-Real-Face-Engine-Test-Plan.md`, and `docs/40-Real-Threshold-Evaluation.md` are satisfied with approved samples.

## Attendance Check-In

Sprint 6 supports secure Student check-in for active lecture sessions:

- The Student is resolved from the authenticated Identity user.
- The Student must have an active account/profile, completed first password change, and active enrollment in the lecture section.
- The lecture session must be active and inside the attendance window.
- A one-time hashed attendance challenge and hashed idempotency key protect replay and repeated submits.
- Attendance requires real-mode face diagnostics; Fake/demo mode is blocked for attendance decisions.
- Browser geolocation is evaluated server-side against the lecture-session policy snapshot.
- Successful check-in creates one `AttendanceRecord` per Student/session and stores only safe metadata.
- Rejected attempts are stored as safe `AttendanceAttempt` rows where a Student/session can be resolved.
- Instructor roster shows enrolled Students, Present/Late/Missing state, and safe attempt metadata.

Attendance does not store raw images, exact submitted Student coordinates by default, plain challenge tokens, embeddings, protected template bytes, template fingerprints, reports, exports, notifications, or liveness decisions. Attendance challenges, idempotency, rate limiting, and unique database constraints reduce duplicate and basic replay risks; they do not make the browser, camera, or location signal tamper-proof.

## Reporting, Analytics, Audit, And Export

Sprint 7 reports are projections over existing Sprint 2-6 tables. No new migration, report snapshot table, persisted absence row, or generated report-file storage was added.

- Students can view and export only their own attendance report.
- Instructors can view and export only attendance for their owned sessions.
- Admins can view global attendance reports, safe attempt summaries, safe audit search, and CSV exports.
- `Missed` is derived only for eligible completed attendance-enabled sessions where an active enrolled Student has no `AttendanceRecord`.
- Attendance percentage is `(Present + Late) / Eligible completed sessions * 100`, rounded to two decimals away from zero.
- CSV exports use UTF-8 with BOM, no-store/private response headers, delimiter/quote/newline escaping, and spreadsheet formula neutralization.

Reports and exports do not expose raw biometric images, embeddings, protected template bytes, template fingerprints, exact submitted Student coordinates, challenge hashes, idempotency hashes, model paths, native exception details, passwords, or password hashes.

## Real Face Engine Setup

Model binaries are not committed. To provision the official OpenCV Zoo models into the ignored repository `models/face-recognition` folder:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\setup-face-models.ps1
dotnet run --project experiments\AttendAI.RealFaceEngine.Poc -- --models-only
```

Verified model-only readiness uses:

- YuNet `face_detection_yunet_2023mar.onnx`, SHA-256 `8f2383e4dd3cfbb4553ea8718107fc0423210dc964f9f4280604804ed2552fa4`.
- SFace `face_recognition_sface_2021dec.onnx`, SHA-256 `0ba9fbfa01b5270c96627c4ef784da859931e02f04419c829e83484087c34e79`.
- SFace input `System.Single [1, 3, 112, 112]`.
- SFace output `System.Single [1, 128]`.

To enable Real mode locally, use User Secrets or environment variables rather than editing tracked credentials or local paths:

```powershell
dotnet user-secrets set "FaceRecognition:Provider" "Real" --project src/AttendAI.Web
```

## Tests and Quality

```bash
dotnet restore AttendAI.sln
dotnet build AttendAI.sln
dotnet test AttendAI.sln
dotnet format AttendAI.sln --verify-no-changes
```

## Localization

Supported cultures:

- `en-US`
- `ar-JO`

Use the language selector in the top bar. The selected culture is persisted in the ASP.NET Core culture cookie. Arabic renders with `dir="rtl"` and English with `dir="ltr"`.

## Theme System

The theme toggle cycles through:

- System preference
- Light
- Dark

The preference is stored in `localStorage` as `attendai-theme`, and the layout applies it before CSS loads to reduce theme flashing.

## Face POC And Engine Gate

The production app depends only on `IFaceRecognitionEngine`. Sprint 1 includes a deterministic fake engine and a console harness:

```bash
dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <image-path>
dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <reference-image-path> <probe-image-path>
```

No personal biometric images are committed. To test negative fake-engine paths, use a local file containing `NO_FACE` or `MULTI_FACE`. For Real model-only readiness, use `experiments/AttendAI.RealFaceEngine.Poc`. Sprint 6 attendance closure records project-owner-confirmed authorized manual real one-to-one verification; broader production calibration remains a separate future gate.

## Sprint 1 Closure Evidence

- SQL Server LocalDB verification database: `AttendAI_Sprint1Closure_20260722`.
- Migration applied: `20260722120650_InitialIdentityFoundation`.
- Runtime pages verified with Edge/Playwright against `https://localhost:7052`.
- Role logins verified for Admin, Instructor, and Student verification users.
- Dark/light and English/Arabic RTL/LTR screenshots captured under ignored `logs/ui-verification/`.
- Secret scan found no non-ignored credentials, private keys, SDK licenses, biometric samples, or model files.

## Sprint 2 Closure Evidence

- SQL Server LocalDB verification database: `AttendAI_Sprint2Closure_20260722`.
- Migrations verified: `InitialIdentityFoundation` and `AddAcademicManagement`.
- Runtime Admin flow created one department, course, classroom, section, student, instructor, enrollment, and instructor assignment.
- Student and Instructor temporary-password login redirected to change password, then reached the correct dashboards.
- Cross-role dashboard access redirected to Access Denied.
- Screenshots were captured under ignored `logs/ui-verification-sprint2/` for English LTR light/dark and Arabic RTL light/dark across desktop and mobile.
- Secret scan found no non-ignored credentials, private keys, SDK licenses, biometric samples, database files, or model files.

## Sprint 3 Verification Evidence

- SQL Server LocalDB verification database: `AttendAI_Sprint3Verification_20260722`.
- Migrations verified: `InitialIdentityFoundation`, `AddAcademicManagement`, and `AddLectureSchedulingAndSessions`.
- Runtime Admin flow created academic setup data, a valid lecture schedule, and rejected a conflicting lecture schedule.
- Instructor temporary-password login redirected to Change Password, then the Instructor opened the schedule, started a Session, saw the temporary code, regenerated it, and ended the Session.
- Student temporary-password login redirected to Change Password, then the Student opened the active Session and did not receive the code.
- SQL smoke evidence confirmed 3 lecture tables, Admin/Instructor/Student roles, terminal code invalidation, and lifecycle events.
- Runtime smoke screenshots were captured under ignored `artifacts/verification/sprint3-visual/`.
- Live Playwright layout verification covered Admin lecture schedules, create/edit/details, classroom/section timetables, active sessions, session details, Instructor schedule/active session, Student schedule/active session, and a redacted Instructor code-card response.
- Viewports verified: 1440x900, 1024x768, 768x1024, 390x844, and 320x568.
- Culture/theme combinations verified: English LTR Light desktop, English LTR Dark mobile, and Arabic RTL Light/Dark desktop, tablet, 390px mobile, and 320px mobile.
- Programmatic overflow result: 134 checks, 0 page-level overflow failures, including Arabic RTL light/dark tablet, Arabic RTL light/dark 390px, and Arabic RTL light/dark 320px.
- Keyboard walkthrough covered mobile navigation, language selector, theme selector, schedule filters/forms, timetable/session links, active-session actions, and Escape close behavior.
- Secret scan found no non-ignored credentials, private keys, SDK licenses, biometric samples, database files, model files, build outputs, or session-code values.

## Sprint 4 Verification Evidence

- Migration created: `20260722190900_AddBiometricEnrollment`.
- Automated tests: 79 total, 79 passed, 0 failed, 0 skipped.
- Unit coverage includes biometric domain rules, capture validation, template protection, and fake enrollment processing.
- Integration coverage includes Student/Admin/Instructor/anonymous biometric authorization boundaries.
- Real face-engine gate: Not Verified.

## Sprint 5 Verification Evidence

- Migration created and applied to LocalDB: `20260722221335_AddFaceVerification`.
- SQL Server verification database: `AttendAI_Sprint5Verification_20260723`.
- Runtime smoke used environment-supplied admin credentials only, created a Student through Admin UI, completed forced password change, accepted biometric consent, enrolled using fake-development captures, ran one match and one no-match self-verification, demonstrated rate limiting, and verified Admin safe attempt details.
- Automated tests: 94 total, 94 passed, 0 failed, 0 skipped after adding Sprint 5 coverage.
- Real face-engine gate: Not Verified.

## Real Face Engine Model Readiness Evidence

- Model provisioning script: `scripts/setup-face-models.ps1`.
- Model-only POC: `dotnet run --project experiments\AttendAI.RealFaceEngine.Poc -- --models-only`.
- Result: Ready, with gate `Runtime Ready - Local Sample Verification Pending`.
- OpenCV version: `4.13.0`.
- SFace input/output: `System.Single [1, 3, 112, 112]` and `System.Single [1, 128]`.
- Automated tests after integration: 119 total, 119 passed, 0 failed, 0 skipped in the latest no-build test run.
- Authorized Sprint 6 live-camera attendance evidence: project-owner-confirmed successful same-person Match and rejection scenarios. Broader production-wide calibration remains a future gate.

## Sprint 6 Verification Evidence

- SQL Server LocalDB verification database: `AttendAI_Sprint6Verification_20260726`.
- Migrations verified through `20260726173403_AddSecureAttendanceCheckIn`.
- Attendance tables, unique attendance indexes, lecture-session attendance policy snapshot columns, and seeded Admin/Instructor/Student roles were confirmed by SQL queries.
- Runtime smoke used generated environment-only demo Admin credentials and verified startup, public routes, Admin login, Admin dashboard, Admin blocked from Student dashboard, change password, and profile.
- Automated tests: 142 total, 142 passed, 0 failed, 0 skipped.
- Formatting verification passed with no changes required.
- Project-owner-confirmed authorized manual runtime evidence verified successful same-person attendance, different-person/no-face/multiple-face/poor-quality rejections, inside-geofence acceptance, outside-geofence/inaccurate-location/permission-denied handling, closed-session rejection, duplicate and repeated-submit handling, restart persistence, Instructor roster update, camera cleanup, client-side location-state cleanup, privacy retention behavior, and English/Arabic LTR/RTL light/dark desktop/mobile UI behavior.
- Manual location method was not recorded as physical GPS versus simulated/browser-overridden evidence.
- Security scan found no tracked real credential, private key, SDK license, biometric image/sample, model binary, database file, user secret, or build output.

## Sprint 7 Verification Evidence

- SQL Server LocalDB verification database: `AttendAI_Sprint7Verification_20260726`.
- Existing migrations verified through `20260726173403_AddSecureAttendanceCheckIn`.
- No Sprint 7 migration was created.
- Runtime smoke used generated environment-only demo Admin credentials and verified startup, home, login, Admin dashboard, Admin reports, Admin audit, Admin CSV export headers, Admin denial from Student reports, privacy, 404, English LTR HTML attributes, Arabic RTL HTML attributes, seeded roles, and existing migration count.
- Automated tests after Sprint 7 implementation: 158 total, 158 passed, 0 failed, 0 skipped.
- Security scan found no tracked real credential, private key, SDK license, biometric image/sample, model binary, database file, user secret, generated report export, or build output.
- Sprint 7 remains Partially completed until live SQL-backed Student/Instructor report walkthroughs and the full manual visual/accessibility matrix are recorded.

## Known Limitations

- No public self-registration.
- No liveness detection, anti-spoofing certification, notifications, one-to-many identification, classroom-camera recognition, cloud deployment, public report links, server-generated PDF exports, or production biometric certification.
- Face verification is real one-to-one verification using YuNet and SFace; it is not one-to-many identification or classroom surveillance.
- Browser geolocation is used as a server-side policy factor, not as device attestation, GPS anti-spoofing, or tamper-proof location proof.
- The implemented duplicate, challenge, idempotency, and rate-limit controls reduce duplicate and basic replay risks but are not complete replay-proof security.
- This remains an academic Localhost implementation and is not production biometric certification or production fraud prevention.
- Student and Instructor account creation currently captures academic profile data only; production onboarding policies and notification delivery remain future work.

## Documentation Index

- [01-SRS.md](docs/01-SRS.md)
- [02-System-Architecture.md](docs/02-System-Architecture.md)
- [03-Database-ERD.md](docs/03-Database-ERD.md)
- [04-Use-Cases.md](docs/04-Use-Cases.md)
- [05-UI-Design-System.md](docs/05-UI-Design-System.md)
- [06-Localization-And-Theming.md](docs/06-Localization-And-Theming.md)
- [07-Face-Recognition-Technical-Spike.md](docs/07-Face-Recognition-Technical-Spike.md)
- [08-Sprint-1-Plan.md](docs/08-Sprint-1-Plan.md)
- [09-Sprint-1-Test-Plan.md](docs/09-Sprint-1-Test-Plan.md)
- [10-Sprint-1-Review.md](docs/10-Sprint-1-Review.md)
- [11-Requirements-Traceability-Matrix.md](docs/11-Requirements-Traceability-Matrix.md)
- [12-Sprint-2-Plan.md](docs/12-Sprint-2-Plan.md)
- [13-Academic-Domain-Model.md](docs/13-Academic-Domain-Model.md)
- [14-Sprint-2-Test-Plan.md](docs/14-Sprint-2-Test-Plan.md)
- [15-Sprint-2-Review.md](docs/15-Sprint-2-Review.md)
- [16-Data-Dictionary.md](docs/16-Data-Dictionary.md)
- [17-Sprint-3-Plan.md](docs/17-Sprint-3-Plan.md)
- [18-Lecture-Scheduling-Domain-Model.md](docs/18-Lecture-Scheduling-Domain-Model.md)
- [19-Sprint-3-Test-Plan.md](docs/19-Sprint-3-Test-Plan.md)
- [20-Sprint-3-Review.md](docs/20-Sprint-3-Review.md)
- [21-Session-Code-Security.md](docs/21-Session-Code-Security.md)
- [22-Sprint-4-Plan.md](docs/22-Sprint-4-Plan.md)
- [23-Biometric-Enrollment-Domain-Model.md](docs/23-Biometric-Enrollment-Domain-Model.md)
- [24-Sprint-4-Test-Plan.md](docs/24-Sprint-4-Test-Plan.md)
- [25-Sprint-4-Review.md](docs/25-Sprint-4-Review.md)
- [26-Biometric-Privacy-And-Security.md](docs/26-Biometric-Privacy-And-Security.md)
- [27-Face-Engine-Readiness-Gate.md](docs/27-Face-Engine-Readiness-Gate.md)
- [28-Sprint-5-Plan.md](docs/28-Sprint-5-Plan.md)
- [29-Face-Verification-Domain-Model.md](docs/29-Face-Verification-Domain-Model.md)
- [30-Sprint-5-Test-Plan.md](docs/30-Sprint-5-Test-Plan.md)
- [31-Sprint-5-Review.md](docs/31-Sprint-5-Review.md)
- [32-Face-Verification-Security.md](docs/32-Face-Verification-Security.md)
- [33-Threshold-Evaluation-And-Calibration.md](docs/33-Threshold-Evaluation-And-Calibration.md)
- [34-Real-Face-Engine-Integration.md](docs/34-Real-Face-Engine-Integration.md)
- [35-Real-Face-Engine-Integration-Plan.md](docs/35-Real-Face-Engine-Integration-Plan.md)
- [36-OpenCV-YuNet-SFace-Architecture.md](docs/36-OpenCV-YuNet-SFace-Architecture.md)
- [37-Real-Face-Engine-Test-Plan.md](docs/37-Real-Face-Engine-Test-Plan.md)
- [38-Real-Face-Engine-Review.md](docs/38-Real-Face-Engine-Review.md)
- [39-Model-Provisioning-And-Licensing.md](docs/39-Model-Provisioning-And-Licensing.md)
- [40-Real-Threshold-Evaluation.md](docs/40-Real-Threshold-Evaluation.md)
- [41-Sprint-6-Plan.md](docs/41-Sprint-6-Plan.md)
- [42-Attendance-Check-In-Domain-Model.md](docs/42-Attendance-Check-In-Domain-Model.md)
- [43-Sprint-6-Test-Plan.md](docs/43-Sprint-6-Test-Plan.md)
- [44-Sprint-6-Review.md](docs/44-Sprint-6-Review.md)
- [45-Attendance-Security-And-Privacy.md](docs/45-Attendance-Security-And-Privacy.md)
- [46-Location-Geofence-Policy.md](docs/46-Location-Geofence-Policy.md)
- [47-Sprint-7-Plan.md](docs/47-Sprint-7-Plan.md)
- [48-Reporting-And-Analytics-Architecture.md](docs/48-Reporting-And-Analytics-Architecture.md)
- [49-Report-Export-Security-And-Privacy.md](docs/49-Report-Export-Security-And-Privacy.md)
- [50-Sprint-7-Test-Plan.md](docs/50-Sprint-7-Test-Plan.md)
- [51-Final-Release-Readiness.md](docs/51-Final-Release-Readiness.md)
- [52-Final-Acceptance-And-Traceability.md](docs/52-Final-Acceptance-And-Traceability.md)
- [53-Sprint-7-Review.md](docs/53-Sprint-7-Review.md)

## Future Sprint Summary

Sprint 7 is the final planned project sprint. Before final graduation-project acceptance, complete and record the pending Sprint 7 manual visual/accessibility checks and live SQL-backed Student/Instructor report walkthroughs. Do not add notifications, one-to-many recognition, classroom-camera recognition, liveness/anti-spoofing, public report links, generated PDF export, cloud deployment, or Sprint 8 features without separate approved scope, dependency review, and security testing.
