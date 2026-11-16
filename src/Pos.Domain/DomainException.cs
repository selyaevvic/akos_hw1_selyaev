namespace Pos.Domain;

public sealed class DomainException(string message) : Exception(message);

internal static class Guard
{
    public static string Name(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Название не может быть пустым.");
        return value.Trim();
    }
}
