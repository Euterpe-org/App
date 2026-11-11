namespace Euterpe.Mvvm;

public sealed record EnumOption<TEnum>(TEnum Value) : IEnumOption where TEnum : struct, Enum
{
    public LocalizedString Display { get; } = XAML.Get($"{typeof(TEnum).Name}_{Enum.GetName(Value)}");
}
