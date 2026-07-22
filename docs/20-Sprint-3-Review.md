# 20. Sprint 3 Review

## Final Status

Sprint 3 is completed. Lecture scheduling, live session management, SQL Server migration, runtime authentication/session flow, secure session-code handling, responsive RTL/mobile closure, keyboard walkthrough, automated tests, formatting, and security audit passed. No Sprint 4 attendance or production biometric workflow was implemented.

## Closure Root Cause

The Arabic RTL dark mobile clipping was caused by the closed mobile sidebar using an off-viewport `translateX(...)` transform. In RTL, the fixed sidebar was anchored to logical inline-start and then translated outside the visual viewport, which caused the headless/mobile capture and overflow inspection to see clipped content at the right edge.

The fix keeps the closed sidebar anchored and hides it with `opacity`, `visibility`, and `pointer-events` instead of an off-screen transform. Additional responsive rules allow long lecture identifiers, button rows, timetable cards, and the session-code card to wrap inside narrow viewports.

## Features Completed

- Admin lecture schedule create/edit/details/list workflows with search, filters, pagination, activation/deactivation, and classroom/section timetables.
- Schedule conflict detection for Classroom, Instructor, and Section overlap.
- Instructor timetable and active-session workflow: start, view code, copy code, regenerate code, countdowns, end, and cancel.
- Student timetable and active-session view without code exposure.
- Admin session monitoring, details, lifecycle event history, stale expiration, and force-end path.
- Secure code generation, hashing, Data Protection storage, expiration, regeneration, and invalidation.
- UTC session persistence with configured application time-zone display.
- Dashboard/navigation/localization/theme/responsive updates.

## Migration And SQL Evidence

- Migration name: `20260722160127_AddLectureSchedulingAndSessions`.
- SQL Server verification database: `AttendAI_Sprint3Verification_20260722`.
- EF update result: exit 0; build succeeded; no migrations were applied because the database was already up to date.
- SQL query evidence: database name `AttendAI_Sprint3Verification_20260722`; Identity table count 7; lecture table count 3; roles `Admin,Instructor,Student`.
- SQL schema evidence: lecture foreign key count 8; lecture check constraint count 7; lecture row-version column count 2; unique occurrence index count 1.
- Runtime SQL smoke evidence: `LectureSchedules`, `LectureSessions`, and `LectureSessionEvents` exist; ended session invalidated code state; 4 lifecycle events recorded.

## Automated Verification

| Check | Exit | Warnings | Errors | Tests |
| --- | --- | --- | --- | --- |
| `dotnet tool restore` | 0 | n/a | n/a | n/a |
| `dotnet restore AttendAI.sln` | 0 | n/a | n/a | n/a |
| `dotnet build AttendAI.sln` | 0 | 0 | 0 | n/a |
| `dotnet test AttendAI.sln` | 0 | n/a | n/a | 63 total, 63 passed, 0 failed, 0 skipped |
| `dotnet format AttendAI.sln --verify-no-changes` | 0 | n/a | n/a | n/a |
| `git diff --check` | 0 | CRLF conversion warnings only | 0 whitespace errors | n/a |

Test split:

- Unit tests: 33 passed.
- Integration tests: 30 passed.

## Runtime Verification

Runtime smoke was executed against `https://127.0.0.1:5443` using SQL Server LocalDB and environment-variable configuration for connection string/demo credentials.

- Admin dashboard returned 200.
- Admin created active Department, Course, Section, Classroom, Instructor, InstructorAssignment, Student, StudentEnrollment, and LectureSchedule records.
- Conflicting schedule POST was rejected.
- Instructor temporary-password login redirected to Change Password; password change succeeded.
- Instructor opened schedule, started an eligible Session, saw the temporary code, regenerated code, and ended Session.
- Student temporary-password login redirected to Change Password; password change succeeded.
- Student opened active Session and did not receive or display the code.
- Student cross-role request to `/InstructorSchedule` redirected to `/Account/AccessDenied?ReturnUrl=/InstructorSchedule`.
- SQL smoke confirmed terminal-code invalidation and 4 lifecycle events.
- No attendance records or Sprint 4 flows were created.

## Responsive And Visual Verification

Final live Playwright verification covered 134 route/viewport/culture/theme checks with 0 failures. Every checked route reported `document.documentElement.scrollWidth <= document.documentElement.clientWidth`.

Viewports checked:

- 1440x900.
- 1024x768.
- 768x1024.
- 390x844.
- 320x568.

Route coverage included Admin lecture schedules index/create/edit/details, classroom timetable, section timetable, active sessions, session details, Instructor schedule, Instructor active session, Student schedule, Student active session, and a redacted Instructor one-time-code card response.

Culture/theme coverage:

- English LTR Light: passed.
- English LTR Dark: passed.
- Arabic RTL Light: passed.
- Arabic RTL Dark desktop/tablet/mobile: passed.
- Arabic RTL Dark 390px: passed.
- Arabic RTL Dark 320px: passed.

Screenshots and Playwright JSON results are stored under ignored `artifacts/verification/playwright-audit/output/`. Runtime smoke snapshots are stored under ignored `artifacts/verification/sprint3-visual/`.

## Timetable Verification

Admin classroom and section timetables, Instructor schedules, and Student schedules were checked on desktop, tablet, 390px, and 320px viewports. Timetable cards used localized text, stayed inside viewport bounds, supported light/dark tokens, and did not create page-level horizontal scrolling. Table overflow, where applicable, remained inside `.table-responsive`.

## Active Session Verification

The Instructor Active Session page was checked at 320px in Arabic RTL dark mode with a redacted one-time code. The code card, copy button, countdown cards, classroom card, regenerate/end/cancel buttons, and localized labels remained readable and inside the viewport. Student active-session HTML did not include the plain code, `ProtectedSessionCode`, or `SessionCodeHash`.

## Keyboard Verification

Playwright tab walkthroughs covered:

- Admin lecture schedules mobile RTL.
- Admin schedule create at 320px RTL.
- Instructor schedule at 320px RTL.
- Instructor active session at 320px RTL.
- Student schedule at 320px RTL.

Verified behavior: logical visible focus order, language selector focus, theme selector focus, filters/forms, timetable/session links, active-session actions, mobile nav keyboard open, and Escape close. No keyboard trap was observed. Sprint 3 uses normal form buttons and browser-managed submit behavior; no custom modal focus trap is implemented in this sprint.

## Security Verification

- State-changing actions use POST and antiforgery.
- Admin, Instructor, Student, anonymous, cross-role, and resource-ownership boundaries are covered by integration/runtime checks.
- Plain temporary codes are not persisted.
- Student DTOs/views exclude plain code, protected code, and hash fields.
- Lifecycle event descriptions are safe and do not include codes.
- Data Protection key material is not tracked.
- Secret scan found only safe placeholders/config keys, ignore rules, and code/documentation field names.
- No tracked biometric sample, model file, database file, private key, SDK license, build output, runtime screenshot, local verification artifact, or real session-code value was found.

## Tracked Artifact Decision

`artifacts/verification/sprint3-runtime-smoke.ps1` is a temporary local verification artifact under the ignored `artifacts/` directory. It is useful for repeatable local QA, but it is not intended for source control in its current form. It contains no real credentials; it generates demo credentials at runtime and clears related environment variables in `finally`.

## Known Limitations

- Real face-recognition runtime remains Not Verified from prior sprints because approved biometric samples and selected licensed model dependencies are not available.
- Attendance registration, student code submission, location validation, reports, exports, notifications, QR attendance, and production biometric workflows are Not Implemented.
- Session-code rate limiting is future Sprint 4 work before public attendance submission.

## Sprint 4 Readiness

Sprint 4 planning may begin from the completed Sprint 3 scheduling/session baseline. Production attendance work must still respect the documented face-recognition gate and add rate limiting before public session-code submission.

## Recommended Commit

`feat(sprint-3): implement lecture scheduling and session management`
