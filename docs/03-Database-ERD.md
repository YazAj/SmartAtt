# 03. Database ERD

Sprint 2 implements ASP.NET Core Identity tables plus academic setup tables. Attendance, biometric templates, recognition attempts, notifications, and activity logs remain future entities.

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
        bool IsActive
        bool IsDisabled
        bool MustChangePassword
        datetime CreatedAtUtc
        datetime UpdatedAtUtc
        datetime LastLoginAtUtc
    }

    AspNetUsers ||--|| Students : profile
    AspNetUsers ||--|| Instructors : profile
    Departments ||--o{ Students : owns
    Departments ||--o{ Instructors : owns
    Departments ||--o{ Courses : owns
    Courses ||--o{ Sections : has
    Students ||--o{ StudentEnrollments : enrolls
    Sections ||--o{ StudentEnrollments : contains
    Instructors ||--o{ InstructorAssignments : teaches
    Sections ||--o{ InstructorAssignments : assigned

    Departments {
        uniqueidentifier Id PK
        string Code UK
        string NameEnglish
        string NameArabic
        bool IsActive
        rowversion RowVersion
    }

    Students {
        uniqueidentifier Id PK
        string ApplicationUserId FK
        string StudentNumber UK
        uniqueidentifier DepartmentId FK
        int EnrollmentYear
        bool IsActive
        rowversion RowVersion
    }

    Instructors {
        uniqueidentifier Id PK
        string ApplicationUserId FK
        string EmployeeNumber UK
        uniqueidentifier DepartmentId FK
        bool IsActive
        rowversion RowVersion
    }

    Courses {
        uniqueidentifier Id PK
        string Code UK
        uniqueidentifier DepartmentId FK
        int CreditHours
        bool IsActive
        rowversion RowVersion
    }

    Sections {
        uniqueidentifier Id PK
        uniqueidentifier CourseId FK
        string SectionNumber
        string AcademicYear
        int Semester
        int Capacity
        bool IsActive
        rowversion RowVersion
    }

    Classrooms {
        uniqueidentifier Id PK
        string Code UK
        int Capacity
        decimal Latitude
        decimal Longitude
        bool IsActive
        rowversion RowVersion
    }

    StudentEnrollments {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        uniqueidentifier SectionId FK
        int EnrollmentStatus
        datetime EnrolledAtUtc
        bool IsActive
        rowversion RowVersion
    }

    InstructorAssignments {
        uniqueidentifier Id PK
        uniqueidentifier InstructorId FK
        uniqueidentifier SectionId FK
        bool IsPrimary
        datetime AssignedAtUtc
        bool IsActive
        rowversion RowVersion
    }

    Sections ||--o{ LectureSessions : future_schedules
    Students ||--o{ FaceProfiles : future_has
    FaceProfiles ||--o{ RecognitionAttempts : future_used_by
    LectureSessions ||--o{ AttendanceRecords : future_records
    Students ||--o{ AttendanceRecords : future_marks
    AspNetUsers ||--o{ Notifications : future_receives
    AspNetUsers ||--o{ ActivityLogs : future_generates
```

Future entities retain `future_` relationship labels and must not be treated as implemented Sprint 2 tables.
