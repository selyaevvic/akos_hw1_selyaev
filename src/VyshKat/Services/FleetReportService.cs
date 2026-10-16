using VyshKat.Domain;

namespace VyshKat.Services;

public sealed record FleetSummary(int VehicleCount, int EquipmentCount, double TotalDailyEnergyKwh)
{
    public int TotalCount => VehicleCount + EquipmentCount;
}

/// <summary>Отчёты по парку. Только читает <see cref="Fleet"/>, ничего не меняет.</summary>
public sealed class FleetReportService
{
    private readonly Fleet _fleet;
    private readonly IBeginnerPolicy _beginnerPolicy;

    public FleetReportService(Fleet fleet, IBeginnerPolicy beginnerPolicy)
    {
        _fleet = fleet;
        _beginnerPolicy = beginnerPolicy;
    }

    public string BeginnerRule => _beginnerPolicy.Description;

    public FleetSummary GetSummary() =>
        new(_fleet.Vehicles.Count(), _fleet.Equipment.Count(), GetTotalDailyEnergyKwh());

    /// <summary>Суммарный суточный расход всех потребителей на балансе — и транспорта, и оборудования.</summary>
    public double GetTotalDailyEnergyKwh() =>
        _fleet.Assets.OfType<IEnergyConsumer>().Sum(c => c.DailyEnergyKwh);

    public IReadOnlyList<Vehicle> GetBeginnerFriendlyVehicles() =>
        _fleet.Vehicles.Where(_beginnerPolicy.IsSuitable).OrderBy(v => v.InventoryNumber).ToList();

    /// <summary>Инвентарная ведомость: вся техника и вещи по возрастанию номера.</summary>
    public IReadOnlyList<Asset> GetInventoryList() =>
        _fleet.Assets.OrderBy(a => a.InventoryNumber).ToList();
}
