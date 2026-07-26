# 46. Location Geofence Policy

## Purpose

Sprint 6 uses browser geolocation as one attendance factor. The server, not the browser, decides whether the reported location is acceptable.

## Policy Inputs

Lecture sessions snapshot attendance policy at start:

- Latitude.
- Longitude.
- Allowed radius in meters.
- Maximum accepted browser accuracy in meters.
- Whether location verification is required.

The snapshot is normally copied from the active classroom coordinates and Attendance configuration.

## Browser Inputs

The browser submits:

- Latitude.
- Longitude.
- Accuracy in meters.
- Browser timestamp when available.

The browser timestamp is informational. Server UTC time controls challenge expiry, session window, and attendance status.

## Validation Rules

The server rejects:

- Missing required server policy.
- NaN or infinite values.
- Latitude outside `-90..90`.
- Longitude outside `-180..180`.
- Negative accuracy.
- Browser accuracy above the configured maximum.
- Distance greater than the allowed radius.

The server accepts:

- Distance less than or equal to the allowed radius.
- Accuracy less than or equal to the maximum accepted accuracy.

The radius is not expanded by the browser accuracy value. This is intentionally conservative.

## Distance Calculation

The implementation uses the Haversine formula with an earth radius of `6,371,000` meters. The computed distance is rounded to three decimal places for safe metadata persistence.

## Stored Metadata

Stored:

- Distance in meters.
- Browser accuracy in meters.
- Allowed radius snapshot.
- Maximum accepted accuracy snapshot.
- Classroom id.

Not stored:

- Exact Student latitude.
- Exact Student longitude.
- Browser location permission state.
- Device id.
- IP geolocation.
- WiFi/Bluetooth/NFC data.

## Current Verification

Automated unit tests cover accepted same-point location, outside geofence, excessive accuracy, invalid coordinates, invalid accuracy, and missing policy.

Sprint 6 is marked Completed based on successful automated verification and project-owner-confirmed authorized manual runtime evidence.

The project owner confirmed:

- Browser location permission flow worked.
- A fresh browser location reading was acquired.
- Server-side geofence validation accepted an inside-geofence position.
- Location permission denial was handled safely.
- Outside-geofence location was rejected.
- Insufficiently accurate location was rejected.
- No successful `AttendanceRecord` was created for rejected location attempts.
- Client-side location state was cleared after completion.

Physical vs simulated location evidence:

- User-confirmed manual location result; physical-versus-simulated method was not recorded in the closure evidence.

## Sprint 7 Reporting Boundary

Sprint 7 reports and CSV exports do not expose exact submitted Student latitude or longitude. They may expose safe attendance status, check-in time, and report percentages, but geofence policy details and browser-submitted coordinate values remain outside the export contract.

## Limitations

- Browser-provided coordinates may be manipulated.
- Server-side geofence validation is implemented, but browser geolocation is not tamper-proof.
- This policy does not implement device attestation, GPS anti-spoofing, WiFi/Bluetooth/NFC verification, or production fraud prevention.
