# 27. Face Engine Readiness Gate

## Gate Result

Not Verified.

Sprint 5 implements enrollment and one-to-one verification workflows with the deterministic fake engine for development, automated tests, and localhost demonstration. Production real face enrollment and verification are not verified.

## Current Engine Modes

| Mode | Enrollment | Verification | Production Safety | Notes |
| --- | --- | --- | --- | --- |
| Fake | Enabled only outside Production | Enabled only as development/demo when allowed | Not production-safe | Deterministic SHA-256 fake templates for tests and UI workflow; UI labels it as demo verification. |
| Disabled | Disabled | Disabled | Safe default | Shows localized unavailable message. |
| Real | Disabled until adapter exists | Not verified until adapter and models pass the gate | Not verified | Placeholder mode documenting future production adapter requirement. |

## Selected Production Engine

No production engine is selected.

## License And Model Status

- Engine license: Not selected.
- Face detection model: Not selected.
- Embedding model: Not selected.
- Model license: Not verified.
- Native/runtime dependencies: Not verified.
- Approved biometric samples: Not available.

## Required Components For Real Engine

- Safe image decoder/preprocessor.
- Face detection model with no-face and multiple-face outcomes.
- Face alignment/preprocessing.
- Face embedding model.
- Template normalization/format definition.
- Quality metrics or reliable rejection strategy.
- Similarity/distance strategy for future Sprint 5 verification.
- One-to-one verification adapter implementation.
- Template compatibility strategy between enrollment and verification.
- Threshold calibration with approved validation samples.
- Model and runtime licensing review.
- Native/runtime deployment package for Windows/.NET hosting.

## Minimum Verification Scenario

Before the gate can pass, run approved local samples outside Git:

1. Valid single-face image.
2. No-face image.
3. Multiple-face image.
4. Low-quality face image.
5. Dark face image.
6. Blurry face image.
7. Alternate capture of the same approved subject.

Record only safe results: engine/model version, outcome code, face count, quality score, embedding dimension, and timing. Do not commit images or template bytes.

## Sprint 5 Compatibility

Sprint 5 added explicit compatibility metadata checks for engine name/version, model name/version, template format version, and embedding dimension. Incompatible active templates are rejected and marked for re-enrollment. Fake templates must not be treated as real biometric evidence and must not be silently accepted by a future real verifier.

Sprint 6 production attendance verification must not begin until this gate is passed with approved samples, selected licensed models, native/runtime deployment evidence, same-person and different-person results, no-face and multiple-face results, low-quality handling, and threshold calibration.

## Sprint 5 Verified Fake Evidence

- Fake-development enrollment and verification ran against SQL Server LocalDB in the Sprint 5 smoke scenario.
- Same deterministic payload produced a Match result.
- Different deterministic payload produced a No Match result.
- Rate limiting produced a safe RateLimited attempt.
- Admin details displayed safe attempt metadata only.

These are workflow/security tests, not real biometric accuracy tests.

## Sprint 5 Real Evidence

Not Verified. No approved biometric samples, selected licensed detection model, selected licensed embedding model, real adapter, native dependencies, threshold calibration set, or model-license package was available locally.
