# 41. Sprint 6 Plan

## Goal

Sprint 6 adds secure Student biometric attendance check-in for active lecture sessions. It closes the gap between Sprint 3 live sessions and Sprint 5 one-to-one face verification without adding dashboards, reports, analytics, exports, notifications, QR/NFC/Bluetooth/WiFi/IP attendance, one-to-many recognition, or liveness/anti-spoofing claims.

## Scope

Implemented scope:

- Student attendance check-in for the authenticated enrolled Student.
- Active lecture-session eligibility.
- One-time server-issued attendance challenge.
- Idempotent check-in submission.
- Fresh one-to-one face verification with purpose `FutureAttendance`.
- Real face-engine gate for attendance.
- Browser geolocation capture and server-side geofence validation.
- Attendance attempt history with safe metadata only.
- One attendance record per Student per lecture session.
- Instructor roster for enrolled Students and safe attendance metadata.
- SQL Server migration for attendance tables and lecture-session attendance policy snapshots.

Explicitly out of scope:

- Attendance dashboards and analytics.
- Reports, exports, notifications, and bulk operations.
- Student code submission as an attendance factor.
- QR, NFC, Bluetooth, WiFi, IP geolocation, or device fingerprinting.
- One-to-many identification.
- Classroom camera or background recognition.
- Liveness, anti-spoofing, depth checks, replay prevention, and presentation-attack detection.

## Implementation Slices

1. Domain and schema:
   - `AttendanceChallenge`
   - `AttendanceAttempt`
   - `AttendanceRecord`
   - attendance/location enums
   - lecture-session attendance policy snapshot fields

2. Application contracts:
   - attendance options
   - check-in commands and DTOs
   - attendance service
   - location verification service
   - attendance rate limiter

3. Infrastructure:
   - EF Core mappings and migration
   - challenge token hashing
   - idempotency hash enforcement
   - transaction-backed check-in orchestration
   - Haversine geofence validation
   - real-engine readiness enforcement
   - one-to-one verification composition

4. Web:
   - Student attendance index
   - Student challenge-backed check-in page
   - Student result page
   - Instructor roster
   - localized navigation
   - browser geolocation capture helper

5. Verification:
   - unit tests for domain and geofence rules
   - integration tests for success, duplicate, no-match, outside geofence, challenge replay, real-engine gating, and endpoint authorization
   - SQL Server migration verification when local SQL Server is available
   - manual runtime browser/camera/location verification before final completion

## Completion Rule

Sprint 6 can be marked Completed only when all of these are true:

- `dotnet build AttendAI.sln` exits 0 with 0 errors.
- `dotnet test AttendAI.sln` exits 0 with all tests passed.
- `dotnet format AttendAI.sln --verify-no-changes` exits 0.
- The migration applies to SQL Server.
- The app starts against SQL Server.
- Real face engine readiness is verified for attendance mode.
- A Student with a real compatible template checks in successfully from an approved browser/device.
- Different-person, no-face, multiple-face, poor-quality, replay, duplicate, outside-geofence, inaccurate-location, expired-challenge, and unauthorized-role cases are verified.
- Instructor roster shows only safe metadata and the correct attendance state.
- No raw biometric image, exact student coordinate, template, secret, model file, or local artifact is tracked.

## Closure Status

Sprint 6 is marked Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence. The project owner confirmed the required successful attendance, face rejection, location rejection, authorization, duplicate/repeated-submit, restart persistence, privacy, camera cleanup, location-state cleanup, and UI scenarios.

The precise manual location method was not recorded as physical GPS versus browser developer-tool override or test double. The closure evidence therefore records: User-confirmed manual location result; physical-versus-simulated method was not recorded in the closure evidence.
