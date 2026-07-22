# AttendAI

AttendAI is a responsive university attendance management web platform for the graduation project **Smart Attendance Management System Using Face Recognition**.

Sprint 1 established the buildable foundation. Sprint 2 adds academic and account management: departments, students, instructors, courses, sections, classrooms, enrollments, instructor assignments, administrator-created accounts, first-login password changes, and role-aware academic dashboards. Lecture sessions, attendance registration, reports, exports, and production biometric workflows are intentionally out of scope.

## Current Sprint Status

Sprint 2 is closed as completed for academic and account management. The solution builds, automated tests pass, formatting passes, SQL Server LocalDB migrations are applied, runtime authentication/account flows were verified, academic CRUD was exercised against SQL Server, and desktop/mobile English/Arabic light/dark screenshots were checked. Real face-recognition runtime remains a documented technical gate before production Face Enrollment because no approved biometric samples or selected model dependencies are committed.

## Technology Stack

- .NET 8, ASP.NET Core 8 MVC, Razor Views
- ASP.NET Core Identity with cookie authentication
- Entity Framework Core with SQL Server provider
- Bootstrap 5, CSS custom properties, JavaScript
- Built-in localization for `en-US` and `ar-JO`
- xUnit, ASP.NET Core integration testing, SQLite in-memory test database

## Architecture Summary

```text
src/AttendAI.Domain          Core domain abstractions, academic entities, and enums
src/AttendAI.Application     Contracts, academic DTOs/services, role routing, safe redirects, face engine abstraction
src/AttendAI.Infrastructure  EF Core, Identity persistence, academic services, seeding, fake face engine
src/AttendAI.Web             MVC controllers, Admin area, Razor views, localization, theming, UI
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

The first non-test startup applies migrations and seeds Identity roles. Use the HTTPS URL printed by ASP.NET Core. Admin users can create Student and Instructor accounts with temporary passwords; those users must change password on first login.

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

## Face POC

The production app depends only on `IFaceRecognitionEngine`. Sprint 1 includes a deterministic fake engine and a console harness:

```bash
dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <image-path>
dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <reference-image-path> <probe-image-path>
```

No personal biometric images are committed. To test negative fake-engine paths, use a local file containing `NO_FACE` or `MULTI_FACE`. Real engine verification remains pending until approved samples and native dependencies are supplied locally.

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

## Known Limitations

- No public self-registration.
- No lecture sessions, attendance registration, reports, exports, liveness detection, or production face enrollment.
- Real face recognition was evaluated and documented, but not executed with biometric samples in Sprint 2.
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

## Future Sprint Summary

The next sprint can begin lecture/session and attendance planning after Sprint 2 acceptance. Do not add production biometric enrollment or attendance marking before the face-recognition technical gate is satisfied.
