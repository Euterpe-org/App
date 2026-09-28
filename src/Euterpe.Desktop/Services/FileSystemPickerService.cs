using Avalonia.Platform.Storage;

namespace Euterpe.Services;

internal sealed class FileSystemPickerService : IFileSystemPickerService
{
    public required TopLevelProxy TopLevel { get; init; }

    public async Task<IStorageFolder?> GetSingleFolderAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders is not [] ? folders[0] : null;
    }

    public async Task<string?> GetSingleFolderPathAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders is not [] ? folders[0].TryGetLocalPath() : null;
    }

    public async Task<IReadOnlyList<IStorageFolder>> GetMultipleFoldersAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders;
    }

    public async Task<IReadOnlyList<string>> GetMultipleFolderPathsAsync(string dialogTitle)
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return folders.GetLocalPaths().ToArray();
    }

    public async Task<IStorageFile?> GetSingleFileAsync(string dialogTitle)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return files is not [] ? files[0] : null;
    }

    public async Task<string?> GetSingleFilePathAsync(string dialogTitle)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = false,
                Title = dialogTitle
            }).ConfigureAwait(true);

        return files is not [] ? files[0].TryGetLocalPath() : null;
    }

    public async Task<IReadOnlyList<IStorageFile>> GetMultipleFilesAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? fileTypes = null)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle,
                FileTypeFilter = fileTypes ?? [new FilePickerFileType(FilePickerFileType_AllFiles) { Patterns = ["*.*"] }]
            }).ConfigureAwait(true);

        return files;
    }

    public async Task<IReadOnlyList<string>> GetMultipleFilePathsAsync(string dialogTitle, IReadOnlyList<FilePickerFileType>? fileTypes = null)
    {
        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions
            {
                AllowMultiple = true,
                Title = dialogTitle,
                FileTypeFilter = fileTypes ?? [new FilePickerFileType(FilePickerFileType_AllFiles) { Patterns = ["*.*"] }]
            }).ConfigureAwait(true);

        return files.GetLocalPaths().ToArray();
    }
}
