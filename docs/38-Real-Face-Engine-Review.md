# 38. Real Face Engine Review

## Status

Partially completed.

The real local engine is integrated and model-only readiness is verified. Real biometric re-enrollment and one same-person Match are verified. Completion is still blocked by missing different-person, no-face, multiple-face, poor-quality, restart, camera-cleanup, and no-attendance-record manual evidence.

## Automated Verification

- `dotnet build AttendAI.sln`: exited 0 with 0 warnings and 0 errors after the manual-evidence documentation update.
- `dotnet test AttendAI.sln`: exited 0 with 119 total tests, 119 passed, 0 failed, 0 skipped.
- `dotnet format AttendAI.sln --verify-no-changes`: exited 0 with no formatting changes.
- `git diff --check`: exited 0; Git printed CRLF conversion notices only.

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
- No raw biometric images were committed or intentionally retained.
- Raw images, aligned crops, and embeddings are not written to disk by the engine or POC.
- Templates remain protected by the existing Data Protection flow.
- Template unprotection remains inside Infrastructure.
- Fake templates are incompatible with Real diagnostics and are marked for re-enrollment by the existing Sprint 5 service path.
- Native and model failures are mapped to safe error codes.

## Manual Real-Face Evidence

| Scenario | Status | Evidence |
| --- | --- | --- |
| Real Student re-enrollment | Verified | Real biometric re-enrollment completed. |
| Previous Fake template replacement | Verified | The previous Fake template was replaced. |
| Protected OpenCV-SFace template | Verified | A protected OpenCV-SFace template was created. |
| Same-person verification | Verified | Returned Match with cosine similarity `0.950795`. |
| Different-person verification | Not Verified | Result was not supplied in the manual evidence available for this update. |
| No-face rejection | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Multiple-face rejection | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Poor-quality rejection | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Verification after application restart | Not Verified | Result was not supplied in the manual evidence available for this update. |
| Camera cleanup | Not Verified | Result was not supplied in the manual evidence available for this update. |
| No attendance record created | Not Verified | Confirmation was not supplied in the manual evidence available for this update. |

## Not Verified

- Different-person No Match.
- No-face and multiple-face camera outcomes.
- Poor-quality live capture outcomes.
- Admin diagnostics viewed through a live browser session.
- Restart-and-reverify.
- Camera cleanup.
- Confirmation that no attendance record was created.
- Threshold calibration.

## Decision

Do not merge to main as Completed. The branch is suitable for review as a partially completed real-engine integration after final commands pass, but the real biometric gate must remain open until the missing manual evidence is recorded. Liveness and anti-spoofing are not implemented and must not be claimed.
