using Moq;
using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.Tests;

public class FleetReportServiceTests
{
    private readonly Fleet _fleet = new(new FakeServiceCenter(approve: true));

    [Fact]
    public void EmptyFleet_ReportsZeros()
    {
        var reports = new FleetReportService(_fleet, new SimplicityBeginnerPolicy());

        Assert.Equal(new FleetSummary(0, 0, 0), reports.GetSummary());
        Assert.Empty(reports.GetBeginnerFriendlyVehicles());
        Assert.Empty(reports.GetInventoryList());
    }

    [Fact]
    public void TotalEnergy_SumsVehiclesAndPoweredEquipment()
    {
        _fleet.AcceptVehicle(TestData.Scooter(number: 1, energy: 0.5));
        _fleet.AcceptVehicle(TestData.ElectricBike(number: 2, energy: 1.25));
        _fleet.AcceptVehicle(TestData.Bicycle(number: 3));
        _fleet.AddEquipment(new DockStation(4, "Док", 2));
        _fleet.AddEquipment(new Helmet(5, "Шлем"));
        var reports = new FleetReportService(_fleet, new SimplicityBeginnerPolicy());

        var summary = reports.GetSummary();

        Assert.Equal(3.75, reports.GetTotalDailyEnergyKwh(), precision: 6);
        Assert.Equal(3, summary.VehicleCount);
        Assert.Equal(2, summary.EquipmentCount);
        Assert.Equal(5, summary.TotalCount);
        Assert.Equal(3.75, summary.TotalDailyEnergyKwh, precision: 6);
    }

    [Fact]
    public void RejectedVehicles_AreNotCountedInEnergy()
    {
        var fleet = new Fleet(new ServiceCenter());
        fleet.AcceptVehicle(TestData.Scooter(number: 1, energy: 0.5));
        fleet.AcceptVehicle(TestData.Scooter(number: 2, energy: 0.7, condition: 10));
        var reports = new FleetReportService(fleet, new SimplicityBeginnerPolicy());

        Assert.Equal(0.5, reports.GetTotalDailyEnergyKwh(), precision: 6);
    }

    [Fact]
    public void BeginnerList_ContainsOnlyVehiclesWithSimplicityAtLeastSix()
    {
        _fleet.AcceptVehicle(TestData.Scooter(number: 3, simplicity: 6));
        _fleet.AcceptVehicle(TestData.Scooter(number: 1, simplicity: 5));
        _fleet.AcceptVehicle(TestData.Bicycle(number: 2, simplicity: 9));
        _fleet.AddEquipment(new Helmet(4, "Шлем"));
        var reports = new FleetReportService(_fleet, new SimplicityBeginnerPolicy());

        var numbers = reports.GetBeginnerFriendlyVehicles().Select(v => v.InventoryNumber);

        Assert.Equal(new[] { 2, 3 }, numbers);
        Assert.Equal("простота ≥ 6", reports.BeginnerRule);
    }

    [Fact]
    public void BeginnerList_DelegatesToInjectedPolicy()
    {
        var policy = new Mock<IBeginnerPolicy>();
        policy.Setup(p => p.IsSuitable(It.IsAny<Vehicle>())).Returns<Vehicle>(v => v is Bicycle);
        _fleet.AcceptVehicle(TestData.Scooter(number: 1, simplicity: 10));
        _fleet.AcceptVehicle(TestData.Bicycle(number: 2, simplicity: 1));
        var reports = new FleetReportService(_fleet, policy.Object);

        var vehicle = Assert.Single(reports.GetBeginnerFriendlyVehicles());

        Assert.IsType<Bicycle>(vehicle);
    }

    [Fact]
    public void InventoryList_ContainsVehiclesAndEquipment_SortedByNumber()
    {
        _fleet.AddEquipment(new ChargingCabinet(30, "Шкаф", 5));
        _fleet.AcceptVehicle(TestData.Scooter(number: 10));
        _fleet.AddEquipment(new Helmet(20, "Шлем"));
        var reports = new FleetReportService(_fleet, new SimplicityBeginnerPolicy());

        var list = reports.GetInventoryList();

        Assert.Equal(new[] { 10, 20, 30 }, list.Select(a => a.InventoryNumber));
        Assert.Equal(new[] { "Электросамокат", "Шлем", "Зарядный шкаф" }, list.Select(a => a.Kind));
    }
}
