namespace VyshKat.Domain;

/// <summary>Прокатный транспорт. Попадает в парк только через техосмотр.</summary>
public abstract class Vehicle : Asset
{
    public const int MinSimplicity = 1;
    public const int MaxSimplicity = 10;

    protected Vehicle(int inventoryNumber, string name, int simplicity, int conditionPercent)
        : base(inventoryNumber, name)
    {
        Simplicity = Guard.InRange(simplicity, MinSimplicity, MaxSimplicity, nameof(simplicity));
        ConditionPercent = Guard.InRange(conditionPercent, 0, 100, nameof(conditionPercent));
    }

    /// <summary>Простота для новичка, 1–10.</summary>
    public int Simplicity { get; }

    /// <summary>Техническое состояние, % (100 — новый).</summary>
    public int ConditionPercent { get; }
}

/// <summary>Транспорт с аккумулятором — потребляет энергию.</summary>
public abstract class ElectricVehicle : Vehicle, IEnergyConsumer
{
    protected ElectricVehicle(
        int inventoryNumber, string name, int simplicity, int conditionPercent, double dailyEnergyKwh)
        : base(inventoryNumber, name, simplicity, conditionPercent)
    {
        DailyEnergyKwh = Guard.Energy(dailyEnergyKwh, nameof(dailyEnergyKwh));
    }

    public double DailyEnergyKwh { get; }
}

public sealed class Bicycle : Vehicle
{
    public Bicycle(int inventoryNumber, string name, int simplicity, int conditionPercent)
        : base(inventoryNumber, name, simplicity, conditionPercent)
    {
    }

    public override string Kind => "Велосипед";
}

public sealed class Scooter : ElectricVehicle
{
    public Scooter(int inventoryNumber, string name, int simplicity, int conditionPercent, double dailyEnergyKwh)
        : base(inventoryNumber, name, simplicity, conditionPercent, dailyEnergyKwh)
    {
    }

    public override string Kind => "Электросамокат";
}

public sealed class ElectricBike : ElectricVehicle
{
    public ElectricBike(int inventoryNumber, string name, int simplicity, int conditionPercent, double dailyEnergyKwh)
        : base(inventoryNumber, name, simplicity, conditionPercent, dailyEnergyKwh)
    {
    }

    public override string Kind => "Электровелосипед";
}
