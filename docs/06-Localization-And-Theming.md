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

## Sprint 4 Localization Additions

Sprint 4 added English and Arabic resources for:

- Biometric navigation.
- Privacy notice and consent text.
- Enrollment status and capture wizard.
- Capture validation and quality errors.
- Face-engine readiness messages.
- Admin biometric overview/details/actions.
- Enrollment events and safe localized event descriptions.

The layout continues to use `lang` and `dir` from the active culture. Biometric pages reuse Bootstrap RTL and logical custom CSS, so camera panels, tables, forms, badges, and action rows align correctly in Arabic RTL and English LTR.

## Sprint 4 Theme Notes

The camera and biometric Admin UI use existing CSS variables such as `--surface`, `--surface-secondary`, `--text-primary`, `--text-secondary`, `--border-color`, `--primary`, `--danger`, and `--success`. No important biometric UI text is intentionally hardcoded to dark colors on dark backgrounds. Theme preference remains persisted as `attendai-theme`; culture preference remains persisted through the ASP.NET Core culture cookie.

## Sprint 5 Localization Additions

Sprint 5 added English and Arabic resources for:

- Student face-verification navigation, status, capture, result, and history pages.
- Admin face-verification overview, filters, details, diagnostics, and dashboard metrics.
- Verification outcome, decision, purpose, score metric, and safe attempt description labels.
- Eligibility, request validation, rate limit, incompatible template, no-face, multiple-face, low-quality, engine-unavailable, match, and no-match messages.
- Fake-development engine warnings and real-engine readiness labels.

Visible Razor text for the Sprint 5 pages is resource-backed through `IStringLocalizer<SharedResource>`. Enum values are rendered through localized keys instead of raw enum names.

## Sprint 5 Theme And Direction Notes

The verification UI uses the existing design tokens and logical CSS properties for English LTR and Arabic RTL. The Student capture page, Student result/history, Admin table/filter/details, diagnostics panels, badges, alerts, and dashboard cards were checked for dark-theme readability and RTL alignment.

Theme preference continues to persist in `localStorage` as `attendai-theme`, and the inline layout script applies the resolved theme before stylesheet loading to reduce incorrect-theme flash. Culture preference continues to persist through the ASP.NET Core culture cookie.

Manual/runtime verification target combinations are recorded in `docs/30-Sprint-5-Test-Plan.md` and `docs/31-Sprint-5-Review.md`. Any item without runtime browser evidence is marked Not Verified rather than Passed.

## Sprint 7 Localization Additions

Sprint 7 added English and Arabic resources for:

- Student, Instructor, and Admin report navigation.
- Report page titles and summaries.
- Date, course, instructor, Student search, status, sort, page, and page-size filters.
- Present, Late, Missed, and percentage labels.
- CSV export and print actions.
- Report empty states.
- Admin audit search, categories, outcomes, and safe descriptions.
- Export validation and safe error labels.

Visible Razor text for Sprint 7 report and audit pages is resource-backed through `IStringLocalizer<SharedResource>`. CSV column headings follow the safe export contract and do not include biometric, exact-location, challenge, idempotency, or secret fields.

## Sprint 7 Theme And Direction Notes

Report and audit pages reuse existing design tokens such as `--surface`, `--surface-secondary`, `--text-primary`, `--text-secondary`, `--border-color`, `--primary`, `--success`, `--warning`, and `--danger`. Sprint 7 CSS uses logical properties and print media rules for responsive report layouts.

Runtime SQL smoke observed:

| Scenario | Evidence | Result |
| --- | --- | --- |
| English report shell | `<html lang="en" dir="ltr">` on Admin report route | Passed |
| Arabic report shell | `<html lang="ar" dir="rtl">` on Admin report route | Passed |

Full manual visual verification for English LTR Light, English LTR Dark, Arabic RTL Light, Arabic RTL Dark, desktop, tablet, 390px, 320px, keyboard navigation, and print layout remains pending for Sprint 7 and must not be marked Passed until browser evidence is recorded.
