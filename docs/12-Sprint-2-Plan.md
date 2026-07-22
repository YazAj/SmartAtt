# 12. Sprint 2 Plan

## Goal

Close the academic and account-management foundation without implementing attendance registration or production biometrics.

## In Scope

- Academic domain entities: Department, Student, Instructor, Course, Section, Classroom, StudentEnrollment, InstructorAssignment.
- Administrator CRUD workflows for academic setup records.
- Administrator-created Student and Instructor Identity accounts linked to academic profiles.
- Temporary password reset and required first-login password change.
- Activation/deactivation rather than hard deletion.
- Student enrollment and Instructor assignment rules.
- SQL Server migration and schema constraints.
- English/Arabic localization, RTL/LTR layout, and light/dark support for the new pages.
- Unit, integration, runtime, SQL Server, security, and git verification.

## Out Of Scope

- Lecture sessions.
- Attendance registration or attendance records.
- Reports, exports, notifications, analytics, or timetable automation.
- Production face enrollment, production face verification, liveness detection, or biometric storage.
- Native mobile applications.

## Architecture Approach

- Keep Domain framework-free.
- Put DTOs, commands, queries, results, and interfaces in Application.
- Put EF Core and Identity implementation details in Infrastructure.
- Put Admin CRUD controllers/views in `src/AttendAI.Web/Areas/Admin`.
- Preserve Sprint 1 authentication, localization, theming, face abstraction, and documentation.

## Verification Strategy

- Build and test the solution after implementation.
- Apply migrations to SQL Server LocalDB.
- Verify runtime Admin, Student, and Instructor account flows over HTTPS.
- Capture browser screenshots for English/Arabic and light/dark desktop/mobile surfaces.
- Search the repository for secrets, biometric samples, model files, database files, and tracked build output.

## Completion Criteria

- `dotnet restore`, `dotnet build`, `dotnet test`, and `dotnet format --verify-no-changes` all pass.
- SQL Server contains Identity and academic tables from the current migration set.
- Admin can create academic setup records and Student/Instructor accounts.
- Student and Instructor users must change temporary passwords before normal dashboard access.
- Role boundaries are enforced.
- No Sprint 3 attendance or production biometric functionality is implemented.
