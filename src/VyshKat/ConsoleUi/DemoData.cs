using VyshKat.Domain;

namespace VyshKat.ConsoleUi;

/// <summary>Пилотный парк для демонстрации: есть и годная техника, и та, что не пройдёт техосмотр.</summary>
public static class DemoData
{
    public static IReadOnlyList<Vehicle> Vehicles() => new Vehicle[]
    {
        new Scooter(101, "Ninebot Max G30", simplicity: 7, conditionPercent: 95, dailyEnergyKwh: 0.6),
        new Scooter(102, "Kugoo Kirin M4", simplicity: 4, conditionPercent: 88, dailyEnergyKwh: 0.9),
        new Bicycle(103, "Stels Navigator 300", simplicity: 9, conditionPercent: 80),
        new ElectricBike(104, "Eltreco XT 850", simplicity: 5, conditionPercent: 70, dailyEnergyKwh: 1.2),
        new Scooter(105, "Xiaomi Pro 2 (б/у с Авито)", simplicity: 8, conditionPercent: 45, dailyEnergyKwh: 0.5),
        new ElectricBike(106, "Wenbox из гаража декана", simplicity: 6, conditionPercent: 90, dailyEnergyKwh: 4.5),
    };

    public static IReadOnlyList<Equipment> Equipment() => new Equipment[]
    {
        new Helmet(201, "Шлем ВышКат, размер M"),
        new Helmet(202, "Шлем ВышКат, размер L"),
        new DockStation(301, "Док-станция у корпуса на Покровке", dailyEnergyKwh: 2.4),
        new ChargingCabinet(302, "Зарядный шкаф на складе", dailyEnergyKwh: 5.0),
    };

    /// <summary>Объект с уже занятым номером 101 — для демонстрации контроля дублей.</summary>
    public static Vehicle Duplicate() => new Bicycle(101, "Велосипед-самозванец", simplicity: 10, conditionPercent: 100);
}
