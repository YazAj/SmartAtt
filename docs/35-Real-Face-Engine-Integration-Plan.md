# 35. Real Face Engine Integration Plan

## Scope

This integration adds a local one-to-one face-recognition engine for Windows x64 localhost using OpenCvSharp, OpenCV YuNet, OpenCV SFace, and ONNX Runtime. It does not add attendance registration, location validation, liveness detection, one-to-many identification, cloud APIs, Python runtime requirements, or Sprint 6 behavior.

## Implementation Steps

1. Preserve `Fake`, `Real`, and `Disabled` modes behind `IFaceRecognitionEngine`.
2. Keep the default provider as `Fake` for automated tests and localhost demos.
3. Add `OpenCvSFaceRecognitionEngine` for Real mode only.
4. Load YuNet through OpenCvSharp `FaceDetectorYN`.
5. Use ONNX Runtime for SFace embedding inference because OpenCvSharp 4.13.0.20260627 exposes `FaceDetectorYN` but not `FaceRecognizerSF`.
6. Resolve model paths from the ASP.NET Core content root and require them to stay under the approved repository-root `models/face-recognition` directory.
7. Reject missing models, small files, Git LFS pointer text, hash mismatches, invalid models, wrong runtime architecture, and native-load failures safely.
8. Store only protected float embeddings, never raw images, aligned crops, or plain embeddings.
9. Reject Fake templates in Real mode and mark incompatible active templates for re-enrollment through the existing Sprint 5 flow.
10. Keep threshold status as `DevelopmentDefault` until approved local samples are evaluated.

## Status

Partially completed.

Implemented and verified:

- Real engine code and DI selection.
- Model provisioning script.
- YuNet model hash and load.
- SFace model hash, ONNX Runtime load, and tensor metadata.
- Deterministic float32 little-endian template codec.
- Fake-to-Real template incompatibility tests.
- No model binaries or biometric samples tracked by Git.

Not verified:

- Three-capture real enrollment with an authorized live Student.
- Same-person and different-person decisions.
- No-face, multiple-face, poor-quality outcomes using authorized local camera captures.
- Threshold calibration.
- Restart-and-reverify with a protected real template.
