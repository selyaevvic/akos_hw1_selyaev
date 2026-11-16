namespace Pos.Domain;

public sealed class Student
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; }

    internal Student(string name) => Name = Guard.Name(name);
}
