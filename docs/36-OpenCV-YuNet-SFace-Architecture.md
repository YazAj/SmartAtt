# 36. OpenCV YuNet SFace Architecture

## Selected Stack

| Component | Selection |
| --- | --- |
| Managed CV package | `OpenCvSharp4` `4.13.0.20260627` |
| Native runtime | `OpenCvSharp4.runtime.win` `4.13.0.20260627` |
| OpenCV native version verified | `4.13.0` |
| Detector | OpenCV Zoo YuNet `face_detection_yunet_2023mar.onnx` |
| Recognizer | OpenCV Zoo SFace `face_recognition_sface_2021dec.onnx` |
| SFace inference runtime | `Microsoft.ML.OnnxRuntime` `1.27.1` |
| Execution mode | CPU default, Windows x64 localhost |

The full Windows OpenCvSharp runtime is used, not a slim runtime, so OpenCV DNN support is available for `FaceDetectorYN`.

## Why ONNX Runtime Is Used For SFace

OpenCvSharp 4.13.0.20260627 exposes `FaceDetectorYN` but does not expose the `FaceRecognizerSF` wrapper. The adapter therefore keeps OpenCvSharp for decoding, YuNet detection, landmarks, alignment, and blob preprocessing, then runs SFace through ONNX Runtime. ONNX Runtime is only an inference runtime; the detector, alignment, preprocessing, normalization, similarity calculation, thresholding, licensing, and deployment checks remain application responsibilities.

## Processing Flow

1. Decode JPEG/PNG bytes in memory with OpenCvSharp.
2. Compute safe brightness and sharpness scores.
3. Load YuNet with configured confidence, NMS, and top-K values.
4. Interpret YuNet rows as box, five landmarks, and confidence.
5. Require exactly one accepted face.
6. Reject low-confidence, invalid box, invalid landmarks, too-small face, and too-large face cases.
7. Align using the five landmarks and the OpenCV SFace destination points.
8. Warp to `112x112`.
9. Create an SFace blob with scale `1`, mean `(0,0,0)`, `swapRB=true`, `crop=false`, NCHW layout.
10. Run ONNX Runtime inference.
11. Require finite 128-d output.
12. L2-normalize and serialize as float32 little-endian.

Enrollment and verification use the same `OpenCvSFaceRecognitionEngine` path.

## Verified Model Metadata

Model-only POC result:

- OpenCV version: `4.13.0`.
- SFace input: `System.Single [1, 3, 112, 112]`.
- SFace output: `System.Single [1, 128]`.
- Confirmed embedding dimension: `128`.

## Native Deployment

`OpenCvSharp4.runtime.win` deploys Windows native DLLs from the NuGet runtime package, including `OpenCvSharpExtern.dll` for win-x64. The target runtime is Windows x64 with the Visual C++ runtime requirements of the OpenCvSharp native package. The application readiness service catches native load failures and reports a safe unavailable state.
