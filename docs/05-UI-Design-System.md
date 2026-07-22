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

## Accessibility

- Visible `:focus-visible` ring.
- Color contrast considered for both themes.
- `prefers-reduced-motion` disables nonessential animation.
- Mobile navigation can be closed with Escape.

## Future UI Work

Future academic modules should reuse the existing page header, app panel, form, table, badge, empty-state, and status styles before adding new patterns.

## Sprint 2 Visual Verification

- English LTR Light: Admin dashboard desktop and Departments mobile checked.
- English LTR Dark: Students desktop checked.
- Arabic RTL Light: Courses desktop checked.
- Arabic RTL Dark: Sections mobile checked.
- Browser overflow checks reported no horizontal overflow for the checked desktop and mobile viewports.
- Theme persisted through `localStorage` as `attendai-theme`.
- Culture persisted through the ASP.NET Core culture cookie.
