namespace Euterpe.Features.Modding;

[Route("/modding", DisplayName = Page_Modding, Icon = "Wrench", Order = 1)]
public sealed partial class ModdingPageViewModel : NavViewModelBase
{
    public IReadOnlyList<DropDownButtonItem> DropDownButtons => field ??=
    [
        new DropDownButtonItem(DropDownButton_Open,
        [
            new DropDownMenuItem(Folder_Mods, OpenFolderCommand, GameConfig.ModsFolder),
            new DropDownMenuItem(Folder_UserData, OpenFolderCommand, GameConfig.UserDataFolder),
            new DropDownMenuItem(Folder_UserLibs, OpenFolderCommand, GameConfig.UserLibsFolder)
        ])
    ];

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(ModdingPageViewModel));
    }

    #region Injections

    public required GameConfig GameConfig { get; init; }
    public required ILogger<ModdingPageViewModel> Logger { get; init; }

    #endregion Injections
}
