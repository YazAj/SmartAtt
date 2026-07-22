# 06. Localization And Theming

## Supported Cultures

- `en-US`
- `ar-JO`

English is the default culture.

## Resource Files

- `src/AttendAI.Web/Resources/SharedResource.en-US.resx`
- `src/AttendAI.Web/Resources/SharedResource.ar-JO.resx`

Main navigation, authentication, dashboards, validation, errors, language controls, theme controls, lecture schedules, lecture sessions, timetables, countdowns, and shared components are localized.

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
| Desktop dark mode | Passed |
| Mobile light mode | Passed |
| Mobile dark mode | Passed |
| Arabic RTL | Passed |
| English LTR | Passed |
| Sidebar behavior | Passed by DOM and screenshot inspection |
| Mobile menu | Passed, including keyboard open and Escape close |
| Login validation | Login page verified; invalid validation-message visual not separately recorded |
| Access denied page | Passed through cross-role runtime verification |
| Keyboard navigation | Passed for the Sprint 3 closure route set |
| Browser refresh theme persistence | Passed |
| Browser refresh culture persistence | Passed through culture cookie state |

## Sprint 3 Verification Evidence

Runtime visual snapshots were generated from authenticated SQL Server-backed flows under ignored `artifacts/verification/sprint3-visual/`. The final closure pass used live Playwright browser contexts against `https://127.0.0.1:5443` for the authoritative responsive and overflow results.

| Combination | Viewport | Direction | Theme | Result |
| --- | --- | --- | --- | --- |
| English lecture/session pages | 1440x900 | `ltr` | `light` | Passed |
| English lecture/session pages | 390x844 | `ltr` | `dark` | Passed |
| Arabic lecture/session pages | 1440x900 | `rtl` | `light` | Passed |
| Arabic lecture/session pages | 1440x900 | `rtl` | `dark` | Passed |
| Arabic lecture/session pages | 768x1024 | `rtl` | `light` | Passed |
| Arabic lecture/session pages | 768x1024 | `rtl` | `dark` | Passed |
| Arabic lecture/session pages | 390x844 | `rtl` | `dark` | Passed |
| Arabic lecture/session pages | 320x568 | `rtl` | `dark` | Passed |

The Playwright closure run covered 134 route/viewport/culture/theme checks with 0 page-level overflow failures. `document.documentElement.scrollWidth` matched `clientWidth` for all checked routes, including Arabic RTL light/dark tablet and Arabic RTL light/dark mobile at 390px and 320px. Theme persistence was verified through `attendai-theme` in localStorage. Culture persistence was verified through the ASP.NET Core culture cookie. The layout continues to set `lang` and `dir` from `CultureInfo.CurrentUICulture`.

The Arabic RTL dark mobile root cause was the closed mobile sidebar's off-viewport `translateX(...)` state. The sidebar now remains fixed at logical inline-start and is hidden with `opacity`, `visibility`, and `pointer-events`, which avoids clipping without hiding meaningful content.
