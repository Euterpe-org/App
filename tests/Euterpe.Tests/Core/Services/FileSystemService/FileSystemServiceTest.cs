using Microsoft.Extensions.Logging.Abstractions;

namespace Euterpe.Tests.Core;

[Category("FileSystemServiceTests")]
[TestSubject(typeof(FileSystemService))]
public sealed partial class FileSystemServiceTest
{
    private static FileSystemService NewService() => new() { Logger = NullLogger<FileSystemService>.Instance };

    private static string NewTempFolder()
    {
        var path = Path.Combine(Path.GetTempPath(), "Euterpe.Tests.Fs_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    [Test]
    public async Task TryMoveFile_SourceMissing_ReturnsFalse()
    {
        var work = NewTempFolder();
        try
        {
            var ok = NewService().TryMoveFile(Path.Combine(work, "missing.txt"), Path.Combine(work, "dst.txt"));

            await Assert.That(ok).IsFalse();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task GetFileLastWriteTimeUtc_Missing_ReturnsNull()
    {
        var work = NewTempFolder();
        try
        {
            var lastWrite = NewService().GetFileLastWriteTimeUtc(Path.Combine(work, "missing.txt"));

            await Assert.That(lastWrite).IsNull();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryOpenReadFile_Missing_ReturnsNull()
    {
        var work = NewTempFolder();
        try
        {
            await using var stream = NewService().TryOpenReadFile(Path.Combine(work, "missing.txt"));

            await Assert.That(stream).IsNull();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task DeleteDirectory_MissingWithIgnoreIfNotFound_DoesNotThrow()
    {
        var work = NewTempFolder();
        try
        {
            var act = () => NewService().DeleteDirectory(Path.Combine(work, "missing_dir"), DeleteOption.IgnoreIfNotFound);

            await Assert.That(act).ThrowsNothing();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryDeleteDirectory_MissingWithFailIfNotFound_ReturnsFalse()
    {
        var work = NewTempFolder();
        try
        {
            // Directory.Delete throws DirectoryNotFoundException on missing paths (unlike File.Delete which is silent).
            var ok = NewService().TryDeleteDirectory(Path.Combine(work, "missing_dir"));

            await Assert.That(ok).IsFalse();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryDeleteDirectory_MissingWithIgnoreIfNotFound_ReturnsTrue()
    {
        var work = NewTempFolder();
        try
        {
            var ok = NewService().TryDeleteDirectory(Path.Combine(work, "missing_dir"), DeleteOption.IgnoreIfNotFound);

            await Assert.That(ok).IsTrue();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task CopyDirectory_SourceContainsNestedFolders_CopiesContentsAndPreservesSource()
    {
        var work = NewTempFolder();
        try
        {
            var src = Path.Combine(work, "src");
            var nested = Path.Combine(src, "nested");
            Directory.CreateDirectory(nested);
            await File.WriteAllTextAsync(Path.Combine(src, "top.txt"), "top");
            await File.WriteAllTextAsync(Path.Combine(nested, "deep.txt"), "deep");

            var dst = Path.Combine(work, "dst");
            NewService().CopyDirectory(src, dst);

            using var _ = Assert.Multiple();
            await Assert.That(Directory.Exists(src)).IsTrue();
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dst, "top.txt"))).IsEqualTo("top");
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(dst, "nested", "deep.txt"))).IsEqualTo("deep");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryMoveDirectoryToAvailablePath_DesiredFree_MovesToDesired()
    {
        var work = NewTempFolder();
        try
        {
            var src = Path.Combine(work, "src");
            Directory.CreateDirectory(src);
            await File.WriteAllTextAsync(Path.Combine(src, "f.txt"), "payload");

            var desired = Path.Combine(work, "chart");
            var ok = NewService().TryMoveDirectoryToAvailablePath(src, desired, out var finalPath);

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsTrue();
            await Assert.That(finalPath).IsEqualTo(desired);
            await Assert.That(Directory.Exists(src)).IsFalse();
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(desired, "f.txt"))).IsEqualTo("payload");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryMoveDirectoryToAvailablePath_DesiredTaken_MovesToSuffixedSibling()
    {
        var work = NewTempFolder();
        try
        {
            var desired = Path.Combine(work, "chart");
            Directory.CreateDirectory(desired);
            await File.WriteAllTextAsync(Path.Combine(desired, "existing.txt"), "old");

            var src = Path.Combine(work, "src");
            Directory.CreateDirectory(src);
            await File.WriteAllTextAsync(Path.Combine(src, "f.txt"), "payload");

            var ok = NewService().TryMoveDirectoryToAvailablePath(src, desired, out var finalPath);
            var suffix = Path.GetFileName(finalPath)["chart-".Length..];

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsTrue();
            await Assert.That(Path.GetFileName(finalPath)).StartsWith("chart-");
            await Assert.That(suffix.Length).IsEqualTo(8);
            await Assert.That(suffix.All(Uri.IsHexDigit)).IsTrue();
            await Assert.That(Directory.Exists(src)).IsFalse();
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(desired, "existing.txt"))).IsEqualTo("old");
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(finalPath, "f.txt"))).IsEqualTo("payload");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryMoveDirectoryToAvailablePath_ParentMissing_CreatesParentAndMoves()
    {
        var work = NewTempFolder();
        try
        {
            var src = Path.Combine(work, "src");
            Directory.CreateDirectory(src);
            await File.WriteAllTextAsync(Path.Combine(src, "f.txt"), "payload");

            var desired = Path.Combine(work, "offline", "chart");
            var ok = NewService().TryMoveDirectoryToAvailablePath(src, desired, out var finalPath);

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsTrue();
            await Assert.That(finalPath).IsEqualTo(desired);
            await Assert.That(await File.ReadAllTextAsync(Path.Combine(desired, "f.txt"))).IsEqualTo("payload");
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }

    [Test]
    public async Task TryMoveDirectoryToAvailablePath_SourceMissing_ReturnsFalse()
    {
        var work = NewTempFolder();
        try
        {
            var ok = NewService().TryMoveDirectoryToAvailablePath(Path.Combine(work, "missing"), Path.Combine(work, "chart"), out var finalPath);

            using var _ = Assert.Multiple();
            await Assert.That(ok).IsFalse();
            await Assert.That(finalPath).IsEmpty();
        }
        finally
        {
            Directory.Delete(work, true);
        }
    }
}
