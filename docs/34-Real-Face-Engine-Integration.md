# 34. Real Face Engine Integration

## Current Status

Not Verified. No production real face engine is integrated for Sprint 5.

## Required Engine Metadata

When selected, document:

- Engine/library name and version.
- Library license.
- Detection model name, source, and license.
- Embedding model name, source, and license.
- Native/runtime dependencies.
- Supported operating system.
- Local model directory.
- Configuration keys.
- Model input dimensions.
- Color-channel order.
- Resize, crop, and alignment behavior.
- Normalization.
- Embedding dimension.
- Similarity or distance metric.
- Threshold meaning.
- Template format version.
- Sprint 4 enrollment compatibility.
- Re-enrollment requirements.
- Known limitations.

## ONNX Runtime Note

ONNX Runtime is an inference runtime, not a complete face-recognition engine by itself. An ONNX solution still requires a licensed face detection model, licensed face embedding model, preprocessing/alignment code, template normalization, similarity or distance calculation, threshold calibration, and native/runtime deployment validation.

## Configuration Boundary

Model paths, native dependency paths, and secrets must remain in User Secrets, environment variables, or ignored local directories. Do not commit models, license keys, SDK activation files, or biometric samples unless licenses and privacy approval explicitly permit it.

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

Until then, Sprint 5 remains Partially completed and Fake mode remains development/demo only.
