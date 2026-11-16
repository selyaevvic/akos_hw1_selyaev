namespace Pos.Domain;

public sealed class Course
{
    private readonly List<Student> _students = [];
    private readonly List<Criterion> _criteria = [];
    private readonly Dictionary<(Guid StudentId, Guid CriterionId), Evaluation> _evaluations = [];

    public IReadOnlyList<Student> Students => _students.AsReadOnly();
    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();
    public IReadOnlyCollection<Evaluation> Evaluations => _evaluations.Values.ToList().AsReadOnly();

    public Student AddStudent(string name)
    {
        var student = new Student(name);
        _students.Add(student);
        return student;
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var criterion = new Criterion(name, maxScore, weight);
        if (_criteria.Any(c => string.Equals(c.Name, criterion.Name, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException("Критерий с таким названием уже существует.");
        _criteria.Add(criterion);
        return criterion;
    }

    public Student GetStudent(Guid id) => _students.FirstOrDefault(s => s.Id == id)
        ?? throw new DomainException("Студент не найден.");

    public Criterion GetCriterion(Guid id) => _criteria.FirstOrDefault(c => c.Id == id)
        ?? throw new DomainException("Критерия нет в рубрикаторе.");

    public void SetEvaluation(Guid studentId, Guid criterionId, decimal score)
    {
        var evaluation = EvaluationFactory.Create(this, studentId, criterionId, score);
        _evaluations[(studentId, criterionId)] = evaluation;
    }

    public void CancelEvaluation(Guid studentId, Guid criterionId)
    {
        GetStudent(studentId);
        GetCriterion(criterionId);
        if (!_evaluations.Remove((studentId, criterionId)))
            throw new DomainException("Оценка ещё не выставлена.");
    }

    public Evaluation? FindEvaluation(Guid studentId, Guid criterionId) =>
        _evaluations.GetValueOrDefault((studentId, criterionId));
}
