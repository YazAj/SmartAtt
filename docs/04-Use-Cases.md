# 04. Use Cases

## UC-01 Login

- Actor: Admin, Instructor, Student
- Trigger: User opens login page.
- Main flow: Enter email and password, validate account, issue secure cookie, redirect to role dashboard.
- Alternate flow: Invalid credentials show friendly validation. Disabled account is rejected.

## UC-02 Logout

- Actor: Authenticated user
- Trigger: User selects logout.
- Main flow: Antiforgery token is validated, cookie is cleared, user returns home.

## UC-03 Change Password

- Actor: Authenticated user
- Trigger: User opens account security page.
- Main flow: Enter current and new password, validate Identity rules, refresh sign-in.
- Sprint 2 extension: Student and Instructor accounts created with temporary passwords are redirected here until password change succeeds.

## UC-04 Open Role Dashboard

- Actor: Authenticated user
- Trigger: User opens dashboard.
- Main flow: Role service routes Admin, Instructor, or Student to the correct dashboard.
- Alternate flow: Cross-role dashboard access is denied.

## UC-05 Switch Language

- Actor: Any user
- Trigger: User selects English or Arabic.
- Main flow: Culture cookie is set and user returns to the local page.
- Security: External return URLs are rejected.

## UC-06 Switch Theme

- Actor: Any user
- Trigger: User activates theme button.
- Main flow: Preference cycles through System, Light, and Dark and persists in localStorage.

## UC-07 Review Biometric Notice

- Actor: Any user
- Trigger: User opens the biometric data notice.
- Main flow: Page explains purpose, storage preference, access limits, and current production biometric limitations.

## UC-08 Manage Departments

- Actor: Admin
- Trigger: Admin opens Academic Management > Departments.
- Main flow: Search, create, edit, view details, activate, and deactivate departments.
- Rule: Department codes are unique and normalized.

## UC-09 Manage Student Accounts

- Actor: Admin
- Trigger: Admin opens Academic Management > Students.
- Main flow: Create Identity account plus Student profile, assign Student role, require first-login password change, edit profile, reset temporary password, activate/deactivate account.
- Rule: Deactivated Student accounts cannot log in.

## UC-10 Manage Instructor Accounts

- Actor: Admin
- Trigger: Admin opens Academic Management > Instructors.
- Main flow: Create Identity account plus Instructor profile, assign Instructor role, require first-login password change, edit profile, reset temporary password, activate/deactivate account.
- Rule: Deactivated Instructor accounts cannot log in.

## UC-11 Manage Courses, Sections, And Classrooms

- Actor: Admin
- Trigger: Admin opens the relevant Academic Management module.
- Main flow: Search, create, edit, view details, activate, and deactivate records.
- Rules: Course credit hours are 1-6; Section capacity is positive; Classroom capacity is positive; coordinates must be supplied as a valid pair.

## UC-12 Manage Enrollments

- Actor: Admin
- Trigger: Admin opens Academic Management > Enrollments.
- Main flow: Enroll active Students in active Sections, view details, and change enrollment status.
- Rules: Duplicate Student/Section enrollments are rejected, and active enrollment count cannot exceed Section capacity.

## UC-13 Manage Instructor Assignments

- Actor: Admin
- Trigger: Admin opens Academic Management > Instructor assignments.
- Main flow: Assign active Instructors to active Sections, view details, deactivate assignments, and set primary instructor status.
- Rule: Only one active primary Instructor assignment is allowed for a Section.

## UC-14 Open Academic Dashboards

- Actor: Admin, Instructor, Student
- Trigger: User opens dashboard.
- Main flow: Admin sees active academic counts and quick links; Instructor sees assigned Sections; Student sees academic profile and active enrollments.
- Alternate flow: Cross-role access redirects to Access Denied.

## UC-15 Manage Lecture Schedules

- Actor: Admin
- Trigger: Admin opens Lecture Management > Lecture schedules.
- Main flow: Search, filter, page, create, edit, view details, activate, deactivate, and open classroom/section timetables.
- Rules: Instructor must be actively assigned to the Section; Classroom capacity must cover active enrollments; active schedules cannot overlap for the same Classroom, Instructor, or Section.

## UC-16 Monitor Lecture Sessions

- Actor: Admin
- Trigger: Admin opens Lecture Management > Lecture sessions.
- Main flow: Search/filter active and historical Sessions, inspect lifecycle events, expire stale Sessions, and force-end an active Session with a safe reason.
- Rule: Force-end invalidates active code state and appends lifecycle events.

## UC-17 View Instructor Schedule

- Actor: Instructor
- Trigger: Instructor opens My schedule.
- Main flow: View assigned weekly timetable; eligible schedule occurrences show Start Session; active Sessions show Open Active Session.
- Rules: Instructor must have an active profile, active Identity account, and active assignment to the Section.

## UC-18 Manage Instructor Lecture Session

- Actor: Instructor
- Trigger: Instructor starts or opens an active lecture session.
- Main flow: Start within configured window, see the temporary code, copy code, view code/session countdowns, regenerate code, end, or cancel.
- Rules: Plain code is returned only in the authorized start/regenerate response; terminal states invalidate code.

## UC-19 View Student Schedule And Active Session

- Actor: Student
- Trigger: Student opens My schedule or active lecture session.
- Main flow: View weekly timetable for actively enrolled Sections and active Session status.
- Rule: Student response never contains plain code, protected code, or code hash.

## UC-20 Manage Student Biometric Profile

- Actor: Student
- Trigger: Student opens Biometric profile.
- Main flow: Review status, open privacy notice, accept consent, enroll with configured captures, view active status, re-enroll, withdraw consent, and review safe history.
- Rules: Student acts only on their own profile; active Student profile and consent are required for enrollment; raw images are not stored.

## UC-21 Admin Biometric Oversight

- Actor: Admin
- Trigger: Admin opens Biometric enrollment oversight.
- Main flow: Search/filter Students by enrollment status, view safe metadata/history, revoke active template, or require re-enrollment with a reason.
- Rules: Admin cannot see protected template bytes and cannot accept Student consent.

## UC-22 Student Face Self-Verification

- Actor: Student
- Trigger: Student opens Face verification.
- Main flow: The system resolves the Student from the authenticated user, checks active account/profile, password-change completion, active consent, active compatible template, enabled engine, cooldown, and rate limits. The Student captures one image, submits it with antiforgery protection, receives Match, No Match, or a safe rejection result, and can review paged safe history.
- Rules: The browser never posts a StudentId or template id. The capture is processed in memory, compared only against the Student's own active compatible template, and is not stored.

## UC-23 Admin Face-Verification Oversight

- Actor: Admin
- Trigger: Admin opens Face verification oversight.
- Main flow: Search and filter attempts by Student, outcome, decision, and date, inspect safe attempt details, and review diagnostics/readiness information.
- Rules: Admin sees safe metadata only. Protected templates, template fingerprints, embeddings, raw captures, model paths, and raw native errors are not exposed.

## Future Use Cases Not Implemented

Attendance registration, student code submission for attendance, location validation for attendance, duplicate attendance prevention, reports, exports, notifications, one-to-many face identification, classroom-camera recognition, liveness production claims, production real-engine verification, and production attendance decisions.
