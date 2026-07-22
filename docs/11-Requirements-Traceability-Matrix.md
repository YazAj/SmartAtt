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

## Not Verified / Future Gates

| ID | Description | Status | Required Before Production |
| --- | --- | --- | --- |
| G-01 | Real face recognition runtime | Not Verified | Approved biometric samples, selected licensed detection and embedding models, preprocessing, threshold calibration, and native deployment package. |
| G-02 | Production attendance registration | Not Implemented | Sprint 4 planning after Sprint 3 acceptance and face-recognition gate. |
| G-03 | Notification delivery for temporary passwords | Not Implemented | Secure out-of-band delivery design; temporary passwords are entered by Admin during Sprint 2. |
