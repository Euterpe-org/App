# C# Rules

## Language & APIs

- Use the newest C# syntax and BCL APIs the repo's `LangVersion`/TFM allows (`field` keyword, `extension` members, collection expressions, span/`IFormatProvider` overloads) when they buy something (less code, fewer branches, better performance) — never an older equivalent out of habit. A newer spelling that compiles to the same code is optional; use whichever reads better.
- When several APIs solve the same problem, pick the best-performing one for the concrete situation (allocations, lookup cost, parse overloads) — deliberately, not first-that-works.

## Naming & comments

- Method and variable names must be self-explanatory; if a name needs a comment to be understood, rename it (e.g. `PlayingFolderName`, not `CurrentlyPlaying` + a comment).
- When a name is hard to settle, mirror an existing name in this project first; with no precedent, follow the general .NET/community convention for that concept (how the BCL or well-known libraries name it).
- No narration comments: never comment what the code does, where it came from, or why a change is correct.
- The only acceptable comment is a single short line stating a constraint the code cannot show (threading requirements, external library quirks). No multi-line essays.
- No XML doc comments on members whose name already says everything.

## Async

- Never wrap an async call chain in `Task.Run`. If the chain has real awaits (file I/O, network), it does not block the caller. `Task.Run` is only justified to push genuinely synchronous blocking work (e.g. synchronous audio decode) off the UI thread.
- `ConfigureAwait(false)` on every await outside UI-bound continuations; use `Dispatcher.UIThread` explicitly when touching UI-bound state afterwards.

## Code organization

- Where a type belongs is defined by the project map in `AGENTS.md`.
- Values derivable from a model are computed properties on the model itself (see `ChartDto.SizeDisplay`, `BpmDisplay`, `DifficultyBadges`), not static helper classes or converter wrappers.
- Large types split into `Name.Topic.cs` partial files.
- Service layout: the suffix-less `<Service>.cs` holds only the public interface implementation. Everything else lives in partials — `<Service>.Private.cs` when the helpers are few, `<Service>.<Topic>.cs` (`.Core`, `.Import`, ...) when there is enough to split by topic.
- Private helper types are nested inside the owning partial class file, not separate top-level types.

## Services & notifications

- Toast notifications live in core services, not viewmodels. Worker methods return results; the public method notifies per granularity: single operation = per-item toast, bulk operation = one summary toast. Never add a notification-suppress flag.
- Bulk methods return a count so an explicit UI action can toast "nothing to do" (silent no-op stays correct for automatic startup paths).
- No redundant wrapper services or pass-through methods: put the method on the existing domain service and make the real method public.
- File/IO primitives go through `IFileSystemService` (`Try*` pattern); never raw `Directory`/`File` calls with ad-hoc try/catch in services.

## Localization

- During development add the neutral `.resx` plus `zh-Hans`/`zh-Hant` only — those are the locales the user can read and test with, and keys may still churn while the feature settles. The remaining locales get batch-translated once the feature is final.
- Pick the table by where the text is shown, not by who assigns it: `XAML` = presentation-surface text (labels, titles, descriptions, button captions, and inline status/progress shown inside a page/panel — even when set from a viewmodel or core service, e.g. `MelonLoader_State_Downloading`, `Setup_Step_Failed`, `Setup_Progress_*`); `Interaction` = transient interaction surfaces (message boxes, toast notifications, OS file/folder pickers). So "code-side messages in `Interaction`" below means dialog/notification text, not every code-set string; a core service reads an `XAML` value via `XAML.Key` (Core imports only `static Interaction`, so add `global using Euterpe.Localization;` there).
- Keys are `Name_Action_State` segments, scoped by where the text belongs:
  - Page/panel-scoped view text: `<PanelName>_<Element>` (`ChartManage_SortBy`, `Setting_Title_Language`).
  - Self-identifying concepts, especially enum members: `<TypeName>_<Member>` (`ChartDifficulty_Easy`, `ChartSortField_MapCount`, `ModFilterType_All`) — never borrow another feature's key because the value happens to match today.
  - Code-side messages in `Interaction`: `<Kind>_Content_<Domain>_<Action>_<State>` (`Notification_Content_Chart_UpdateAll_Success`, `MessageBox_Content_SetPathEnvironment_Windows`).
- Keep `.resx` data entries alphabetically ordered — merge new keys into place, never append at the end.

## Misc

- Deduplicate shared one-liners into a single home (e.g. `ChartFiles.MapName`), even tiny ones.
- Before writing anything by hand, check the packages already referenced (`Directory.Packages.props`) for an existing facility — especially the easily-forgotten ones: CommunityToolkit.Mvvm's `[NotifyPropertyChangedFor]` for dependent properties instead of manually raising `OnPropertyChanged`, R3 `Observable`s for event wiring/composition instead of manual event handlers.
- Prefer an existing library over hand-rolling, but verify its actual behavior (decompile if needed) before adopting or rejecting it.
- App logging uses Microsoft.Extensions.Logging message templates backed by NLog (`Logger.LogInformation("Imported chart {ChartName} from {Path}", name, path)`); `Euterpe.Releaser` uses Cake logging (`context.Information("Packing {0}", channel)`). Log event messages are fragments and do not end with sentence punctuation or ellipses.
- DI: `public required T Name { get; init; }` properties inside `#region Injections`.
- `.cs` files end with exactly one trailing newline (`insert_final_newline` applies to `[*.cs]` only).
