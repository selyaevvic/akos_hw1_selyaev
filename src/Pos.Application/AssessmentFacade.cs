using Pos.Domain;

namespace Pos.Application;

public sealed class AssessmentFacade(
    ICourseRepository repository, FinalScoreService scoreService, IResultExporter exporter)
{
    public IReadOnlyList<Student> Students => repository.Get().Students;
    public IReadOnlyList<Criterion> Criteria => repository.Get().Criteria;
    public IReadOnlyCollection<Evaluation> Evaluations => repository.Get().Evaluations;

    public Student AddStudent(string name)
    {
        var course = repository.Get();
        var student = course.AddStudent(name);
        repository.Save(course);
        return student;
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        var course = repository.Get();
        var criterion = course.AddCriterion(name, maxScore, weight);
        repository.Save(course);
        return criterion;
    }

    public void SetEvaluation(Guid studentId, Guid criterionId, decimal score)
    {
        var course = repository.Get();
        course.SetEvaluation(studentId, criterionId, score);
        repository.Save(course);
    }

    public void CancelEvaluation(Guid studentId, Guid criterionId)
    {
        var course = repository.Get();
        course.CancelEvaluation(studentId, criterionId);
        repository.Save(course);
    }

    public IReadOnlyList<ResultRow> GetResults()
    {
        var course = repository.Get();
        return course.Students.Select(student =>
        {
            try
            {
                var score = scoreService.Calculate(course, student.Id);
                var complete = course.Criteria.All(c => course.FindEvaluation(student.Id, c.Id) is not null);
                return new ResultRow(student.Id, student.Name, score, complete ? "Полный" : "Есть пропуски");
            }
            catch (DomainException exception)
            {
                return new ResultRow(student.Id, student.Name, null, exception.Message);
            }
        }).ToList().AsReadOnly();
    }

    public void ExportResults(string path) => exporter.Export(GetResults(), path);
}
