# 11. Requirements Traceability Matrix

## Sprint 1 Foundation

| ID | Description | Implementation Location | Test / Verification Location | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| R-01 | Clean Architecture solution | `src/` projects, `AttendAI.sln` | `dotnet build AttendAI.sln` | Passed | Build succeeded with 0 warnings and 0 errors. |
| R-02 | Identity login/logout/change password | `AccountController`, Account views, Identity configuration | `AccountFlowTests` and runtime verification | Passed | Valid login, password change, logout, and secure-cookie flows passed. |
| R-03 | Role constants and seeding | `RoleConstants`, `IdentitySeedService` | `RoleConstantsTests`, SQL role query | Passed | Admin, Instructor, and Student roles exist in SQL Server. |
| R-04 | Role dashboards | `DashboardController`, dashboard views | `DashboardAuthorizationTests`, runtime verification | Passed | Role dashboard access and login redirects passed. |
| R-05 | Safe return URLs | `SafeRedirectService`, Account and Preferences controllers | `SafeRedirectServiceTests`, `CultureSwitchTests` | Passed | Unit and integration tests passed. |
| R-06 | SQL Server EF Core foundation | `ApplicationDbContext`, migrations | SQL Server LocalDB verification | Passed | `InitialIdentityFoundation` applied in `AttendAI_Sprint2Closure_20260722`. |
| R-07 | Responsive UI foundation | Layout, `site.css`, pages | Browser screenshots and overflow checks | Passed | Checked desktop/mobile pages reported no horizontal overflow. |
| R-08 | English and Arabic localization | `Resources/*.resx`, localization setup | `CultureSwitchTests`, browser culture checks | Passed | English rendered `ltr`; Arabic rendered `rtl`; culture persisted. |
| R-09 | Theme system | `site.css`, `site.js`, layout head script | Browser localStorage checks | Passed | Light/dark preferences persisted after refresh. |
| R-10 | Face abstraction and fake engine | Application face contracts, `FakeFaceRecognitionEngine` | `FakeFaceRecognitionEngineTests` | Passed | Fake engine unit tests passed. |
| R-11 | Separate face POC | `experiments/AttendAI.FaceRecognition.Poc` | `dotnet build AttendAI.sln` | Passed | POC project included in successful solution build. |

## Sprint 2 Academic And Account Management

| ID | Description | Implementation Location | Test / Verification Location | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| R-12 | Academic domain model | `src/AttendAI.Domain/Academic`, `src/AttendAI.Domain/Enums` | `AcademicEntityTests` | Passed | Domain validation tests passed. |
| R-13 | Application service contracts and DTOs | `src/AttendAI.Application/Academic`, `Common/Models` | Build and integration tests | Passed | Solution build and service tests passed. |
| R-14 | SQL Server academic migration | `20260722140818_AddAcademicManagement` | EF update and SQL queries | Passed | Migration present; academic tables, FKs, indexes, and checks verified. |
| R-15 | Department management | Admin Departments controller/views, `DepartmentService` | Runtime Admin CRUD, SQL count | Passed | Department created and details page returned 200. |
| R-16 | Student account/profile management | Admin Students controller/views, `StudentService` | `AcademicManagementServiceTests`, runtime flow | Passed | Student account created with Student role and required password change. |
| R-17 | Instructor account/profile management | Admin Instructors controller/views, `InstructorService` | `AcademicManagementServiceTests`, runtime flow | Passed | Instructor account created with Instructor role and required password change. |
| R-18 | Course management | Admin Courses controller/views, `CourseService` | Runtime Admin CRUD, SQL count | Passed | Course created and linked to Department. |
| R-19 | Section management | Admin Sections controller/views, `SectionService` | Runtime Admin CRUD, SQL count | Passed | Section created and linked to Course. |
| R-20 | Classroom management | Admin Classrooms controller/views, `ClassroomService` | Runtime Admin CRUD, SQL count | Passed | Classroom created with capacity and coordinate pair. |
| R-21 | Student enrollment rules | Admin Enrollments controller/views, `EnrollmentService` | `Duplicate_enrollment_and_capacity_are_rejected`, runtime flow | Passed | Duplicate enrollment and over-capacity enrollment rejected; one enrollment created at runtime. |
| R-22 | Instructor assignment rules | Admin InstructorAssignments controller/views, `InstructorAssignmentService` | `Section_allows_only_one_active_primary_instructor`, runtime flow | Passed | Primary instructor uniqueness enforced; one assignment created at runtime. |
| R-23 | Academic dashboards | `DashboardController`, Dashboard views, `AcademicDashboardService` | Runtime dashboard checks | Passed | Admin, Instructor, and Student dashboards returned 200 with academic data. |
| R-24 | Admin authorization boundary | Admin area base controller | `AcademicManagementAuthorizationTests` | Passed | Admin can access; Instructor/Student forbidden; anonymous unauthorized. |
| R-25 | Cross-role dashboard boundary | Dashboard authorization attributes | Runtime verification | Passed | Student and Instructor cross-role requests redirected to Access Denied. |
| R-26 | Admin antiforgery | `AdminControllerBase`, Admin forms | `Admin_post_without_antiforgery_token_is_rejected` | Passed | Authenticated Admin POST without token returned 400. |
| R-27 | Localization and RTL for academic UI | `SharedResource.*.resx`, Admin views, layout | Browser screenshots and overflow checks | Passed | English LTR and Arabic RTL checked in light/dark desktop/mobile surfaces. |
| R-28 | No Sprint 3 attendance behavior | Absence of lecture/attendance production controllers and tables | Source review | Passed | No lecture session, attendance record, face enrollment, report, or production verification feature implemented. |
| R-29 | Secrets and sensitive files | `.gitignore`, config, docs, source | Repository secret scan and `git check-ignore` | Passed | No non-ignored credential, key, biometric sample, model, database, or build output found. |
| R-30 | Automated quality gate | Whole solution | Restore/build/test/format | Passed | Restore/build/test/format all exited 0; 47 tests passed. |

## Sprint 3 Lecture Scheduling And Session Management

| ID | Description | Implementation Location | Test / Verification Location | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| R-31 | Lecture scheduling domain model | `AttendAI.Domain/Academic`, lecture enums | `LectureDomainTests` | Passed | Domain tests cover schedule validation and session transitions. |
| R-32 | Lecture application contracts | `AttendAI.Application/Lectures` | Build and integration tests | Passed | DTOs, commands, queries, result models, and service interfaces compile and are exercised. |
| R-33 | SQL Server lecture migration | `20260722160127_AddLectureSchedulingAndSessions` | EF update and SQL smoke query | Passed | `LectureSchedules`, `LectureSessions`, and `LectureSessionEvents` exist in LocalDB verification database. |
| R-34 | Admin schedule CRUD | Admin `LectureSchedulesController` and views | Runtime smoke flow | Passed | Admin created a valid Lecture Schedule against SQL Server. |
| R-35 | Schedule conflict detection | `LectureScheduleService` | Unit/integration/runtime smoke | Passed | Runtime conflicting Schedule was rejected; tests cover protected-resource conflicts. |
| R-36 | Instructor assignment enforcement | `LectureLookupService`, `LectureScheduleService`, `LectureSessionService` | Integration tests and runtime smoke | Passed | Only assigned Instructor could start/manage the runtime Session. |
| R-37 | Instructor timetable | `InstructorScheduleController`, Instructor views | Runtime smoke | Passed | Instructor changed password, opened schedule, and started an eligible Session. |
| R-38 | Student timetable and active-session view | `StudentScheduleController`, Student views | Runtime smoke | Passed | Student changed password, saw active Session, and code was hidden. |
| R-39 | Secure session-code lifecycle | `SessionCodeService`, `LectureSessionService` | Unit/integration/runtime smoke | Passed | Code generated, regenerated, previous code invalidated, terminal state invalidated code. |
| R-40 | Admin session monitoring and force-end | Admin `LectureSessionsController` and views | Integration tests and code review | Passed | Admin controller/actions exist; force-end service path appends events and invalidates code. |
| R-41 | Session lifecycle audit | `LectureSessionEvents` and session services | SQL smoke query | Passed | Runtime smoke recorded 4 lifecycle events for the verified Session. |
| R-42 | Time-zone strategy | `ApplicationTimeZoneService`, `LectureSchedulingOptions` | `LectureSecurityAndTimeZoneTests` | Passed | UTC/local conversion tests pass for configured application time zone. |
| R-43 | Dashboards and navigation | Layout and dashboard views | Runtime smoke/build | Passed | Lecture schedule/session links render for Admin, Instructor, and Student dashboards. |
| R-44 | Sprint 3 localization | `SharedResource.en-US.resx`, `SharedResource.ar-JO.resx`, Razor views | Browser snapshots | Passed | English LTR and Arabic RTL strings render in checked lecture pages. |
| R-45 | Sprint 3 theme coverage | `site.css`, `site.js`, layout | Live Playwright browser checks | Passed | English LTR light desktop/dark mobile and Arabic RTL light/dark desktop, tablet, 390px mobile, and 320px mobile passed; 134 checks reported 0 page-level overflow failures. |
| R-46 | No Sprint 4 attendance behavior | Source review and SQL schema | `rg` source search, migration review | Passed | No attendance records, student code submission, face enrollment, reports, notifications, or location validation were added. |
| R-47 | Sprint 3 automated quality gate | Whole solution | Restore/build/test/format | Passed | Restore/build/test/format exited 0; 63 tests passed; `git diff --check` exited 0. |

## Sprint 4 Biometric Face Enrollment

| ID | Description | Implementation Location | Test / Verification Location | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| R-48 | Biometric consent domain | `BiometricConsent`, EF configuration | `BiometricDomainTests`, migration review | Passed | Consent supports version/hash, withdrawal, active flag, rowversion, and one active consent index. |
| R-49 | Protected Student face template domain | `StudentFaceTemplate`, EF configuration | `BiometricDomainTests`, migration review | Passed | Protected template metadata, revocation, versioning, checks, and one active template index implemented. |
| R-50 | Append-only enrollment events | `FaceEnrollmentEvent`, `BiometricEnrollmentService` | Build, service review | Passed | Consent, enrollment, replacement, revocation, and failure events append safe data only. |
| R-51 | Application biometric contracts | `AttendAI.Application/Biometrics` | Build | Passed | DTOs, commands, queries, result models, and service interfaces compile. |
| R-52 | Face engine modes and readiness | `FaceEngineReadinessService`, DI | `FaceEnrollmentProcessorTests`, docs gate | Passed | Fake/Disabled/Real modes implemented; Fake blocked in Production; Real gate Not Verified. |
| R-53 | Capture validation | `FaceCaptureValidator`, controller request checks | `BiometricCaptureValidatorTests` | Passed | MIME, signature, size, dimensions, total request size, and unexpected field checks implemented. |
| R-54 | Template protection | `BiometricTemplateProtector` | `BiometricTemplateProtectorTests` | Passed | Protected bytes differ from original and round-trip through Data Protection. |
| R-55 | Student biometric UI | `BiometricEnrollmentController`, Student views, `site.js`, `site.css` | Build, integration authorization tests, runtime smoke | Passed | Privacy, consent, status, enrollment, re-enrollment, withdrawal, and history pages verified against SQL Server. |
| R-56 | Admin biometric management | Admin `BiometricEnrollmentsController` and views | `BiometricEnrollmentAuthorizationTests`, runtime smoke | Passed | Overview, filters, details, safe metadata, revocation, require re-enrollment, and history verified; template bytes were not exposed. |
| R-57 | Biometric dashboard/navigation | Layout and dashboard views/services | Build | Passed | Admin metrics and Student status link added; Instructor has no biometric controls. |
| R-58 | Biometric localization and theme | `SharedResource.*.resx`, `site.css` | Playwright browser checks | Passed | English LTR light desktop, English LTR dark mobile, Arabic RTL light desktop, Arabic RTL dark tablet, Arabic RTL dark 390px, and Arabic RTL dark 320px checked without page-level overflow. |
| R-59 | SQL Server biometric migration | `20260722190900_AddBiometricEnrollment` | SQL Server LocalDB verification | Passed | Migration applied to `AttendAI_Sprint4Verification_20260722`; 3 biometric tables, 6 FKs, 2 filtered unique indexes, rowversions, 4 checks, and no raw-image column verified. |
| R-60 | No raw image retention | Schema, controller, service, docs | Migration/source scan | Passed | No raw-image table/column added; captures are processed from memory and not saved. |
| R-61 | No Sprint 5 behavior | Source/migration review | `rg` source and migration review | Passed | No attendance submission, face verification UI, location validation, reports, or attendance decisions added. |

## Sprint 5 Face Verification And One-To-One Matching

| ID | Description | Implementation Location | Test / Verification Location | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| R-62 | Verification attempt domain model | `FaceVerificationAttempt`, verification enums | `FaceVerificationDomainTests`, migration review | Passed | Append-only safe metadata entity and enums compile and tests pass. |
| R-63 | Verification application contracts | `AttendAI.Application/FaceVerification` | Build and integration tests | Passed | Commands, queries, DTOs, options, result models, and service interfaces compile and are exercised. |
| R-64 | SQL Server verification migration | `20260722221335_AddFaceVerification` | EF update and SQL schema queries | Passed | `FaceVerificationAttempts` exists with FKs, indexes, checks, and no image/embedding/template columns. |
| R-65 | Student ownership and eligibility | `FaceVerificationService`, `FaceVerificationEligibilityService`, `FaceVerificationController` | Integration tests and runtime smoke | Passed | Student is resolved from authenticated user; inactive/password/consent/template/engine/rate checks are enforced. |
| R-66 | One-to-one verification pipeline | `OneToOneFaceVerifier` | Integration tests and runtime smoke | Passed | Capture validates, one face is required, quality checked, active template loaded, unprotected in Infrastructure, compared one-to-one, and attempt persisted. |
| R-67 | Threshold and score policy | `FaceVerificationPolicyService`, options | `FaceVerificationPolicyTests`, runtime diagnostics | Passed | Cosine similarity matches when score is greater than or equal to threshold; Euclidean distance matches when score is less than or equal to threshold. |
| R-68 | Template compatibility and re-enrollment | `TemplateCompatibilityService`, `FaceVerificationService` | `TemplateCompatibilityServiceTests`, `VerifyCurrentStudentAsync_marks_incompatible_active_template_for_re_enrollment` | Passed | Engine/model/format/dimension mismatch rejects safely and marks active template for re-enrollment. |
| R-69 | Rate limiting and idempotency | `InMemoryFaceVerificationRateLimiter`, unique filtered index | Runtime smoke, integration tests, migration review | Passed | Repeated client request id returns the existing attempt; rate-limit attempt is recorded safely. |
| R-70 | Student verification UI | `Views/FaceVerification`, `FaceVerificationController`, `site.js` | Runtime smoke and manual UI checks | Passed | Status, capture, result, history, demo warning, and camera-track cleanup implemented. |
| R-71 | Admin verification oversight | Admin `FaceVerificationsController` and views | Authorization tests and runtime smoke | Passed | Overview, filters, details, diagnostics, and safe metadata verified. |
| R-72 | Authorization boundaries | Controllers and role attributes | `FaceVerificationAuthorizationTests`, runtime smoke | Passed | Student allowed only for Student routes; Admin allowed only for Admin routes; Instructor/anonymous rejected. |
| R-73 | Privacy and sensitive data boundary | Schema, DTOs, views, services | SQL/source/HTML/runtime scans | Passed | Attempts and HTML omit raw images, encodings, protected templates, template fingerprints, model paths, and attendance data. |
| R-74 | Localization and theme coverage | `SharedResource.*.resx`, Razor views, CSS/JS | Browser/manual verification and build | Passed | English/Arabic, LTR/RTL, light/dark resources and tokenized styling are implemented. |
| R-75 | Dashboards and navigation | Layout, dashboards, `AcademicDashboardService` | Runtime smoke and build | Passed | Admin verification metrics and Student verification status/link added; Instructor remains unchanged. |
| R-76 | Automated Sprint 5 quality gate | Whole solution | Restore/build/test/format/git diff | Passed | Final command results are recorded in `docs/31-Sprint-5-Review.md`. |
| R-77 | Real engine readiness | `FaceEngineDiagnosticsService`, docs | Gate review | Not Verified | Approved samples, licensed models, adapter, native dependencies, and threshold calibration are unavailable. |
| R-78 | No Sprint 6 scope | Source/migration review | `rg` source search and schema review | Passed | No attendance records/statuses/reports, location validation, exports, notifications, or one-to-many identification were implemented. |

## Not Verified / Future Gates

| ID | Description | Status | Required Before Production |
| --- | --- | --- | --- |
| G-01 | Real face recognition runtime | Not Verified | Approved biometric samples, selected licensed detection and embedding models, preprocessing, threshold calibration, production adapter, and native deployment package. |
| G-02 | Production attendance registration | Not Implemented | Sprint 6 planning after Sprint 5 acceptance and real face-engine/threshold gate. |
| G-03 | Notification delivery for temporary passwords | Not Implemented | Secure out-of-band delivery design; temporary passwords are entered by Admin during Sprint 2. |
| G-04 | Production liveness and anti-spoofing | Not Implemented | Separate approved design, licensed model/dependency review, and security testing. |
