# 37. Real Face Engine Test Plan

## Automated Tests

Automated tests must never require committed model binaries or biometric samples. The current suite covers:

- Real model path validation.
- Path traversal rejection.
- Missing YuNet model.
- Missing SFace model.
- Empty model file.
- Git LFS pointer rejection.
- Incorrect model hash.
- Invalid ONNX model safe failure.
- Empty image.
- Invalid image.
- YuNet no-face row interpretation.
- YuNet multiple-face row interpretation.
- Invalid landmarks.
- Too-small face filtering.
- Empty, non-finite, corrupt, and dimension-mismatched SFace template payloads.
- Float32 little-endian normalized template codec round trip.
- Cosine similarity and threshold equality behavior.
- Fake template incompatibility in Real mode.
- Real template metadata compatibility.
- Fake/Real/Disabled DI selection.
- Sprint 1-5 regression tests.

## Model-Only Verification

Run:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\setup-face-models.ps1
dotnet run --project experiments\AttendAI.RealFaceEngine.Poc -- --models-only
```

Expected model-only result:

- Windows x64 runtime.
- OpenCvSharp native library loads.
- YuNet detector model exists, hash matches, and loads.
- SFace recognizer model exists, hash matches, and loads.
- SFace input metadata is `System.Single [1, 3, 112, 112]`.
- SFace output metadata is `System.Single [1, 128]`.
- Gate remains `Runtime Ready - Local Sample Verification Pending`.

## Authorized Manual Sample Tests

Use only explicitly approved local captures stored under ignored local folders such as `artifacts/local-face-tests/`. Do not commit, copy, or publish images.

Required scenarios before completion:

- Same Student: enroll with three live captures, verify with a fourth capture, expected score above the evaluated threshold.
- Different person: with explicit permission, verify a different participant against the first Student template, expected score below threshold.
- No face: submit an object or blank scene, expected `NoFaceDetected`.
- Multiple faces: submit two consenting participants, expected `MultipleFacesDetected`.
- Poor quality: dark, bright, blurry, too-far, and too-close captures, expected quality rejection or safe engine failure.
- Restart persistence: restart app and verify the protected real template still works.

## Status

Automated and model-only readiness tests are implemented. Authorized live-camera and biometric sample tests are Not Verified.
