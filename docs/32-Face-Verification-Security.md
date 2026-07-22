# 32. Face Verification Security

## Boundary

Sprint 5 performs Student self-verification only. It verifies the authenticated Student against that Student's active compatible template and never searches across Students.

## Student Ownership

The controller passes the authenticated Identity user id to the service. The browser does not post StudentId, face template id, template bytes, or embeddings. Infrastructure resolves the Student profile from the authenticated user.

## Capture Handling

- One verification capture is accepted.
- Request size, file size, field name, MIME type, signature, and image dimensions are checked.
- Captures are processed in memory.
- Captures are not stored in SQL Server, disk, logs, query strings, localStorage, sessionStorage, or IndexedDB by the verification workflow.
- Camera tracks are stopped on submit and page unload where the browser permits it.

## Template Handling

- Active consent is required.
- An active Student template is required.
- Template metadata is checked against current engine diagnostics.
- Template bytes are loaded and unprotected inside Infrastructure only.
- Unprotected template buffers are zeroed where practical after verification.
- Web DTOs and Razor views never receive protected template bytes, unprotected bytes, embeddings, or template fingerprints.

## Decision Policy

The configured `ScoreMetric` controls score direction:

- Cosine similarity: match when score is greater than or equal to threshold.
- Euclidean distance: match when score is less than or equal to threshold.
- Engine-defined: currently treated conservatively by policy and must be documented by a real adapter before production use.

## Rate Limiting And Idempotency

`IFaceVerificationRateLimiter` enforces cooldown and maximum attempts per window. `ClientRequestId` supports duplicate browser submission protection and is bounded to safe length. The database includes a filtered unique index on `(StudentId, ClientRequestId)` when the request id is not empty.

## Admin Access

Admin pages show safe attempt metadata and diagnostics. They do not expose captures, embeddings, templates, fingerprints, model paths, or raw native exceptions.

## Logging Restrictions

Do not log:

- Passwords.
- Raw images.
- Image base64.
- Face encodings or embeddings.
- Protected or unprotected templates.
- Template fingerprints.
- Session codes.
- Model file paths.
- Raw native engine exceptions.

## Threats And Mitigations

| Threat | Sprint 5 Mitigation | Remaining Risk |
| --- | --- | --- |
| Cross-Student verification | Student resolved from authenticated user; no posted StudentId accepted. | Future attendance flows must keep the same ownership rule. |
| Template exposure | Template access stays in Infrastructure; DTOs/views are safe. | Production logging and diagnostics must preserve this boundary. |
| Duplicate submissions | Client request id plus unique index and rate limit. | Distributed rate limit would be needed for multi-server production. |
| Fake engine mistaken for real | UI warning and documentation label it as development/demo. | Demo operators must not present fake results as biometric accuracy. |
| Photo spoofing | No production liveness claim is made. | Liveness/anti-spoofing requires separate future work. |
| Real engine model mismatch | Compatibility check rejects mismatches and marks re-enrollment. | Real compatibility must be documented per selected model. |

## Localhost Scope

AttendAI remains a localhost graduation-project prototype. The architecture still treats biometric material as sensitive and keeps production gates explicit.
