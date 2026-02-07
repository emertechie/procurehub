# ProcureHub Blazor App - LLM Guidelines

This is a .Net Blazor client for the ProcureHub application.

It does not call the `ProcureHub.WebApi`. Instead, command / query handlers from the `ProcureHub` project are injected into Razor components and used directly. Example: `ProcureHub.BlazorApp/Components/Pages/Admin/Users/Index.razor`.

# Blazor Guidelines

- Favor small, single-responsibility components (one clear purpose); break down large UIs into multiple nested components instead of one monolithic file.
- When a component grows beyond a clear responsibility, extract subcomponents and compose them rather than extending the file.

## UI Framework

- This app uses **Radzen Blazor Components** for the UI
  - Documentation: https://blazor.radzen.com
  - Use the "radzen.mcp" MCP tool to answer questions about the library
- If using a `RadzenDataGrid` with a `LoadData` attribute, don't also load the data in the `OnInitializedAsync` method as it's not needed.   

### Customizing Theme Colors

**Always use `--rz-` prefixed CSS variables** to customize Radzen component colors. Never override component class styles directly.

#### Core Theme Variables

Override these in `:root` in `wwwroot/app.css`:

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

### Icons

Use Material Icons with `RadzenIcon`:

```razor
<RadzenIcon Icon="inventory_2" />
```

Icon reference: https://fonts.google.com/icons?icon.set=Material+Symbols

## Forms

- Never use floating labels for form fields. Always use a separate label above the form control like below:  
```html
    <RadzenStack Orientation="Orientation.Vertical" Gap="0.5rem">
        <RadzenLabel Text="First Name" Component="FirstNameInput" />
        <RadzenTextBox Name="FirstNameInput" @bind-Value="@_model.FirstName" Placeholder="Enter your first name" />
    </RadzenStack>
```

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
- Private fields in a Razor code block must use correct naming style (leading underscore)
  - Correct: `@code { int _totalCount; }`
