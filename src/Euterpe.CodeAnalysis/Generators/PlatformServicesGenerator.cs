namespace Euterpe.CodeAnalysis;

[Generator(LanguageNames.CSharp)]
public sealed class PlatformServicesGenerator : IIncrementalGenerator
{
    private const string PlatformServiceAttributeName = "Euterpe.Shared.Attributes.PlatformServiceAttribute";
    private const string SupportedOSPlatformAttributeName = "System.Runtime.Versioning.SupportedOSPlatformAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var registrations = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                SupportedOSPlatformAttributeName,
                static (node, _) => node is ClassDeclarationSyntax,
                ExtractRegistration)
            .Collect();

        context.RegisterSourceOutput(registrations, Generate);
    }

    private static RegistrationData? ExtractRegistration(GeneratorAttributeSyntaxContext context, CancellationToken _)
    {
        var implementation = (INamedTypeSymbol)context.TargetSymbol;
        foreach (var contract in implementation.Interfaces)
        {
            var attribute = contract.GetAttributes().FirstOrDefault(static attribute =>
                attribute.AttributeClass?.ToDisplayString() is PlatformServiceAttributeName);

            if (attribute is null)
            {
                continue;
            }

            return new RegistrationData(
                implementation.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                contract.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                attribute.ConstructorArguments is [{ Value: 0 }]);
        }

        return null;
    }

    private static void Generate(SourceProductionContext spc, ImmutableArray<RegistrationData?> candidates)
    {
        var registrations = candidates
            .OfType<RegistrationData>()
            .OrderBy(static registration => registration.ContractFullName, StringComparer.Ordinal)
            .ToArray();

        if (registrations.Length is 0)
        {
            return;
        }

        var cb = new CodeBuilder();
        cb.Append(Header).AppendLine();
        cb.AppendLine("namespace Euterpe.Platform;");
        cb.AppendLine();

        using (cb.Block("internal sealed class PlatformServices : global::Euterpe.IPlatformServices"))
        {
            cb.AppendLine(GetGeneratedCodeAttribute(nameof(PlatformServicesGenerator)));
            using (cb.Block("public static void RegisterServices(global::Autofac.ContainerBuilder builder)"))
            {
                cb.AppendLine("#pragma warning disable CA1416");
                foreach (var registration in registrations)
                {
                    var lifetime = registration.IsAppSingleton
                        ? "SingleInstance()"
                        : "InstancePerMatchingLifetimeScope(global::Euterpe.IocContainer.GameScopeTag)";
                    cb.AppendLine($"builder.RegisterType<{registration.ImplementationFullName}>().As<{registration.ContractFullName}>().PropertiesAutowired().{lifetime};");
                }

                cb.AppendLine("#pragma warning restore CA1416");
            }
        }

        spc.AddSource("PlatformServices.g.cs", cb.ToString());
    }

    private sealed record RegistrationData(
        string ImplementationFullName,
        string ContractFullName,
        bool IsAppSingleton);
}
