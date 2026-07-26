# 43. Sprint 6 Test Plan

## Automated Tests

Unit tests:

- Attendance domain validation rejects invalid identifiers and invalid success/failure combinations.
- Attendance challenge is single-use.
- Lecture session attendance policy requires coordinate pairs.
- Location service accepts same-point valid location.
- Location service rejects outside geofence.
- Location service rejects inaccurate browser location.
- Location service rejects invalid latitude, longitude, and accuracy.
- Location service rejects missing server policy.

Integration tests:

- Matching real-ready face plus valid location creates one attendance record.
- Instructor roster shows Present count and safe row metadata.
- Duplicate check-in does not create a second record.
- Different-person face result is rejected without an attendance record.
- Outside geofence is rejected and consumes the challenge.
- Reusing a consumed challenge is rejected.
- Attendance challenge issuance requires the real face engine gate.
- Student can access the attendance index.
- Admin and Instructor cannot access Student attendance.
- Student cannot access Instructor attendance roster.
- Anonymous attendance access is challenged by the test authentication handler.

Automated test doubles may simulate a real-ready face verifier. They do not prove real biometric accuracy, browser camera behavior, GPS behavior, or liveness.

## SQL Server Verification

Required checks:

- Restore local tools.
- Apply migration `AddSecureAttendanceCheckIn`.
- Confirm these tables exist:
  - `AttendanceChallenges`
  - `AttendanceAttempts`
  - `AttendanceRecords`
- Confirm `LectureSessions` has attendance policy snapshot columns.
- Confirm unique indexes:
  - `UX_AttendanceRecords_Session_Student`
  - `UX_AttendanceAttempts_Student_Session_Idempotency`
  - `UX_AttendanceChallenges_TokenHash`
- Confirm no raw biometric image, exact Student latitude/longitude, unprotected template, or plain token column exists on attendance tables.

## Manual Browser Verification

Required routes:

- Student `/Attendance`.
- Student `/Attendance/CheckIn/{lectureSessionId}`.
- Student attendance result.
- Instructor `/InstructorAttendance/Roster/{lectureSessionId}`.
- Cross-role and anonymous access.

Required UI matrix:

- English LTR Light.
- English LTR Dark.
- Arabic RTL Light.
- Arabic RTL Dark.
- Desktop and mobile viewports.

Required controls:

- Sidebar link.
- Navbar/profile menu.
- Tables.
- Cards.
- Badges.
- Form validation.
- Camera controls.
- Browser location button.
- Submit button.
- Empty states.
- Result page.

## Manual Real-Device Attendance Verification

Required approved-sample scenarios:

- Successful same-person check-in with real compatible template.
- Different-person rejection.
- No-face rejection.
- Multiple-face rejection.
- Poor-quality rejection.
- Outside-geofence rejection.
- Inaccurate-location rejection.
- Expired challenge rejection.
- Challenge replay rejection.
- Duplicate attendance rejection.
- Verification after application restart.
- Camera cleanup after submit/navigation.
- Instructor roster after successful and rejected attempts.

No personal biometric images may be committed or intentionally retained.

## Completion Decision

Automated tests and SQL Server migration/runtime smoke passing are necessary but insufficient. Sprint 6 remains Partially completed until approved manual real-device attendance evidence and multilingual light/dark responsive visual evidence are recorded.
