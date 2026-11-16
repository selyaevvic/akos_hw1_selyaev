using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;

namespace Pos.Tests;

public sealed class ApplicationTests
{
    [Fact]
    public void EndToEndScenarioExportsCurrentTotals()
    {
        var exporter = new CapturingExporter();
        var app = new AssessmentFacade(new InMemoryCourseRepository(),
            new FinalScoreService(new WeightedScoreStrategy()), exporter);
        var student = app.AddStudent("Анна");
        var criterion = app.AddCriterion("ДЗ", 10, 1);
        app.SetEvaluation(student.Id, criterion.Id, 8);
        app.ExportResults("results.csv");
        Assert.Equal(8, Assert.Single(exporter.Rows!).Score);
        app.CancelEvaluation(student.Id, criterion.Id);
        Assert.Equal(0, Assert.Single(app.GetResults()).Score);
    }

    [Fact]
    public void IncompleteFinalResultIsNotExportedAsZero()
    {
        var app = new AssessmentFacade(new InMemoryCourseRepository(),
            new FinalScoreService(new CompleteScoreStrategy()), new CapturingExporter());
        app.AddStudent("Анна");
        app.AddCriterion("ДЗ", 10, 1);
        var row = Assert.Single(app.GetResults());
        Assert.Null(row.Score);
        Assert.Contains("нужны оценки", row.Status);
    }

    [Fact]
    public void CsvEscapesNamesUsesInvariantNumbersAndDoesNotOverwrite()
    {
        var path = Path.Combine(Path.GetTempPath(), $"pos-{Guid.NewGuid()}.csv");
        try
        {
            var rows = new[] { new ResultRow(Guid.NewGuid(), "=Анна;\"Иванова\"\n", 8.6m, "Полный") };
            var exporter = new CsvResultExporter();
            exporter.Export(rows, path);
            var csv = File.ReadAllText(path);
            Assert.Contains("\"'=Анна;\"\"Иванова\"\"\n\";8.60;\"Полный\"", csv);
            Assert.Throws<IOException>(() => exporter.Export(rows, path));
            Assert.Equal(csv, File.ReadAllText(path));
        }
        finally { File.Delete(path); }
    }

    private sealed class CapturingExporter : IResultExporter
    {
        public IReadOnlyList<ResultRow>? Rows { get; private set; }
        public void Export(IReadOnlyList<ResultRow> results, string path) => Rows = results;
    }
}
