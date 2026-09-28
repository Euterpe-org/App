using System.ComponentModel;

namespace Euterpe.Extensions;

internal static class PropertyChangedExtensions
{
    extension(INotifyPropertyChanged source)
    {
        public Observable<string?> ObservePropertyChanges() =>
            Observable.FromEvent<PropertyChangedEventHandler, string?>(
                static handler => (_, args) => handler(args.PropertyName),
                handler => source.PropertyChanged += handler,
                handler => source.PropertyChanged -= handler);
    }
}
