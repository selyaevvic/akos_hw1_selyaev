using Pos.Domain;

namespace Pos.Application;

public interface ICourseRepository
{
    Course Get();
    void Save(Course course);
}

public sealed record ResultRow(Guid StudentId, string StudentName, decimal? Score, string Status);

public interface IResultExporter
{
    void Export(IReadOnlyList<ResultRow> results, string path);
}
