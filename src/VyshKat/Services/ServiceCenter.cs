using VyshKat.Domain;

namespace VyshKat.Services;

/// <summary>Решение техосмотра.</summary>
public sealed record InspectionResult(bool Approved, string Reason)
{
    public static InspectionResult Approve(string reason = "техосмотр пройден") => new(true, reason);

    public static InspectionResult Reject(string reason) => new(false, reason);
}

/// <summary>Техосмотр: решает, можно ли принять транспорт в активный парк.</summary>
public interface IServiceCenter
{
    InspectionResult Inspect(Vehicle vehicle);
}

/// <summary>
/// Базовый регламент техосмотра «ВышКат»:
/// состояние не ниже 60 % и, для электротранспорта, отсутствие аномального расхода энергии.
/// </summary>
public sealed class ServiceCenter : IServiceCenter
{
    public const int MinConditionPercent = 60;

    /// <summary>Расход выше этого порога говорит о деградации аккумулятора.</summary>
    public const double MaxDailyEnergyKwh = 3.0;

    public InspectionResult Inspect(Vehicle vehicle)
    {
        ArgumentNullException.ThrowIfNull(vehicle);

        if (vehicle.ConditionPercent < MinConditionPercent)
        {
            return InspectionResult.Reject(
                $"износ: состояние {vehicle.ConditionPercent}% < {MinConditionPercent}%");
        }

        if (vehicle is IEnergyConsumer consumer && consumer.DailyEnergyKwh > MaxDailyEnergyKwh)
        {
            return InspectionResult.Reject(
                $"аномальный расход энергии {consumer.DailyEnergyKwh:0.##} кВт·ч/сут > {MaxDailyEnergyKwh:0.##} — проверить АКБ");
        }

        return InspectionResult.Approve();
    }
}
