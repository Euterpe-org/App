using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Euterpe.Windows;
using TUnit.Core.Enums;

namespace Euterpe.Tests.Platforms.Windows;

[Category("WindowsPlatformInfoTests")]
[TestSubject(typeof(WindowsPlatformInfo))]
[RunOn(OS.Windows)]
[SupportedOSPlatform(nameof(OSPlatform.Windows))]
public sealed class WindowsPlatformInfoTest
{
    [Test]
    public async Task OsString_WindowsPlatformInfo_ReturnsWin()
    {
        var info = new WindowsPlatformInfo();
        await Assert.That(info.OsString).IsEqualTo("win");
    }
}
