using Euterpe.CodeAnalysis.Analyzers;

namespace Euterpe.CodeAnalysis.Tests.Analyzers;

[TestSubject(typeof(ArchitectureAnalyzer))]
[Category("ArchitectureAnalyzerTests")]
public sealed class ArchitectureAnalyzerTest
{
    [Test]
    [Arguments("Euterpe.Windows")]
    [Arguments("Euterpe.Linux")]
    public async Task AnalyzeNamedType_PlatformPublicService_ReportsDiagnostic(string namespaceName)
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe",
            $"namespace {namespaceName}; public class Service;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.PlatformTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_AbstractionsNonPublicClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Abstractions",
            "namespace Euterpe.Abstractions; internal sealed class Service;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.AbstractionsTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_AbstractionsSubnamespaceNonPublicClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Abstractions",
            "namespace Euterpe.Abstractions.Specialized; internal sealed class Service;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.AbstractionsTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_CorePublicUnsealedClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Core",
            "namespace Euterpe.Core; public class Service;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.CoreTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_SharedInternalClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Shared",
            "namespace Euterpe.Shared; internal static class Helper;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.SharedTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_ModelsInternalClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Models",
            "namespace Euterpe.Models; internal sealed class Model;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.ModelsTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_SharedSubnamespaceInternalClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Shared",
            "namespace Euterpe.Shared.Collections; internal sealed class Collection;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.SharedTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_ModelsSubnamespaceInternalClass_ReportsDiagnostic()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Models",
            "namespace Euterpe.Models.Charts; internal sealed class Chart;").ConfigureAwait(false);

        await Assert.That(diagnostics.Select(static diagnostic => diagnostic.Id))
            .IsEquivalentTo([ArchitectureAnalyzer.ModelsTypeRuleId]);
    }

    [Test]
    public async Task AnalyzeNamedType_CoreSubnamespaceClass_NoDiagnostics()
    {
        var diagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Core",
            "namespace Euterpe.Core.Http; public class Handler;").ConfigureAwait(false);

        await Assert.That(diagnostics).IsEmpty();
    }

    [Test]
    public async Task AnalyzeNamedType_ValidTypes_NoDiagnostics()
    {
        var abstractionsDiagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Abstractions",
            "namespace Euterpe.Abstractions; public interface IService;").ConfigureAwait(false);
        var coreDiagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Core",
            "namespace Euterpe.Core; internal sealed class Service;").ConfigureAwait(false);
        var sharedDiagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Shared",
            "namespace Euterpe.Shared; public static class Helper;").ConfigureAwait(false);
        var modelsDiagnostics = await AnalyzerTestHelper.RunAsync<ArchitectureAnalyzer>(
            "Euterpe.Models",
            "namespace Euterpe.Models; public sealed class Model;").ConfigureAwait(false);

        using var _ = Assert.Multiple();
        await Assert.That(abstractionsDiagnostics).IsEmpty();
        await Assert.That(coreDiagnostics).IsEmpty();
        await Assert.That(sharedDiagnostics).IsEmpty();
        await Assert.That(modelsDiagnostics).IsEmpty();
    }
}
