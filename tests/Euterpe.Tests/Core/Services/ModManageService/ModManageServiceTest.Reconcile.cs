namespace Euterpe.Tests.Core;

public sealed partial class ModManageServiceTest
{
    [Test]
    public async Task ReconcileModsAsync_LocalFileAppearsForCatalogMod_InstallsKeepingWebMetadata()
    {
        var local = new MutableModLocalService();
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("WebMod")),
            modLocalService: local);
        await sut.InitializeModsAsync();
        await Assert.That(sut.FindModByName("WebMod")!.IsLocal).IsFalse();

        local.Set("/mods/WebMod.dll", LocalMod("WebMod"));
        await sut.ReconcileModsAsync();

        var mod = sut.FindModByName("WebMod")!;
        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsTrue();
        await Assert.That(mod.HasDownloadSource).IsTrue();
    }

    [Test]
    public async Task ReconcileModsAsync_CatalogModFileDeleted_RevertsToNotInstalledKeepingWebMetadata()
    {
        var local = new MutableModLocalService();
        local.Set("/mods/WebMod.dll", LocalMod("WebMod"));
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("WebMod")),
            modLocalService: local);
        await sut.InitializeModsAsync();
        await Assert.That(sut.FindModByName("WebMod")!.IsLocal).IsTrue();

        local.Clear();
        await sut.ReconcileModsAsync();

        var mod = sut.FindModByName("WebMod");
        using var _ = Assert.Multiple();
        await Assert.That(mod).IsNotNull();
        await Assert.That(mod!.IsLocal).IsFalse();
        await Assert.That(mod.HasDownloadSource).IsTrue();
    }

    [Test]
    public async Task ReconcileModsAsync_LocalOnlyModFileDeleted_RemovesFromCache()
    {
        var local = new MutableModLocalService();
        local.Set("/mods/LocalMod.dll", LocalMod("LocalMod"));
        var sut = CreateModManageService(modLocalService: local);
        await sut.InitializeModsAsync();
        await Assert.That(sut.FindModByName("LocalMod")).IsNotNull();

        local.Clear();
        await sut.ReconcileModsAsync();

        await Assert.That(sut.FindModByName("LocalMod")).IsNull();
    }

    [Test]
    public async Task ReconcileModsAsync_NewLocalOnlyModFile_AddsToCache()
    {
        var local = new MutableModLocalService();
        var sut = CreateModManageService(modLocalService: local);
        await sut.InitializeModsAsync();

        local.Set("/mods/NewMod.dll", LocalMod("NewMod"));
        await sut.ReconcileModsAsync();

        var mod = sut.FindModByName("NewMod");
        using var _ = Assert.Multiple();
        await Assert.That(mod).IsNotNull();
        await Assert.That(mod!.IsLocal).IsTrue();
    }

    [Test]
    public async Task ReconcileModsAsync_NoDiskChange_LeavesInstalledModUnchanged()
    {
        var local = new MutableModLocalService();
        local.Set("/mods/WebMod.dll", LocalMod("WebMod", sha: "shared"));
        var sut = CreateModManageService(
            gameDownloadManager: DownloadManagerWith(CreateWebMod("WebMod", sha256: "shared")),
            modLocalService: local);
        await sut.InitializeModsAsync();
        var stateBefore = sut.FindModByName("WebMod")!.State;

        await sut.ReconcileModsAsync();

        var mod = sut.FindModByName("WebMod")!;
        using var _ = Assert.Multiple();
        await Assert.That(mod.IsLocal).IsTrue();
        await Assert.That(mod.State).IsEqualTo(stateBefore);
    }

    [Test]
    public async Task ReconcileModsAsync_DuplicateLocalFilesAppear_MarksDuplicated()
    {
        var local = new MutableModLocalService();
        var sut = CreateModManageService(modLocalService: local);
        await sut.InitializeModsAsync();

        local.Set("/mods/Dup.dll", LocalMod("Dup", sha: "a"));
        local.Set("/mods/Dup-copy.dll", LocalMod("Dup", "Dup-copy.dll", sha: "b"));
        await sut.ReconcileModsAsync();

        var mod = sut.FindModByName("Dup")!;
        using var _ = Assert.Multiple();
        await Assert.That(mod.State).IsEqualTo(ModState.Duplicated);
        await Assert.That(mod.DuplicatedModPaths).Contains("Dup.dll");
        await Assert.That(mod.DuplicatedModPaths).Contains("Dup-copy.dll");
    }

    [Test]
    public async Task ReconcileModsAsync_DuplicateFileRemoved_ClearsDuplicated()
    {
        var local = new MutableModLocalService();
        local.Set("/mods/Dup.dll", LocalMod("Dup", sha: "a"));
        local.Set("/mods/Dup-copy.dll", LocalMod("Dup", "Dup-copy.dll", sha: "b"));
        var sut = CreateModManageService(modLocalService: local);
        await sut.InitializeModsAsync();
        await Assert.That(sut.FindModByName("Dup")!.State).IsEqualTo(ModState.Duplicated);

        local.Remove("/mods/Dup-copy.dll");
        await sut.ReconcileModsAsync();

        var mod = sut.FindModByName("Dup")!;
        using var _ = Assert.Multiple();
        await Assert.That(mod.State).IsEqualTo(ModState.Normal);
        await Assert.That(mod.DuplicatedModPaths).IsEmpty();
    }

    private static ModDto LocalMod(string name, string? fileName = null, string version = "1.0.0", string sha = "sha", bool disabled = false)
    {
        var file = fileName ?? $"{name}.dll";
        return new ModDto
        {
            Name = name,
            LocalVersion = version,
            LocalSHA256 = sha,
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(file),
            IsDisabled = disabled
        };
    }

    private sealed class MutableModLocalService : IModLocalService
    {
        private readonly Dictionary<string, ModDto> _files = [];

        public IEnumerable<string> GetModFilePaths() => _files.Keys;

        public IEnumerable<string> GetLibFilePaths() => [];

        public Task<ModDto?> LoadModFromPathAsync(string filePath) =>
            Task.FromResult(_files.TryGetValue(filePath, out var template)
                ? new ModDto
                {
                    Name = template.Name,
                    LocalVersion = template.LocalVersion,
                    LocalSHA256 = template.LocalSHA256,
                    FileNameWithoutExtension = template.FileNameWithoutExtension,
                    IsDisabled = template.IsDisabled,
                    Author = template.Author
                }
                : null);

        public Task<LibDto> LoadLibFromPathAsync(string filePath) => throw new NotSupportedException();

        public void Set(string path, ModDto mod) => _files[path] = mod;

        public void Remove(string path) => _files.Remove(path);

        public void Clear() => _files.Clear();
    }
}
