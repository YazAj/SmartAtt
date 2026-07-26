# 40. Real Threshold Evaluation

## Current Threshold

Real mode uses:

- Metric: `CosineSimilarity`.
- Direction: higher is more similar.
- Development threshold: `0.363`.
- Threshold status: `DevelopmentDefault`.

This value is separated from the Fake engine threshold. It is not calibrated for AttendAI users.

## Evidence

Model-only readiness is verified. Authorized manual evidence now includes one same-person verification result:

- Same-person decision: Match.
- Same-person cosine similarity: `0.950795`.
- Same-person score distribution: Not Verified; only one score is recorded.
- Different-person score distribution: Not Verified; result was not supplied in the manual evidence available for this update.
- False-acceptance estimate: Not Verified.
- False-rejection estimate: Not Verified.
- Local calibration status: Not Verified; threshold remains `DevelopmentDefault`.

Manual closure evidence also verifies real biometric re-enrollment, replacement of the previous Fake template, and creation of a protected OpenCV-SFace template.

Remaining manual evidence not supplied for this update:

- Different-person verification result.
- No-face rejection result.
- Multiple-face rejection result.
- Poor-quality rejection result.
- Verification after application restart.
- Camera cleanup result.
- Confirmation that no attendance record was created.

## Required Calibration

Before production face enrollment or attendance work depends on Real mode:

1. Collect approved local samples outside Git.
2. Enroll each participating Student with three live captures.
3. Compare same-person probes against the active template.
4. Compare different-person probes with explicit consent.
5. Include no-face, multiple-face, dark, bright, blurry, too-far, and too-close samples.
6. Record only safe metadata: score, threshold, decision, outcome, model version, and processing time.
7. Select and document an operating threshold.
8. Mark threshold status as `LocallyEvaluated` or `Calibrated` only when evidence exists.

## Limitation

This integration does not implement liveness detection, anti-spoofing, depth checks, replay-attack prevention, or presentation-attack detection. A photograph or screen replay may still fool basic face recognition.
