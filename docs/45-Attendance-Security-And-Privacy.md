# 45. Attendance Security And Privacy

## Trust Boundaries

The attendance check-in workflow trusts server-side data only for Student identity, enrollment, lecture session, timing, classroom policy, face template, and attendance decisions.

The browser may provide:

- A captured image file.
- Browser geolocation coordinates and accuracy.
- A one-time challenge token.
- An idempotency key.

The browser must not provide:

- Student id.
- Attendance status.
- Classroom policy.
- Approved radius.
- Face template id.
- Template bytes.
- Face score, threshold, or decision.

## Check-In Controls

- Student identity is resolved from the authenticated user id.
- Student account/profile must be active.
- First-login password change must be complete.
- Student must be actively enrolled in the lecture section.
- Lecture session must be active and inside the check-in window.
- Attendance challenge must be valid, unexpired, and unused.
- Idempotency key prevents repeated submit side effects.
- Unique `(LectureSessionId, StudentId)` index prevents concurrent duplicate attendance records.
- Browser location is validated server-side against the session policy snapshot.
- Face verification must be a fresh one-to-one check for purpose `FutureAttendance`.
- Attendance requires Real mode diagnostics and blocks Fake/demo mode.

## Data Retention

Attendance stores:

- Attendance status.
- Check-in timestamp.
- Related Student, lecture session, classroom, face verification attempt, and attendance attempt ids.
- Distance from approved classroom policy.
- Browser accuracy.
- Allowed radius and maximum accepted accuracy snapshots.
- Safe outcome/failure enum values.
- Processing duration.

Attendance does not store:

- Raw biometric image.
- Face crop or aligned crop.
- Base64 capture.
- Unprotected embedding.
- Protected template bytes.
- Template fingerprint.
- Exact submitted Student latitude or longitude.
- Plain challenge token.
- Plain idempotency key.
- Device fingerprint.
- Native exception details.

## Replay And Duplicate Handling

Challenge tokens are random and stored only as SHA-256 hashes. A consumed challenge cannot be reused. Rejected attempts consume valid challenges by default to reduce replay opportunities. Idempotency hashes allow repeated browser submits to return the original attempt result. A unique database index is the final duplicate-attendance guard.

These controls reduce duplicate and basic replay risks. They do not constitute complete replay-attack prevention.

## Location Privacy

The submitted latitude/longitude is used only in memory to calculate distance. The database stores distance and accuracy metadata, not exact Student coordinates. Classroom/session policy coordinates are configuration/academic resource data and may be stored as part of classroom and lecture-session policy snapshots.

## Biometric Privacy

Attendance composes the existing one-to-one verifier. Template unprotection remains inside Infrastructure. Attendance does not expose templates, embeddings, template fingerprints, or raw image bytes to Web DTOs or views.

## Sprint 6 Closure Evidence

Sprint 6 is marked Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence.

The project owner confirmed:

- Successful same-person attendance using real OpenCV YuNet/SFace one-to-one verification.
- Different-person, no-face, multiple-face, and poor-quality captures were rejected.
- No successful `AttendanceRecord` was created for rejected face or location attempts.
- Browser location permission denial, outside-geofence location, and insufficiently accurate location were handled safely.
- Closed attendance session, duplicate attendance, repeated submit, multiple-tab, and restart persistence behavior were verified.
- Instructor roster and attendance count updated correctly.
- Camera stream stopped after completion.
- Client-side captured-image state and location state were cleared.
- No raw camera image, aligned face image, embedding, or face-template bytes were intentionally retained in attendance tables.
- Exact Student coordinates were not retained by default.
- Sprint 5 standalone face verification did not create `AttendanceRecord`.

Physical-versus-simulated location method was not recorded in the closure evidence.

## Sprint 7 Reporting And Export Privacy

Sprint 7 reports read safe attendance projections only. CSV export does not include raw biometric images, aligned images, embeddings, protected template bytes, template fingerprints, exact submitted Student coordinates, challenge hashes, idempotency hashes, native model paths, raw exception details, passwords, or password hashes.

Attendance reports may include safe academic labels, session times, Present/Late/derived Missed status, check-in timestamp, and summary percentages. Downloaded CSV files are sensitive university records and should be handled according to local institutional policy.

## Limitations

- Browser geolocation can be spoofed. Sprint 6 uses it as a policy control but does not implement device attestation.
- No liveness or anti-spoofing is implemented.
- A printed photo, replayed screen, or presentation attack may still fool basic face recognition.
- Complete replay-attack prevention, tamper-proof browser geolocation, GPS anti-spoofing, production biometric certification, production fraud prevention, population-wide threshold calibration, one-to-many identification, and classroom surveillance are not implemented or claimed.
- This is an academic Localhost implementation.
