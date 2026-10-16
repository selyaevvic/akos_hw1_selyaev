using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.Tests;

/// <summary>Fake техосмотра: выносит заранее заданное решение и запоминает, кого осматривал.</summary>
internal sealed class FakeServiceCenter : IServiceCenter
{
    private readonly bool _approve;

    public FakeServiceCenter(bool approve)
    {
        _approve = approve;
    }

    public List<Vehicle> Inspected { get; } = new();

    public InspectionResult Inspect(Vehicle vehicle)
    {
        Inspected.Add(vehicle);
        return _approve ? InspectionResult.Approve("fake: ок") : InspectionResult.Reject("fake: отказ");
    }
}

internal static class TestData
{
    public static Scooter Scooter(
        int number = 1, int simplicity = 7, int condition = 90, double energy = 0.5) =>
        new(number, "Тестовый самокат", simplicity, condition, energy);

    public static Bicycle Bicycle(int number = 2, int simplicity = 8, int condition = 90) =>
        new(number, "Тестовый велосипед", simplicity, condition);

    public static ElectricBike ElectricBike(int number = 3, int simplicity = 5, int condition = 90, double energy = 1.0) =>
        new(number, "Тестовый электровелосипед", simplicity, condition, energy);
}
