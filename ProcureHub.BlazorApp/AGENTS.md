# ProcureHub Blazor App - LLM Guidelines

## UI Framework

This app uses **Radzen Blazor Components** for the UI. Documentation: https://blazor.radzen.com

### Theme Selection

- Theme is set via RadzenTheme component. Example: `<RadzenTheme Theme="software" />
- Available free themes: `material`, `material-dark`, `standard`, `standard-dark`, `default`, `dark`, `humanistic`, `humanistic-dark`, `software`, `software-dark`

### Customizing Theme Colors

**Always use `--rz-` prefixed CSS variables** to customize Radzen component colors. Never override component class styles directly.

#### Core Theme Variables

Override these in `:root` in `wwwroot/app.css`:

```css
:root {
    /* Primary color palette */
    --rz-primary: #0d9488;
    --rz-primary-light: #2dd4bf;
    --rz-primary-lighter: rgba(13, 148, 136, 0.16);
    --rz-primary-dark: #0f766e;
    --rz-primary-darker: #115e59;

    /* Secondary, info, success, warning, danger follow same pattern */
    --rz-secondary: #...;
    --rz-info: #...;
    --rz-success: #...;
    --rz-warning: #...;
    --rz-danger: #...;
}
```

#### All Available Color Variables

Each color has 5 variants:
- `--rz-{color}` - base color
- `--rz-{color}-light` - lighter variant
- `--rz-{color}-lighter` - lightest (often semi-transparent)
- `--rz-{color}-dark` - darker variant
- `--rz-{color}-darker` - darkest variant

Colors: `primary`, `secondary`, `info`, `success`, `warning`, `danger`

#### Base/Neutral Colors

```css
--rz-base-50 through --rz-base-900  /* Grayscale palette */
--rz-base-light, --rz-base-lighter, --rz-base-dark, --rz-base-darker
--rz-white, --rz-black
```

#### Text Colors

```css
--rz-text-title-color
--rz-text-color
--rz-text-secondary-color
--rz-text-tertiary-color
--rz-text-disabled-color
--rz-text-contrast-color
```

#### Using Colors in Inline Styles

Use `var(--rz-primary)` syntax in component styles:

```razor
<RadzenIcon Icon="inventory_2" Style="color: var(--rz-primary);" />
```

#### Using Color Utility Classes

Radzen provides utility classes for colors:
- Background: `.rz-background-color-primary`, `.rz-background-color-success`, etc.
- Text: `.rz-color-primary`, `.rz-color-danger`, etc.
- Border: `.rz-border-color-primary`, etc.

### Component Services

Register Radzen services in `Program.cs`:

```csharp
builder.Services.AddRadzenComponents();
```

This registers: `DialogService`, `NotificationService`, `TooltipService`, `ContextMenuService`, `ThemeService`

### Icons

Use Material Icons with `RadzenIcon`:

```razor
<RadzenIcon Icon="inventory_2" />
```

Icon reference: https://fonts.google.com/icons

## Project Structure

- `Components/` - Razor components
  - `Layout/` - Layout components (AuthenticatedLayout, UnauthenticatedLayout)
  - `Pages/` - Page components
  - `Account/` - Identity/auth related components
- `wwwroot/` - Static assets
  - `app.css` - Custom styles and theme overrides
- `Program.cs` - App configuration and DI setup

## General

- After any significant change, make sure there are no build errors
