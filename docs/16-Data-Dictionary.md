# 16. Data Dictionary

## Identity Extension

| Table | Column | Type | Notes |
| --- | --- | --- | --- |
| AspNetUsers | FullName | nvarchar(200) | Display name. |
| AspNetUsers | IsActive | bit | Sprint 2 login gate for administrator-managed accounts. |
| AspNetUsers | IsDisabled | bit | Existing Sprint 1 disabled-account gate. |
| AspNetUsers | MustChangePassword | bit | Forces redirect to Change Password until cleared. |
| AspNetUsers | CreatedAtUtc | datetimeoffset | Creation timestamp. |
| AspNetUsers | UpdatedAtUtc | datetimeoffset nullable | Last update timestamp. |
| AspNetUsers | LastLoginAtUtc | datetimeoffset nullable | Last successful login timestamp. |

## Academic Tables

| Table | Key Columns | Required Fields | Important Constraints |
| --- | --- | --- | --- |
| Departments | Id, Code | Code, NameEnglish, NameArabic | Unique Code; activation state; rowversion. |
| Students | Id, ApplicationUserId, StudentNumber, DepartmentId | ApplicationUserId, StudentNumber, names, DepartmentId, EnrollmentYear, AcademicLevel | Unique ApplicationUserId; unique StudentNumber; Department FK; enrollment year range. |
| Instructors | Id, ApplicationUserId, EmployeeNumber, DepartmentId | ApplicationUserId, EmployeeNumber, names, DepartmentId, AcademicTitle | Unique ApplicationUserId; unique EmployeeNumber; Department FK. |
| Courses | Id, Code, DepartmentId | Code, names, DepartmentId, CreditHours | Unique Code; Department FK; credit hours 1-6. |
| Sections | Id, CourseId | CourseId, SectionNumber, AcademicYear, Semester, Capacity | Unique CourseId/AcademicYear/Semester/SectionNumber; capacity > 0. |
| Classrooms | Id, Code | Code, building names, RoomNumber, Capacity | Unique Code; capacity > 0; latitude/longitude pair and range checks. |
| StudentEnrollments | Id, StudentId, SectionId | StudentId, SectionId, EnrollmentStatus, EnrolledAtUtc | Unique StudentId/SectionId; Section and Student FKs. |
| InstructorAssignments | Id, InstructorId, SectionId | InstructorId, SectionId, IsPrimary, AssignedAtUtc | Unique InstructorId/SectionId; filtered unique active primary assignment per Section. |

## Shared Academic Columns

| Column | Type | Notes |
| --- | --- | --- |
| Id | uniqueidentifier | Primary key. |
| CreatedAtUtc | datetimeoffset | Created by domain base class. |
| UpdatedAtUtc | datetimeoffset nullable | Updated when entity changes. |
| IsActive | bit | Used for activation/deactivation instead of hard delete. |
| RowVersion | rowversion | SQL Server optimistic concurrency token. |

## Enum Values

| Enum | Values |
| --- | --- |
| Semester | First, Second, Summer |
| EnrollmentStatus | Active, Dropped, Completed, Cancelled |

## Sensitive Data Notes

- No raw biometric image, face template, model file, database file, private key, SDK license, or real password is part of the Sprint 2 schema or tracked repository files.
- Identity passwords remain in ASP.NET Core Identity password hashes.
- Temporary password values are accepted through Admin forms but are not stored in academic tables.
