namespace Euterpe.CodeAnalysis.Tests;

internal static class AnalyzerTestHelper
{
    public static async Task<ImmutableArray<Diagnostic>> RunAsync<TAnalyzer>(string assemblyName, params string[] sources)
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            sources.Select(static source => CSharpSyntaxTree.ParseText(source)),
            Net110.References.All,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
        var compilationErrors = compilation.GetDiagnostics()
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();
        if (compilationErrors.Length > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, compilationErrors.AsEnumerable()));
        }

        return await compilation
            .WithAnalyzers([new TAnalyzer()])
            .GetAnalyzerDiagnosticsAsync()
            .ConfigureAwait(false);
    }
}
