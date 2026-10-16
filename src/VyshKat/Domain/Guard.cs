namespace VyshKat.Domain;

/// <summary>Проверки инвариантов доменных объектов.</summary>
internal static class Guard
{
    public static int InRange(int value, int min, int max, string paramName)
    {
        if (value < min || value > max)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Значение {value} вне диапазона {min}–{max}.");
        }

        return value;
    }

    public static double Energy(double kwh, string paramName)
    {
        if (double.IsNaN(kwh) || double.IsInfinity(kwh) || kwh < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, $"Суточный расход энергии должен быть неотрицательным числом, получено {kwh}.");
        }

        return kwh;
    }
}
