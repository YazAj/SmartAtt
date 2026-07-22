# 27. Face Engine Readiness Gate

## Gate Result

Not Verified.

Sprint 4 implements the enrollment workflow with the deterministic fake engine for development and automated tests. Production real face enrollment is not verified.

## Current Engine Modes

| Mode | Enrollment | Production Safety | Notes |
| --- | --- | --- | --- |
| Fake | Enabled only outside Production | Not production-safe | Deterministic SHA-256 fake templates for tests and UI workflow. |
| Disabled | Disabled | Safe default | Shows localized unavailable message. |
| Real | Disabled until adapter exists | Not verified | Placeholder mode documenting future production adapter requirement. |

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

Sprint 5 must not begin production attendance verification until this gate is passed. Fake-engine workflow can continue supporting automated tests and UI development, but fake templates must not be treated as real biometric evidence.

