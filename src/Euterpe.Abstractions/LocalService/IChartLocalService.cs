namespace Euterpe.Abstractions;

public interface IChartLocalService
{
    IEnumerable<string> GetChartFolderPaths(ChartSource source);
    Task<ChartDto?> LoadChartFromPathAsync(string chartFolder, ChartSource source);
}
