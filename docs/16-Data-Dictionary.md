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
| LectureSessionStatus | Active, Ended, Cancelled, Expired |
| LectureSessionEventType | Started, CodeGenerated, CodeRegenerated, Ended, Cancelled, Expired, AdminForceEnded |
| ScheduleConflictType | Classroom, Instructor, Section |

## LectureSchedules

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Schedule identifier. |
| SectionId | uniqueidentifier | Yes | n/a | FK `Sections`; indexed | Internal | Scheduled Section. |
| InstructorId | uniqueidentifier | Yes | n/a | FK `Instructors`; indexed | Internal | Assigned Instructor. |
| ClassroomId | uniqueidentifier | Yes | n/a | FK `Classrooms`; indexed | Internal | Protected classroom resource. |
| DayOfWeek | int | Yes | n/a | conflict-query index | Internal | Recurring local academic day. |
| StartTime | time | Yes | n/a | conflict-query index | Internal | Local academic start time. |
| EndTime | time | Yes | n/a | conflict-query index | Internal | Local academic end time. |
| EffectiveFrom | date | Yes | n/a | conflict-query index | Internal | First active date. |
| EffectiveTo | date | Yes | n/a | conflict-query index | Internal | Last active date. |
| DefaultLateThresholdMinutes | int | Yes | n/a | check constraint | Internal | Future attendance timing threshold; not used to mark attendance in Sprint 3. |
| DefaultAllowedRadiusMeters | int | Yes | n/a | check constraint | Internal | Future location radius; not used for validation in Sprint 3. |
| IsActive | bit | Yes | n/a | filtered workflow queries | Internal | Activation flag; schedules are not hard-deleted. |
| CreatedAtUtc | datetimeoffset | Yes | n/a | audit | Internal | Creation timestamp. |
| UpdatedAtUtc | datetimeoffset nullable | No | n/a | audit | Internal | Last update timestamp. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency. |

## LectureSessions

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Session identifier. |
| LectureScheduleId | uniqueidentifier | Yes | n/a | FK `LectureSchedules`; unique with `SessionDate` | Internal | Source recurring schedule. |
| SectionId | uniqueidentifier | Yes | n/a | FK `Sections`; indexed | Internal | Historical Section snapshot. |
| InstructorId | uniqueidentifier | Yes | n/a | FK `Instructors`; indexed | Internal | Historical Instructor snapshot. |
| ClassroomId | uniqueidentifier | Yes | n/a | FK `Classrooms`; indexed | Internal | Historical Classroom snapshot. |
| SessionDate | date | Yes | n/a | unique occurrence index | Internal | Local academic occurrence date. |
| ScheduledStartUtc | datetimeoffset | Yes | n/a | indexed | Internal | Scheduled absolute start. |
| ScheduledEndUtc | datetimeoffset | Yes | n/a | indexed | Internal | Scheduled absolute end. |
| ActualStartUtc | datetimeoffset | Yes | n/a | lifecycle | Internal | Actual start timestamp. |
| ActualEndUtc | datetimeoffset nullable | No | n/a | lifecycle | Internal | Actual terminal timestamp. |
| Status | int | Yes | n/a | indexed | Internal | Current lifecycle state. |
| SessionCodeHash | nvarchar(512) nullable | No | 512 | none | Sensitive | Hash of active temporary code; never display. |
| ProtectedSessionCode | nvarchar(max) nullable | No | n/a | none | Sensitive | Data Protection payload for authorized service use only. |
| SessionCodeExpiresAtUtc | datetimeoffset nullable | No | n/a | expiration index | Sensitive | Active code expiration timestamp. |
| SessionCodeVersion | int | Yes | n/a | none | Internal | Incremented after each generation/regeneration. |
| LateThresholdMinutes | int | Yes | n/a | check constraint | Internal | Historical threshold snapshot for future attendance. |
| AllowedRadiusMeters | int | Yes | n/a | check constraint | Internal | Historical radius snapshot for future location validation. |
| StartedByUserId | nvarchar(450) | Yes | 450 | FK `AspNetUsers` | Internal | User that started the Session. |
| EndedByUserId | nvarchar(450) nullable | No | 450 | FK `AspNetUsers` | Internal | User that ended/force-ended the Session. |
| CancelledByUserId | nvarchar(450) nullable | No | 450 | FK `AspNetUsers` | Internal | User that cancelled the Session. |
| EndReason | nvarchar(300) nullable | No | 300 | none | Internal | Safe terminal reason. |
| CreatedAtUtc | datetimeoffset | Yes | n/a | audit | Internal | Creation timestamp. |
| UpdatedAtUtc | datetimeoffset nullable | No | n/a | audit | Internal | Last update timestamp. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency. |

## LectureSessionEvents

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Event identifier. |
| LectureSessionId | uniqueidentifier | Yes | n/a | FK `LectureSessions`; indexed | Internal | Parent Session. |
| EventType | int | Yes | n/a | indexed | Internal | Lifecycle event type. |
| OccurredAtUtc | datetimeoffset | Yes | n/a | indexed | Internal | Event time. |
| PerformedByUserId | nvarchar(450) nullable | No | 450 | FK `AspNetUsers` | Internal | Actor, when known. |
| PreviousStatus | int nullable | No | n/a | none | Internal | Previous lifecycle state. |
| NewStatus | int nullable | No | n/a | none | Internal | New lifecycle state. |
| SafeDescription | nvarchar(300) nullable | No | 300 | none | Internal | Safe message; no plain code, password, or biometric data. |
| CreatedAtUtc | datetimeoffset | Yes | n/a | audit | Internal | Creation timestamp. |

## Sensitive Data Notes

- No raw biometric image, face template, model file, database file, private key, SDK license, real password, or plain session code is part of the Sprint 3 schema or tracked repository files.
- Identity passwords remain in ASP.NET Core Identity password hashes.
- Temporary password values are accepted through Admin forms but are not stored in academic tables.
- Temporary session codes are returned only in authorized runtime responses and are not persisted in plain text.
