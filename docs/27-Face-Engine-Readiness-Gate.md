# 27. Face Engine Readiness Gate

## Gate Result

Partially completed.

Sprint 5 implements enrollment and one-to-one verification workflows with the deterministic fake engine for development, automated tests, and localhost demonstration. A local Real adapter is now integrated and model-only readiness is verified, but production real face enrollment and verification remain Not Verified until approved biometric sample testing and threshold calibration are complete.

## Current Engine Modes

| Mode | Enrollment | Verification | Production Safety | Notes |
| --- | --- | --- | --- | --- |
| Fake | Enabled only outside Production | Enabled only as development/demo when allowed | Not production-safe | Deterministic SHA-256 fake templates for tests and UI workflow; UI labels it as demo verification. |
| Disabled | Disabled | Disabled | Safe default | Shows localized unavailable message. |
| Real | Enabled when local model readiness passes | Enabled when local model readiness passes | Not production-certified | OpenCvSharp YuNet plus ONNX Runtime SFace adapter; requires re-enrollment of Fake templates and approved sample verification before completion. |

## Selected Production Engine

Local graduation-project engine selected for verification: OpenCV YuNet detector plus SFace recognizer. It is not production-certified and does not include liveness detection.

## License And Model Status

- Engine packages: `OpenCvSharp4` and `OpenCvSharp4.runtime.win` `4.13.0.20260627`; `Microsoft.ML.OnnxRuntime` `1.27.1`.
- Face detection model: OpenCV Zoo YuNet `face_detection_yunet_2023mar.onnx`.
- Embedding model: OpenCV Zoo SFace `face_recognition_sface_2021dec.onnx`.
- Model license: YuNet MIT, SFace Apache-2.0 per OpenCV Zoo model directories.
- Native/runtime dependencies: Windows x64, OpenCV native runtime verified by model-only POC.
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

Model-only readiness verified:

- YuNet model exists, hash matches, and loads.
- SFace model exists, hash matches, and loads.
- OpenCV native version: `4.13.0`.
- SFace input: `System.Single [1, 3, 112, 112]`.
- SFace output: `System.Single [1, 128]`.
- Gate result: `Runtime Ready - Local Sample Verification Pending`.

Not Verified: approved biometric samples, same-person/different-person outcomes, no-face/multiple-face camera captures, poor-quality camera captures, restart-and-reverify, and threshold calibration.
