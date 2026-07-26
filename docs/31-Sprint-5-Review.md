# 31. Sprint 5 Review

## Final Status

Partially completed.

The one-to-one verification architecture, database migration, fake-development runtime workflow, Student/Admin UI, authorization, security controls, localization resources, and tests are implemented. The real face-engine gate remains Not Verified because approved biometric samples, selected licensed models, production adapter, native dependencies, and threshold calibration are unavailable.

## Automated Verification

Final command results on 2026-07-23:

| Command | Exit | Warnings | Errors | Result |
| --- | --- | --- | --- | --- |
| `dotnet tool restore` | 0 | 0 | 0 | `dotnet-ef` 8.0.29 restored. |
| `dotnet restore AttendAI.sln` | 0 | 0 | 0 | All projects up to date. |
| `dotnet build AttendAI.sln` | 0 | 0 | 0 | Build succeeded. |
| `dotnet test AttendAI.sln` | 0 | 0 | 0 | 94 total, 94 passed, 0 failed, 0 skipped. |
| `dotnet format AttendAI.sln --verify-no-changes` | 0 | 0 | 0 | No formatting changes required. |
| `git diff --check` | 0 | 0 blocking | 0 | Whitespace check passed; Git printed CRLF conversion notices only. |
| `git status --short` | 0 | n/a | n/a | Shows only Sprint 5 modified/untracked source and docs. |

Implemented Sprint 5 automated coverage includes 49 unit tests and 45 integration tests.

## Runtime Verification

SQL Server LocalDB smoke ran against `AttendAI_Sprint5Verification_20260723` using environment-supplied credentials only. Evidence captured:

- Admin login redirected to `/Dashboard/Admin`.
- Admin face-verification overview returned 200.
- Admin created a Student through the UI.
- Student login redirected to Change Password.
- Student changed password successfully.
- Student opened verification status before enrollment.
- Student accepted biometric consent.
- Student enrolled a fake-development template.
- Student ran a match verification.
- Student ran a no-match verification.
- A rate-limited attempt was recorded safely.
- Student access to Admin verification routes was denied through the access-denied flow.
- Admin overview/details showed safe attempt metadata.

## SQL Server Verification

- Migration: `20260722221335_AddFaceVerification`.
- Table: `FaceVerificationAttempts`.
- Foreign keys: `Students`, `StudentFaceTemplates`.
- Indexes: Student/time, outcome/time, template id, and filtered idempotency index on Student/client request id.
- Constraints: bounded enum, score, threshold, image dimension, face-count, quality, and processing-duration checks.
- No raw image, capture bytes, encoding, embedding, protected template, or template fingerprint column exists on `FaceVerificationAttempts`.

## Manual Visual Verification

Manual/runtime UI checks covered Student verification status/capture/result/history, Admin overview/details/diagnostics, dashboards, profile, login, privacy notice, access-denied flow, and 404 handling.

Browser evidence on Edge through Playwright:

- 51 valid dashboard/verification/admin/public route checks across the required culture/theme/viewport matrix.
- 12 corrected `/Profile` checks after fixing mobile profile overflow.
- 63 meaningful route checks total.
- Statuses: 62 pages returned 200 and the intentional missing page returned 404.
- Max horizontal overflow after the profile CSS fix: 0.
- Dark-theme black-text issue pages: 0.
- Culture direction and persisted theme matched expected values for English LTR Light, English LTR Dark, Arabic RTL Light, and Arabic RTL Dark at 1440px, 1024px, 768px, 390px, and 320px coverage points.

The first profile pass used the wrong URL (`/Account/Profile`) and produced expected 404s. The actual route is `/Profile`; the corrected pass returned 200 for Admin and Student in all checked combinations. A real mobile overflow on long profile emails was fixed with scoped wrapping rules in `site.css`.

Items not directly proven by approved real images remain Not Verified.

## Security Verification

- Student id is resolved from the authenticated user.
- Controllers do not receive template bytes.
- Views do not render protected template, template fingerprint, embedding bytes, or captured image bytes.
- Captures are not stored in SQL Server or disk by the verification workflow.
- Antiforgery is required for verification POSTs.
- Rate limiting and idempotency are implemented.
- Fake mode is visibly labeled as development/demo.

## Not Verified

- Real engine initialization.
- Real detection model loading.
- Real embedding model loading.
- Approved same-person comparison.
- Approved different-person comparison.
- Real no-face, multiple-face, and low-quality behavior.
- Threshold calibration with real samples.
- Production liveness or anti-spoofing.

## Remaining Risks

- Fake engine validates workflow only; it is not biometric recognition.
- Real engine/model licensing, native packaging, preprocessing, normalization, and threshold calibration remain mandatory gates before production attendance work.
- Capture validation is sufficient for Sprint 5 fake-development workflow but a production adapter should use a hardened image decoder and documented preprocessing path.
- Retention policy for verification attempts and revoked templates must be finalized before production.

## Recommended Commit

`feat(sprint-5): implement one-to-one face verification`

## Real Engine Update

Sprint 5 verification can now resolve `OpenCvSFaceRecognitionEngine` in Real mode. Model-only readiness has been verified with OpenCV `4.13.0`, YuNet load, SFace load, and SFace output dimension `128`.

Still Not Verified:

- Approved same-person comparison.
- Approved different-person comparison.
- No-face, multiple-face, and poor-quality camera behavior.
- Restart-and-reverify with a protected real template.
- Threshold calibration.
- Production liveness or anti-spoofing.

Sprint 5 remains partially completed for real biometric verification until those items are recorded.
