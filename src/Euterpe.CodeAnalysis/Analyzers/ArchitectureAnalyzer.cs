namespace Euterpe.CodeAnalysis.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ArchitectureAnalyzer : DiagnosticAnalyzer
{
    public const string AbstractionsTypeRuleId = "EUT0001";
    public const string CoreTypeRuleId = "EUT0002";
    public const string SharedTypeRuleId = "EUT0003";
    public const string ModelsTypeRuleId = "EUT0004";
    public const string PlatformTypeRuleId = "EUT0005";

    private const string Category = "Architecture";

    private static readonly DiagnosticDescriptor AbstractionsTypeRule = new(
        AbstractionsTypeRuleId,
        "Abstractions types must be public interfaces",
        $"Type '{{0}}' in {AbstractionsAssemblyName} must be a public interface",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor CoreTypeRule = new(
        CoreTypeRuleId,
        "Core services must be internal and sealed",
        $"Type '{{0}}' in {CoreAssemblyName} must be internal and sealed",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor SharedTypeRule = new(
        SharedTypeRuleId,
        "Shared classes must be public",
        $"Type '{{0}}' in {SharedAssemblyName} must be public",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor ModelsTypeRule = new(
        ModelsTypeRuleId,
        "Model classes must be public",
        $"Type '{{0}}' in {ModelsAssemblyName} must be public",
        Category,
        DiagnosticSeverity.Error,
        true);

    private static readonly DiagnosticDescriptor PlatformTypeRule = new(
        PlatformTypeRuleId,
        "Platform services must be internal and sealed",
        "Platform service '{0}' must be internal and sealed",
        Category,
        DiagnosticSeverity.Error,
        true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [AbstractionsTypeRule, CoreTypeRule, SharedTypeRule, ModelsTypeRule, PlatformTypeRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is INamedTypeSymbol { IsImplicitlyDeclared: false, ContainingType: null } type
            && GetViolatedRule(type) is { } rule)
        {
            context.ReportDiagnostic(Diagnostic.Create(rule, type.Locations[0], type.Name));
        }
    }

    private static DiagnosticDescriptor? GetViolatedRule(INamedTypeSymbol type) =>
        type.ContainingAssembly.Name switch
        {
            AbstractionsAssemblyName when type is not
                { TypeKind: TypeKind.Interface, DeclaredAccessibility: Accessibility.Public } => AbstractionsTypeRule,
            CoreAssemblyName when IsCoreNamespace(type.ContainingNamespace)
                                  && type is { TypeKind: TypeKind.Class, IsStatic: false }
                                      and not { DeclaredAccessibility: Accessibility.Internal, IsSealed: true } => CoreTypeRule,
            SharedAssemblyName when type is
                { TypeKind: TypeKind.Class, DeclaredAccessibility: not Accessibility.Public } => SharedTypeRule,
            ModelsAssemblyName when type is
                { TypeKind: TypeKind.Class, IsStatic: false, DeclaredAccessibility: not Accessibility.Public } => ModelsTypeRule,
            "Euterpe" when type.ContainingNamespace.ToDisplayString() is "Euterpe.Windows" or "Euterpe.Linux"
                           && type is { TypeKind: TypeKind.Class, IsStatic: false }
                               and not { DeclaredAccessibility: Accessibility.Internal, IsSealed: true } => PlatformTypeRule,
            _ => null
        };

    private static bool IsCoreNamespace(INamespaceSymbol symbol) => symbol is { Name: "Core", ContainingNamespace.Name: "Euterpe" };
}
