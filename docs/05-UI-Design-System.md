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
