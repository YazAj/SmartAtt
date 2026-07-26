# 03. Database ERD

AttendAI implements ASP.NET Core Identity tables, academic setup tables, lecture schedules, lecture sessions, session lifecycle events, biometric consent/templates/events, Sprint 5 safe face-verification attempts, and Sprint 6 attendance challenges/attempts/records. Sprint 7 reports are projections over those existing tables and do not add report tables. Notifications, one-to-many recognition, and activity logs remain future entities.

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

    Students ||--o{ BiometricConsents : grants
    Students ||--o{ StudentFaceTemplates : owns
    Students ||--o{ FaceVerificationAttempts : self_verifies
    LectureSessions ||--o{ AttendanceChallenges : issues
    Students ||--o{ AttendanceChallenges : receives
    LectureSessions ||--o{ AttendanceAttempts : attempts
    Students ||--o{ AttendanceAttempts : submits
    LectureSessions ||--o{ AttendanceRecords : records
    Students ||--o{ AttendanceRecords : marks
    AspNetUsers ||--o{ Notifications : future_receives
    AspNetUsers ||--o{ ActivityLogs : future_generates
```

Future notification/activity-log entities retain `future_` relationship labels and must not be treated as implemented tables. Sprint 7 reporting is implemented without a persisted reporting entity.

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

## Sprint 5 Face Verification ERD Delta

```mermaid
erDiagram
    Students ||--o{ FaceVerificationAttempts : owns
    StudentFaceTemplates ||--o{ FaceVerificationAttempts : used_by

    FaceVerificationAttempts {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        uniqueidentifier FaceTemplateId FK
        int VerificationPurpose
        int Outcome
        int Decision
        string ErrorCode
        decimal Score
        decimal Threshold
        int ScoreMetric
        string EngineName
        string EngineVersion
        string ModelName
        string ModelVersion
        string TemplateFormatVersion
        datetime AttemptedAtUtc
        string ClientRequestId
        string SafeDescription
        int ImageWidth
        int ImageHeight
        int DetectedFaceCount
        decimal QualityScore
        int ProcessingDurationMilliseconds
    }
```

SQL Server constraints include two foreign keys, seven check constraints for bounded enum/score/image/face-count/timing metadata, indexes for Student/time and outcome/time queries, and a filtered unique idempotency index on `(StudentId, ClientRequestId)` when `ClientRequestId` is not empty.

The table intentionally has no raw image, image path, base64 image, encoding, embedding, protected template, template fingerprint, facial landmarks, session code, attendance status, location, model path, or raw exception column.

## Sprint 6 Attendance ERD Delta

```mermaid
erDiagram
    Students ||--o{ AttendanceChallenges : receives
    LectureSessions ||--o{ AttendanceChallenges : issues
    Students ||--o{ AttendanceAttempts : submits
    LectureSessions ||--o{ AttendanceAttempts : checks
    FaceVerificationAttempts ||--o{ AttendanceAttempts : supports
    Students ||--o{ AttendanceRecords : earns
    LectureSessions ||--o{ AttendanceRecords : records
    AttendanceAttempts ||--|| AttendanceRecords : creates
    FaceVerificationAttempts ||--|| AttendanceRecords : verifies
    Classrooms ||--o{ AttendanceRecords : policy_reference

    AttendanceChallenges {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        uniqueidentifier LectureSessionId FK
        string TokenHash UK
        datetime IssuedAtUtc
        datetime ExpiresAtUtc
        datetime ConsumedAtUtc
        uniqueidentifier ConsumedByAttendanceAttemptId
        rowversion RowVersion
    }

    AttendanceAttempts {
        uniqueidentifier Id PK
        uniqueidentifier StudentId FK
        uniqueidentifier LectureSessionId FK
        uniqueidentifier FaceVerificationAttemptId FK
        int Outcome
        int FailureReason
        int LocationOutcome
        datetime AttemptedAtUtc
        string IdempotencyKeyHash
        string ChallengeTokenHash
        string SafeDescription
        decimal DistanceMeters
        decimal BrowserAccuracyMeters
        int AllowedRadiusMeters
        int MaximumAcceptedAccuracyMeters
        int ProcessingDurationMilliseconds
    }

    AttendanceRecords {
        uniqueidentifier Id PK
        uniqueidentifier LectureSessionId FK
        uniqueidentifier StudentId FK
        uniqueidentifier AttendanceAttemptId FK
        uniqueidentifier FaceVerificationAttemptId FK
        int Status
        datetime CheckedInAtUtc
        uniqueidentifier ClassroomId FK
        int AllowedRadiusMeters
        int MaximumAcceptedAccuracyMeters
        decimal DistanceMeters
        decimal BrowserAccuracyMeters
        rowversion RowVersion
    }
```

SQL Server constraints include unique attendance record per `(LectureSessionId, StudentId)`, unique challenge token hash, unique attempt idempotency hash per Student/session, unique successful attendance attempt/face attempt links, foreign keys, rowversion concurrency on challenge/record rows, and nonnegative location metadata checks.

Attendance tables intentionally have no raw image, image path, base64 image, encoding, embedding, protected template, template fingerprint, exact submitted Student latitude/longitude, plain challenge token, plain idempotency key, report/export payload, device fingerprint, or raw exception column.

## Sprint 7 Reporting ERD Decision

Sprint 7 did not add tables, columns, foreign keys, indexes, report snapshots, export storage, or persisted absence rows.

Reporting queries derive rows from:

- Active `StudentEnrollments`.
- Completed attendance-enabled `LectureSessions`.
- Successful `AttendanceRecords`.
- Safe `AttendanceAttempts`.
- Safe `LectureSessionEvents`.
- Safe `FaceVerificationAttempts`.
- Safe `FaceEnrollmentEvents`.

The derived `Missed` reporting state is not stored in SQL Server. CSV exports are generated in memory from authorized report DTOs and are not represented in the ERD.
