# Changelog

## 2026-07-22

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
