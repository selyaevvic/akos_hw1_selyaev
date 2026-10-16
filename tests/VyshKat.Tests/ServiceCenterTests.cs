using VyshKat.Services;

namespace VyshKat.Tests;

public class ServiceCenterTests
{
    private readonly ServiceCenter _serviceCenter = new();

    [Fact]
    public void Inspect_GoodScooter_Approves()
    {
        var result = _serviceCenter.Inspect(TestData.Scooter(condition: 95, energy: 0.6));

        Assert.True(result.Approved);
    }

    [Theory]
    [InlineData(ServiceCenter.MinConditionPercent, true)]
    [InlineData(ServiceCenter.MinConditionPercent - 1, false)]
    public void Inspect_ConditionThreshold(int condition, bool expected)
    {
        var result = _serviceCenter.Inspect(TestData.Bicycle(condition: condition));

        Assert.Equal(expected, result.Approved);
    }

    [Fact]
    public void Inspect_WornVehicle_RejectsWithReason()
    {
        var result = _serviceCenter.Inspect(TestData.Scooter(condition: 30));

        Assert.False(result.Approved);
        Assert.Contains("износ", result.Reason);
    }

    [Theory]
    [InlineData(ServiceCenter.MaxDailyEnergyKwh, true)]
    [InlineData(ServiceCenter.MaxDailyEnergyKwh + 0.1, false)]
    public void Inspect_ElectricVehicle_EnergyThreshold(double energy, bool expected)
    {
        var result = _serviceCenter.Inspect(TestData.ElectricBike(energy: energy));

        Assert.Equal(expected, result.Approved);
    }

    [Fact]
    public void Inspect_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _serviceCenter.Inspect(null!));
    }
}
