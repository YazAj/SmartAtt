# 01. Software Requirements Specification

## Purpose

AttendAI is a responsive university attendance management platform for the graduation project **Smart Attendance Management System Using Face Recognition**. Sprint 1 establishes a secure, documented, buildable foundation.

## Scope

Sprint 1 includes authentication, role authorization, dashboard shells, localization, RTL/LTR layout support, theming, database foundation, a face-recognition abstraction, a technical spike, and tests.

Sprint 2 adds academic and account management for administrator-created Student and Instructor users. It includes departments, students, instructors, courses, sections, classrooms, enrollments, instructor assignments, academic dashboards, activation/deactivation, temporary password reset, and first-login password change. It does not implement lecture sessions, attendance registration, reports, exports, production face enrollment, production face verification, or liveness detection.

## User Roles

- Admin: future academic setup and governance owner.
- Instructor: assigned section viewer and future lecture session/attendance review owner.
- Student: academic profile/enrollment viewer and future attendance registration user.

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

## Non-Functional Requirements

- The solution must build on .NET 8.
- Automated tests must not require a camera or personal biometric files.
- UI must be responsive and readable in both supported themes.
- Layout must be accessible by keyboard and maintain visible focus states.
- Configuration must not contain production secrets.
- Academic setup must preserve historical relationships through activation/deactivation rather than hard deletion.
- SQL Server constraints and service rules must protect duplicate academic records and invalid ranges.

## Security Requirements

- HTTPS redirection and HSTS outside development.
- Secure, HTTP-only authentication cookies with SameSite Lax.
- Anti-forgery validation for state-changing MVC actions.
- Safe return URL handling to prevent open redirects.
- Production-safe error pages with trace identifiers.
- No password, raw biometric image, template, or secret logging.
- Administrator-created Student and Instructor users must be assigned only their intended role.
- Inactive academic user accounts must be blocked from login.

## Privacy Requirements

- Face data is intended for identity verification only.
- The project should store a face template where possible.
- The system should avoid permanent storage of raw images.
- Access to biometric data must be restricted.
- Sprint 2 does not perform production attendance verification.

## Localization Requirements

- Support `en-US` and `ar-JO`.
- Persist selected culture in a culture cookie.
- Localize primary navigation, authentication, dashboard, validation, error, theme, and language text.

## Theme Requirements

- Support Light, Dark, and System preference.
- Persist preference in `localStorage`.
- Use CSS custom properties for theme-sensitive colors.

## Business Rules

- Public registration is not required in Sprint 2.
- Public registration is not required in Sprint 2.
- Student and Instructor production accounts are administrator-created.
- The initial face approach is one-to-one verification.
- Duplicate attendance prevention, location validation, and lecture time validation are future attendance rules.
- Department, Course, Classroom, Student, and Instructor codes/numbers are unique.
- Sections are unique per Course, academic year, semester, and section number.
- Student enrollment is unique per Student and Section.
- Only one active primary Instructor assignment is allowed per Section.

## Face Recognition Statement

The project integrates a pre-trained third-party face recognition engine. The recognition model itself is not trained from scratch.

The initial version uses one-to-one face verification, not one-to-many identification.

## Out Of Scope

Lecture sessions, attendance registration, reports, exports, production face enrollment, production verification, liveness detection, and native mobile applications are out of scope for Sprint 2.

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
