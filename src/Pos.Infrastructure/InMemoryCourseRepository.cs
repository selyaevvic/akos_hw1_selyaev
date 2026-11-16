using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public sealed class InMemoryCourseRepository : ICourseRepository
{
    private Course _course = new();

    public Course Get() => _course;
    public void Save(Course course) => _course = course;
}
