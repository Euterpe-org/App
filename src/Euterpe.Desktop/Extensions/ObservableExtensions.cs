namespace Euterpe.Extensions;

internal static class ObservableExtensions
{
    extension(Observable<string?> changes)
    {
        public Observable<Unit> DebounceSearch(string searchProperty) =>
            changes.Where(searchProperty, static (name, searchName) => name != searchName)
                .Merge(changes.Where(searchProperty, static (name, searchName) => name == searchName)
                    .Debounce(AppConstants.SearchDebounce))
                .AsUnitObservable();
    }
}
