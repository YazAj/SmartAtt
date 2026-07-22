# 01. Software Requirements Specification

## Purpose

AttendAI is a responsive university attendance management platform for the graduation project **Smart Attendance Management System Using Face Recognition**. Sprint 1 establishes a secure, documented, buildable foundation.

## Scope

Sprint 1 includes authentication, role authorization, dashboard shells, localization, RTL/LTR layout support, theming, database foundation, a face-recognition abstraction, a technical spike, and tests. It does not implement academic CRUD or attendance registration.

## User Roles

- Admin: future academic setup and governance owner.
- Instructor: future lecture session and attendance review owner.
- Student: future attendance registration user.

## Product Functions

- Authenticate university users with ASP.NET Core Identity.
- Redirect users to role-specific dashboards.
- Block cross-role dashboard access.
- Support English and Arabic.
- Support light, dark, and system themes.
- Provide a biometric data notice.
- Abstract one-to-one face verification behind a replaceable engine interface.

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

## Non-Functional Requirements

- The solution must build on .NET 8.
- Automated tests must not require a camera or personal biometric files.
- UI must be responsive and readable in both supported themes.
- Layout must be accessible by keyboard and maintain visible focus states.
- Configuration must not contain production secrets.

## Security Requirements

- HTTPS redirection and HSTS outside development.
- Secure, HTTP-only authentication cookies with SameSite Lax.
- Anti-forgery validation for state-changing MVC actions.
- Safe return URL handling to prevent open redirects.
- Production-safe error pages with trace identifiers.
- No password, raw biometric image, template, or secret logging.

## Privacy Requirements

- Face data is intended for identity verification only.
- The project should store a face template where possible.
- The system should avoid permanent storage of raw images.
- Access to biometric data must be restricted.
- Sprint 1 does not perform production attendance verification.

## Localization Requirements

- Support `en-US` and `ar-JO`.
- Persist selected culture in a culture cookie.
- Localize primary navigation, authentication, dashboard, validation, error, theme, and language text.

## Theme Requirements

- Support Light, Dark, and System preference.
- Persist preference in `localStorage`.
- Use CSS custom properties for theme-sensitive colors.

## Business Rules

- Public registration is not required in Sprint 1.
- Future production accounts should be administrator-created.
- The initial face approach is one-to-one verification.
- Duplicate attendance prevention, location validation, and lecture time validation are future attendance rules.

## Face Recognition Statement

The project integrates a pre-trained third-party face recognition engine. The recognition model itself is not trained from scratch.

The initial version uses one-to-one face verification, not one-to-many identification.

## Out Of Scope

Students, instructors, departments, courses, sections, classrooms, enrollments, instructor assignments, lecture sessions, attendance registration, reports, exports, production face enrollment, production verification, liveness detection, and native mobile applications are out of scope for Sprint 1.

## Acceptance Criteria

- Solution builds.
- Tests pass.
- Identity roles and dashboards are implemented.
- Cross-role dashboard access is denied.
- Localization and themes are implemented.
- Face abstraction, fake engine, and POC project exist.
- Required documentation exists.
