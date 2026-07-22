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

This is a mandatory technical gate before Sprint 4 or any production Face Enrollment work.
