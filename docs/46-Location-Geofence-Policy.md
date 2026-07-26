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

Manual real-device browser verification remains required before Sprint 6 can be marked Completed.
