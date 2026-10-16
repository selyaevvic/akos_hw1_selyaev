using VyshKat.Domain;

namespace VyshKat.Services;

public sealed record VehicleSpec(
    int InventoryNumber, string Name, int Simplicity, int ConditionPercent, double DailyEnergyKwh);

public sealed record EquipmentSpec(int InventoryNumber, string Name, double DailyEnergyKwh);

/// <summary>
/// Вид транспорта, доступный для приёмки. Регистрируется в DI-контейнере,
/// поэтому новый вид = новый класс + одна строка регистрации, без правки UI.
/// </summary>
public sealed record VehicleKind(string Title, bool IsElectric, Func<VehicleSpec, Vehicle> Create);

/// <summary>Вид вещи, которую можно поставить на баланс.</summary>
public sealed record EquipmentKind(string Title, bool IsPowered, Func<EquipmentSpec, Equipment> Create);
