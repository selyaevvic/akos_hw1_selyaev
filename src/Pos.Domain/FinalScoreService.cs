namespace Pos.Domain;

public interface IFinalScoreStrategy
{
    decimal Calculate(Course course, Guid studentId);
}

// Пропуски дают нулевой вклад, знаменатель включает весь рубрикатор.
public sealed class WeightedScoreStrategy : IFinalScoreStrategy
{
    public decimal Calculate(Course course, Guid studentId)
    {
        course.GetStudent(studentId);
        if (course.Criteria.Count == 0)
            throw new DomainException("Нельзя рассчитать итог без критериев.");
        var totalWeight = course.Criteria.Sum(c => c.Weight);
        return 10 * course.Criteria.Sum(c =>
            (course.FindEvaluation(studentId, c.Id)?.Score ?? 0) / c.MaxScore * (c.Weight / totalWeight));
    }
}

// Для окончательной ведомости требуется полный набор оценок.
public sealed class CompleteScoreStrategy : IFinalScoreStrategy
{
    public decimal Calculate(Course course, Guid studentId)
    {
        course.GetStudent(studentId);
        if (course.Criteria.Any(c => course.FindEvaluation(studentId, c.Id) is null))
            throw new DomainException("Для окончательного итога нужны оценки по всем критериям.");
        return new WeightedScoreStrategy().Calculate(course, studentId);
    }
}

public sealed class FinalScoreService(IFinalScoreStrategy strategy)
{
    public decimal Calculate(Course course, Guid studentId) =>
        decimal.Round(strategy.Calculate(course, studentId), 2, MidpointRounding.AwayFromZero);
}
