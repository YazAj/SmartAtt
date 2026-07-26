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
| FaceEnrollmentStatus | NotStarted, ConsentRequired, ReadyToEnroll, Active, RequiresReEnrollment, Withdrawn, EngineUnavailable |
| FaceEnrollmentEventType | ConsentAccepted, EnrollmentSucceeded, EnrollmentFailed, ReEnrollmentSucceeded, ReEnrollmentRequired, TemplateRevoked, ConsentWithdrawn |
| FaceCaptureRejectionReason | None, EmptyImage, UnsupportedMimeType, FileTooLarge, ImageTooSmall, ImageTooLarge, InvalidImage, NoFace, MultipleFaces, QualityTooLow, LowBrightness, HighBrightness, LowSharpness, TooManyCaptures, RequestTooLarge, UnexpectedField |
| FaceVerificationOutcome | Matched, NotMatched, NoActiveConsent, NoActiveTemplate, RequiresReEnrollment, IncompatibleTemplate, NoFace, MultipleFaces, LowQuality, InvalidImage, RateLimited, EngineUnavailable, ProcessingFailed, Cancelled |
| FaceVerificationPurpose | SelfTest, EngineValidation, FutureAttendance |
| FaceVerificationDecision | Unknown, Match, NoMatch, NoFaceDetected, MultipleFacesDetected, EngineError |
| ScoreMetric | CosineSimilarity, EuclideanDistance, EngineDefined |
| AttendanceStatus | Present, Late |
| AttendanceAttemptOutcome | Succeeded, Rejected, Duplicate |
| AttendanceFailureReason | None, StudentNotFound, StudentInactive, PasswordChangeRequired, NotEnrolled, SessionNotFound, SessionNotActive, AttendanceWindowClosed, AttendanceLocationUnavailable, LocationInvalid, LocationInaccurate, OutsideGeofence, ChallengeMissing, ChallengeInvalid, ChallengeExpired, ChallengeAlreadyUsed, IdempotencyKeyInvalid, FaceVerificationFailed, FaceNotMatched, FaceNoFace, FaceMultipleFaces, FaceLowQuality, FaceEngineUnavailable, DuplicateAttendance, PersistenceFailed, RateLimited |
| LocationVerificationOutcome | NotEvaluated, Accepted, MissingPolicy, Invalid, Inaccurate, OutsideGeofence |

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
| AttendanceLatitude | decimal(9,6) nullable | No | n/a | check constraint | Internal | Attendance policy latitude snapshot from classroom/session policy. |
| AttendanceLongitude | decimal(9,6) nullable | No | n/a | check constraint | Internal | Attendance policy longitude snapshot from classroom/session policy. |
| MaximumAcceptedAccuracyMeters | int | Yes | n/a | check constraint | Internal | Maximum browser geolocation accuracy accepted for attendance. |
| AttendanceCheckInEnabled | bit | Yes | n/a | default true | Internal | Enables Student attendance check-in for this session. |
| LocationVerificationRequired | bit | Yes | n/a | default true | Internal | Requires server-side browser-location validation. |
| FaceVerificationRequired | bit | Yes | n/a | default true | Internal | Requires one-to-one face verification for attendance. |
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

- No raw biometric image, model file, database file, private key, SDK license, real password, or plain session code is part of the tracked repository files.
- Identity passwords remain in ASP.NET Core Identity password hashes.
- Temporary password values are accepted through Admin forms but are not stored in academic tables.
- Temporary session codes are returned only in authorized runtime responses and are not persisted in plain text.
- Sprint 4 stores protected face template bytes only in `StudentFaceTemplates.ProtectedTemplate`; public DTOs and views expose metadata only.
- Sprint 5 verification attempts store safe metadata only. They do not store raw images, image paths, base64 images, face encoding bytes, embeddings, protected templates, template fingerprints, facial landmarks, session codes, attendance status, location data, model file paths, or raw native exceptions.
- Sprint 6 attendance attempts and records store safe metadata only. They do not store raw images, image paths, base64 images, face encodings, embeddings, protected templates, template fingerprints, exact submitted Student latitude/longitude, plain challenge tokens, plain idempotency keys, reports, exports, notifications, or raw native exceptions.

## Biometric Enrollment Tables

### BiometricConsents

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Consent identifier. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; filtered active unique index | Internal | Student who accepted consent. |
| ConsentVersion | nvarchar(40) | Yes | 40 | none | Internal | Version of the biometric privacy notice. |
| ConsentTextHash | nvarchar(128) | Yes | 128 | none | Internal | Hash of canonical notice text. |
| AcceptedAtUtc | datetimeoffset | Yes | n/a | none | Internal | Acceptance timestamp. |
| WithdrawnAtUtc | datetimeoffset nullable | No | n/a | none | Internal | Withdrawal timestamp. |
| AcceptedByUserId | nvarchar(450) | Yes | 450 | Identity user id | Internal | Actor that accepted consent. |
| WithdrawnByUserId | nvarchar(450) nullable | No | 450 | Identity user id | Internal | Actor that withdrew consent. |
| IsActive | bit | Yes | n/a | filtered active unique index | Internal | Active consent flag. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency. |

### StudentFaceTemplates

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Template identifier. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; filtered active unique index | Internal | Owning Student. |
| BiometricConsentId | uniqueidentifier | Yes | n/a | FK `BiometricConsents` | Internal | Consent authorizing the template. |
| ProtectedTemplate | varbinary(max) | Yes | n/a | none | Sensitive | Data Protection payload; never render. |
| TemplateFingerprint | nvarchar(128) | Yes | 128 | indexed | Sensitive metadata | Hash/fingerprint for internal integrity checks; not exposed in UI. |
| EngineName | nvarchar(80) | Yes | 80 | none | Internal | Engine that produced template. |
| EngineVersion | nvarchar(80) | Yes | 80 | none | Internal | Engine version. |
| ModelName | nvarchar(120) | Yes | 120 | none | Internal | Model name. |
| ModelVersion | nvarchar(80) | Yes | 80 | none | Internal | Model version. |
| TemplateFormatVersion | nvarchar(80) | Yes | 80 | none | Internal | Template format. |
| EmbeddingDimension | int | Yes | n/a | check > 0 | Internal | Template dimension. |
| QualityScore | decimal(5,4) | Yes | n/a | check 0..1 | Internal | Selected capture quality. |
| CaptureCount | int | Yes | n/a | check > 0 | Internal | Accepted samples processed. |
| TemplateVersion | int | Yes | n/a | unique per Student | Internal | Version incremented by re-enrollment. |
| EnrolledAtUtc | datetimeoffset | Yes | n/a | none | Internal | Enrollment timestamp. |
| RevokedAtUtc | datetimeoffset nullable | No | n/a | none | Internal | Revocation timestamp. |
| RevokedByUserId | nvarchar(450) nullable | No | 450 | Identity user id | Internal | Actor that revoked template. |
| RevocationReason | nvarchar(300) nullable | No | 300 | none | Internal | Safe reason. |
| RequiresReEnrollment | bit | Yes | n/a | status index | Internal | Indicates required fresh enrollment. |
| IsActive | bit | Yes | n/a | filtered active unique index | Internal | Active template flag. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency. |

### FaceEnrollmentEvents

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Event identifier. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; indexed with time | Internal | Student subject. |
| FaceTemplateId | uniqueidentifier nullable | No | n/a | FK `StudentFaceTemplates` | Internal | Related template when applicable. |
| BiometricConsentId | uniqueidentifier nullable | No | n/a | FK `BiometricConsents` | Internal | Related consent when applicable. |
| EventType | int | Yes | n/a | none | Internal | Enrollment event type. |
| Outcome | nvarchar(40) | Yes | 40 | none | Internal | Safe outcome key. |
| ErrorCode | nvarchar(80) | Yes | 80 | none | Internal | Safe error code only. |
| OccurredAtUtc | datetimeoffset | Yes | n/a | indexed with StudentId | Internal | Event time. |
| PerformedByUserId | nvarchar(450) | Yes | 450 | Identity user id | Internal | Actor user id. |
| EngineName | nvarchar(80) | Yes | 80 | none | Internal | Engine name. |
| EngineVersion | nvarchar(80) | Yes | 80 | none | Internal | Engine version. |
| SafeDescription | nvarchar(300) | Yes | 300 | none | Internal | Localizable safe description key. |

## Face Verification Tables

### FaceVerificationAttempts

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Attempt identifier. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; indexed with AttemptedAtUtc | Internal | Student subject resolved from authenticated user. |
| FaceTemplateId | uniqueidentifier nullable | No | n/a | FK `StudentFaceTemplates`; indexed | Internal | Template used when one was loaded. |
| VerificationPurpose | int | Yes | n/a | check constraint | Internal | Sprint 5 UI uses `SelfTest`; Sprint 6 attendance uses `FutureAttendance`. |
| Outcome | int | Yes | n/a | indexed with AttemptedAtUtc; check constraint | Internal | Safe localized outcome. |
| Decision | int | Yes | n/a | check constraint | Internal | Match, no-match, or safe rejection decision. |
| ErrorCode | nvarchar(80) | Yes | 80 | none | Internal | Safe error code only; no raw exception text. |
| Score | decimal(9,6) nullable | No | n/a | check constraint | Internal | Similarity/distance score when produced. |
| Threshold | decimal(9,6) | Yes | n/a | check constraint | Internal | Configured threshold at attempt time. |
| ScoreMetric | int | Yes | n/a | check constraint | Internal | Explicit metric/direction for interpreting score. |
| EngineName | nvarchar(80) | Yes | 80 | none | Internal | Engine name. |
| EngineVersion | nvarchar(80) | Yes | 80 | none | Internal | Engine version. |
| ModelName | nvarchar(120) | Yes | 120 | none | Internal | Model name. |
| ModelVersion | nvarchar(80) | Yes | 80 | none | Internal | Model version. |
| TemplateFormatVersion | nvarchar(80) | Yes | 80 | none | Internal | Expected template format. |
| AttemptedAtUtc | datetimeoffset | Yes | n/a | Student/time and outcome/time indexes | Internal | Attempt timestamp. |
| ClientRequestId | nvarchar(80) | Yes | 80 | filtered unique `(StudentId, ClientRequestId)` | Internal | Optional idempotency key; must not contain sensitive data. |
| SafeDescription | nvarchar(300) | Yes | 300 | none | Internal | Localizable safe description key. |
| ImageWidth | int nullable | No | n/a | check constraint | Internal | Validated capture width, if known. |
| ImageHeight | int nullable | No | n/a | check constraint | Internal | Validated capture height, if known. |
| DetectedFaceCount | int nullable | No | n/a | check constraint | Internal | Safe face-count outcome. |
| QualityScore | decimal(5,4) nullable | No | n/a | check constraint | Internal | Normalized quality score, if known. |
| ProcessingDurationMilliseconds | int nullable | No | n/a | check constraint | Internal | Safe processing timing. |
| CreatedAtUtc | datetimeoffset | Yes | n/a | audit | Internal | Creation timestamp. |
| UpdatedAtUtc | datetimeoffset nullable | No | n/a | audit | Internal | Present from shared base but not used for normal editing. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency token. |

No normal UI path edits or physically deletes verification attempts. Retention policy remains a future production governance item.

## Real Face Template Format Update

No database migration was required for the Real engine. `StudentFaceTemplates.ProtectedTemplate` already stores protected opaque template bytes, and the existing metadata columns store engine name/version, model name/version, template format version, embedding dimension, capture count, quality score, and re-enrollment state.

Real mode stores:

- `EngineName`: `OpenCV-SFace`.
- `ModelName`: `SFace`.
- `ModelVersion`: `2021dec`.
- `TemplateFormatVersion`: `opencv-sface-f32le-v1`.
- `EmbeddingDimension`: `128`.
- Protected payload before Data Protection: finite L2-normalized float32 little-endian embedding bytes.

The schema still has no raw image, face crop, aligned crop, unprotected embedding, model binary, or attendance column for face templates or verification attempts.

## Attendance Tables

### AttendanceChallenges

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Challenge identifier. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; indexed with session/expiry | Internal | Student receiving the challenge. |
| LectureSessionId | uniqueidentifier | Yes | n/a | FK `LectureSessions`; indexed | Internal | Lecture session for the challenge. |
| TokenHash | nvarchar(128) | Yes | 128 | unique | Sensitive metadata | SHA-256 hash of the one-time challenge token. Plain token is not stored. |
| IssuedAtUtc | datetimeoffset | Yes | n/a | none | Internal | Issue timestamp. |
| ExpiresAtUtc | datetimeoffset | Yes | n/a | check > issued | Internal | Expiry timestamp. |
| ConsumedAtUtc | datetimeoffset nullable | No | n/a | none | Internal | Consumption timestamp. |
| ConsumedByAttendanceAttemptId | uniqueidentifier nullable | No | n/a | logical link | Internal | Attendance attempt that consumed the challenge. |
| IsActive | bit | Yes | n/a | shared | Internal | Shared academic active flag. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency token. |

### AttendanceAttempts

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Attempt identifier. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; indexed with time | Internal | Authenticated Student subject. |
| LectureSessionId | uniqueidentifier | Yes | n/a | FK `LectureSessions`; indexed with time | Internal | Session being checked in to. |
| FaceVerificationAttemptId | uniqueidentifier nullable | No | n/a | FK `FaceVerificationAttempts`; indexed | Internal | Related face attempt when one was executed. |
| Outcome | int | Yes | n/a | enum | Internal | Succeeded, Rejected, or Duplicate. |
| FailureReason | int | Yes | n/a | enum | Internal | Safe rejection reason. |
| LocationOutcome | int | Yes | n/a | enum | Internal | Safe geofence outcome. |
| AttemptedAtUtc | datetimeoffset | Yes | n/a | indexed | Internal | Attempt timestamp. |
| IdempotencyKeyHash | nvarchar(128) | Yes | 128 | unique with Student/session | Sensitive metadata | SHA-256 hash of the browser idempotency key. |
| ChallengeTokenHash | nvarchar(128) | Yes | 128 | none | Sensitive metadata | SHA-256 hash of submitted challenge token when present. |
| SafeDescription | nvarchar(300) | Yes | 300 | none | Internal | Localizable safe description key. |
| DistanceMeters | decimal(9,3) nullable | No | n/a | check >= 0 | Internal | Calculated distance from approved policy. |
| BrowserAccuracyMeters | decimal(9,3) nullable | No | n/a | check >= 0 | Internal | Browser-reported accuracy. |
| AllowedRadiusMeters | int nullable | No | n/a | check >= 0 | Internal | Allowed radius snapshot. |
| MaximumAcceptedAccuracyMeters | int nullable | No | n/a | check >= 0 | Internal | Accuracy policy snapshot. |
| ProcessingDurationMilliseconds | int nullable | No | n/a | check >= 0 | Internal | Safe processing duration. |
| CreatedAtUtc | datetimeoffset | Yes | n/a | audit | Internal | Creation timestamp. |
| UpdatedAtUtc | datetimeoffset nullable | No | n/a | audit | Internal | Set when related face attempt is attached. |

### AttendanceRecords

| Column | SQL Type | Required | Max | Index / Relationship | Privacy | Description |
| --- | --- | --- | --- | --- | --- | --- |
| Id | uniqueidentifier | Yes | n/a | PK | Internal | Attendance record identifier. |
| LectureSessionId | uniqueidentifier | Yes | n/a | FK `LectureSessions`; unique with Student | Internal | Lecture session attended. |
| StudentId | uniqueidentifier | Yes | n/a | FK `Students`; unique with session | Internal | Student subject. |
| AttendanceAttemptId | uniqueidentifier | Yes | n/a | FK `AttendanceAttempts`; unique | Internal | Attempt that created the record. |
| FaceVerificationAttemptId | uniqueidentifier | Yes | n/a | FK `FaceVerificationAttempts`; unique | Internal | Matching face verification attempt. |
| Status | int | Yes | n/a | indexed with session | Internal | Present or Late. |
| CheckedInAtUtc | datetimeoffset | Yes | n/a | none | Internal | Successful check-in timestamp. |
| ClassroomId | uniqueidentifier | Yes | n/a | FK `Classrooms`; indexed | Internal | Classroom policy reference. |
| AllowedRadiusMeters | int | Yes | n/a | check >= 0 | Internal | Allowed radius snapshot. |
| MaximumAcceptedAccuracyMeters | int | Yes | n/a | check >= 0 | Internal | Accuracy policy snapshot. |
| DistanceMeters | decimal(9,3) | Yes | n/a | check >= 0 | Internal | Distance from approved classroom policy. |
| BrowserAccuracyMeters | decimal(9,3) | Yes | n/a | check >= 0 | Internal | Browser-reported accuracy. |
| IsActive | bit | Yes | n/a | shared | Internal | Shared active flag; records are not normally edited. |
| RowVersion | rowversion | Yes | n/a | concurrency token | Internal | Optimistic concurrency token. |

Attendance tables intentionally omit raw capture bytes, image paths, base64 images, exact submitted Student coordinates, unprotected embeddings, protected template bytes, template fingerprints, plain challenge tokens, and plain idempotency keys.
