# 03. Database ERD

Sprint 1 implements ASP.NET Core Identity tables and `ApplicationUser` extensions only. Planned academic and attendance tables are included for architectural direction and are not implemented in code during Sprint 1.

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : has
    AspNetRoles ||--o{ AspNetUserRoles : grants
    AspNetUsers ||--o{ AspNetUserClaims : has
    AspNetUsers ||--o{ AspNetUserLogins : has
    AspNetUsers ||--o{ AspNetUserTokens : has
    AspNetRoles ||--o{ AspNetRoleClaims : has

    AspNetUsers {
        string Id PK
        string Email
        string FullName
        bool IsDisabled
        datetime CreatedAtUtc
        datetime LastLoginAtUtc
    }

    Students ||--|| AspNetUsers : future_profile
    Instructors ||--|| AspNetUsers : future_profile
    Departments ||--o{ Courses : future_owns
    Courses ||--o{ Sections : future_has
    Classrooms ||--o{ Sections : future_hosts
    Students ||--o{ StudentEnrollments : future_enrolls
    Sections ||--o{ StudentEnrollments : future_contains
    Instructors ||--o{ InstructorAssignments : future_teaches
    Sections ||--o{ InstructorAssignments : future_assigned
    Sections ||--o{ LectureSessions : future_schedules
    Students ||--o{ FaceProfiles : future_has
    FaceProfiles ||--o{ RecognitionAttempts : future_used_by
    LectureSessions ||--o{ AttendanceRecords : future_records
    Students ||--o{ AttendanceRecords : future_marks
    AspNetUsers ||--o{ Notifications : future_receives
    AspNetUsers ||--o{ ActivityLogs : future_generates
```

Future entities are marked with `future_` relationship labels and must not be treated as implemented Sprint 1 tables.
