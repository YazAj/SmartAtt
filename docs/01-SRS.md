# 01. Software Requirements Specification

## Real Face Engine Integration Update

The system now includes an optional local Real face engine for Windows x64 localhost using OpenCvSharp YuNet detection and ONNX Runtime SFace embeddings. The default mode remains `Fake`. Real mode is limited to one-to-one enrollment and verification of the authenticated Student's active template; it must not perform attendance registration, location validation, one-to-many identification, or browser-side biometric decisions.

Real mode is partially verified by model-only readiness. Production biometric completion remains blocked until authorized live-camera enrollment, same-person, different-person, no-face, multiple-face, poor-quality, restart, and threshold-calibration evidence is recorded. The system does not claim liveness detection or anti-spoofing.

## Purpose

AttendAI is a responsive university attendance management platform for the graduation project **Smart Attendance Management System Using Face Recognition**. Sprint 5 builds on the verified identity, academic, lecture, and biometric-enrollment foundations by adding secure one-to-one Student self-verification.

## Scope

Sprint 1 includes authentication, role authorization, dashboard shells, localization, RTL/LTR layout support, theming, database foundation, a face-recognition abstraction, a technical spike, and tests.

Sprint 2 adds academic and account management for administrator-created Student and Instructor users. It includes departments, students, instructors, courses, sections, classrooms, enrollments, instructor assignments, academic dashboards, activation/deactivation, temporary password reset, and first-login password change.

Sprint 3 adds weekly lecture schedules, conflict detection, instructor/student timetables, live lecture sessions, temporary session-code management, admin session monitoring, lifecycle audit events, and session timing configuration.

Sprint 4 adds biometric privacy notice, explicit Student consent, capture validation, protected template storage, Student biometric enrollment and re-enrollment, consent withdrawal, Admin biometric oversight, and the real-engine readiness gate.

Sprint 5 adds one-to-one Student self-verification, eligibility checks, capture validation, template compatibility, safe attempt metadata, Admin verification oversight, diagnostics, rate limiting, idempotency, threshold policy, and fake-development runtime verification. It does not implement attendance registration, attendance records, attendance statuses, attendance reports, production real face recognition, liveness detection, location validation, notifications, exports, or one-to-many identification.

## User Roles

- Admin: academic setup, lecture scheduling, session monitoring, biometric oversight, and face-verification oversight owner.
- Instructor: assigned section viewer and authorized lecture-session operator.
- Student: academic profile, enrollment, schedule viewer, biometric profile owner, and one-to-one self-verification actor.

## Product Functions

- Authenticate university users with ASP.NET Core Identity.
- Redirect users to role-specific dashboards.
- Block cross-role dashboard access.
- Support English and Arabic.
- Support light, dark, and system themes.
- Provide a biometric data notice.
- Abstract one-to-one face verification behind a replaceable engine interface.
- Manage academic departments, courses, sections, and classrooms.
- Create Student and Instructor Identity accounts linked to academic profiles.
- Require administrator-created Student and Instructor users to change temporary passwords at first login.
- Enroll students in sections while respecting section capacity.
- Assign instructors to sections while enforcing one active primary instructor.
- Manage weekly lecture schedules for existing Sections, Instructors, and Classrooms.
- Detect Classroom, Instructor, and Section schedule conflicts.
- Allow Instructors and Students to view scoped weekly timetables.
- Allow authorized Instructors to start, regenerate code for, end, and cancel lecture sessions.
- Allow Admin users to monitor, force-end, and expire stale active lecture sessions.
- Preserve session lifecycle audit history.
- Hide temporary session codes from Student responses.
- Manage Student biometric consent, protected template enrollment, re-enrollment, withdrawal, and safe history.
- Allow eligible Students to perform one-to-one self-verification against their own active compatible template.
- Persist safe verification-attempt metadata without raw captures, encodings, protected templates, or template fingerprints.
- Allow Admin users to review verification attempts and engine diagnostics using safe metadata only.
- Clearly label fake-engine verification as development/demo behavior.

## Functional Requirements

| ID | Requirement |
| --- | --- |
| FR-01 | Users can log in and log out with cookie authentication. |
| FR-02 | Authenticated users can change password. |
| FR-03 | Admin, Instructor, and Student roles exist as constants and seedable roles. |
| FR-04 | Users are routed to the correct dashboard for their role. |
| FR-05 | Users cannot access another role's dashboard. |
| FR-06 | Users can switch between English and Arabic. |
| FR-07 | Arabic pages render right-to-left and English pages render left-to-right. |
| FR-08 | Users can use light, dark, or system theme preference. |
| FR-09 | A biometric privacy notice is available. |
| FR-10 | A face-recognition engine abstraction exists for future one-to-one verification. |
| FR-11 | Admin users can manage active/inactive Departments. |
| FR-12 | Admin users can create, update, activate/deactivate, and reset passwords for Student accounts linked to Student profiles. |
| FR-13 | Admin users can create, update, activate/deactivate, and reset passwords for Instructor accounts linked to Instructor profiles. |
| FR-14 | Admin users can manage Courses linked to active Departments. |
| FR-15 | Admin users can manage Sections linked to active Courses by academic year, semester, section number, and capacity. |
| FR-16 | Admin users can manage Classrooms with capacity and optional coordinate pair validation. |
| FR-17 | Admin users can enroll active Students in active Sections without duplicate active enrollment and without exceeding capacity. |
| FR-18 | Admin users can assign active Instructors to active Sections and enforce one active primary instructor per Section. |
| FR-19 | Instructor dashboards show assigned Sections. |
| FR-20 | Student dashboards show academic profile and active enrollments. |
| FR-21 | Non-Admin users cannot access Admin academic management pages. |
| FR-22 | Admin state-changing actions require antiforgery validation. |
| FR-23 | Admin users can create, edit, activate, deactivate, search, filter, and page weekly Lecture Schedules. |
| FR-24 | Lecture Schedule creation and editing reject Classroom, Instructor, and Section time conflicts. |
| FR-25 | Instructors can view schedules for Sections to which they are actively assigned. |
| FR-26 | Students can view schedules for Sections in which they are actively enrolled. |
| FR-27 | Authorized Instructors can start eligible Lecture Sessions inside the configured start window. |
| FR-28 | Lecture Sessions generate secure temporary codes visible only to authorized Instructor/Admin response paths. |
| FR-29 | Authorized Instructors can regenerate, end, and cancel their active Lecture Sessions. |
| FR-30 | Admin users can monitor active/historical Lecture Sessions and force-end active Sessions. |
| FR-31 | Lecture Session lifecycle events are persisted without storing plain codes. |
| FR-32 | Ended, cancelled, expired, and force-ended Sessions invalidate active code state. |
| FR-33 | Students can accept biometric consent, enroll, re-enroll, withdraw consent, and view safe enrollment history. |
| FR-34 | Admin users can view biometric enrollment status and safe metadata without template exposure. |
| FR-35 | Eligible Students can open self-verification status, capture one image, submit it, and receive a localized safe result. |
| FR-36 | Verification uses only the authenticated Student's active compatible template and never performs one-to-many search. |
| FR-37 | Verification attempts are append-only safe metadata and store no raw image, encoding, protected template, template fingerprint, session code, or location data. |
| FR-38 | Verification applies an explicit configurable threshold and score metric. |
| FR-39 | Incompatible templates are rejected safely and marked for re-enrollment. |
| FR-40 | Admin users can search/filter verification attempts, view safe details, and inspect diagnostics/readiness status. |
| FR-41 | Student, Admin, Instructor, anonymous, and cross-Student authorization boundaries are enforced for verification. |

## Non-Functional Requirements

- The solution must build on .NET 8.
- Automated tests must not require a camera or personal biometric files.
- UI must be responsive and readable in both supported themes.
- Layout must be accessible by keyboard and maintain visible focus states.
- Configuration must not contain production secrets.
- Academic setup must preserve historical relationships through activation/deactivation rather than hard deletion.
- SQL Server constraints and service rules must protect duplicate academic records and invalid ranges.
- Lecture scheduling must use server-side conflict checks, transactions, pagination, and efficient projections.
- Session timestamps must be stored in UTC and displayed in the configured application time zone.
- Biometric verification must process captures in memory, persist safe metadata only, and use server-side rate limiting and idempotency.
- Real face-engine verification must not be marked passed without approved samples, licensed models, adapter initialization, and threshold evidence.

## Security Requirements

- HTTPS redirection and HSTS outside development.
- Secure, HTTP-only authentication cookies with SameSite Lax.
- Anti-forgery validation for state-changing MVC actions.
- Safe return URL handling to prevent open redirects.
- Production-safe error pages with trace identifiers.
- No password, raw biometric image, template, or secret logging.
- Administrator-created Student and Instructor users must be assigned only their intended role.
- Inactive academic user accounts must be blocked from login.
- Temporary session codes must be generated securely, hashed/protected at rest, invalidated on terminal transitions, and omitted from Student responses.
- Lecture-session state changes must use antiforgery validation, authorization checks, and optimistic concurrency.
- Face verification POST actions must use antiforgery validation, size limits, MIME/signature validation, safe error mapping, rate limiting, and Student ownership checks.
- The Web layer must not receive template bytes, embeddings, template fingerprints, or raw native model paths.

## Privacy Requirements

- Face data is intended for identity verification only.
- The project should store a face template where possible.
- The system should avoid permanent storage of raw images.
- Access to biometric data must be restricted.
- Sprint 5 performs Student self-verification only; it does not create attendance data.
- Verification captures are not intentionally retained in SQL Server, disk, browser storage, query strings, or logs.

## Localization Requirements

- Support `en-US` and `ar-JO`.
- Persist selected culture in a culture cookie.
- Localize primary navigation, authentication, dashboard, validation, error, theme, language, lecture schedule, lecture session, timetable, countdown, biometric enrollment, and face-verification text.

## Theme Requirements

- Support Light, Dark, and System preference.
- Persist preference in `localStorage`.
- Use CSS custom properties for theme-sensitive colors.

## Business Rules

- Public registration is not required in Sprint 3.
- Student and Instructor production accounts are administrator-created.
- The initial face approach is one-to-one verification.
- Duplicate attendance prevention, location validation, and attendance decisions are future attendance rules.
- Department, Course, Classroom, Student, and Instructor codes/numbers are unique.
- Sections are unique per Course, academic year, semester, and section number.
- Student enrollment is unique per Student and Section.
- Only one active primary Instructor assignment is allowed per Section.
- Lecture Schedules cannot overlap for the same Classroom, Instructor, or Section.
- Lecture Sessions are unique per Lecture Schedule and Session Date.
- A Student can see active-session state but cannot see temporary session codes.
- A Student may verify only themselves after password-change completion, active account/profile checks, active consent, active template, compatible template metadata, enabled engine, cooldown, and rate-limit checks.
- Verification outcomes must be Match, No Match, or a safe rejection state; they must not create attendance records.

## Face Recognition Statement

The project integrates a pre-trained third-party face recognition engine. The recognition model itself is not trained from scratch.

The initial version uses one-to-one face verification, not one-to-many identification.

## Sprint 5 Face Verification Requirements

- Resolve the Student from the authenticated Identity user.
- Reject inactive users, inactive Student profiles, and users still required to change password.
- Require active biometric consent and an active compatible template.
- Validate exactly one capture file named for the verification workflow.
- Validate MIME type, file signature, size, request size, image dimensions, face-count outcome, and quality outcome.
- Unprotect templates only inside Infrastructure and zero unprotected bytes where practical.
- Compare the capture only with the authenticated Student's template.
- Apply `ScoreMetric` and `VerificationThreshold` from configuration.
- Persist only safe `FaceVerificationAttempt` metadata.
- Provide Student status/history/capture/result pages and Admin overview/details/diagnostics pages.
- Keep fake mode labeled as development/demo and keep real mode Not Verified until actual gate evidence exists.

## Out Of Scope

Attendance registration, student code submission for attendance, attendance records, attendance statuses, reports, exports, production real face recognition, liveness detection, anti-spoofing production claims, location validation, notifications, QR attendance, one-to-many identification, classroom-camera recognition, background recognition, and native mobile applications are out of scope for Sprint 5.

## Acceptance Criteria

- Solution builds.
- Tests pass.
- Identity roles and dashboards are implemented.
- Cross-role dashboard access is denied.
- Localization and themes are implemented.
- Face abstraction, fake engine, and POC project exist.
- Required documentation exists.
- Academic migration applies to SQL Server.
- Admin academic CRUD and account workflows run against SQL Server.
- Student and Instructor first-login password change works.
- Role boundaries are enforced for Admin, Instructor, and Student dashboards.
- Lecture schedule migration applies to SQL Server.
- Admin schedule management and conflict detection work.
- Instructor session lifecycle works with secure temporary code handling.
- Student active-session views exclude code data.
- Lifecycle audit events persist.
- Biometric enrollment migration applies to SQL Server.
- Student biometric consent/enrollment/re-enrollment/withdrawal workflows run against SQL Server.
- Admin biometric oversight exposes safe metadata only.
- Face verification migration applies to SQL Server.
- Student self-verification match, no-match, safe rejection, rate-limit, and idempotency paths are tested.
- Admin verification oversight exposes safe attempt metadata only.
- No verification attempt stores raw image, encoding, protected template, template fingerprint, attendance status, or location data.

## Sprint 4 Biometric Enrollment Requirements

- Students must review a localized biometric privacy notice before enrollment.
- Students must explicitly accept versioned biometric consent; Admins cannot accept consent for a Student through normal UI.
- Students may withdraw consent; withdrawal revokes the active protected template.
- Students may enroll and re-enroll only their own profile.
- Enrollment requires the configured capture count and server-side validation for MIME type, signature, size, dimensions, total request size, and expected multipart field.
- Enrollment must reject no-face, multiple-face, invalid-image, and safe quality-failure outcomes.
- Only a protected template and safe metadata may be stored.
- Raw camera frames and uploaded face images must not be intentionally persisted.
- Admins may view enrollment status and safe metadata only.
- Admins may revoke an active template or require re-enrollment with a reason.
- Enrollment events must be append-only and safe for display.
- Fake engine mode may support development/tests but must be blocked in Production.
- Real-engine gate must remain Not Verified until approved samples, selected licensed models, native dependencies, and adapter verification are complete.
- Sprint 4 does not implement attendance submission, face verification against stored templates, attendance decisions, liveness, one-to-many identification, or location validation.

## Sprint 5 Face Verification Status

- One-to-one verification architecture, fake-development workflow, SQL migration, Student/Admin UI, localization, tests, and documentation are implemented.
- Real face-engine verification remains Not Verified because approved samples, selected licensed models, native dependencies, production adapter, and threshold calibration are not available.
- Sprint 5 is therefore Partially completed by the project status rules.
