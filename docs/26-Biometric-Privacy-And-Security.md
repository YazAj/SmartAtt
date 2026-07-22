# 26. Biometric Privacy And Security

## Data Collected

- Explicit consent metadata: Student id, consent version, consent text hash, accepted/withdrawn timestamps, actor user ids.
- Protected face template bytes created by the configured face engine.
- Safe template metadata: engine/model names and versions, template format, embedding dimension, quality score, capture count, template version, enrollment/revocation timestamps, revocation reason.
- Safe audit events with event type, outcome, safe error code, actor, engine metadata, and localizable safe description key.

## Data Not Collected Or Retained

- Original camera frames or uploaded face images are not intentionally stored.
- Original filenames are not used.
- Raw template bytes are not stored unprotected.
- Template bytes are not rendered in Controllers, Razor Views, HTML, URLs, or public DTOs.
- One-to-many biometric search and duplicate-person detection are not implemented.
- Attendance decisions and location data are not implemented in Sprint 4.

## Consent

Students must review the biometric privacy notice and explicitly accept consent before enrollment. Consent is versioned with `ConsentVersion` and `ConsentTextHash`. Withdrawal is available from the Student biometric profile page and revokes the active template.

Admins cannot accept consent for Students through the normal UI.

## Template Protection

`IBiometricTemplateProtector` uses ASP.NET Core Data Protection. The application stores `ProtectedTemplate` only. Data Protection key material must be stored outside source control and configured for the deployment topology.

## Authorization

- Students access only their own biometric profile through authenticated user id.
- Admins can view safe oversight pages and take revocation actions.
- Instructors have no biometric management controls.
- Anonymous users are rejected.

## Request Validation

- Allowed MIME types are configurable.
- JPEG/PNG signatures are validated.
- Minimum and maximum dimensions are checked.
- Per-capture and total request byte limits are enforced.
- Unexpected multipart file fields are rejected.
- Enrollment attempts are rate-limited per user.
- State-changing forms use antiforgery.

## Revocation And Re-Enrollment

Student re-enrollment creates a new template version and revokes the old active template only after the new protected template is ready. Admin revocation requires a reason. Admin may require re-enrollment, which revokes the active template and marks the status accordingly.

## Threats And Mitigations

| Threat | Mitigation |
| --- | --- |
| Raw image retention | Captures are processed in memory and never saved to disk/database. |
| Plain template storage | Templates are protected with Data Protection before persistence. |
| Template exposure in UI | Public DTOs expose metadata only; views never render template bytes. |
| Unauthorized role access | Role attributes and integration tests cover Admin/Student/Instructor/anonymous boundaries. |
| Repeated enrollment abuse | In-memory rate limiter limits attempts per configured window. |
| Fake engine in production | Readiness service disables fake enrollment in Production. |
| Unsafe audit content | Events store safe error codes and localizable description keys only. |

## Remaining Production Risks

- No real engine is verified yet.
- No formal legal compliance claim is made.
- Full image decoding/decompression-bomb mitigation should be revisited when the real engine/image library is selected.
- Data retention period and disposal policy for revoked templates require product/legal approval.
- Data Protection keys need production storage/rotation policy.

