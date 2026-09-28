namespace Euterpe.Shared;

public static class DotNetRuntimeDownload
{
    public static string GetUrl(string runtimeVersion) => $"https://aka.ms/dotnet/{runtimeVersion}/dotnet-runtime-win-x64.zip";
}
