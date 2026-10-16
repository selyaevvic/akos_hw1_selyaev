using Microsoft.Extensions.DependencyInjection;
using VyshKat.Composition;
using VyshKat.ConsoleUi;
using VyshKat.Services;

namespace VyshKat.Tests;

public class DependencyInjectionTests
{
    private static ServiceCollection CreateServices(string input = "")
    {
        var services = new ServiceCollection();
        services.AddVyshKat();
        services.AddSingleton<TextReader>(new StringReader(input));
        services.AddSingleton<TextWriter>(new StringWriter());
        return services;
    }

    [Fact]
    public void Container_ResolvesWholeGraph()
    {
        using var provider = CreateServices().BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.NotNull(provider.GetRequiredService<FleetConsoleApp>());
        Assert.IsType<ServiceCenter>(provider.GetRequiredService<IServiceCenter>());
        Assert.Equal(3, provider.GetServices<VehicleKind>().Count());
        Assert.Equal(3, provider.GetServices<EquipmentKind>().Count());
    }

    [Fact]
    public void Fleet_IsSingleton_SharedWithReports()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var fleet = provider.GetRequiredService<Fleet>();
        var reports = provider.GetRequiredService<FleetReportService>();

        fleet.AcceptVehicle(TestData.Bicycle());

        Assert.Same(fleet, provider.GetRequiredService<Fleet>());
        Assert.Equal(1, reports.GetSummary().VehicleCount);
    }

    [Fact]
    public void ServiceCenter_CanBeReplacedInContainer_FleetUsesFake()
    {
        // Подмена техосмотра через DI: последняя регистрация IServiceCenter побеждает.
        var fake = new FakeServiceCenter(approve: false);
        var services = CreateServices();
        services.AddSingleton<IServiceCenter>(fake);
        using var provider = services.BuildServiceProvider();
        var fleet = provider.GetRequiredService<Fleet>();

        // Идеальный самокат: реальный ServiceCenter его бы принял, fake — отклоняет.
        var result = fleet.AcceptVehicle(TestData.Scooter(condition: 100, energy: 0.1));

        Assert.False(result.Approved);
        Assert.Single(fake.Inspected);
        Assert.Empty(fleet.Assets);
    }

    [Fact]
    public void ServiceCenter_ReplacedWithApprovingFake_AcceptsWornVehicle()
    {
        var services = CreateServices();
        services.AddSingleton<IServiceCenter>(new FakeServiceCenter(approve: true));
        using var provider = services.BuildServiceProvider();
        var fleet = provider.GetRequiredService<Fleet>();

        var result = fleet.AcceptVehicle(TestData.Scooter(condition: 0));

        Assert.True(result.Approved);
        Assert.Single(fleet.Vehicles);
    }

    [Fact]
    public void VehicleKinds_CreateMatchingTypes()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var spec = new VehicleSpec(1, "Модель", 6, 90, 0.5);

        var kinds = provider.GetServices<VehicleKind>().Select(k => k.Create(spec).Kind);

        Assert.Equal(new[] { "Электросамокат", "Велосипед", "Электровелосипед" }, kinds);
    }

    [Fact]
    public void EquipmentKinds_CreateMatchingTypes()
    {
        using var provider = CreateServices().BuildServiceProvider();
        var spec = new EquipmentSpec(1, "Вещь", 1);

        var kinds = provider.GetServices<EquipmentKind>().Select(k => k.Create(spec).Kind);

        Assert.Equal(new[] { "Шлем", "Док-станция", "Зарядный шкаф" }, kinds);
    }
}
