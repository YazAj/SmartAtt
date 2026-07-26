# 34. Real Face Engine Integration

## Current Status

Partially completed. A local real engine adapter is integrated for Sprint 5 using OpenCvSharp YuNet detection and ONNX Runtime SFace embeddings. Model-only readiness is verified, but approved live-camera biometric enrollment and one-to-one same-person/different-person verification are not verified.

## Required Engine Metadata

Selected metadata:

- Engine: `OpenCV-SFace`.
- Managed packages: `OpenCvSharp4` `4.13.0.20260627`, `OpenCvSharp4.runtime.win` `4.13.0.20260627`, `Microsoft.ML.OnnxRuntime` `1.27.1`.
- OpenCV native version verified: `4.13.0`.
- Detector: OpenCV Zoo YuNet `face_detection_yunet_2023mar.onnx`, MIT license in the OpenCV Zoo YuNet directory.
- Recognizer: OpenCV Zoo SFace `face_recognition_sface_2021dec.onnx`, Apache-2.0 license in the OpenCV Zoo SFace directory.
- Local model directory: repository-root `models/face-recognition`, configured from the Web content root as `../../models/face-recognition`.
- SFace input: `System.Single [1, 3, 112, 112]`.
- SFace output: `System.Single [1, 128]`.
- Preprocessing: YuNet five landmarks, SFace 112x112 alignment, `blobFromImage` scale `1`, mean `(0,0,0)`, `swapRB=true`, `crop=false`, NCHW.
- Template format: `opencv-sface-f32le-v1`.
- Serialized template: finite L2-normalized float32 little-endian vector.
- Score metric: cosine similarity.
- Real threshold: `0.363`, status `DevelopmentDefault`.
- Compatibility: Fake templates are incompatible in Real mode and require re-enrollment.

## ONNX Runtime Note

ONNX Runtime is an inference runtime, not a complete face-recognition engine by itself. An ONNX solution still requires a licensed face detection model, licensed face embedding model, preprocessing/alignment code, template normalization, similarity or distance calculation, threshold calibration, and native/runtime deployment validation.

## Configuration Boundary

Model paths, native dependency paths, and secrets must remain in User Secrets, environment variables, or ignored local directories. Do not commit models, license keys, SDK activation files, generated embeddings, or biometric samples. Model binaries must not be placed in `wwwroot`.

## Adapter Requirements

A real adapter must:

- Initialize and health-check model/runtime dependencies.
- Reject unavailable dependencies safely.
- Decode and preprocess images consistently for enrollment and verification.
- Detect no-face and multiple-face cases.
- Produce stable embeddings with documented dimension and normalization.
- Compare one-to-one only.
- Return safe error codes.
- Avoid logging images, embeddings, templates, model paths, and raw native exceptions.
- Expose safe diagnostics for Admin readiness pages.

## Verification Gate

The gate can pass only after actual local evidence proves:

- Engine initializes.
- Models load.
- No-face image is rejected.
- Multiple-face image is rejected.
- Valid single-face image produces an embedding.
- Same-person pair compares as expected.
- Different-person pair compares as expected.
- Embedding dimensions are consistent.
- Enrollment and verification preprocessing match.
- Template format is compatible.
- Low-quality handling works.
- No raw image or template is retained or logged.
- Threshold behavior is documented.
- Runtime dependencies and model licenses are documented.

Current model-only evidence:

- `scripts/setup-face-models.ps1`: downloaded and verified both official OpenCV Zoo models.
- `dotnet run --project experiments\AttendAI.RealFaceEngine.Poc -- --models-only`: Ready.
- Gate result: `Runtime Ready - Local Sample Verification Pending`.
- Authorized sample evaluation: Not Verified.

Until authorized live-sample testing is complete, Sprint 5 remains Partially completed and Fake mode remains development/demo only.
