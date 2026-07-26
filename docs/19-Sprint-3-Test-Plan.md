# 19. Sprint 3 Test Plan

## Automated Verification

| Command | Expected |
| --- | --- |
| `dotnet tool restore` | Exit 0. |
| `dotnet restore AttendAI.sln` | Exit 0. |
| `dotnet build AttendAI.sln` | Exit 0, 0 warnings, 0 errors. |
| `dotnet test AttendAI.sln` | Exit 0, all tests pass. |
| `dotnet format AttendAI.sln --verify-no-changes` | Exit 0, no formatting changes required. |
| `git diff --check` | Exit 0. |

## Unit Tests

- Lecture schedule validation: time order, effective dates, duration, thresholds, radius, activation.
- Lecture session transitions: start, regenerate, end, cancel, expire, terminal-state protection.
- Lifecycle events do not store plain codes.
- Secure session-code generation, hash verification, protected-code round trip, and incorrect-code rejection.
- Application time-zone conversion between UTC and `Asia/Amman`/fallback zone.
- Existing Sprint 1 and Sprint 2 unit tests remain green.

## Integration Tests

- Admin can access schedule and session management pages.
- Instructor and Student cannot access Admin lecture management.
- Anonymous users are redirected to login.
- Admin can create valid schedules.
- Classroom, Instructor, and Section conflicts are rejected.
- Instructor sees only assigned schedules.
- Student sees only actively enrolled-section schedules.
- Assigned Instructor can start an eligible session.
- Unassigned Instructor cannot manage another Instructor's session.
- Session code is generated and regenerated.
- Previous code is invalid after regeneration.
- End, cancel, force-end, and expiration create lifecycle events and invalidate codes.
- Student active-session response contains no plain code, protected code, or hash.
- Sprint 1 authentication and Sprint 2 academic workflows still pass.

## SQL Server Verification

- Restore local tools.
- Configure `ConnectionStrings:DefaultConnection` through User Secrets or environment variables.
- Apply all migrations through `AddLectureSchedulingAndSessions`.
- Confirm the database exists.
- Confirm `LectureSchedules`, `LectureSessions`, and `LectureSessionEvents` exist.
- Confirm foreign keys to Sections, Instructors, Classrooms, schedules, and Identity users.
- Confirm row-version columns, check constraints, indexes, and unique schedule occurrence constraint.
- Confirm Admin, Instructor, and Student roles still exist.
- Confirm no plain session-code column exists.
- Confirm ended/cancelled/expired sessions invalidate code state.

## Runtime Verification Scenario

- Configure a demo Admin securely.
- Create active Department, Course, Section, Classroom, Instructor, InstructorAssignment, Student, and StudentEnrollment.
- Create a valid Lecture Schedule.
- Verify a conflicting Schedule is rejected.
- Login as Instructor with temporary password, change password, view schedule, start session, see code, regenerate code, and end session.
- Login as Student with temporary password, change password, view active session, and confirm code is hidden.
- Verify cross-role access redirects to Access Denied.
- Verify lifecycle events and session history are persisted.
- Verify no attendance records are created.

## Visual Verification

Required combinations:

- English LTR Light Desktop.
- English LTR Dark Mobile.
- Arabic RTL Light Desktop.
- Arabic RTL Dark Mobile.

Check schedule lists, filters, forms, timetable cards, active-session pages, countdown text, buttons, tables, badges, alerts, profile menu, dashboards, Access Denied, Privacy, and 404/error pages where applicable.

## Closure Verification Results

Final Sprint 3 closure verification was executed against SQL Server LocalDB database `AttendAI_Sprint3Verification_20260722` and the live app at `https://127.0.0.1:5443`.

| Area | Result |
| --- | --- |
| Automated commands | `dotnet tool restore`, restore, build, test, and format verification exited 0. |
| Test count | 63 total; 63 passed; 0 failed; 0 skipped. Unit: 33 passed. Integration: 30 passed. |
| SQL Server | Migration `20260722160127_AddLectureSchedulingAndSessions` remained applied; 7 Identity tables, 3 lecture tables, 8 lecture foreign keys, 7 lecture check constraints, 2 row-version columns, 1 unique occurrence index, and roles `Admin,Instructor,Student` verified. |
| Runtime flow | Admin schedule creation/conflict rejection, Instructor password change/start/regenerate/end, Student password change/code exclusion, and cross-role Access Denied passed. |
| Responsive matrix | 1440x900, 1024x768, 768x1024, 390x844, and 320x568 passed. |
| Programmatic overflow | 134 live Playwright checks; 0 page-level overflow failures. |
| Arabic RTL dark mobile | Passed at 390px and 320px. |
| Arabic RTL dark tablet | Passed at 768x1024. |
| Active Session 320px | Redacted session-code card, countdown cards, and action buttons stayed inside the viewport. |
| Keyboard walkthrough | Mobile navigation, filters/forms, timetable/session links, active-session actions, language selector, theme selector, and Escape close behavior passed. |
| Session-code security | Student active-session HTML did not contain the plain code, `ProtectedSessionCode`, or `SessionCodeHash`; regeneration/end invalidation passed. |

The original Arabic RTL dark mobile clipping source was the closed fixed mobile sidebar's translated off-viewport state. The closure fix keeps the closed sidebar anchored with opacity/visibility/pointer-events and adds safe wrapping/min-inline-size rules for long lecture/session content.

## Not Verified In Sprint 3

- Production attendance registration.
- Student code submission.
- Production face enrollment or verification.
- GPS/location validation.
- Reports, exports, notifications, QR attendance, and mobile-native workflows.

Historical note: Sprint 6 later implements secure biometric attendance check-in and records its verification plan in `docs/43-Sprint-6-Test-Plan.md`.
