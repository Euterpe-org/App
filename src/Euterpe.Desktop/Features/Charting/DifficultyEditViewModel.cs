using System.Collections.ObjectModel;

namespace Euterpe.Features.Charting;

public sealed partial class DifficultyEditViewModel : ObservableObject
{
    public ChartDifficulty Difficulty { get; }

    public LocalizedString DifficultyName => $"ChartDifficulty_{Difficulty.ToStringFast()}";

    [ObservableProperty]
    public partial string Rating { get; set; }

    public ObservableCollection<string> Charters { get; }

    public DifficultyEditViewModel(ChartDifficulty difficulty, string rating, IEnumerable<string> charters)
    {
        Difficulty = difficulty;
        Rating = rating;
        Charters = [.. charters];
    }
}
