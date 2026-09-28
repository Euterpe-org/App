using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using DynamicData.Binding;

namespace Euterpe.Reactive;

internal static class ChangeSetExtensions
{
    public static IObservable<IChangeSet<TObject, TKey>> SortAndBindOnUI<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TObject, TKey>(
        this IObservable<IChangeSet<TObject, TKey>> source,
        out ReadOnlyObservableCollection<TObject> collection,
        IObservable<IComparer<TObject>> comparer)
        where TObject : notnull
        where TKey : notnull
        => source.SortAndBind(out collection, comparer, new SortAndBindOptions { Scheduler = AvaloniaScheduler.Instance });

    public static IObservable<IChangeSet<TObject, TKey>> SortAndBindOnUI<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TObject, TKey>(
        this IObservable<IChangeSet<TObject, TKey>> source,
        out ReadOnlyObservableCollection<TObject> collection,
        IComparer<TObject> comparer)
        where TObject : notnull
        where TKey : notnull
        => source.SortAndBind(out collection, comparer, new SortAndBindOptions { Scheduler = AvaloniaScheduler.Instance });
}
