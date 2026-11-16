using Pos.Domain;

namespace Pos.Tests;

public sealed class DomainTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void InvalidScoreDoesNotReplacePreviousEvaluation(int score)
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("ДЗ", 10, 1);
        course.SetEvaluation(student.Id, criterion.Id, 7);
        Assert.Throws<DomainException>(() => course.SetEvaluation(student.Id, criterion.Id, score));
        Assert.Equal(7, course.FindEvaluation(student.Id, criterion.Id)!.Score);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ScoreBoundariesAreAllowed(int score)
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("ДЗ", 10, 1);
        course.SetEvaluation(student.Id, criterion.Id, score);
        Assert.Equal(score, course.FindEvaluation(student.Id, criterion.Id)!.Score);
    }

    [Fact]
    public void UnknownStudentOrCriterionIsRejected()
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("ДЗ", 10, 1);
        Assert.Throws<DomainException>(() => course.SetEvaluation(student.Id, Guid.NewGuid(), 1));
        Assert.Throws<DomainException>(() => course.SetEvaluation(Guid.NewGuid(), criterion.Id, 1));
        Assert.Empty(course.Evaluations);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 1)]
    [InlineData(10, 0)]
    [InlineData(10, -1)]
    [InlineData(1000001, 1)]
    [InlineData(1, 1000001)]
    public void InvalidCriterionIsRejected(int maximum, int weight)
    {
        var course = new Course();
        Assert.Throws<DomainException>(() => course.AddCriterion("ДЗ", maximum, weight));
        Assert.Empty(course.Criteria);
    }

    [Fact]
    public void NamesAreValidatedAndCriterionNamesAreUnique()
    {
        var course = new Course();
        Assert.Throws<DomainException>(() => course.AddStudent(" "));
        Assert.Throws<DomainException>(() => course.AddCriterion(" ", 10, 1));
        course.AddCriterion(" ДЗ ", 10, 1);
        Assert.Throws<DomainException>(() => course.AddCriterion("дз", 10, 2));
        Assert.Single(course.Criteria);
        Assert.NotEqual(course.AddStudent("Анна").Id, course.AddStudent("Анна").Id);
    }

    [Fact]
    public void TotalTracksCorrectionsCancellationsAndNewCriteria()
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var homework = course.AddCriterion("ДЗ", 10, 40);
        var exam = course.AddCriterion("Экзамен", 100, 60);
        var service = new FinalScoreService(new WeightedScoreStrategy());
        course.SetEvaluation(student.Id, homework.Id, 8);
        course.SetEvaluation(student.Id, exam.Id, 90);
        Assert.Equal(8.6m, service.Calculate(course, student.Id));
        course.SetEvaluation(student.Id, homework.Id, 10);
        Assert.Equal(9.4m, service.Calculate(course, student.Id));
        Assert.Equal(2, course.Evaluations.Count);
        course.CancelEvaluation(student.Id, exam.Id);
        Assert.Equal(4m, service.Calculate(course, student.Id));
        course.AddCriterion("Проект", 10, 100);
        Assert.Equal(2m, service.Calculate(course, student.Id));
        Assert.Throws<DomainException>(() => course.CancelEvaluation(student.Id, exam.Id));
    }

    [Fact]
    public void CompleteStrategyDistinguishesMissingScoreFromZero()
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("ДЗ", 10, 1);
        var service = new FinalScoreService(new CompleteScoreStrategy());
        Assert.Throws<DomainException>(() => service.Calculate(course, student.Id));
        course.SetEvaluation(student.Id, criterion.Id, 0);
        Assert.Equal(0, service.Calculate(course, student.Id));
    }

    [Fact]
    public void EmptyRubricHasNoTotal()
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        Assert.Throws<DomainException>(() => new FinalScoreService(new WeightedScoreStrategy()).Calculate(course, student.Id));
        Assert.Throws<DomainException>(() => new FinalScoreService(new CompleteScoreStrategy()).Calculate(course, student.Id));
    }

    [Fact]
    public void RoundingUsesTwoDigitsAwayFromZero()
    {
        var course = new Course();
        var student = course.AddStudent("Анна");
        var criterion = course.AddCriterion("ДЗ", 1000, 1);
        course.SetEvaluation(student.Id, criterion.Id, 125.5m);
        Assert.Equal(1.26m, new FinalScoreService(new WeightedScoreStrategy()).Calculate(course, student.Id));
    }
}
