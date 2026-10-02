namespace Euterpe.Extensions;

internal static class ObservableExtensions
{
    private static readonly TimeSpan SearchDebounce = TimeSpan.FromMilliseconds(200);

    extension(Observable<string?> changes)
    {
        public Observable<Unit> DebounceSearch(string searchProperty) =>
            changes.Where(searchProperty, static (name, searchName) => name != searchName)
                .Merge(changes.Where(searchProperty, static (name, searchName) => name == searchName)
                    .Debounce(SearchDebounce))
                .AsUnitObservable();
    }
}
