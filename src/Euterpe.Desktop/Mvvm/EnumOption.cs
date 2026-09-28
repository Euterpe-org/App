namespace Euterpe.Mvvm;

public sealed record EnumOption<TEnum>(TEnum Value, LocalizedString Display) : IEnumOption where TEnum : struct, Enum;
