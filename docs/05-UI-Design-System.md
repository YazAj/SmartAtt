# 05. UI Design System

## Principles

- Mobile-first, professional, university-appropriate interface.
- CSS custom properties for all theme-sensitive colors.
- Cards use an 8px radius.
- Layout uses logical spacing so RTL and LTR share one stylesheet.
- No fake attendance analytics are displayed.

## Layout Components

- Responsive sidebar.
- Sticky top navigation.
- Main content region.
- Mobile overlay navigation.
- Breadcrumb-lite title area.
- Account menu.
- Language switcher.
- Theme switcher.
- Footer.
- Global alert area.
- Loading indicator.

## Reusable Component Styles

Implemented CSS classes cover:

- Primary, secondary, and danger buttons.
- Icon buttons.
- Form fields and validation messages.
- Cards and statistic cards.
- Tables.
- Badges and status indicators.
- Modal content.
- Alert messages.
- Pagination.
- Search and filter controls.
- Skeleton loading indicators.
- Empty states.

Sprint 2 reuses these styles for Admin academic management lists, filters, details pages, create/edit forms, reset-password pages, enrollment status pages, primary-assignment pages, and role dashboards.

Sprint 3 extends these styles for lecture schedule lists, filters, create/edit forms, details pages, classroom/section timetables, active-session panels, lifecycle event tables, countdown cards, and code-copy actions.

Sprint 6 extends these styles for Student attendance check-in, browser-location status, challenge countdowns, safe result details, recent attempt tables, and Instructor attendance roster tables.

Sprint 7 extends these styles for report summary cards, report meters, filter grids, responsive report tables, audit tables, CSV export controls, and print-friendly pages.

## Accessibility

- Visible `:focus-visible` ring.
- Color contrast considered for both themes.
- `prefers-reduced-motion` disables nonessential animation.
- Mobile navigation can be closed with Escape.

## Sprint 3 UI Components

- `timetable-grid`, `timetable-day`, and `timetable-item` render weekly schedules without adding a separate calendar framework.
- `session-code-panel` displays the temporary code only for authorized Instructor/Admin response paths.
- `stat-card` countdowns show display-only code/session timers.
- Responsive lecture identifier headings stack long course/section codes on narrow screens.
- Mobile off-canvas navigation remains keyboard-closeable and hidden when closed without using an off-viewport translated state.

## Future UI Work

Future attendance modules should reuse the existing page header, app panel, form, table, badge, empty-state, timetable, and status styles before adding new patterns.

## Sprint 2 Visual Verification

- English LTR Light: Admin dashboard desktop and Departments mobile checked.
- English LTR Dark: Students desktop checked.
- Arabic RTL Light: Courses desktop checked.
- Arabic RTL Dark: Sections mobile checked.
- Browser overflow checks reported no horizontal overflow for the checked desktop and mobile viewports.
- Theme persisted through `localStorage` as `attendai-theme`.
- Culture persisted through the ASP.NET Core culture cookie.

## Sprint 4 UI Components

- `camera-grid` lays out the live preview and capture controls responsively.
- `camera-preview` contains the browser video stream and a localized placeholder.
- `capture-counter` shows how many configured samples are ready.
- `capture-strip` and `capture-thumb` show local preview thumbnails only before submit.
- Biometric Admin pages reuse existing `app-panel`, `details-grid`, `app-table`, `status-badge`, and `filter-grid` patterns.
- Student and Admin dashboards use existing `stat-card` patterns for biometric status and counts.

## Sprint 4 Visual Verification Targets

Required manual/browser combinations:

- English LTR Light Desktop.
- English LTR Dark Mobile.
- Arabic RTL Light Desktop.
- Arabic RTL Dark Tablet.
- Arabic RTL Dark 390px.
- Arabic RTL Dark 320px.

The implementation uses existing design tokens for dark/light colors and logical CSS properties for RTL/LTR alignment. Raw image previews are held in browser memory/file inputs only and are not stored in localStorage.

## Sprint 3 Visual Verification

- English LTR Light Desktop: Admin lecture schedules checked.
- English LTR Dark Mobile: Instructor active-session page checked after responsive heading/action fixes.
- Arabic RTL Light Desktop: Student active-session page checked.
- Arabic RTL Dark Mobile: Admin lecture schedules, schedule forms/details, classroom and section timetables, active sessions, Instructor active session, Student active session, and the redacted code-card response checked at 390px and 320px.
- Arabic RTL Dark Tablet: Admin, Instructor, and Student lecture/session pages checked at 768x1024.
- Programmatic live-browser result: 134 checks, 0 page-level horizontal overflow failures, and no offending visible elements outside the viewport.
- Root cause resolved: the closed mobile sidebar previously used an RTL translate transform that placed hidden fixed content outside the viewport; it now remains anchored with opacity/visibility/pointer-events for the closed state.
- Keyboard walkthrough passed for mobile navigation, filters/forms, timetable/session links, active-session actions, language selector, theme selector, and Escape close behavior.
- Theme persisted through `localStorage` as `attendai-theme`.
- Culture persisted through the ASP.NET Core culture cookie.

## Sprint 5 UI Components

- Student face-verification status uses existing `app-panel`, `status-badge`, `details-grid`, `empty-state`, and action button patterns.
- Student capture reuses the camera grid and preview behavior from biometric enrollment but posts exactly one verification capture.
- Result pages use localized safe outcome copy and score/threshold metadata without displaying images, embeddings, template ids, or template fingerprints.
- Student history and Admin attempt overview reuse `app-table`, `filter-grid`, pagination, badges, and empty states.
- Admin diagnostics uses compact status panels so the fake-development warning, threshold, score metric, rate-limit policy, and real-engine gate are visible without implying production readiness.
- Instructor navigation remains unchanged and has no face-verification management controls.

## Sprint 5 Visual Verification Targets

Required runtime/manual combinations:

- English LTR Light at 1440x900.
- English LTR Dark at 390x844.
- Arabic RTL Light at 1024x768.
- Arabic RTL Dark at 768x1024.
- Arabic RTL Dark at 390x844.
- Arabic RTL Dark at 320x568.

Checked surfaces include sidebar, navbar, breadcrumbs/page titles, dropdowns, forms, placeholders, validation messages, buttons, cards, tables, badges, alerts, empty states, profile menu, Student verification pages, Admin verification pages, dashboards, and error/access-denied flows. Verification capture previews are kept in browser memory/file inputs only and are not written to localStorage, IndexedDB, query strings, or persisted DOM after submit.

## Real Engine UI Update

Real mode reuses the existing biometric enrollment and one-to-one verification UI. The UI must show only safe readiness, match/no-match/capture-rejected/re-enrollment-required/engine-unavailable states. Normal Student pages must not display embeddings, absolute model paths, protected templates, template fingerprints, native exception details, or raw model diagnostics. Admin diagnostics may show safe engine/model names, versions, threshold, metric, expected template format, and embedding dimension.

Manual visual verification for Real camera enrollment and verification remains Not Verified until authorized local participants are available. Existing English/Arabic, LTR/RTL, light/dark responsive checks remain valid for the shared UI shell and Fake workflow.

## Sprint 6 UI Components

- Student attendance index uses existing `page-header`, `app-panel`, `details-grid`, `app-table`, badges, and empty states.
- Student check-in reuses the one-capture camera UI from Sprint 5 and adds a browser-location capture button/status badge.
- Challenge expiry uses the existing countdown helper.
- Student result page renders safe outcome, location, distance, face score, and attendance record metadata without showing images or exact coordinates.
- Instructor roster uses dashboard `stat-card` counts and a responsive `app-table` for enrolled Students.
- Student navigation includes `Attendance check-in`; Instructor active-session view links to the roster for that session.

## Sprint 6 Visual Verification Targets

Required manual/browser combinations:

- English LTR Light desktop.
- English LTR Dark desktop and mobile.
- Arabic RTL Light desktop and mobile.
- Arabic RTL Dark desktop and mobile.

Required surfaces:

- Sidebar and navigation active states.
- Student attendance index.
- Student check-in form, camera controls, file fallback, challenge countdown, browser-location status, validation messages, and submit state.
- Student accepted/rejected result page.
- Student recent attempt table and empty state.
- Instructor roster summary cards, table, missing state, and safe metadata.
- Access denied/unauthorized routes.

Current status: implementation is present; final manual visual verification is pending Sprint 6 closure evidence.

## Sprint 7 UI Components

- Student, Instructor, and Admin report pages reuse `page-header`, `app-panel`, `filter-grid`, `stat-card`, `app-table`, `status-badge`, pagination, alerts, and empty states.
- Report summary cards display eligible sessions, Present, Late, Missed, and attendance percentage using server-calculated values.
- Report meters are tokenized and avoid using color alone as the only meaning.
- Admin audit search uses the same filter and table patterns as existing Admin pages.
- CSV and print actions are regular keyboard-reachable buttons/links.
- Print styling hides navigation and filter chrome while retaining the authorized report content.

## Sprint 7 Visual Verification Targets

Required manual/browser combinations:

- English LTR Light desktop, tablet, 390px, and 320px.
- English LTR Dark desktop, tablet, 390px, and 320px.
- Arabic RTL Light desktop, tablet, 390px, and 320px.
- Arabic RTL Dark desktop, tablet, 390px, and 320px.

Required surfaces:

- Sidebar and active report navigation.
- Navbar, language selector, theme selector, and profile menu.
- Student report page.
- Instructor report page.
- Admin report page.
- Admin audit page.
- Filters, dropdowns, placeholders, validation messages, buttons, cards, tables, badges, alerts, empty states, pagination, CSV export control, and print action.

Current status: the implementation uses existing design tokens and logical CSS. Final full manual visual/accessibility verification for Sprint 7 remains pending and must not be marked Passed until browser evidence is recorded.
