# 49. Report Export Security And Privacy

## Role Boundaries

Sprint 7 report controllers enforce role authorization:

- `/Reports/Student` requires `Student`.
- `/Reports/Instructor` requires `Instructor`.
- `/Admin/Reports` and `/Admin/Reports/Audit` require `Admin`.

Student and Instructor reports also resolve the academic profile from the authenticated Identity user id. Browser-posted Student or Instructor ownership is not trusted.

## Sensitive Data Exclusion

Report DTOs and CSV exports intentionally omit:

- Raw biometric images.
- Face crops or aligned face images.
- Embeddings and template bytes.
- Protected template payloads.
- Template fingerprints.
- Plain challenge tokens.
- Plain idempotency keys.
- Exact submitted Student latitude/longitude.
- Native model paths.
- Raw native exception details.
- Passwords or password hashes.

Attendance reports expose only academic labels, local session times, attendance status, and check-in time.

## CSV Export

Attendance CSV export is available to:

- The authenticated Student for their own attendance report.
- The authenticated Instructor for sessions they taught.
- Admin users for filtered attendance report rows.

Exports are bounded to the server-side filtered report set and capped by implementation limit. They are generated on demand and are not retained by AttendAI.

## Export Columns

Attendance CSV includes:

- Session date.
- Scheduled start.
- Scheduled end.
- Course code.
- Course name.
- Section.
- Student number.
- Student name.
- Instructor.
- Classroom.
- Status.
- Checked-in timestamp.

Attendance CSV does not include face scores, distance values, browser accuracy, exact coordinates, challenge hashes, idempotency hashes, template metadata, protected templates, or images.

## CSV Safety

`SafeCsvReportWriter`:

- Emits UTF-8 CSV with BOM.
- Escapes commas, quotes, and new lines.
- Prefixes spreadsheet formula starters with an apostrophe.
- Uses fixed safe headers.
- Omits biometric and exact-location columns.

Responses set:

- `Cache-Control: no-store, private`
- `Pragma: no-cache`
- `X-Content-Type-Options: nosniff`

## Audit Search

Admin audit search reads from existing safe event sources:

- Lecture session lifecycle events.
- Attendance attempts.
- Face verification attempts.
- Face enrollment events.

The audit view supports date, category, and text search filters with pagination. It is intended for operational review, not regulatory-grade immutable audit certification.

## Audit Export

Admin audit CSV includes:

- Occurred at.
- Category.
- Event type.
- Actor.
- Subject.
- Outcome.
- Safe description.

The audit export excludes sensitive biometric and security material.

## Retention

Sprint 7 does not implement export retention, scheduled report generation, report delivery, or generated PDF files. Any downloaded CSV should be stored and shared according to local university policy.

## Remaining Security Limitations

- Browser location is still not tamper-proof.
- Liveness and anti-spoofing are not implemented.
- Report authorization depends on correct Identity role assignment and profile linkage.
- CSV consumers must still treat downloaded files as sensitive university attendance records.
- Population-wide biometric threshold calibration remains outside Sprint 7.

