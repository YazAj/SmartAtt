# 22. Sprint 4 Plan

## Goal

Sprint 4 adds face enrollment and biometric profile management while preserving Sprint 1 identity, Sprint 2 academic management, and Sprint 3 lecture scheduling/session behavior. It does not add attendance submission, attendance decisions, location validation, liveness, one-to-many identification, or production face verification.

## Scope

- Student biometric privacy notice, explicit consent, status page, enrollment, re-enrollment, withdrawal, and safe event history.
- Camera-based capture wizard with upload fallback for approved local fake payload testing.
- Server-side capture validation for MIME type, magic bytes, size, dimensions, request size, expected multipart field, and required sample count.
- Face-enrollment processor coordinating fake/disabled/future real engine modes, face-count outcomes, safe quality rejection, and template extraction.
- Protected template storage using ASP.NET Core Data Protection.
- Admin biometric oversight, search/filter, safe metadata details, revocation, require re-enrollment, and audit history.
- SQL Server migration for consent, protected templates, and append-only enrollment events.
- Bilingual, RTL/LTR, light/dark, responsive UI.

## Out Of Scope

- Attendance registration or attendance status decisions.
- Student code submission.
- Face verification against enrolled templates.
- GPS/location capture or validation.
- Production real-engine adapter.
- Instructor/Admin biometric enrollment.
- Biometric export/reporting.

## Engine Strategy

Modes are explicit:

- `Fake`: development/test workflow only; disabled in Production by readiness service.
- `Disabled`: enrollment unavailable.
- `Real`: not verified until selected adapter, licensed models, native dependencies, and approved test samples are supplied.

Sprint 4 can only be marked fully completed after real-engine verification passes. In this implementation the feature is partially completed because the real-engine gate remains Not Verified.

## Verification Plan

- Unit tests: domain transitions, capture validation, template protection, fake enrollment processing.
- Integration tests: anonymous/Admin/Instructor/Student access boundaries.
- SQL Server verification: migration applies; tables, FKs, filtered active indexes, rowversions, checks, and no raw-image column exist.
- Runtime smoke: consent, fake enrollment, re-enrollment, withdrawal, Admin safe metadata.
- Visual checks: English/Arabic, LTR/RTL, light/dark, desktop/tablet/mobile where available.

