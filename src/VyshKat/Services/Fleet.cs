using VyshKat.Domain;

namespace VyshKat.Services;

/// <summary>
/// Парк «ВышКат»: единственная точка постановки объектов на баланс.
/// Гарантирует уникальность инвентарных номеров и то, что транспорт проходит техосмотр.
/// </summary>
public sealed class Fleet
{
    private readonly IServiceCenter _serviceCenter;
    private readonly List<Asset> _assets = new();

    public Fleet(IServiceCenter serviceCenter)
    {
        _serviceCenter = serviceCenter;
    }

    public IReadOnlyList<Asset> Assets => _assets;

    public IEnumerable<Vehicle> Vehicles => _assets.OfType<Vehicle>();

    public IEnumerable<Equipment> Equipment => _assets.OfType<Equipment>();

    /// <summary>Отправляет транспорт на техосмотр и при одобрении принимает в парк.</summary>
    /// <exception cref="InvalidOperationException">Инвентарный номер уже занят.</exception>
    public InspectionResult AcceptVehicle(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        EnsureNumberIsFree(vehicle);

        var result = _serviceCenter.Inspect(vehicle);
        if (result.Approved)
        {
            _assets.Add(vehicle);
        }

        return result;
    }

    /// <summary>Ставит вещь на баланс (техосмотр для вещей не требуется).</summary>
    /// <exception cref="InvalidOperationException">Инвентарный номер уже занят.</exception>
    public void AddEquipment(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);
        EnsureNumberIsFree(equipment);
        _assets.Add(equipment);
    }

    private void EnsureNumberIsFree(Asset asset)
    {
        var existing = _assets.FirstOrDefault(a => a.InventoryNumber == asset.InventoryNumber);
        if (existing is not null)
        {
            throw new InvalidOperationException(
                $"Инвентарный номер {asset.InventoryNumber} уже занят: {existing}.");
        }
    }
}
