# 17. Sprint 3 Plan

## Goal

Implement lecture scheduling and live lecture-session management on top of the completed Sprint 1 identity foundation and Sprint 2 academic/account management modules.

## Scope

- Admin weekly lecture schedules for existing Sections, Instructors, and Classrooms.
- Classroom, Instructor, and Section schedule conflict detection.
- Instructor and Student timetable views.
- Instructor session start, active-session view, code regeneration, end, and cancel.
- Admin active-session monitoring, details, force-end, stale-expiration action, and lifecycle audit history.
- Secure temporary session codes for future attendance use.
- UTC persistence with application time-zone display for Jordan.
- English/Arabic, LTR/RTL, light/dark, responsive UI updates.

## Out Of Scope

Sprint 3 does not implement attendance registration, attendance records, student code submission, production face enrollment, production face verification, liveness detection, camera capture, GPS/location validation, reports, exports, notifications, QR codes, or Sprint 4 attendance-engine behavior. Sprint 6 later adds secure attendance check-in through a separate documented workflow.

## User Stories

- As an Admin, I can create and edit recurring lecture schedules only when protected resources do not conflict.
- As an Admin, I can inspect active and historical lecture sessions and force-end an active session when necessary.
- As an Instructor, I can see my weekly schedule, start an eligible lecture session, view the temporary code, regenerate it, and close the session.
- As a Student, I can see schedules and active lecture indicators for sections in which I am actively enrolled, without receiving the session code.

## Technical Tasks

- Add Domain entities: `LectureSchedule`, `LectureSession`, and `LectureSessionEvent`.
- Add Domain enums: `LectureSessionStatus`, `LectureSessionEventType`, and `ScheduleConflictType`.
- Add Application DTOs, commands, queries, conflict results, session results, service contracts, and time-zone/code contracts.
- Add Infrastructure EF configurations, services, conflict queries, transactions, Data Protection, secure code generation, hashing, lifecycle events, and expiration logic.
- Add EF Core migration `20260722160127_AddLectureSchedulingAndSessions`.
- Add Admin, Instructor, and Student MVC controllers and Razor views.
- Extend dashboards, navigation, localization resources, CSS, and JavaScript countdown/copy-code helpers.

## Security Tasks

- Generate codes with a cryptographically secure random-number generator.
- Store only a hash and protected copy of active codes; never store plain codes.
- Return a plain code only in the authorized Instructor/Admin initiating response.
- Exclude code fields from Student DTOs and views.
- Use POST plus antiforgery for state changes.
- Enforce resource ownership and role boundaries in services/controllers.
- Use row-version concurrency and transactions around schedule/session changes.
- Keep verification credentials in User Secrets or environment variables only.

## UI Tasks

- Reuse existing layout, page header, app panel, form, table, badge, empty-state, and responsive grid styles.
- Add timetable and active-session surfaces.
- Add localized countdown and copy-code UI.
- Check desktop/mobile and English/Arabic light/dark combinations.

## Testing Tasks

- Preserve all Sprint 1 and Sprint 2 tests.
- Add unit tests for lecture domain rules, time-zone conversion, and code-security behavior.
- Add integration tests for schedule conflicts, session lifecycle, authorization, and Student code exclusion.
- Apply migration to SQL Server LocalDB and run an end-to-end runtime smoke test.
- Run restore, build, test, format verification, `git diff --check`, and `git status`.

## Risks And Dependencies

- Real attendance acceptance still depends on the future Attendance Engine and face-recognition technical gate.
- Temporary session codes need future rate limiting before public attendance submission.
- SQL Server cannot express all recurring-overlap constraints declaratively; the implementation uses service validation and serializable transactions.
- Closure verification resolved the Arabic RTL dark mobile clipping gap with live-browser checks at 390px and 320px.

## Acceptance Criteria

- Clean Architecture boundaries remain intact.
- Migration applies to SQL Server.
- Admin can manage schedules and detect conflicts.
- Instructor can start/regenerate/end/cancel authorized sessions.
- Admin can monitor and force-end sessions.
- Students can view active sessions without codes.
- Session lifecycle events persist.
- Tests, build, formatting, and git whitespace checks pass.
- English/Arabic, LTR/RTL, light/dark, desktop, tablet, 390px mobile, and 320px mobile lecture/session views pass responsive no-overflow checks.
- No secrets, codes, biometric samples, database files, or build outputs are tracked.

## Definition Of Done

Sprint 3 is done only when implementation, migration, SQL verification, runtime auth/session verification, documentation, automated tests, security audit, and applicable visual checks are complete. Any unchecked item must remain explicitly Not Verified.
