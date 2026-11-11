namespace Euterpe.Controls.Models;

public sealed class NavItem(LocalizedString displayName, string navigateKey, string iconResourceKey = "") : ObservableObject
{
    public NavItem[] Children { get; init; } = [];
    public LocalizedString DisplayName { get; set; } = displayName;
    public string NavigateKey { get; init; } = navigateKey;
    public string IconResourceKey { get; set; } = iconResourceKey;
    public string? Status { get; init; }
}
