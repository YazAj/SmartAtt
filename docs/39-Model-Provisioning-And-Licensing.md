# 39. Model Provisioning And Licensing

## Local Model Directory

Default repository-root paths:

- `models/face-recognition/yunet/face_detection_yunet_2023mar.onnx`
- `models/face-recognition/sface/face_recognition_sface_2021dec.onnx`

When running `src/AttendAI.Web` locally, tracked configuration resolves these from the Web content root as:

- `../../models/face-recognition/...`

The repository-root `models/` folder and all `.onnx` files are ignored by Git.

## Provisioning Command

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts\setup-face-models.ps1
```

The script downloads only official OpenCV Zoo URLs, verifies minimum file size, rejects Git LFS pointer text files, verifies SHA-256, and never downloads biometric sample images.

## Models

| Model | File | SHA-256 | License | Official source |
| --- | --- | --- | --- | --- |
| YuNet | `face_detection_yunet_2023mar.onnx` | `8f2383e4dd3cfbb4553ea8718107fc0423210dc964f9f4280604804ed2552fa4` | MIT for OpenCV Zoo YuNet directory | `https://github.com/opencv/opencv_zoo/tree/main/models/face_detection_yunet` |
| SFace | `face_recognition_sface_2021dec.onnx` | `0ba9fbfa01b5270c96627c4ef784da859931e02f04419c829e83484087c34e79` | Apache-2.0 for OpenCV Zoo SFace directory | `https://github.com/opencv/opencv_zoo/tree/main/models/face_recognition_sface` |

## Redistribution Decision

Model binaries are not committed or redistributed in this repository. Local developers provision them into ignored directories after reviewing model licenses.

## Package Licenses

- OpenCvSharp4/OpenCvSharp4.runtime.win: NuGet runtime/managed packages for OpenCVSharp. Verify package license notices before redistribution.
- Microsoft.ML.OnnxRuntime 1.27.1: ONNX Runtime managed/native package; license review required for any packaged distribution.

## Safety Rules

- Do not place ONNX files in `wwwroot`.
- Do not commit model files, biometric samples, generated embeddings, database files, private keys, SDK license files, or Data Protection keys.
- Do not download random mirrored model files.
- Do not use internet photos or celebrity images for evaluation.
