# 31. Sprint 5 Review

## Final Status

Partially completed.

The one-to-one verification architecture, database migration, fake-development runtime workflow, Student/Admin UI, authorization, security controls, localization resources, and tests are implemented. Real model readiness is verified, and one authorized same-person real verification has now passed. Sprint 5 remains partially completed because different-person, no-face, multiple-face, poor-quality, restart, camera-cleanup, and no-attendance-record manual evidence was not supplied for this closure update.

## Automated Verification

Final command results on 2026-07-26 after the manual-evidence documentation update:

| Command | Exit | Warnings | Errors | Result |
| --- | --- | --- | --- | --- |
| `dotnet build AttendAI.sln` | 0 | 0 | 0 | Build succeeded. |
| `dotnet test AttendAI.sln` | 0 | 0 | 0 | 119 total, 119 passed, 0 failed, 0 skipped. |
| `dotnet format AttendAI.sln --verify-no-changes` | 0 | 0 | 0 | No formatting changes required. |
| `git diff --check` | 0 | 0 blocking | 0 | Whitespace check passed; Git printed CRLF conversion notices only. |
| `git status` | 0 | n/a | n/a | Shows documentation-only changes from this closure update. |

Implemented automated coverage now reports 74 unit tests and 45 integration tests.

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

## Manual Real-Face Closure Evidence

Evidence recorded on 2026-07-26 from the authorized manual real-face verification update:

| Scenario | Status | Evidence |
| --- | --- | --- |
| Real biometric re-enrollment | Verified | Re-enrollment completed in Real mode. |
| Previous Fake template replacement | Verified | Previous Fake template was replaced. |
| Protected OpenCV-SFace template | Verified | A protected OpenCV-SFace template was created. |
| Same-person verification | Verified | Returned Match with cosine similarity `0.950795`. |
| Different-person verification | Not Verified | Result was not supplied in the manual evidence available for this update. |
| No-face rejection | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Multiple-face rejection | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Poor-quality rejection | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Verification after application restart | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Camera cleanup | Not Verified | Result was not supplied in the manual evidence available for this update. |
| No attendance record created | Not Verified | Confirmation was not supplied in the manual evidence available for this update. |
| Raw biometric image retention | Verified | No raw biometric images were committed or intentionally retained. |
| Liveness and anti-spoofing | Not Implemented | No liveness, anti-spoofing, depth, replay, or presentation-attack protection is implemented or claimed. |

## Security Verification

- Student id is resolved from the authenticated user.
- Controllers do not receive template bytes.
- Views do not render protected template, template fingerprint, embedding bytes, or captured image bytes.
- Captures are not stored in SQL Server or disk by the verification workflow.
- Antiforgery is required for verification POSTs.
- Rate limiting and idempotency are implemented.
- Fake mode is visibly labeled as development/demo.

## Not Verified

- Different-person comparison.
- Real no-face, multiple-face, and low-quality behavior.
- Restart-and-reverify after application restart.
- Camera cleanup after manual verification.
- Confirmation that no attendance record was created.
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

Manual evidence now verified:

- Real biometric re-enrollment completed.
- Previous Fake template replaced.
- Protected OpenCV-SFace template created.
- Approved same-person comparison returned Match with cosine similarity `0.950795`.

Still Not Verified:

- Approved different-person comparison.
- No-face, multiple-face, and poor-quality camera behavior.
- Restart-and-reverify with a protected real template.
- Camera cleanup.
- Confirmation that no attendance record was created.
- Threshold calibration.
- Production liveness or anti-spoofing.

Sprint 5 remains partially completed for real biometric verification until those remaining items are recorded.

## Sprint 6 Scope Note

Sprint 6 now implements attendance check-in separately from this Sprint 5 review. The Sprint 5 evidence above remains evidence for one-to-one self-verification only. Any attendance acceptance, duplicate prevention, geofence validation, or Instructor roster evidence must be recorded in `docs/44-Sprint-6-Review.md` and must not be inferred from Sprint 5 self-verification evidence.
