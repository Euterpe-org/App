using Euterpe.Contracts.Account;

namespace Euterpe.Models.Auth;

public sealed partial class AuthState : ObservableObject
{
    [ObservableProperty]
    public partial string? AuthorizeUrl { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AvatarUrl))]
    public partial UserInfo? CurrentUser { get; set; }

    public string AvatarUrl => $"{EuterpeWeb.BaseUrl}/{CurrentUser?.AvatarUrl}";

    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public DateTimeOffset AccessTokenExpiry { get; set; }

    public void Clear()
    {
        AccessToken = null;
        RefreshToken = null;
        AccessTokenExpiry = default;
        CurrentUser = null;
    }
}
