# 03. Database ERD

Sprint 3 implements ASP.NET Core Identity tables, academic setup tables, lecture schedules, lecture sessions, and session lifecycle events. Attendance records, biometric templates, recognition attempts, notifications, and activity logs remain future entities.

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

    Sections ||--o{ LectureSchedules : scheduled
    Instructors ||--o{ LectureSchedules : teaches
    Classrooms ||--o{ LectureSchedules : hosts
    LectureSchedules ||--o{ LectureSessions : creates
    Sections ||--o{ LectureSessions : session_snapshot
    Instructors ||--o{ LectureSessions : session_snapshot
    Classrooms ||--o{ LectureSessions : session_snapshot
    LectureSessions ||--o{ LectureSessionEvents : audits

    LectureSchedules {
        uniqueidentifier Id PK
        uniqueidentifier SectionId FK
        uniqueidentifier InstructorId FK
        uniqueidentifier ClassroomId FK
        int DayOfWeek
        time StartTime
        time EndTime
        date EffectiveFrom
        date EffectiveTo
        int DefaultLateThresholdMinutes
        int DefaultAllowedRadiusMeters
        bool IsActive
        rowversion RowVersion
    }

    LectureSessions {
        uniqueidentifier Id PK
        uniqueidentifier LectureScheduleId FK
        uniqueidentifier SectionId FK
        uniqueidentifier InstructorId FK
        uniqueidentifier ClassroomId FK
        date SessionDate
        datetime ScheduledStartUtc
        datetime ScheduledEndUtc
        datetime ActualStartUtc
        datetime ActualEndUtc
        int Status
        string SessionCodeHash
        string ProtectedSessionCode
        datetime SessionCodeExpiresAtUtc
        int SessionCodeVersion
        int LateThresholdMinutes
        int AllowedRadiusMeters
        rowversion RowVersion
    }

    LectureSessionEvents {
        uniqueidentifier Id PK
        uniqueidentifier LectureSessionId FK
        int EventType
        datetime OccurredAtUtc
        string PerformedByUserId FK
        int PreviousStatus
        int NewStatus
        string SafeDescription
    }

    Students ||--o{ FaceProfiles : future_has
    FaceProfiles ||--o{ RecognitionAttempts : future_used_by
    LectureSessions ||--o{ AttendanceRecords : future_records
    Students ||--o{ AttendanceRecords : future_marks
    AspNetUsers ||--o{ Notifications : future_receives
    AspNetUsers ||--o{ ActivityLogs : future_generates
```

Future attendance/reporting entities retain `future_` relationship labels and must not be treated as implemented tables.

## Sprint 4 Biometric Enrollment ERD Delta

```mermaid
erDiagram
    Students ||--o{ BiometricConsents : grants
    Students ||--o{ StudentFaceTemplates : owns
    BiometricConsents ||--o{ StudentFaceTemplates : authorizes
    Students ||--o{ FaceEnrollmentEvents : audits
    BiometricConsents ||--o{ FaceEnrollmentEvents : referenced_by
    StudentFaceTemplates ||--o{ FaceEnrollmentEvents : referenced_by

    BiometricConsents {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        string ConsentVersion
        string ConsentTextHash
        datetime AcceptedAtUtc
        datetime WithdrawnAtUtc
        string AcceptedByUserId
        string WithdrawnByUserId
        bool IsActive
        rowversion RowVersion
    }

    StudentFaceTemplates {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        uniqueidentifier BiometricConsentId FK
        binary ProtectedTemplate
        string TemplateFingerprint
        string EngineName
        string EngineVersion
        string ModelName
        string ModelVersion
        string TemplateFormatVersion
        int EmbeddingDimension
        decimal QualityScore
        int CaptureCount
        int TemplateVersion
        datetime EnrolledAtUtc
        datetime RevokedAtUtc
        string RevokedByUserId
        string RevocationReason
        bool RequiresReEnrollment
        bool IsActive
        rowversion RowVersion
    }

    FaceEnrollmentEvents {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        uniqueidentifier FaceTemplateId FK
        uniqueidentifier BiometricConsentId FK
        int EventType
        string Outcome
        string ErrorCode
        datetime OccurredAtUtc
        string PerformedByUserId
        string EngineName
        string EngineVersion
        string SafeDescription
    }
```

SQL Server constraints include one active consent per Student, one active template per Student, unique template version per Student, rowversion concurrency for consent/template rows, and check constraints for quality score, capture count, embedding dimension, and template version. No raw-image column exists.
