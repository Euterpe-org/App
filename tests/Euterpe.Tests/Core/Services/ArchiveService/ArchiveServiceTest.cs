using System.IO.Compression;
using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Core;

[Category("ArchiveServiceTests")]
[TestSubject(typeof(ArchiveService))]
public sealed class ArchiveServiceTest
{
    private static ArchiveService NewService() => new() { Logger = NullLogger<ArchiveService>.Instance };

    private static string NewTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "Euterpe.Tests.Archive_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Test]
    public async Task CreateZipFile_ExistingZip_OverwritesIt()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "new.txt"), "new");
            var zipPath = Path.Combine(work, "out.zip");
            await File.WriteAllTextAsync(zipPath, "stale-content");

            NewService().CreateZipFile(source, zipPath);

            await using var zip = ZipFile.OpenRead(zipPath);
            await Assert.That(zip.Entries.Single().Name).IsEqualTo("new.txt");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CreateZipFileAsync_ExistingZip_OverwritesIt()
    {
        var work = NewTempFolder();
        try
        {
            var source = Path.Combine(work, "src");
            Directory.CreateDirectory(source);
            await File.WriteAllTextAsync(Path.Combine(source, "fresh.txt"), "fresh");
            var zipPath = Path.Combine(work, "out.zip");
            await File.WriteAllTextAsync(zipPath, "stale");

            await NewService().CreateZipFileAsync(source, zipPath);

            await using var zip = ZipFile.OpenRead(zipPath);
            await Assert.That(zip.Entries.Single().Name).IsEqualTo("fresh.txt");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }
}
