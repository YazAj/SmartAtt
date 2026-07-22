# 15. Sprint 2 Review

## Final Status

Sprint 2 is completed for academic and account management. Sprint 3 attendance behavior and production biometric workflows were not implemented.

## Automated Verification

| Check | Exit | Warnings | Errors | Tests |
| --- | --- | --- | --- | --- |
| `dotnet tool restore` | 0 | n/a | n/a | n/a |
| `dotnet restore AttendAI.sln` | 0 | n/a | n/a | n/a |
| `dotnet build AttendAI.sln` | 0 | 0 | 0 | n/a |
| `dotnet test AttendAI.sln` | 0 | n/a | n/a | 47 total, 47 passed, 0 failed, 0 skipped |
| `dotnet format AttendAI.sln --verify-no-changes` | 0 | n/a | n/a | n/a |

## SQL Server Verification

- Verification database: `AttendAI_Sprint2Closure_20260722`.
- EF update result: build succeeded; no pending migrations after runtime migration application.
- Migration history: `20260722120650_InitialIdentityFoundation`, `20260722140818_AddAcademicManagement`.
- Roles verified: Admin, Instructor, Student.
- Tables verified: `AspNetRoles`, `AspNetUsers`, `Departments`, `Students`, `Instructors`, `Courses`, `Sections`, `Classrooms`, `StudentEnrollments`, `InstructorAssignments`.
- Runtime sample counts: 1 Department, 1 Course, 1 Classroom, 1 Section, 1 Student, 1 Instructor, 1 StudentEnrollment, 1 InstructorAssignment.
- Unique indexes, foreign keys, and check constraints were queried directly from SQL Server.

## Runtime Verification

- App started over HTTPS at `https://localhost:7052`.
- Demo Admin configured with User Secrets; credential values were not committed or documented.
- Admin dashboard returned 200.
- Admin create/details flows succeeded for Department, Course, Classroom, Section, Student, Instructor, Enrollment, and InstructorAssignment.
- Profile, Change Password, Access Denied, Privacy, and 404 pages returned expected statuses.
- Student temporary-password login redirected to Change Password; password change succeeded; Student dashboard returned 200.
- Instructor temporary-password login redirected to Change Password; password change succeeded; Instructor dashboard returned 200.
- Student request to Instructor dashboard and Instructor request to Student dashboard redirected to Access Denied.

## Manual Visual Verification

Screenshots were captured under ignored `logs/ui-verification-sprint2/`.

| Surface | Evidence |
| --- | --- |
| English LTR Light Desktop | `admin-dashboard-en-light-desktop.png` |
| English LTR Light Desktop Final Smoke | `admin-dashboard-en-light-desktop-final.png` |
| English LTR Dark Desktop | `students-en-dark-desktop.png` |
| English LTR Light Mobile | `departments-en-light-mobile-verified.png` |
| Arabic RTL Light Desktop | `courses-ar-light-desktop.png` |
| Arabic RTL Dark Mobile | `sections-ar-dark-mobile.png` |

Browser overflow checks reported no horizontal overflow in the checked desktop and mobile viewports. Dark-mode screenshots were inspected for readable text on dark backgrounds. Arabic screenshots were inspected for RTL page direction and right-aligned academic management UI.

## Security And Secrets

- `appsettings.json` contains only a LocalDB development connection string with trusted authentication and no password.
- `appsettings.Development.json` contains empty `DemoAdmin` placeholders.
- Runtime demo credentials were configured through User Secrets and not tracked.
- High-risk secret regex scan found no non-ignored literal credentials, private keys, API keys, or client secrets.
- Sensitive artifact scan found no non-ignored images, biometric samples, model files, private keys, database files, or license-key files.
- `.gitignore` was corrected so root face model assets are ignored without ignoring source `Models` folders.

## Not Verified Items

- Real face-recognition runtime remains Not Verified because approved biometric samples, selected licensed models, preprocessing, threshold calibration, and native deployment requirements are not available.
- Production attendance registration is Not Implemented.
- Secure notification delivery for temporary passwords is Not Implemented.

## Remaining Risks

- Some rare low-level domain exception text may surface through generic validation wrappers before every message is mapped to a localized resource.
- Temporary passwords are admin-entered in Sprint 2; production delivery and expiration policy should be designed before real onboarding.
- Classroom coordinates are setup data only; no attendance location validation exists yet.

## Recommended Commit

`feat(sprint-2): add academic and account management`
