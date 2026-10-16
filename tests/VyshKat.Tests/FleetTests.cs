using Moq;
using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.Tests;

public class FleetTests
{
    [Fact]
    public void AcceptVehicle_WhenInspectionApproves_AddsToFleet()
    {
        var serviceCenter = new FakeServiceCenter(approve: true);
        var fleet = new Fleet(serviceCenter);
        var scooter = TestData.Scooter();

        var result = fleet.AcceptVehicle(scooter);

        Assert.True(result.Approved);
        Assert.Contains(scooter, fleet.Vehicles);
        Assert.Same(scooter, Assert.Single(serviceCenter.Inspected));
    }

    [Fact]
    public void AcceptVehicle_WhenInspectionRejects_DoesNotAdd()
    {
        var fleet = new Fleet(new FakeServiceCenter(approve: false));

        var result = fleet.AcceptVehicle(TestData.Scooter());

        Assert.False(result.Approved);
        Assert.Equal("fake: отказ", result.Reason);
        Assert.Empty(fleet.Assets);
    }

    [Fact]
    public void AcceptVehicle_UsesDecisionOfInjectedServiceCenter()
    {
        // Mock решает по-своему: принимает только велосипеды. Fleet обязан подчиниться.
        var serviceCenter = new Mock<IServiceCenter>();
        serviceCenter.Setup(s => s.Inspect(It.IsAny<Vehicle>()))
            .Returns<Vehicle>(v => v is Bicycle ? InspectionResult.Approve() : InspectionResult.Reject("только велосипеды"));
        var fleet = new Fleet(serviceCenter.Object);

        fleet.AcceptVehicle(TestData.Scooter(number: 1));
        fleet.AcceptVehicle(TestData.Bicycle(number: 2));

        Assert.IsType<Bicycle>(Assert.Single(fleet.Vehicles));
        serviceCenter.Verify(s => s.Inspect(It.IsAny<Vehicle>()), Times.Exactly(2));
    }

    [Fact]
    public void AcceptVehicle_DuplicateNumber_ThrowsWithoutInspection()
    {
        var serviceCenter = new Mock<IServiceCenter>();
        var fleet = new Fleet(serviceCenter.Object);
        fleet.AddEquipment(new Helmet(7, "Шлем"));

        Assert.Throws<InvalidOperationException>(() => fleet.AcceptVehicle(TestData.Scooter(number: 7)));
        serviceCenter.Verify(s => s.Inspect(It.IsAny<Vehicle>()), Times.Never);
    }

    [Fact]
    public void AddEquipment_DoesNotRequireInspection()
    {
        var serviceCenter = new Mock<IServiceCenter>(MockBehavior.Strict);
        var fleet = new Fleet(serviceCenter.Object);

        fleet.AddEquipment(new DockStation(10, "Док", 2));

        Assert.Single(fleet.Equipment);
        Assert.Empty(fleet.Vehicles);
    }

    [Fact]
    public void AddEquipment_DuplicateNumber_Throws()
    {
        var fleet = new Fleet(new FakeServiceCenter(approve: true));
        fleet.AcceptVehicle(TestData.Bicycle(number: 5));

        var ex = Assert.Throws<InvalidOperationException>(() => fleet.AddEquipment(new Helmet(5, "Шлем")));
        Assert.Contains("5", ex.Message);
        Assert.Single(fleet.Assets);
    }

    [Fact]
    public void RejectedVehicle_DoesNotOccupyNumber()
    {
        var fleet = new Fleet(new ServiceCenter());
        fleet.AcceptVehicle(TestData.Scooter(number: 1, condition: 10));

        var result = fleet.AcceptVehicle(TestData.Scooter(number: 1, condition: 100));

        Assert.True(result.Approved);
    }

    [Fact]
    public void Nulls_AreRejected()
    {
        var fleet = new Fleet(new FakeServiceCenter(approve: true));

        Assert.Throws<ArgumentNullException>(() => fleet.AcceptVehicle(null!));
        Assert.Throws<ArgumentNullException>(() => fleet.AddEquipment(null!));
    }
}
