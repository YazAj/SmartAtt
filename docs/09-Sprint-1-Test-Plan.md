# 09. Sprint 1 Test Plan

## Automated Unit Tests

- Role constants contain Admin, Instructor, and Student.
- Dashboard routing maps roles to the correct dashboards.
- Safe redirect service rejects external URLs.
- Fake face engine rejects empty, no-face, and multi-face payloads.
- Fake face engine matches identical payloads and rejects different payloads.

## Automated Integration Tests

- Application home page starts.
- Login page is available.
- Valid Identity credentials redirect to the role dashboard.
- Authenticated user can change password.
- Authenticated user can log out.
- Anonymous users cannot access protected dashboards.
- Admin can access Admin dashboard.
- Student cannot access Admin dashboard.
- Culture switch sets culture cookie.
- Culture switch rejects external return URLs.
- Test environment uses SQLite provider.

## Manual UI Checklist

| Scenario | Status |
| --- | --- |
| Desktop light mode | Passed |
| Desktop dark mode | Partially verified by dark token checks; desktop dark screenshot not captured |
| Mobile light mode | Partially verified by responsive CSS and no-overflow checks; mobile light screenshot not captured |
| Mobile dark mode | Passed |
| Arabic RTL | Passed |
| English LTR | Passed |
| Sidebar behavior | Passed by render/DOM inspection |
| Mobile menu | Rendered; click interaction not separately recorded |
| Login validation | Login page and successful login verified; invalid-message visual not separately recorded |
| Access denied page | Passed |
| Keyboard navigation | CSS focus states verified; full keyboard walk-through not recorded |
| Browser refresh theme persistence | Passed |
| Browser refresh culture persistence | Passed |

Screenshots were captured in ignored `logs/ui-verification/`.

## SQL Server Runtime Checks

- Local tools restored.
- Existing migration applied to `AttendAI_Sprint1Closure_20260722`.
- Identity tables verified through `INFORMATION_SCHEMA.TABLES`.
- Roles `Admin`, `Instructor`, and `Student` verified in `AspNetRoles`.
- Demo admin seeded through User Secrets.
- Runtime login, role redirects, cross-role denial, profile, change password, privacy, 404, and error page verified against the running app.

## Quality Commands

```bash
dotnet restore AttendAI.sln
dotnet build AttendAI.sln
dotnet test AttendAI.sln
dotnet format AttendAI.sln --verify-no-changes
```
