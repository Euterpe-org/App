using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Euterpe.Contracts.Distribution;
using Euterpe.Shared;
using Semver;
using DirectoryPath = Cake.Core.IO.DirectoryPath;
using static Euterpe.Releaser.ReleaseContext;
using Path = System.IO.Path;

namespace Euterpe.Releaser.Tests;

[Category("ReleaserTests")]
[TestSubject(typeof(ReleaseContext))]
public sealed class ReleaseContextTest
{
    [Test]
    [Arguments("win-x64", "-Setup.exe", true)]
    [Arguments("win-arm64", "-Setup.exe", true)]
    [Arguments("linux-x64", ".AppImage", false)]
    [Arguments("linux-arm64", ".AppImage", false)]
    public async Task Constructor_SupportedRid_SetsPlatformOptions(
        string rid,
        string expectedInstallerFileSuffix,
        bool expectedDisablePortable)
    {
        var context = CreateContext(rid: rid);

        await Assert.That(context.ApplicationProject).IsEqualTo(expectedDisablePortable
            ? "src/Platforms/Euterpe.Windows/Euterpe.Windows.csproj"
            : "src/Platforms/Euterpe.Linux/Euterpe.Linux.csproj");
        await Assert.That(context.InstallerFileSuffix).IsEqualTo(expectedInstallerFileSuffix);
        await Assert.That(context.PlatformVpkArguments.SequenceEqual(
                expectedDisablePortable
                    ? ["--noPortable", "--icon", PackageIconPath]
                    : []))
            .IsTrue();
        await Assert.That(context.StableChannel).IsEqualTo($"{rid}-stable");
        await Assert.That(context.BetaChannel).IsEqualTo($"{rid}-beta");
    }

    [Test]
    public async Task Constructor_UnsupportedPlatform_Throws()
    {
        await Assert.That(() => CreateContext(rid: "osx-arm64")).Throws<InvalidOperationException>();
    }

    [Test]
    [Arguments(null)]
    [Arguments("false")]
    public async Task EnsureGitHubActions_LocalEnvironment_Throws(string? githubActions)
    {
        var context = CreateContext(githubActions);

        await Assert.That(context.EnsureGitHubActions).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task EnsureGitHubActions_GitHubActions_Succeeds()
    {
        var context = CreateContext("true");

        await Assert.That(context.EnsureGitHubActions).ThrowsNothing();
    }

    [Test]
    public async Task IsVersionPublished_NoReleaseBases_DoesNotSkipLocalBuild()
    {
        var context = CreateContext();

        await Assert.That(context.IsVersionPublished).IsFalse();
    }

    [Test]
    public async Task IsVersionPublished_AllChannelsPublished_SkipsStaging()
    {
        var context = CreateContext();
        foreach (var channel in context.BaseChannels)
        {
            context.ReleaseBases.Add(channel, new VelopackReleaseBase(context.Version.ToString(), "unused"));
        }

        await Assert.That(context.IsVersionPublished).IsTrue();
    }

    [Test]
    public async Task GetPackageFileNames_ReturnsVelopackNames()
    {
        var context = CreateContext();

        await Assert.That(context.GetFullPackageFileName("win-x64-stable"))
            .IsEqualTo($"Euterpe-{context.Version}-win-x64-stable-full.nupkg");
        await Assert.That(context.GetFullPackageFileName("win-x64-stable", "2.2.0"))
            .IsEqualTo("Euterpe-2.2.0-win-x64-stable-full.nupkg");
        await Assert.That(context.GetDeltaPackageFileName("win-x64-stable"))
            .IsEqualTo($"Euterpe-{context.Version}-win-x64-stable-delta.nupkg");
    }

    [Test]
    public async Task GetInstallerFileName_SupportedRuntimes_ReturnsVelopackNames()
    {
        await Assert.That(CreateContext(rid: "win-x64").GetInstallerFileName("win-x64-stable"))
            .IsEqualTo("Euterpe-win-x64-stable-Setup.exe");
        await Assert.That(CreateContext(rid: "linux-x64").GetInstallerFileName("linux-x64-beta"))
            .IsEqualTo("Euterpe-linux-x64-beta.AppImage");
    }

    [Test]
    public async Task GitHubInstallerFileName_SupportedRuntimes_ReturnsPublicNames()
    {
        await Assert.That(CreateContext(rid: "win-x64").GitHubInstallerFileName)
            .IsEqualTo("Euterpe-win-x64-Setup.exe");
        await Assert.That(CreateContext(rid: "linux-arm64").GitHubInstallerFileName)
            .IsEqualTo("Euterpe-linux-arm64.AppImage");
    }

    [Test]
    public async Task PackageChannels_Prerelease_ReturnsBetaChannel()
    {
        var context = CreateContext(version: "2.2.0-beta.1");
        var channels = context.PackageChannels;

        await Assert.That(channels.Count).IsEqualTo(1);
        await Assert.That(channels[0]).IsEqualTo("win-x64-beta");
    }

    [Test]
    public async Task PackageChannels_StableWithoutBetaBase_ReturnsStableChannel()
    {
        var context = CreateContext(version: "2.2.0");
        var channels = context.PackageChannels;

        await Assert.That(channels.Count).IsEqualTo(1);
        await Assert.That(channels[0]).IsEqualTo("win-x64-stable");
    }

    [Test]
    public async Task PackageChannels_StableWithBetaBase_ReturnsStableAndBetaChannels()
    {
        var context = CreateContext(version: "2.2.0");
        context.ReleaseBases.Add(context.BetaChannel, new VelopackReleaseBase("2.2.0-beta.1", "unused"));
        var channels = context.PackageChannels;

        await Assert.That(channels.Count).IsEqualTo(2);
        await Assert.That(channels[0]).IsEqualTo("win-x64-stable");
        await Assert.That(channels[1]).IsEqualTo("win-x64-beta");
    }

    private static ReleaseContext CreateContext(string? githubActions = null, string rid = "win-x64", string? version = null)
    {
        var arguments = ICakeArguments.Mock();
        arguments.HasArgument("rid").Returns(true);
        arguments.GetArguments("rid").Returns([rid]);
        var environment = ICakeEnvironment.Mock();
        environment.WorkingDirectory.Returns(new DirectoryPath(Path.GetTempPath()));
        environment.GetEnvironmentVariable("GITHUB_ACTIONS").Returns(githubActions!);
        var cakeContext = ICakeContext.Mock();
        cakeContext.Arguments.Returns(arguments.Object);
        cakeContext.Environment.Returns(environment.Object);
        cakeContext.FileSystem.Returns(IFileSystem.Mock().Object);
        cakeContext.ProcessRunner.Returns(IProcessRunner.Mock().Object);
        cakeContext.Log.Returns(ICakeLog.Mock().Object);
        return new ReleaseContext(cakeContext.Object)
        {
            Version = SemVersion.Parse(version ?? BuildInfo.AppVersion, SemVersionStyles.Strict)
        };
    }
}
