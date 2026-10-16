using Microsoft.Extensions.DependencyInjection;
using VyshKat.ConsoleUi;
using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.Composition;

/// <summary>Корень композиции: все зависимости приложения собираются здесь и только здесь.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует доменные сервисы и консольный UI.
    /// Ввод/вывод (<see cref="TextReader"/>/<see cref="TextWriter"/>) регистрирует вызывающий код,
    /// чтобы в тестах их можно было подменить.
    /// </summary>
    public static IServiceCollection AddVyshKat(this IServiceCollection services)
    {
        services.AddSingleton<IServiceCenter, ServiceCenter>();
        services.AddSingleton<IBeginnerPolicy>(_ => new SimplicityBeginnerPolicy());
        services.AddSingleton<Fleet>();
        services.AddSingleton<FleetReportService>();

        services.AddSingleton(new VehicleKind("Электросамокат", IsElectric: true,
            s => new Scooter(s.InventoryNumber, s.Name, s.Simplicity, s.ConditionPercent, s.DailyEnergyKwh)));
        services.AddSingleton(new VehicleKind("Велосипед", IsElectric: false,
            s => new Bicycle(s.InventoryNumber, s.Name, s.Simplicity, s.ConditionPercent)));
        services.AddSingleton(new VehicleKind("Электровелосипед", IsElectric: true,
            s => new ElectricBike(s.InventoryNumber, s.Name, s.Simplicity, s.ConditionPercent, s.DailyEnergyKwh)));

        services.AddSingleton(new EquipmentKind("Шлем", IsPowered: false,
            s => new Helmet(s.InventoryNumber, s.Name)));
        services.AddSingleton(new EquipmentKind("Док-станция", IsPowered: true,
            s => new DockStation(s.InventoryNumber, s.Name, s.DailyEnergyKwh)));
        services.AddSingleton(new EquipmentKind("Зарядный шкаф", IsPowered: true,
            s => new ChargingCabinet(s.InventoryNumber, s.Name, s.DailyEnergyKwh)));

        services.AddSingleton<ConsoleIo>();
        services.AddSingleton<FleetConsoleApp>();

        return services;
    }
}
