# 23. Biometric Enrollment Domain Model

## Entities

### BiometricConsent

Represents explicit Student consent for a versioned biometric privacy notice.

Important fields:

- `StudentId`
- `ConsentVersion`
- `ConsentTextHash`
- `AcceptedAtUtc`
- `WithdrawnAtUtc`
- `AcceptedByUserId`
- `WithdrawnByUserId`
- `IsActive`
- `RowVersion`

Rules:

- Only one active consent may exist per Student.
- Consent is explicitly accepted by the Student.
- Withdrawal is preserved historically and cannot be reactivated.
- Withdrawal revokes the active face template through the application service transaction.

### StudentFaceTemplate

Stores only the protected face template and safe metadata.

Important fields:

- `StudentId`
- `BiometricConsentId`
- `ProtectedTemplate`
- `TemplateFingerprint`
- `EngineName`, `EngineVersion`
- `ModelName`, `ModelVersion`
- `TemplateFormatVersion`
- `EmbeddingDimension`
- `QualityScore`
- `CaptureCount`
- `TemplateVersion`
- `EnrolledAtUtc`
- `RevokedAtUtc`
- `RevokedByUserId`
- `RevocationReason`
- `RequiresReEnrollment`
- `IsActive`
- `RowVersion`

Rules:

- Template creation requires active consent.
- Only one active template may exist per Student.
- Re-enrollment creates a new version and revokes the prior active template transactionally after the new protected template is ready.
- Revoked templates cannot be reactivated.
- Template bytes are never exposed by public DTOs or Razor views.

### FaceEnrollmentEvent

Append-only safe audit record for consent, enrollment, re-enrollment, revocation, and failure events.

Events store only safe metadata:

- Student/template/consent identifiers.
- Event type.
- Outcome key.
- Safe error code.
- Actor user id.
- Engine name/version.
- Localizable safe description key.

Events never store raw images, plain template bytes, biometric vectors, passwords, or session codes.

## Enums

- `FaceEnrollmentStatus`: `NotEnrolled`, `EnrollmentInProgress`, `Active`, `RequiresReEnrollment`, `Revoked`, `ConsentWithdrawn`, `EngineUnavailable`.
- `FaceEnrollmentEventType`: `ConsentAccepted`, `ConsentWithdrawn`, `EnrollmentStarted`, `CaptureRejected`, `TemplateCreated`, `ReEnrollmentStarted`, `TemplateReplaced`, `TemplateRevoked`, `AdminRevoked`, `EnrollmentFailed`, `EngineUnavailable`.
- `FaceCaptureRejectionReason`: safe rejection taxonomy for no face, multiple faces, invalid images, quality failures, engine availability, and processing failures.

## Service Boundary

Domain entities enforce invariants and state transitions. Application contracts define DTOs and operations. Infrastructure owns EF persistence, Data Protection, image validation, rate limiting, and engine coordination. Web controllers remain thin and do not access template bytes directly.

