# 07. Face Recognition Technical Spike

## Goal

Assess a realistic path for one-to-one face verification without training a model from scratch and without making Sprint 1 depend on a commercial license.

## Application Abstraction

Implemented in `src/AttendAI.Application/FaceRecognition`:

- `IFaceRecognitionEngine`
- `FaceEncodingResult`
- `FaceVerificationResult`
- `StoredFaceTemplate`
- `FaceRecognitionErrorCode`

Infrastructure implements `FakeFaceRecognitionEngine` for deterministic automated testing.

## Engine Evaluation

### FaceRecognitionDotNet

- Source: https://github.com/takuya-takeuchi/FaceRecognitionDotNet
- NuGet: https://www.nuget.org/packages/FaceRecognitionDotNet/
- License: MIT for FaceRecognitionDotNet. Its README lists dlib under Boost, DlibDotNet under MIT, face_recognition under MIT, and face_recognition_models under CC0.
- Compatibility: NuGet lists .NET Standard 2.0 and computed .NET 8 compatibility.
- Native dependencies: DlibDotNet/dlib native runtime dependencies.
- Release state observed: latest NuGet version `1.3.0.7`, last updated July 29, 2022.
- Limitation: native packaging and model-file handling must be verified on the target deployment environment before production use.

### ONNX Runtime Alternative

- Source: https://github.com/microsoft/onnxruntime
- C# docs: https://onnxruntime.ai/docs/get-started/with-csharp.html
- NuGet: https://www.nuget.org/packages/Microsoft.ML.OnnxRuntime
- License: MIT.
- Compatibility: C# API supports .NET Standard; NuGet provides native CPU packages for supported platforms and optional GPU packages.
- Limitation: ONNX Runtime is an inference runtime, not a complete face-recognition engine by itself. AttendAI must choose and validate licensed face detection and embedding models separately.

An ONNX-based production solution requires:

- Face detection model.
- Face embedding model.
- Image preprocessing and alignment.
- Template normalization.
- Similarity or distance calculation.
- Threshold calibration with approved validation samples.
- Model licensing review.
- Native/runtime deployment validation for the target OS and hosting environment.

### KBY-AI

Not selected for Sprint 1 because commercial licensing and device activation concerns conflict with the requirement to avoid a commercial production dependency during the foundation sprint.

## POC Implementation

Project: `experiments/AttendAI.FaceRecognition.Poc`

Commands:

```bash
dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <image-path>
dotnet run --project experiments/AttendAI.FaceRecognition.Poc -- <reference-image-path> <probe-image-path>
```

The Sprint 1 POC harness loads the engine once, reads local paths from command-line arguments, extracts a deterministic fake template, compares two samples, and reports measurable similarity.

Negative fake-engine markers:

- `NO_FACE`
- `MULTI_FACE`

## Actual Verified Results

- The POC project builds as part of `dotnet build AttendAI.sln`.
- Automated tests verify empty image, no-face marker, multiple-face marker, matching payload, and non-matching payload behavior.
- The fake-engine POC harness was executed with ignored non-biometric local payloads:
  - Same payload: `Succeeded=True`, `IsMatch=True`, `Similarity=1.000`.
  - Different payload: `Succeeded=True`, `IsMatch=False`, `Similarity=0.000`.
  - `NO_FACE` marker: `Succeeded=False`, `Code=NoFaceDetected`.
  - `MULTI_FACE` marker: `Succeeded=False`, `Code=MultipleFacesDetected`.
- Real face recognition runtime was not executed because no approved local biometric samples, selected face detection model, selected embedding model, or native/model dependencies were available.

## Recommendation

For production, prefer an ONNX Runtime adapter with explicitly licensed face detection and embedding models if the team can validate accuracy, thresholding, deployment packaging, and privacy handling. Use FaceRecognitionDotNet as a fallback if its native dlib dependency chain is acceptable in the final deployment environment.

## Pending Manual Verification

- Supply approved local face samples outside Git.
- Install selected native/model dependencies.
- Run one same-person and one different-person comparison.
- Record thresholds, false match behavior, and deployment constraints.

This is a mandatory technical gate before production face verification, Sprint 5 attendance verification, or marking Sprint 4 fully complete.

## Sprint 4 Gate Update

Sprint 4 introduced application support for explicit engine modes:

- `Fake`: deterministic development/test engine; may support UI and automated workflow testing; blocked in Production enrollment by readiness logic.
- `Disabled`: enrollment unavailable.
- `Real`: reserved for a future production adapter; currently Not Verified.

The fake engine now returns safe template metadata used by the enrollment workflow. It remains unsuitable for production biometric enrollment and must not be represented as a real biometric engine.

Sprint 4 still has no selected production engine. A real ONNX-based option would require face detection, face embedding, preprocessing/alignment, template normalization, similarity/distance strategy, threshold calibration, licensing review, and native/runtime deployment packaging. See `docs/27-Face-Engine-Readiness-Gate.md`.

## Sprint 5 Update

Sprint 5 uses the same `IFaceRecognitionEngine` abstraction for one-to-one self-verification. The verified runtime evidence is limited to the deterministic fake-development engine. Fake verification is useful for exercising workflow, authorization, persistence, localization, and threshold-policy plumbing, but it is not biometric evidence.

The real-engine gate remains Not Verified:

- Engine/library: Not selected.
- Detection model: Not selected.
- Embedding model: Not selected.
- Model licenses: Not verified.
- Native/runtime package: Not verified.
- Approved same-person samples: Not available.
- Approved different-person samples: Not available.
- No-face and multiple-face real image tests: Not executed.
- Threshold calibration: Not performed.

Sprint 5 added explicit template compatibility checks using engine name/version, model name/version, template format version, and embedding dimension. Incompatible active templates are rejected safely and marked for re-enrollment. This prevents fake templates from being silently accepted by a future real verifier.

## Real Engine Integration Update

The selected local adapter is OpenCvSharp YuNet plus ONNX Runtime SFace:

- OpenCvSharp4/OpenCvSharp4.runtime.win `4.13.0.20260627`.
- Microsoft.ML.OnnxRuntime `1.27.1`.
- YuNet `face_detection_yunet_2023mar.onnx`, MIT license in OpenCV Zoo.
- SFace `face_recognition_sface_2021dec.onnx`, Apache-2.0 license in OpenCV Zoo.
- Verified OpenCV native version: `4.13.0`.
- Verified SFace input: `System.Single [1, 3, 112, 112]`.
- Verified SFace output: `System.Single [1, 128]`.

ONNX Runtime remains only the SFace inference runtime. AttendAI still owns detection, five-landmark alignment, preprocessing, template normalization, cosine comparison, threshold policy, licensing, deployment, and privacy controls.

Before Sprint 6 production attendance work depends on face verification, the team must provide approved local samples outside Git, document same-person and different-person scores, verify no-face/multiple-face/low-quality handling, and calibrate a threshold. Do not commit biometric images, model files, embeddings, or license keys.
