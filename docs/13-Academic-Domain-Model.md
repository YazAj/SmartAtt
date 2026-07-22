# 13. Academic Domain Model

## Entities

| Entity | Purpose | Key Rules |
| --- | --- | --- |
| Department | Academic owner for students, instructors, and courses. | Unique normalized code; English and Arabic names required; soft activation state. |
| Student | Academic profile linked one-to-one to `ApplicationUser`. | Unique student number; Department required; enrollment year 2000-2100; active state syncs with Identity login. |
| Instructor | Academic profile linked one-to-one to `ApplicationUser`. | Unique employee number; Department required; academic title required; active state syncs with Identity login. |
| Course | Catalog item linked to Department. | Unique normalized code; credit hours 1-6; active Department required. |
| Section | Course offering by academic year and semester. | Unique Course/year/semester/section number; capacity must be positive. |
| Classroom | Physical room record for future location validation. | Unique normalized code; capacity positive; latitude and longitude must be supplied together and in range. |
| StudentEnrollment | Student-to-Section relationship. | Student and Section required; duplicate pair rejected; active count cannot exceed Section capacity. |
| InstructorAssignment | Instructor-to-Section relationship. | Instructor and Section required; only one active primary Instructor per Section. |

## Shared Fields

Academic entities inherit `Id`, `CreatedAtUtc`, `UpdatedAtUtc`, `IsActive`, and `RowVersion`. SQL Server uses `rowversion` for optimistic concurrency; SQLite tests configure the same property as a regular concurrency token.

## Enums

- `Semester`: `First`, `Second`, `Summer`.
- `EnrollmentStatus`: `Active`, `Dropped`, `Completed`, `Cancelled`.

## Identity Link

`Students.ApplicationUserId` and `Instructors.ApplicationUserId` reference `AspNetUsers.Id`. Admin account creation uses a transaction so the Identity user, role assignment, and academic profile are created consistently. Temporary passwords are not stored outside Identity password hashes.

## Activation Model

Sprint 2 uses activation/deactivation rather than hard delete for business records. Deactivating Student or Instructor profiles also disables login through `ApplicationUser.IsActive = false`. Reactivation re-enables the profile and account.

## Future Boundaries

Classrooms include optional coordinates only as setup data. No lecture session, attendance validation, production face enrollment, or biometric template entity is implemented in Sprint 2.
