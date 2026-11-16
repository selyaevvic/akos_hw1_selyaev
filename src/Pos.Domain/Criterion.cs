namespace Pos.Domain;

public sealed class Criterion
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }
    public decimal MaxScore { get; }
    public decimal Weight { get; }

    internal Criterion(string name, decimal maxScore, decimal weight)
    {
        Name = Guard.Name(name);
        if (maxScore <= 0 || maxScore > 1_000_000)
            throw new DomainException("Максимум должен быть больше 0 и не больше 1000000.");
        if (weight <= 0 || weight > 1_000_000)
            throw new DomainException("Вес должен быть больше 0 и не больше 1000000.");
        MaxScore = maxScore;
        Weight = weight;
    }
}
