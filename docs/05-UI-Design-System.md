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

## Accessibility

- Visible `:focus-visible` ring.
- Color contrast considered for both themes.
- `prefers-reduced-motion` disables nonessential animation.
- Mobile navigation can be closed with Escape.

## Future UI Work

Future academic modules should reuse the existing page header, app panel, form, table, badge, empty-state, and status styles before adding new patterns.
