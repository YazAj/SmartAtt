# 28. Sprint 5 Plan

## Objective

Implement secure one-to-one Student face verification for localhost demonstration and automated testing while preserving Sprints 1-4 and avoiding Sprint 6 attendance scope.

Sprint 5 verifies only the authenticated Student against that Student's active compatible template. It does not create attendance records, attendance statuses, reports, exports, notifications, location validation, or one-to-many identification.

## Scope

- Student verification eligibility.
- Camera/file capture for a single verification sample.
- Server-side request, MIME, signature, dimension, face-count, and quality validation.
- Active consent and active template checks.
- Explicit template compatibility checks.
- Template unprotection inside Infrastructure only.
- One-to-one comparison through `IFaceRecognitionEngine`.
- Configurable score metric and threshold policy.
- Safe verification-attempt metadata persistence.
- Student status/result/history UI.
- Admin overview/details/diagnostics UI.
- Rate limiting and idempotency.
- English/Arabic, LTR/RTL, light/dark support.

## Out Of Scope

- Attendance registration.
- Attendance records or statuses.
- Session-code attendance submission.
- Location, GPS, geofencing, or distance validation.
- Attendance reports, analytics, Excel/PDF export.
- Notifications.
- Instructor manual attendance.
- One-to-many identification or template search.
- Classroom-camera or background recognition.
- Production liveness or anti-spoofing claims.
- Sprint 6 features.

## Architecture Plan

- Domain owns `FaceVerificationAttempt` and verification enums.
- Application owns commands, queries, DTOs, options, service contracts, and safe result models.
- Infrastructure owns EF persistence, eligibility, compatibility, diagnostics, rate limiting, template loading, template unprotection, engine calls, threshold policy, and attempt persistence.
- Web owns MVC controllers/views, antiforgery, request-size checks, model binding, localization, and safe display only.

## Real Engine Plan

The real engine is gated. Sprint 5 may use Fake mode for development/demo and automated tests. Real mode can be marked verified only after approved local samples, selected licensed models, adapter initialization, native/runtime dependencies, and threshold calibration are all available and tested.

## Acceptance Strategy

- Pass restore/build/test/format.
- Apply the Sprint 5 migration to SQL Server LocalDB.
- Run a fake-development runtime smoke against SQL Server.
- Confirm no raw image, embedding, protected template, template fingerprint, secret, model, license file, or database file is tracked.
- Mark Sprint 5 Partially completed if real-engine gate remains Not Verified.
