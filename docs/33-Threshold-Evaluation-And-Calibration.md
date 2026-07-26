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

Partially evaluated.

Model-only readiness is verified for OpenCV YuNet and SFace. One authorized same-person verification returned Match with cosine similarity `0.950795`. This proves the enrolled protected OpenCV-SFace template can match a same-person probe, but it is not enough to calibrate a production threshold.

Current Real development values:

- Metric: `CosineSimilarity`.
- Direction: higher score is more similar.
- Threshold: `0.363`.
- Threshold status: `DevelopmentDefault`.
- SFace output dimension: `128`.

Recorded real evidence:

| Scenario | Result |
| --- | --- |
| Real biometric re-enrollment | Verified completed. |
| Previous Fake template replacement | Verified replaced. |
| Protected OpenCV-SFace template creation | Verified created. |
| Same-person verification | Match, cosine similarity `0.950795`. |
| Different-person verification | Not Verified; result not supplied in the manual evidence available for this update. |
| No-face rejection | Not Verified; result not supplied in the manual evidence available for this update. |
| Multiple-face rejection | Not Verified; result not supplied in the manual evidence available for this update. |
| Poor-quality rejection | Not Verified; result not supplied in the manual evidence available for this update. |
| Verification after application restart | Not Verified; result not supplied in the manual evidence available for this update. |

## Required Real Evaluation

Before real verification can be accepted:

- Collect approved samples outside Git.
- Run and record different-person, no-face, multiple-face, and low-quality scenarios.
- Run and record verification after application restart.
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

Sprint 6 attendance implementation requires Real diagnostics and blocks Fake/demo mode. Sprint 6 must remain partially completed for production-readiness purposes until this document contains approved-sample same-person and different-person score distributions, rejection scenarios, and an accepted operating threshold.

Liveness detection, anti-spoofing, depth checks, replay prevention, and presentation-attack detection are not implemented and must not be claimed.
