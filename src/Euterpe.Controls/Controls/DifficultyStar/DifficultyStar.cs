using Euterpe.Models.Charts;

namespace Euterpe.Controls;

public sealed partial class DifficultyStar : TemplatedControl
{
    [GeneratedStyledProperty(DefaultValue = ChartDifficulty.Easy)]
    public partial ChartDifficulty Difficulty { get; set; }

    static DifficultyStar()
    {
        DifficultyProperty.Changed.AddClassHandler<DifficultyStar>(static (star, change) =>
            star.UpdatePseudoClasses(change.GetNewValue<ChartDifficulty>()));
    }

    public DifficultyStar() => UpdatePseudoClasses(Difficulty);

    private void UpdatePseudoClasses(ChartDifficulty difficulty)
    {
        PseudoClasses.Set(":easy", difficulty is ChartDifficulty.Easy);
        PseudoClasses.Set(":hard", difficulty is ChartDifficulty.Hard);
        PseudoClasses.Set(":master", difficulty is ChartDifficulty.Master);
        PseudoClasses.Set(":hidden", difficulty is ChartDifficulty.Hidden);
    }
}
