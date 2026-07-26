# 38. Real Face Engine Review

## Status

Partially completed.

The real local engine is integrated and model-only readiness is verified. Completion is blocked by missing authorized live-camera and same-person/different-person biometric test evidence.

## Automated Verification

- `dotnet build AttendAI.sln`: passed with 0 warnings and 0 errors after integration build checks.
- `dotnet test AttendAI.sln --no-build`: passed with 119 total tests, 119 passed, 0 failed, 0 skipped.
- Additional final restore/build/test/format evidence must be recorded in the final report after documentation changes.

## Model Readiness Evidence

`dotnet run --project experiments\AttendAI.RealFaceEngine.Poc -- --models-only` returned:

- Status: Ready.
- Gate: Runtime Ready - Local Sample Verification Pending.
- OpenCV version: 4.13.0.
- SFace input: `System.Single [1, 3, 112, 112]`.
- SFace output: `System.Single [1, 128]`.
- Confirmed embedding dimension: 128.
- Detector model: `yunet/face_detection_yunet_2023mar.onnx`.
- Recognizer model: `sface/face_recognition_sface_2021dec.onnx`.

## Security Review

- ONNX files are stored under ignored local paths.
- Biometric samples are not committed.
- Raw images, aligned crops, and embeddings are not written to disk by the engine or POC.
- Templates remain protected by the existing Data Protection flow.
- Template unprotection remains inside Infrastructure.
- Fake templates are incompatible with Real diagnostics and are marked for re-enrollment by the existing Sprint 5 service path.
- Native and model failures are mapped to safe error codes.

## Not Verified

- Real Student enrollment through browser camera.
- Real protected template stored in SQL Server.
- Same-person Match.
- Different-person No Match.
- No-face and multiple-face camera outcomes.
- Poor-quality live capture outcomes.
- Admin diagnostics viewed through a live browser session.
- Restart-and-reverify.

## Decision

Do not merge to main as Completed. The branch is suitable for review as a partially completed real-engine integration after final commands pass, but the real biometric gate must remain open until authorized sample testing is complete.
