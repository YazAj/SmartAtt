# 11. Requirements Traceability Matrix

| ID | Description | Implementation Location | Test Location | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| R-01 | Clean Architecture solution | `src/` projects, `AttendAI.sln` | `dotnet build AttendAI.sln` | Passed | Build succeeded with 0 warnings and 0 errors. |
| R-02 | Identity login/logout/change password | `AccountController`, Account views, Identity configuration | `AccountFlowTests` | Passed | Valid login, password change, and logout integration tests passed. |
| R-03 | Role constants and seeding | `RoleConstants`, `IdentitySeedService` | `RoleConstantsTests` | Passed | Unit test passed. |
| R-04 | Role dashboards | `DashboardController`, dashboard views | `DashboardAuthorizationTests`, `AccountFlowTests` | Passed | Role dashboard access and login redirect integration tests passed. |
| R-05 | Safe return URLs | `SafeRedirectService`, Account and Preferences controllers | `SafeRedirectServiceTests`, `CultureSwitchTests` | Passed | Unit and integration tests passed. |
| R-06 | SQL Server EF Core foundation | `ApplicationDbContext`, migration | SQL Server LocalDB verification; `DatabaseConfigurationTests` | Passed | Migration applied to `AttendAI_Sprint1Closure_20260722`; Identity tables and migration history verified. |
| R-07 | Responsive UI foundation | Layout, `site.css`, pages | Edge/Playwright screenshots and no-overflow checks | Passed | English desktop and mobile dark screenshots inspected; mobile table clipping fixed. |
| R-08 | English and Arabic localization | `Resources/*.resx`, localization setup | `CultureSwitchTests`; Edge/Playwright culture-cookie checks | Passed | English and Arabic rendered with expected localized text and persisted culture cookie. |
| R-09 | RTL/LTR support | `_Layout.cshtml`, Bootstrap RTL, logical CSS | Edge/Playwright `dir` checks and screenshots | Passed | English `ltr`, Arabic `rtl`; Arabic desktop/mobile screenshots inspected. |
| R-10 | Theme system | `site.css`, `site.js`, layout head script | Edge/Playwright localStorage checks and screenshots | Passed | Light/dark persisted after refresh; contrast checks passed. |
| R-11 | Face abstraction and fake engine | Application face contracts, `FakeFaceRecognitionEngine` | `FakeFaceRecognitionEngineTests` | Passed | Fake engine unit tests passed. |
| R-12 | Separate face POC | `experiments/AttendAI.FaceRecognition.Poc` | `dotnet build AttendAI.sln` | Passed | POC project included in successful solution build. |
| R-13 | Documentation set | `docs/`, `README.md`, `CHANGELOG.md` | File audit | Passed | All required documentation files exist and closure docs updated. |
| R-14 | Formatting verification | Whole solution | `dotnet format AttendAI.sln --verify-no-changes` | Passed | Format verification succeeded with exit code 0 and no changes required. |
| R-15 | Runtime SQL authentication | Running web app, Identity DB | Edge/Playwright against LocalDB | Passed | Admin, Instructor, and Student verification users logged in and reached correct dashboards. |
| R-16 | Secrets and sensitive files | `.gitignore`, config, docs, source | Repository secret scan and `git check-ignore` | Passed | No non-ignored credentials, keys, biometric samples, model files, or license files found. |
