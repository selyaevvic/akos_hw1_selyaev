using VyshKat.Domain;

namespace VyshKat.Tests;

public class DomainTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void Vehicle_SimplicityOutOfRange_Throws(int simplicity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestData.Scooter(simplicity: simplicity));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Vehicle_ConditionOutOfRange_Throws(int condition)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestData.Bicycle(condition: condition));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Asset_NonPositiveInventoryNumber_Throws(int number)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Helmet(number, "Шлем"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Asset_EmptyName_Throws(string name)
    {
        Assert.Throws<ArgumentException>(() => new Helmet(1, name));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(double.NaN)]
    public void EnergyConsumers_InvalidEnergy_Throws(double energy)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TestData.Scooter(energy: energy));
        Assert.Throws<ArgumentOutOfRangeException>(() => new DockStation(1, "Док", energy));
    }

    [Fact]
    public void Asset_TrimsName_AndFormatsToString()
    {
        var helmet = new Helmet(42, "  Шлем M  ");

        Assert.Equal("Шлем M", helmet.Name);
        Assert.Equal("#42 Шлем «Шлем M»", helmet.ToString());
    }

    [Fact]
    public void OnlyPoweredObjects_AreEnergyConsumers()
    {
        Assert.IsAssignableFrom<IEnergyConsumer>(TestData.Scooter());
        Assert.IsAssignableFrom<IEnergyConsumer>(TestData.ElectricBike());
        Assert.IsAssignableFrom<IEnergyConsumer>(new ChargingCabinet(1, "Шкаф", 5));
        Assert.False(typeof(IEnergyConsumer).IsAssignableFrom(typeof(Bicycle)));
        Assert.False(typeof(IEnergyConsumer).IsAssignableFrom(typeof(Helmet)));
    }

    [Fact]
    public void AllAssets_AreInventoried()
    {
        Asset[] assets =
        {
            TestData.Scooter(), TestData.Bicycle(), TestData.ElectricBike(),
            new Helmet(4, "Шлем"), new DockStation(5, "Док", 1), new ChargingCabinet(6, "Шкаф", 2),
        };

        Assert.All(assets, a => Assert.IsAssignableFrom<IInventoried>(a));
        Assert.Equal(6, assets.Select(a => a.Kind).Distinct().Count());
    }
}
