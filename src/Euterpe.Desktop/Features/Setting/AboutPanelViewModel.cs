using Euterpe.Contracts.Credits;
using Euterpe.Core.Http.Clients;

namespace Euterpe.Features.Setting;

[Route("/setting/about", DisplayName = Panel_Setting_About, Order = 0)]
[AppSingleton]
public sealed partial class AboutPanelViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ContributorGroup[] ContributorGroups { get; private set; } = [];

    [ObservableProperty]
    public partial bool AllContributorsLoaded { get; private set; }

    protected override async Task OnInitializeAsync()
    {
        await base.OnInitializeAsync().ConfigureAwait(false);

        await LoadContributorsAsync().ConfigureAwait(false);

        Logger.LogInformation("{ViewModel} Initialized", nameof(AboutPanelViewModel));
    }

    private async Task LoadContributorsAsync()
    {
        try
        {
            var response = await CreditsClient
                .GetCreditsAsync(LanguageCodeMappings.ToCreditsLanguageCode(Config.LanguageCode))
                .ConfigureAwait(true);

            ContributorGroups = Array.ConvertAll(response.Sections, ToContributorGroup);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load contributors");
        }
        finally
        {
            AllContributorsLoaded = true;
        }
    }

    [RelayCommand]
    private async Task CheckUpdateAsync()
    {
        string? newVersion;
        try
        {
            newVersion = await UpdateService.CheckForUpdatesAsync().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to check for updates");
            await MessageBoxService.ErrorAsync(MessageBox_Content_Update_Check_Failed).ConfigureAwait(false);
            return;
        }

        if (newVersion is null)
        {
            await MessageBoxService.SuccessAsync(MessageBox_Content_NoUpdatesFound).ConfigureAwait(false);
            return;
        }

        await UpdateDialogService.ShowAsync(newVersion, MainWindowViewModel.DialogHostId).ConfigureAwait(false);
    }

    private static ContributorGroup ToContributorGroup(CreditsSection section) =>
        new(section.Title, Array.ConvertAll(section.Items, ToContributor));

    private static Contributor ToContributor(CreditsPerson person) =>
        new(person.Name,
            EuterpeWeb.BaseUrl + person.Avatar,
            person.Description,
            person.Links.Length is 0
                ? null
                : Array.ConvertAll(person.Links, static link => new ContributorLink(link.Name, link.Url)));

    #region Injections

    public required Config Config { get; init; }
    public required UpdateDialogService UpdateDialogService { get; init; }
    public required IEuterpeCreditsClient CreditsClient { get; init; }
    public required ILogger<AboutPanelViewModel> Logger { get; init; }
    public required IMessageBoxService MessageBoxService { get; init; }
    public required IUpdateService UpdateService { get; init; }

    #endregion Injections
}
