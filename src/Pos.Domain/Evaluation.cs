namespace Pos.Domain;

// Неизменяемый факт: при исправлении оценки создаётся новое значение.
public sealed record Evaluation
{
    public Guid StudentId { get; }
    public Guid CriterionId { get; }
    public decimal Score { get; }

    internal Evaluation(Guid studentId, Guid criterionId, decimal score)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
    }
}

public static class EvaluationFactory
{
    public static Evaluation Create(Course course, Guid studentId, Guid criterionId, decimal score)
    {
        course.GetStudent(studentId);
        var criterion = course.GetCriterion(criterionId);
        if (score < 0 || score > criterion.MaxScore)
            throw new DomainException($"Балл должен быть от 0 до {criterion.MaxScore}.");
        return new Evaluation(studentId, criterionId, score);
    }
}
