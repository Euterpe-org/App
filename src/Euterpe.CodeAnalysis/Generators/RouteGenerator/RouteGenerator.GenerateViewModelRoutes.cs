namespace Euterpe.CodeAnalysis;

public sealed partial class RouteGenerator
{
    private static string GenerateViewModelRoutes(string viewModelNamespace, string viewModelName, ImmutableArray<RouteData> children, bool isRoot) =>
        isRoot
            ? GenerateRootRoutes(viewModelNamespace, viewModelName, children)
            : GenerateSwitchingRoutes(viewModelNamespace, viewModelName, children);

    private static string GenerateRootRoutes(string viewModelNamespace, string viewModelName, ImmutableArray<RouteData> children)
    {
        var attribute = GetGeneratedCodeAttribute(nameof(RouteGenerator));
        var cb = BeginFile(viewModelNamespace);

        using (cb.Block($"partial class {viewModelName}"))
        {
            foreach (var child in children)
            {
                var viewModel = $"global::{child.Namespace}.{child.ClassName}";
                var host = child.IsAppSingleton
                    ? $"global::Euterpe.Mvvm.PageHost.App(global::Euterpe.IocContainer.Resolve<{viewModel}>())"
                    : $"global::Euterpe.Mvvm.PageHost.PerGame<{viewModel}>()";
                cb.AppendLine($"public global::Euterpe.Mvvm.PageHost {HostName(child)} {{ get; }} = {host};");
            }

            cb.AppendLine();
            cb.AppendLine(attribute);
            cb.AppendLine("public override global::System.Collections.Generic.IReadOnlyList<global::Euterpe.Controls.Models.NavItem> NavItems { get; } =");
            cb.AppendLine("[");
            using (cb.Indent())
            {
                foreach (var child in children)
                {
                    cb.AppendLine(BuildNavItem(child));
                }
            }

            cb.AppendLine("];");
            cb.AppendLine();

            cb.AppendLine(attribute);
            cb.AppendLine(
                $"public override global::System.Collections.Generic.IReadOnlyList<global::Euterpe.Mvvm.PageHost> Pages => field ??= [{string.Join(", ", children.Select(HostName))}];");
            cb.AppendLine();

            cb.AppendLine(attribute);
            cb.AppendLine("private static readonly FrozenDictionary<string, int> RouteIndex = new global::System.Collections.Generic.Dictionary<string, int>");
            cb.AppendLine("{");
            using (cb.Indent())
            {
                for (var i = 0; i < children.Length; i++)
                {
                    cb.AppendLine($"""["{children[i].Path}"] = {i},""");
                }
            }

            cb.AppendLine("}.ToFrozenDictionary();");
            cb.AppendLine();

            cb.AppendLine(attribute);
            cb.AppendLine("protected override global::Euterpe.Mvvm.PageHost ResolveRoute(string route) => Pages[RouteIndex[route]];");
        }

        return cb.ToString();
    }

    private static string GenerateSwitchingRoutes(string viewModelNamespace, string viewModelName, ImmutableArray<RouteData> children)
    {
        var attribute = GetGeneratedCodeAttribute(nameof(RouteGenerator));
        var cb = BeginFile(viewModelNamespace);

        using (cb.Block($"partial class {viewModelName}"))
        {
            cb.AppendLine(attribute);
            cb.AppendLine("public override global::System.Collections.Generic.IReadOnlyList<global::Euterpe.Controls.Models.NavItem> NavItems { get; } =");
            cb.AppendLine("[");
            using (cb.Indent())
            {
                foreach (var child in children)
                {
                    cb.AppendLine(BuildNavItem(child));
                }
            }

            cb.AppendLine("];");
            cb.AppendLine();

            const string lookupType = "global::System.Func<global::Autofac.IComponentContext, global::Euterpe.Mvvm.ViewModelBase>";
            cb.AppendLine(attribute);
            cb.AppendLine($"private static readonly FrozenDictionary<string, {lookupType}> RouteLookup = new global::System.Collections.Generic.Dictionary<string, {lookupType}>");
            cb.AppendLine("{");
            using (cb.Indent())
            {
                foreach (var child in children)
                {
                    // Game-scoped children (the default) live only in the active game scope, never in the parent's
                    // captured Container (the root scope when the parent page is an app singleton), so resolve them
                    // through IocContainer.Resolve to hit the active game scope instead.
                    var resolver = child.IsAppSingleton
                        ? $"static ctx => ctx.Resolve<global::{child.Namespace}.{child.ClassName}>()"
                        : $"static _ => global::Euterpe.IocContainer.Resolve<global::{child.Namespace}.{child.ClassName}>()";
                    cb.AppendLine($"""["{child.Path}"] = {resolver},""");
                }
            }

            cb.AppendLine("}.ToFrozenDictionary();");
            cb.AppendLine();

            cb.AppendLine(attribute);
            cb.AppendLine("protected override global::Euterpe.Mvvm.ViewModelBase ResolveRoute(string route) => RouteLookup[route](Container);");
        }

        return cb.ToString();
    }

    private static CodeBuilder BeginFile(string viewModelNamespace)
    {
        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine("using System.Collections.Frozen;");
        cb.AppendLine();
        cb.AppendLine($"namespace {viewModelNamespace};");
        cb.AppendLine();
        return cb;
    }

    private static string HostName(RouteData route)
    {
        const string suffix = "ViewModel";
        var name = route.ClassName.EndsWith(suffix, StringComparison.Ordinal) ? route.ClassName[..^suffix.Length] : route.ClassName;
        return name + "Host";
    }

    private static string BuildNavItem(RouteData route)
    {
        var icon = string.IsNullOrEmpty(route.Icon) ? string.Empty : $$""" { IconResourceKey = "{{route.Icon}}" }""";
        return $"""new(global::Euterpe.Localization.XAML.{route.DisplayName}, "{route.Path}"){icon},""";
    }
}
