# 47. Sprint 7 Plan

## Goal

Sprint 7 is the final planned release-hardening sprint. It adds role-scoped attendance reporting, analytics summaries, safe CSV export, safe audit/event search, and final documentation closure without changing the biometric attendance decision pipeline.

## Scope

- Student attendance report for the authenticated Student.
- Instructor attendance report for sessions taught by the authenticated Instructor.
- Admin attendance report across completed attendance-enabled sessions.
- Derived `Missed` state for completed attendance-enabled sessions where an enrolled active Student has no `AttendanceRecord`.
- Summary metrics for eligible sessions, present count, late count, missed count, and attendance percentage.
- Server-side filtering, sorting, pagination, and CSV export.
- Admin audit search over existing safe event/attempt tables.
- English/Arabic localization resources and existing RTL/LTR theme-aware UI patterns.
- Final release documentation and traceability updates.

## Non-Goals

- No Sprint 8 scope.
- No persisted absence rows.
- No new biometric recognition behavior.
- No liveness or anti-spoofing.
- No one-to-many identification.
- No QR, NFC, Bluetooth, WiFi, IP geolocation, GPS anti-spoofing, or device attestation.
- No public report links.
- No PDF export.
- No cloud deployment, CI pipeline, or remote push.

## Database Decision

No new migration is required. Sprint 7 reports are projections over existing Sprint 2-6 tables:

- `StudentEnrollments`
- `LectureSessions`
- `AttendanceRecords`
- `AttendanceAttempts`
- `LectureSessionEvents`
- `FaceVerificationAttempts`
- `FaceEnrollmentEvents`

The implementation derives analytics at query time and does not store report snapshots, exported files, or additional audit rows.

## Security Principles

- Role scope is enforced server-side.
- Student and Instructor reports resolve the academic profile from the authenticated Identity user.
- CSV exports are generated in memory.
- CSV fields are escaped and spreadsheet-formula values are neutralized.
- Exported attendance data excludes raw biometric images, protected templates, embeddings, template fingerprints, exact submitted Student coordinates, plain challenge tokens, and model paths.
- Audit search exposes safe metadata only.

## Acceptance Targets

- Solution builds with 0 errors.
- Automated tests pass.
- Formatting verification passes.
- Report endpoints enforce role boundaries.
- Attendance percentage and derived missed sessions are covered by tests.
- CSV export safety is covered by tests.
- Documentation distinguishes verified, not verified, and remaining-risk items.

## Final Closure Status

Sprint 7 is marked Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence.

The project owner confirmed Admin, Instructor, and Student report workflows, filtering, pagination, role isolation, report calculations, CSV export, print-friendly views, localization, RTL/LTR, dark/light themes, responsive layouts, keyboard access, visible focus, semantic tables, and Sprint 1-6 regressions.

AttendAI is ready for final academic graduation-project acceptance, subject to the documented academic and security limitations.
