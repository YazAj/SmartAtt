# 06. Localization And Theming

## Supported Cultures

- `en-US`
- `ar-JO`

English is the default culture.

## Resource Files

- `src/AttendAI.Web/Resources/SharedResource.en-US.resx`
- `src/AttendAI.Web/Resources/SharedResource.ar-JO.resx`

Main navigation, authentication, dashboards, validation, errors, language controls, theme controls, and shared components are localized.

## Culture Switching

`PreferencesController.SetCulture` accepts a supported culture, writes the ASP.NET Core culture cookie, validates the return URL, and redirects only to a local URL.

## Direction

The layout computes direction from `CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft`.

```html
<html lang="ar" dir="rtl">
<html lang="en" dir="ltr">
```

Bootstrap RTL CSS is loaded for Arabic. Custom CSS uses logical properties such as `padding-inline`, `border-inline-end`, `margin-inline`, and `inset-inline`.

## Theme Support

Supported preferences:

- System
- Light
- Dark

The selected preference is stored in `localStorage` under `attendai-theme`. An inline head script applies the resolved theme before CSS loads.

## Closure Verification Evidence

Runtime visual and state checks were performed against the live app at `https://localhost:7052` using Edge through Playwright.

| Combination | Viewport | Direction | Theme | Result |
| --- | --- | --- | --- | --- |
| English light desktop | 1366x900 | `ltr` | `light` | Passed |
| English dark mobile | 390x844 | `ltr` | `dark` | Passed |
| Arabic light desktop | 1366x900 | `rtl` | `light` | Passed |
| Arabic dark mobile | 390x844 | `rtl` | `dark` | Passed |

The checks verified `lang`, `dir`, persisted `attendai-theme`, sidebar/topbar/language/theme controls, no body/table horizontal overflow, and readable contrast. Screenshots were saved under ignored `logs/ui-verification/` for local inspection.

## Manual Verification Checklist

| Scenario | Status |
| --- | --- |
| Desktop light mode | Passed |
| Desktop dark mode | Partially verified by dark token checks; desktop dark screenshot not captured |
| Mobile light mode | Partially verified by responsive rules and no-overflow checks; mobile light screenshot not captured |
| Mobile dark mode | Passed |
| Arabic RTL | Passed |
| English LTR | Passed |
| Sidebar behavior | Passed by DOM and screenshot inspection |
| Mobile menu | Rendered; open/close click interaction not separately recorded |
| Login validation | Login page verified; invalid validation-message visual not separately recorded |
| Access denied page | Passed through cross-role runtime verification |
| Keyboard navigation | Focus styles verified in CSS; full keyboard walk-through not recorded |
| Browser refresh theme persistence | Passed |
| Browser refresh culture persistence | Passed through culture cookie state |
