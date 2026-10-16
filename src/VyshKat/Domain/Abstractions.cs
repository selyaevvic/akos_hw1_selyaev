namespace VyshKat.Domain;

/// <summary>Объект, стоящий на балансе: имеет инвентарный номер и наименование.</summary>
public interface IInventoried
{
    int InventoryNumber { get; }

    string Name { get; }
}

/// <summary>Устройство, потребляющее электроэнергию.</summary>
public interface IEnergyConsumer
{
    /// <summary>Суточный расход энергии, кВт·ч.</summary>
    double DailyEnergyKwh { get; }
}
