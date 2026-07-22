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
- Main flow: Page explains purpose, storage preference, access limits, and Sprint 1 limitations.

## Future Use Cases Not Implemented

Academic CRUD, lecture session management, attendance registration, location validation, duplicate prevention, reports, production face enrollment, and production face verification.
