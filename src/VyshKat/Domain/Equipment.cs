namespace VyshKat.Domain;

/// <summary>Вещи на балансе (не прокатный транспорт): шлемы, док-станции, зарядные шкафы.</summary>
public abstract class Equipment : Asset
{
    protected Equipment(int inventoryNumber, string name)
        : base(inventoryNumber, name)
    {
    }
}

/// <summary>Оборудование, подключённое к сети.</summary>
public abstract class PoweredEquipment : Equipment, IEnergyConsumer
{
    protected PoweredEquipment(int inventoryNumber, string name, double dailyEnergyKwh)
        : base(inventoryNumber, name)
    {
        DailyEnergyKwh = Guard.Energy(dailyEnergyKwh, nameof(dailyEnergyKwh));
    }

    public double DailyEnergyKwh { get; }
}

public sealed class Helmet : Equipment
{
    public Helmet(int inventoryNumber, string name)
        : base(inventoryNumber, name)
    {
    }

    public override string Kind => "Шлем";
}

public sealed class DockStation : PoweredEquipment
{
    public DockStation(int inventoryNumber, string name, double dailyEnergyKwh)
        : base(inventoryNumber, name, dailyEnergyKwh)
    {
    }

    public override string Kind => "Док-станция";
}

public sealed class ChargingCabinet : PoweredEquipment
{
    public ChargingCabinet(int inventoryNumber, string name, double dailyEnergyKwh)
        : base(inventoryNumber, name, dailyEnergyKwh)
    {
    }

    public override string Kind => "Зарядный шкаф";
}
