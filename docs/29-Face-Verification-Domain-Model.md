# 29. Face Verification Domain Model

## Purpose

Sprint 5 adds safe metadata for one-to-one verification attempts. The domain model records what happened without storing biometric captures or vectors.

## Entity: FaceVerificationAttempt

`FaceVerificationAttempt` is append-only in normal UI flows and belongs to exactly one Student.

Required metadata:

- Student id.
- Optional face-template id used by the attempt.
- Verification purpose.
- Outcome and decision.
- Safe error code.
- Score, threshold, and score metric.
- Engine/model/template-format metadata.
- Attempt timestamp.
- Optional client request id for idempotency.
- Safe localized description key.
- Optional width, height, detected face count, quality score, and processing duration.

Not stored:

- Raw image.
- Image path.
- Base64 image.
- Face encoding or embedding bytes.
- Protected template bytes.
- Template fingerprint.
- Facial landmarks.
- Session code.
- Password.
- Browser camera stream.
- Model file path.
- Raw native exception.
- Attendance status or location.

## Enums

`FaceVerificationOutcome`:

- Matched.
- NotMatched.
- NoActiveConsent.
- NoActiveTemplate.
- RequiresReEnrollment.
- IncompatibleTemplate.
- NoFace.
- MultipleFaces.
- LowQuality.
- InvalidImage.
- RateLimited.
- EngineUnavailable.
- ProcessingFailed.
- Cancelled.

`FaceVerificationPurpose`:

- SelfTest.
- EngineValidation.
- FutureAttendance.

Sprint 5 UI uses only `SelfTest`.

`ScoreMetric`:

- CosineSimilarity.
- EuclideanDistance.
- EngineDefined.

## Compatibility Rule

Templates are compatible only when current diagnostics match the stored template metadata:

- Engine name.
- Engine version.
- Model name.
- Model version.
- Template format version.
- Embedding dimension.

An incompatible active template is rejected and marked for re-enrollment. The system must never compare embeddings produced by incompatible engines or models.

## Privacy Boundary

Domain objects can reference safe identifiers and metadata. They do not expose or require template bytes. Template protection and unprotection remain Infrastructure responsibilities.
