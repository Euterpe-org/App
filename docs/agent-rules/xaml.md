# XAML Rules

## Formatting

- XamlStyler formatting: one attribute per line, attributes alphabetically ordered, two-space comment padding (`<!--  text  -->`).
- Files are UTF-8 without BOM, LF line endings, no trailing newline (`insert_final_newline = false` for non-`.cs` files).

## Bindings

- Always use compiled bindings: `x:DataType` on the root element and on every `DataTemplate`. The app is `PublishAot`, so reflection bindings break AOT/trimming — never reach for `{ReflectionBinding}` or `x:CompileBindings="False"` to escape a binding that won't compile.
- When a reusable control's theme `ItemTemplate` is fed item types it can't reference (the control sits below the app), it still needs an `x:DataType` to compile: extract the minimal display contract as an interface in `Euterpe.Controls.Models` (e.g. `IEnumOption { LocalizedString Display { get; } }`), have the app type implement it, and `x:DataType` the interface — do not move the whole app/VM type down into `Euterpe.Controls`, and do not fall back to reflection.
- Bind directly to model computed properties (`{Binding SizeDisplay}`); do not add a converter for anything derivable from the model — add a property to the model instead.
- Converters that encode genuine view logic (icons, brushes, multi-input visual state) live in `FuncValueConverters` as static `FuncValueConverter`/`FuncMultiValueConverter` properties.

## Localization

- All user-facing text via `{Localize Key}` — never hardcoded strings.

## Styling

- Prefer Semi theme tokens (`{DynamicResource SemiBlue6}`, `SemiColorText0`, `SecondaryCardColor`, ...) over hardcoded hex. Alpha overlay masks (`#40000000`-style) are acceptable when no token fits.
- A local property value outranks a style setter: when a pseudo-class (`:pointerover`) must change a property, define both the idle and the pseudo-class value as style setters.
- Styles reused across views live in `Euterpe.Controls/Themes/ControlStyles/` (one file per control type, e.g. `ButtonStyles.axaml`); any `.axaml` there is picked up by the theme generation automatically — no manual include.
- A page/panel's own styles stay inline only while small; once they dominate the view file and bury the actual content, extract them to `ControlStyles/Pages/` or `ControlStyles/Panels/` as `<Name>Styles.axaml`.
- Comments: short section labels or one-line constraint notes only.

## Controls

- Need a new control? Source it in this order: Ursa first, then a mature community package, and only write our own (in `Euterpe.Controls`) when neither fits.
- Before adopting a community package, vet it: maintenance activity, Avalonia version compatibility, AOT/trimming friendliness — and verify its actual behavior per the C# library rule.
