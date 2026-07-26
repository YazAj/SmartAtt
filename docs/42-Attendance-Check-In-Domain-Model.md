# 42. Attendance Check-In Domain Model

## Entities

### AttendanceChallenge

One-time challenge issued to a specific Student for a specific active lecture session.

Stored fields:

- `StudentId`
- `LectureSessionId`
- `TokenHash`
- `IssuedAtUtc`
- `ExpiresAtUtc`
- `ConsumedAtUtc`
- `ConsumedByAttendanceAttemptId`
- `RowVersion`

The plain challenge token is returned to the browser once and is never stored. The database stores only a SHA-256 hash.

### AttendanceAttempt

Append-only safe metadata for every server-processed check-in submission that can be tied to an authenticated Student and lecture session.

Stored fields include:

- Student/session ids.
- Outcome and failure reason.
- Location verification outcome.
- Idempotency key hash.
- Challenge token hash.
- Optional face verification attempt id.
- Distance and browser accuracy metadata.
- Allowed radius and maximum accepted accuracy snapshots.
- Processing duration.
- Safe localizable description key.

Not stored:

- Raw biometric images.
- Base64 images.
- Face embeddings.
- Protected template bytes.
- Template fingerprints.
- Exact Student latitude or longitude.
- Plain challenge token.
- Browser device fingerprint.
- Raw native errors.

### AttendanceRecord

Successful attendance decision for one Student in one lecture session.

Important constraints:

- Unique `(LectureSessionId, StudentId)`.
- Unique `AttendanceAttemptId`.
- Unique `FaceVerificationAttemptId`.
- Foreign keys to `LectureSessions`, `Students`, `AttendanceAttempts`, `FaceVerificationAttempts`, and `Classrooms`.

Stored location data is intentionally limited to safe metadata: distance, browser accuracy, allowed radius, maximum accepted accuracy, and classroom/policy reference. Exact submitted Student coordinates are evaluated in memory and not retained.

## Lecture Session Attendance Snapshot

`LectureSession` now snapshots attendance policy at session start:

- `AttendanceLatitude`
- `AttendanceLongitude`
- `MaximumAcceptedAccuracyMeters`
- `AttendanceCheckInEnabled`
- `LocationVerificationRequired`
- `FaceVerificationRequired`

These values are copied from the active classroom/scheduling configuration at session start. The snapshot prevents later classroom edits from changing the policy used for an already-started session.

## Status Enums

- `AttendanceStatus`: `Present`, `Late`.
- `AttendanceAttemptOutcome`: `Succeeded`, `Rejected`, `Duplicate`.
- `AttendanceFailureReason`: safe rejection categories such as `NotEnrolled`, `OutsideGeofence`, `ChallengeExpired`, `FaceNotMatched`, and `DuplicateAttendance`.
- `LocationVerificationOutcome`: `NotEvaluated`, `Accepted`, `MissingPolicy`, `Invalid`, `Inaccurate`, `OutsideGeofence`.

## Timing Rules

- A lecture session must be `Active`.
- Check-in is accepted only from `ActualStartUtc` through `ScheduledEndUtc`, inclusive.
- Status is `Present` when `CheckedInAtUtc <= ActualStartUtc + LateThresholdMinutes`.
- Status is `Late` after that threshold while the active check-in window remains open.

## Duplicate And Replay Rules

- Idempotency key hash protects repeated browser submits.
- Unique attendance record index prevents concurrent duplicate records.
- Challenge tokens are one-time and expire.
- A consumed challenge cannot be reused after a rejected or successful attempt.
- A Student with an existing attendance record receives a duplicate result without creating a second record.

## Face Verification Rule

Attendance uses `IOneToOneFaceVerifier` with `FaceVerificationPurpose.FutureAttendance`. Attendance requires real-engine diagnostics to report Real mode, verification enabled, production-safe readiness, and non-demo status. Fake mode can still support earlier development workflows, but it cannot create Sprint 6 attendance records.
