# 33. Threshold Evaluation And Calibration

## Current Policy

Sprint 5 uses a configurable `FaceVerification:VerificationThreshold` and `FaceVerification:ScoreMetric` for Fake mode. Real mode uses `FaceRecognition:RealEngine:CosineSimilarityThreshold` so the Fake threshold cannot silently become the SFace threshold.

Default development values:

- Metric: `CosineSimilarity`.
- Direction: higher score is more similar.
- Threshold: `0.95`.

For Euclidean distance, lower score is more similar and a match requires score less than or equal to the threshold.

## Fake Engine Results

Fake-development workflow evidence:

- Same deterministic payload produced a Match.
- Different deterministic payload produced a No Match.
- Rate limiting produced a safe RateLimited attempt.

These results exercise policy plumbing only. They are not real biometric accuracy evidence.

## Real Calibration Status

Not Verified.

Model-only readiness is verified for OpenCV YuNet and SFace. Real threshold calibration remains Not Verified because no approved same-person pair, different-person sample, no-face sample, multiple-face sample, or poor-quality live capture set is available.

Current Real development values:

- Metric: `CosineSimilarity`.
- Direction: higher score is more similar.
- Threshold: `0.363`.
- Threshold status: `DevelopmentDefault`.
- SFace output dimension: `128`.

## Required Real Evaluation

Before real verification can be accepted:

- Collect approved samples outside Git.
- Run at least same-person, different-person, no-face, multiple-face, and low-quality scenarios.
- Record engine/model version, preprocessing, embedding dimension, score metric, score direction, threshold, and safe outcomes.
- Compare score distributions for same-person and different-person pairs.
- Estimate false acceptance and false rejection behavior.
- Select an operating threshold appropriate for the graduation-project risk model.
- Document limitations and do not claim statistical validation from a tiny sample set.

## False Acceptance And False Rejection

- False acceptance: a different person is incorrectly accepted as a match.
- False rejection: the same person is incorrectly rejected as no match.

Threshold calibration balances these risks. A stricter cosine threshold usually reduces false acceptance but can increase false rejection. A looser threshold does the reverse.

## Sprint 6 Gate

Sprint 6 attendance decisions must not depend on real biometric verification until this document contains actual approved-sample evidence and the readiness gate has passed.
