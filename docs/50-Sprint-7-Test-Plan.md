# 50. Sprint 7 Test Plan

## Automated Commands

Required closure commands:

```powershell
dotnet tool restore
dotnet restore AttendAI.sln
dotnet build AttendAI.sln
dotnet test AttendAI.sln
dotnet format AttendAI.sln --verify-no-changes
git diff --check
git status
git diff --stat
git diff --name-only
```

## Unit Test Coverage

- Attendance percentage calculation.
- Zero eligible-session handling.
- Negative count rejection.
- CSV escaping for comma-containing values.
- CSV spreadsheet-formula neutralization.
- CSV sensitive-column exclusion.

## Integration Test Coverage

- Student report derives missed sessions without storing absence rows.
- Future sessions are excluded.
- Cancelled sessions are excluded.
- Attendance-disabled sessions are excluded.
- Instructor report returns the same scoped summary for the owning Instructor.
- Admin report can filter to a Student and match the scoped summary.
- Admin audit search returns safe attendance event metadata.
- Anonymous users are challenged for report routes.
- Wrong roles are forbidden for report routes.
- Student, Instructor, and Admin report owners can open their report pages.
- Admin CSV export returns CSV with no-store headers and safe columns.

## SQL Server Runtime Targets

- Application starts against SQL Server.
- Existing migrations apply successfully.
- Admin, Instructor, and Student roles are seeded.
- Admin login works.
- Report navigation renders for Admin.
- Admin report and audit pages return 200.
- Cross-role report access is forbidden or redirected to Access Denied.
- CSV download returns `text/csv` and no-store headers.

## Manual Visual Verification Targets

- English LTR Light.
- English LTR Dark.
- Arabic RTL Light.
- Arabic RTL Dark.
- Desktop report layout.
- Tablet report layout.
- 390px and 320px report layout.
- Keyboard navigation and visible focus.
- Print layout.
- Sidebar, navbar, page title, dropdowns, forms, placeholders, validation messages, buttons, cards, tables, badges, alerts, empty states, profile menu, report pages, audit page, and export/print actions.

Manual visual items must not be marked Passed unless a runtime browser check is actually executed and recorded.

## Final Manual Acceptance Evidence

Status: Passed by project-owner-confirmed authorized manual runtime evidence.

Admin evidence confirmed:

- Admin reports page loaded successfully.
- Date filters worked.
- Course, offering, Instructor, and status filters worked.
- Summary metrics matched the available attendance data.
- Safe audit search worked.
- Admin CSV export worked and reflected the authorized filtered dataset.
- Export used safe cache headers.
- Admin could not impersonate a Student through the Student report endpoint.
- No biometric data or exact coordinates appeared.

Instructor evidence confirmed:

- Instructor report loaded successfully.
- Instructor saw only owned offerings and sessions.
- Cross-Instructor access was denied.
- Date, offering, session, Student-search, and status filters worked.
- Pagination worked.
- Present, Late, Missed, and eligible-session calculations were correct.
- Future, cancelled, and attendance-disabled sessions were excluded.
- Instructor CSV export and print-friendly view worked.
- No unrelated Student data appeared.

Student evidence confirmed:

- Student report loaded successfully.
- Student saw only personal attendance data.
- Cross-Student access was denied.
- URL/filter manipulation did not reveal another Student's data.
- Course/date filters and pagination worked.
- Personal CSV export worked.
- Attendance percentage was correct.
- Zero eligible sessions returned `0.00%`.
- No biometric score, raw location, challenge, idempotency, or internal secret appeared.

CSV security evidence confirmed Arabic Unicode, English text, commas, quotes, newlines, and formula-injection protection for `=`, `+`, `-`, `@`, tab, and carriage return. Generated CSV files were not committed.

UI/accessibility evidence confirmed English LTR Light, English LTR Dark, Arabic RTL Light, Arabic RTL Dark, desktop, tablet, 390px, 320px, keyboard navigation, visible focus, semantic report tables, usable pagination, print preview, no page-level horizontal overflow, readable dark-mode text, and long Arabic wrapping.

## Regression Targets

- Sprint 1 authentication and role routing.
- Sprint 2 academic profile linkage.
- Sprint 3 lecture/session lifecycle.
- Sprint 4 biometric enrollment boundary.
- Sprint 5 one-to-one verification boundary.
- Sprint 6 attendance record creation, attempt retention, and Instructor roster boundary.

Normal automated tests must not require camera access, physical GPS, model files, or personal biometric images.

## Out Of Scope For Testing

- Liveness or anti-spoofing.
- One-to-many face identification.
- Public report links.
- PDF generation.
- Cloud deployment.
- Production biometric certification.
