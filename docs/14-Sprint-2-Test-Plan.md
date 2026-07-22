# 14. Sprint 2 Test Plan

## Automated Verification

| Command | Expected |
| --- | --- |
| `dotnet restore AttendAI.sln` | Exit 0. |
| `dotnet build AttendAI.sln` | Exit 0, 0 warnings, 0 errors. |
| `dotnet test AttendAI.sln` | Exit 0, all tests pass. |
| `dotnet format AttendAI.sln --verify-no-changes` | Exit 0, no formatting changes required. |

## Unit Tests

- Academic entity validation.
- Code/number normalization.
- Capacity and coordinate validation.
- Enrollment status changes.
- Instructor primary assignment flag behavior.
- Existing role routing, safe redirect, and fake face engine behavior.

## Integration Tests

- Admin can access academic management.
- Instructor and Student cannot access Admin academic management.
- Anonymous user cannot access Admin academic management.
- Admin POST without antiforgery token is rejected.
- Student account creation assigns Student role and requires password change.
- Temporary password login redirects to Change Password.
- Deactivated Student account cannot log in.
- Duplicate enrollment and over-capacity enrollment are rejected.
- Only one active primary Instructor assignment is allowed per Section.

## SQL Server Verification

- Restore local tools.
- Configure `ConnectionStrings:DefaultConnection` through User Secrets or environment variable.
- Apply migrations to SQL Server LocalDB.
- Confirm database, migration history, Identity roles, academic tables, unique indexes, foreign keys, and check constraints.

## Runtime Verification

- Start `AttendAI.Web` over HTTPS against SQL Server.
- Log in as configured demo Admin.
- Create Department, Course, Classroom, Section, Student, Instructor, Enrollment, and InstructorAssignment records.
- Confirm Admin pages return 200.
- Confirm Student and Instructor temporary-password users are redirected to Change Password and can change password.
- Confirm each role reaches the correct dashboard and cross-role dashboard access redirects to Access Denied.

## Visual Verification

- English LTR Light desktop.
- English LTR Dark desktop.
- English LTR Light mobile.
- Arabic RTL Light desktop.
- Arabic RTL Dark mobile.
- Check sidebar/navbar, filters, forms, buttons, cards, tables, badges, profile menu, dashboards, access denied, privacy, and 404 behavior.

## Not Verified In Sprint 2

- Real face recognition runtime with biometric samples.
- Production attendance workflows.
- Production notification delivery for temporary passwords.
