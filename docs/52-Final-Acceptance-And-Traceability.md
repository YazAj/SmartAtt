# 52. Final Acceptance And Traceability

## Sprint 7 Acceptance Logic

Sprint 7 may be marked `Completed` only when all of the following have evidence:

- Source-of-truth scope implemented.
- Reporting calculations are correct.
- Role authorization passes.
- Student self-only reporting passes.
- Instructor ownership isolation passes.
- Admin reporting and audit pass.
- Filters and pagination pass.
- Attendance percentage and derived missed-session logic pass.
- Future, cancelled, and attendance-disabled sessions are excluded.
- CSV export passes with formula-injection protection and no-store headers.
- Accessibility, localization, responsive layout, RTL/LTR, and dark/light themes pass manual verification.
- Sprint 1-6 regressions pass.
- Build, tests, format, security scan, and documentation pass.

When core implementation and automated tests pass but manual UI/runtime evidence remains, the status must be `Partially completed`.

## Sprint 7 Traceability Summary

| Requirement | Status | Evidence |
| --- | --- | --- |
| Role-scoped Student report | Passed by automated tests | `ReportingAuthorizationTests`, `ReportingServiceTests` |
| Role-scoped Instructor report | Passed by automated tests | `ReportingAuthorizationTests`, `ReportingServiceTests` |
| Admin global report and audit | Passed by automated and SQL runtime smoke | Admin report/audit routes returned 200 in SQL smoke |
| Attendance percentage calculation | Passed by automated tests | `AttendanceReportingTests` |
| Derived missed-session rule | Passed by automated tests | `ReportingServiceTests` |
| Future/cancelled/disabled exclusion | Passed by automated tests | `ReportingServiceTests` |
| CSV export and injection protection | Passed by automated and SQL runtime smoke | `AttendanceReportingTests`, Admin CSV smoke |
| Sensitive biometric/location exclusion | Passed by code review and automated CSV assertions | Safe DTO/export column checks |
| SQL migration | Passed | No new migration required; existing six migrations applied |
| Localization resources | Implemented, partially runtime verified | English and Arabic resource updates; `lang`/`dir` SQL smoke |
| Dark/light responsive UI | Partially verified | Tokenized CSS implemented; final manual visual matrix pending |
| Live Student/Instructor SQL report accounts | Partially verified | Integration tests pass; live SQL runtime smoke focused on Admin |
| CI workflow | Out of scope | No existing workflow; no new CI added |
| Liveness/anti-spoofing | Not implemented | Explicitly excluded and not claimed |
| One-to-many identification | Not implemented | Explicitly excluded and not claimed |
| Production biometric certification | Not implemented | Explicitly excluded and not claimed |

## Final Acceptance Position

At code closure, Sprint 7 is implementation-complete for reporting, export, and audit, but final acceptance depends on the recorded manual evidence in `docs/53-Sprint-7-Review.md`.

