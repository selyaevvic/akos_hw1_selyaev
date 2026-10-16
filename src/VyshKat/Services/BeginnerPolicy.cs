using VyshKat.Domain;

namespace VyshKat.Services;

/// <summary>Правило «можно выдавать новичку».</summary>
public interface IBeginnerPolicy
{
    string Description { get; }

    bool IsSuitable(Vehicle vehicle);
}

/// <summary>Базовое правило заказчика: простота не ниже порога (по умолчанию 6).</summary>
public sealed class SimplicityBeginnerPolicy : IBeginnerPolicy
{
    public const int DefaultMinSimplicity = 6;

    private readonly int _minSimplicity;

    public SimplicityBeginnerPolicy(int minSimplicity = DefaultMinSimplicity)
    {
        _minSimplicity = Guard.InRange(minSimplicity, Vehicle.MinSimplicity, Vehicle.MaxSimplicity, nameof(minSimplicity));
    }

    public string Description => $"простота ≥ {_minSimplicity}";

    public bool IsSuitable(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);
        return vehicle.Simplicity >= _minSimplicity;
    }
}
