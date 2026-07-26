# 51. Final Release Readiness

## Implemented Hardening

- Role-scoped report controllers.
- Profile-derived Student and Instructor report ownership.
- Safe report DTOs.
- In-memory CSV export.
- CSV formula-injection neutralization.
- No-store export response headers.
- Existing theme tokens reused for report UI.
- Existing RTL/LTR layout strategy reused.
- New report text added to English and Arabic resources.
- Automated test coverage increased for reporting, export, audit, and authorization.

## Database Readiness

No new migration was created for Sprint 7. Reporting uses existing constraints:

- Unique Student/section enrollment rules.
- Unique lecture session occurrence rules.
- Unique attendance record per Student/session.
- Unique attendance idempotency key per Student/session.
- Safe event/attempt tables from Sprints 3-6.

The Sprint 7 LocalDB verification database used the existing six migrations through `20260726173403_AddSecureAttendanceCheckIn`.

## Local Release Instructions

1. Restore tools and packages.
2. Configure `ConnectionStrings:DefaultConnection` through User Secrets or an environment variable.
3. Provision Real face models locally only when Real biometric workflows are part of the demo.
4. Apply EF Core migrations.
5. Configure any demo administrator through User Secrets or environment variables.
6. Run the web app over HTTPS localhost.
7. Verify Admin, Instructor, and Student role accounts against the configured SQL Server database.

Do not commit secrets, raw biometric images, model binaries, database files, generated CSV files, logs, or Data Protection keys.

## CI Status

No `.github` directory or GitHub Actions workflow exists in the repository at Sprint 7 closure. No CI workflow was added because the prompt forbade committing/pushing and the source-of-truth documentation did not require CI as an existing project deliverable. The documented local release gate remains restore, build, test, format verify, SQL Server migration update, security scan, and manual runtime verification.

## Known Release Limitations

- Sprint 7 manual responsive/dark/light browser evidence is pending unless recorded in `docs/53-Sprint-7-Review.md`.
- Live SQL-backed Student and Instructor report account flows are pending unless recorded in `docs/53-Sprint-7-Review.md`.
- No generated server PDF export exists; browser print is the supported print path.
- No cloud deployment was performed.
- No liveness/anti-spoofing was added or claimed.
- No browser-location tamper-proofing, GPS anti-spoofing, or device attestation was added.
- No persisted audit-certification ledger was added.
- No biometric model or raw biometric image is committed.

