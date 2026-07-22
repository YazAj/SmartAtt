# AttendAI

AttendAI is a responsive university attendance management web platform for the graduation project **Smart Attendance Management System Using Face Recognition**.

Sprint 1 established the buildable foundation. Sprint 2 adds academic and account management: departments, students, instructors, courses, sections, classrooms, enrollments, instructor assignments, administrator-created accounts, first-login password changes, and role-aware academic dashboards. Sprint 3 adds weekly lecture schedules, schedule conflict detection, instructor/student timetables, live lecture sessions, secure temporary session codes, and session lifecycle audit history. Sprint 4 adds biometric consent, face enrollment profile management, protected template storage, Admin biometric oversight, and the real-engine readiness gate. Attendance registration, reports, exports, location validation, face verification, and attendance decisions remain intentionally out of scope.

## Current Sprint Status

Sprint 4 is partially completed for biometric face enrollment and profile management. The implemented workflow uses the explicit Fake/Disabled/Real engine modes, protects templates with ASP.NET Core Data Protection, stores no raw images, and keeps Admin views limited to safe metadata. The solution currently builds with 0 warnings and 0 errors, and 79 automated tests pass. Full Sprint 4 completion is blocked by the real face-engine gate: approved biometric samples, selected licensed models, native/runtime dependencies, and production adapter verification are not available.

## Technology Stack

- .NET 8, ASP.NET Core 8 MVC, Razor Views
- ASP.NET Core Identity with cookie authentication
- Entity Framework Core with SQL Server provider
- Bootstrap 5, CSS custom properties, JavaScript
- Built-in localization for `en-US` and `ar-JO`
- xUnit, ASP.NET Core integration testing, SQLite in-memory test database

## Architecture Summary

```text
src/AttendAI.Domain          Core domain abstractions, academic/lecture/biometric entities, and enums
src/AttendAI.Application     Contracts, academic/lecture/biometric DTOs/services, role routing, safe redirects, face engine abstraction
src/AttendAI.Infrastructure  EF Core, Identity persistence, academic/lecture/biometric services, seeding, fake/disabled face engines
src/AttendAI.Web             MVC controllers, Admin area, lecture/biometric UI, Razor views, localization, theming
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

The first non-test startup applies migrations and seeds Identity roles. Use the HTTPS URL printed by ASP.NET Core. Admin users can create Student and Instructor accounts with temporary passwords; those users must change password on first login. Admins can then create lecture schedules from active sections, assigned instructors, and classrooms. Instructors can start eligible lecture sessions and receive temporary codes only in the authorized response. Students can view active session state for enrolled sections but cannot see session codes. Students can also manage biometric consent and enrollment status; Admins can review biometric status and safe metadata.

## Biometric Enrollment

Sprint 4 supports biometric profile management only:

- Student privacy notice and explicit consent.
- Camera capture wizard with approved file-upload fallback.
- Configurable capture count, MIME, size, dimension, quality, and rate-limit settings under `BiometricEnrollment`.
- Protected template storage through ASP.NET Core Data Protection.
- Student re-enrollment and consent withdrawal.
- Admin safe metadata view, revocation, and require re-enrollment.

The default `FaceRecognition:Provider` is `Fake` for development and automated tests. Fake mode is blocked in Production by the readiness service. `Real` mode is documented but remains unavailable until an adapter, licensed detection/embedding models, native dependencies, threshold calibration, and approved test images are supplied.

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

No personal biometric images are committed. To test negative fake-engine paths, use a local file containing `NO_FACE` or `MULTI_FACE`. Real engine verification remains pending until approved samples, selected licensed models, and native dependencies are supplied locally.

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

## Known Limitations

- No public self-registration.
- No attendance registration, reports, exports, liveness detection, location validation, notifications, face verification, or production real-engine enrollment.
- Real face recognition was evaluated and documented, but not executed with approved biometric samples.
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

## Future Sprint Summary

Sprint 5 planning may begin only after the team accepts Sprint 4 as a partial implementation and explicitly tracks the real-engine gate. Do not add production attendance marking, face verification, or location validation before the face-recognition technical gate is satisfied.
