# 30. Sprint 5 Test Plan

## Automated Commands

Run:

```powershell
dotnet tool restore
dotnet restore AttendAI.sln
dotnet build AttendAI.sln
dotnet test AttendAI.sln
dotnet format AttendAI.sln --verify-no-changes
git diff --check
git status
```

If formatting fails, run `dotnet format AttendAI.sln`, then rerun build, tests, format verification, and git diff check.

## Unit Tests

- Domain construction and safe metadata validation.
- Cosine similarity threshold policy.
- Euclidean distance threshold policy.
- Template compatibility success and failure.
- Fake engine match, no-match, no-face, and multiple-face behavior.

## Integration Tests

- Student verification route authorization.
- Admin verification route authorization.
- Instructor and anonymous access rejection.
- Match and no-match attempt persistence.
- Idempotent client request id behavior.
- Incompatible template marking for re-enrollment.
- No attendance table or side effect from verification.

## SQL Server Verification

- Restore tools and apply all migrations through `20260722221335_AddFaceVerification`.
- Confirm `FaceVerificationAttempts` exists.
- Confirm expected foreign keys, indexes, filtered idempotency index, and check constraints.
- Confirm migration history includes Sprint 1-5 migrations.
- Confirm no raw-image, encoding, embedding, protected-template, or template-fingerprint column exists on `FaceVerificationAttempts`.
- Confirm Identity roles and existing Sprint 1-4 data remain valid after app startup seeding.

## Runtime Smoke

Use SQL Server LocalDB and environment/User Secrets for credentials:

1. Admin logs in.
2. Admin opens face-verification overview.
3. Admin creates a Student.
4. Student logs in and is redirected to Change Password.
5. Student changes password.
6. Student accepts biometric consent.
7. Student enrolls a fake-development template.
8. Student opens verification status.
9. Student submits a matching capture.
10. Student submits a different capture.
11. Rate limiting is demonstrated safely.
12. Student cannot access Admin verification routes.
13. Admin sees safe attempt metadata.
14. SQL confirms no attendance table/record is created by Sprint 5 verification.

## Manual Browser Verification

Check:

- Student status, capture, processing/result, history, and demo warning.
- Admin overview, filters, details, diagnostics, and readiness state.
- Access denied page for cross-role route attempts.
- Dashboards for Admin, Student, and Instructor.
- Sidebar, navbar, profile menu, breadcrumbs/page title, dropdowns, forms, placeholders, validation, buttons, cards, tables, badges, alerts, empty states, and error pages.

Required combinations:

- English LTR Light desktop at 1440x900.
- English LTR Dark mobile at 390x844.
- Arabic RTL Light desktop/tablet at 1024x768.
- Arabic RTL Dark tablet at 768x1024.
- Arabic RTL Dark mobile at 390x844.
- Arabic RTL Dark narrow mobile at 320x568.

## Sensitive-Data Checks

- Repository secret scan.
- Tracked file extension scan for images, databases, models, keys, and licenses.
- Migration/schema scan for raw capture or embedding columns.
- Runtime HTML scan for protected template, template fingerprint, embedding, and capture bytes.
- Browser storage check for biometric data.
- Ignored artifacts check for local smoke scripts and generated payloads.

## Real Engine Tests

Status: Not Verified.

Required before gate can pass:

- Approved same-person image pair.
- Approved different-person image.
- Approved no-face image.
- Approved multiple-face image when supported.
- Selected licensed detection and embedding models.
- Adapter, native dependencies, preprocessing, normalization, threshold calibration, and model-license documentation.
