# 24. Sprint 4 Test Plan

## Automated Unit Tests

- `BiometricDomainTests`
  - Consent withdrawal is idempotent and cannot be reactivated.
  - Face templates copy protected bytes, revoke once, and cannot reactivate.
  - Invalid quality scores are rejected.
- `BiometricCaptureValidatorTests`
  - Supported PNG dimensions pass.
  - Unsupported MIME type fails.
  - Too-small images fail.
- `BiometricTemplateProtectorTests`
  - Protect/unprotect round-trips and protected bytes differ from original bytes.
- `FaceEnrollmentProcessorTests`
  - Required fake captures create a template.
  - `NO_FACE` marker maps to no-face rejection.
  - `LOW_QUALITY` marker maps to quality rejection.

## Automated Integration Tests

- `BiometricEnrollmentAuthorizationTests`
  - Student can access own biometric profile page.
  - Instructor cannot access Student biometric page.
  - Admin can access biometric oversight.
  - Student/Instructor cannot access Admin biometric oversight.
  - Anonymous users cannot access biometric pages.

## Regression Coverage

Existing Sprint 1-3 tests remain in the full solution test run:

- Identity/account flow.
- Dashboard routing and authorization.
- Academic management.
- Lecture scheduling and live session lifecycle.
- Fake face-recognition engine behavior.
- Culture switching.

## Manual Runtime Plan

Use local SQL Server and approved non-biometric fake payloads:

1. Apply migration.
2. Create or reuse Admin and active Student accounts.
3. Student opens biometric profile.
4. Student reviews privacy notice and accepts consent.
5. Student submits three approved fake PNG/JPEG captures.
6. Enrollment succeeds and active status is displayed.
7. Admin sees safe metadata and no template bytes.
8. Student re-enrolls; template version increments.
9. Student withdraws consent; active template is revoked.
10. Audit history shows safe localized events.

Real face-engine verification remains Not Verified until approved biometric images, models, licenses, and native dependencies are available.

## Sensitive-Data Checks

- No raw-image database column.
- No template bytes in public DTOs/views.
- No image persistence to `wwwroot`, Git, ignored verification folders, browser storage, or URLs.
- Data Protection keys and model/license files remain untracked.

