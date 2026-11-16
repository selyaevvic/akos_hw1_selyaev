using System.Globalization;
using Pos.Application;
using Pos.Domain;

namespace Pos.ConsoleUi;

public sealed class AssessmentConsole(AssessmentFacade assessments)
{
    public void Run()
    {
        Console.WriteLine("Данные хранятся в памяти до выхода. Итоги можно экспортировать в CSV.");
        while (true)
        {
            Console.WriteLine("\n1 Студент + | 2 Студенты | 3 Критерий + | 4 Критерии");
            Console.WriteLine("5 Выставить/исправить | 6 Оценки | 7 Отменить | 8 Итоги | 9 CSV | 0 Выход");
            var command = Console.ReadLine();
            if (command is null or "0") return;
            try
            {
                switch (command.Trim())
                {
                    case "1":
                        assessments.AddStudent(Read("Имя студента: "));
                        Console.WriteLine("Студент добавлен.");
                        break;
                    case "2": ShowStudents(); break;
                    case "3":
                        var name = Read("Название критерия: ");
                        var maximum = ReadDecimal("Максимальный балл: ");
                        var weight = ReadDecimal("Вес (положительное число): ");
                        assessments.AddCriterion(name, maximum, weight);
                        Console.WriteLine("Критерий добавлен.");
                        break;
                    case "4": ShowCriteria(); break;
                    case "5":
                        var student = SelectStudent();
                        var criterion = SelectCriterion();
                        assessments.SetEvaluation(student, criterion, ReadDecimal("Балл: "));
                        Console.WriteLine("Оценка сохранена.");
                        break;
                    case "6": ShowEvaluations(); break;
                    case "7":
                        assessments.CancelEvaluation(SelectStudent(), SelectCriterion());
                        Console.WriteLine("Оценка отменена.");
                        break;
                    case "8": ShowResults(); break;
                    case "9":
                        assessments.ExportResults(Read("Путь нового CSV-файла: "));
                        Console.WriteLine("Экспорт завершён.");
                        break;
                    default: Console.WriteLine("Выберите пункт от 0 до 9."); break;
                }
            }
            catch (EndOfStreamException) { return; }
            catch (Exception exception) when (exception is DomainException or IOException or UnauthorizedAccessException or ArgumentException)
            {
                Console.WriteLine($"Ошибка: {exception.Message}");
            }
        }
    }

    public void RunDemo(string path)
    {
        var anna = assessments.AddStudent("Анна Иванова");
        var ivan = assessments.AddStudent("Иван Петров");
        var homework = assessments.AddCriterion("Домашняя работа", 10, 40);
        var exam = assessments.AddCriterion("Экзамен", 100, 60);
        assessments.SetEvaluation(anna.Id, homework.Id, 8);
        assessments.SetEvaluation(anna.Id, exam.Id, 90);
        assessments.SetEvaluation(ivan.Id, homework.Id, 6);
        assessments.SetEvaluation(ivan.Id, exam.Id, 70);
        ShowStudents();
        ShowCriteria();
        ShowEvaluations();
        ShowResults();
        assessments.ExportResults(path);
        Console.WriteLine($"Итоги экспортированы: {Path.GetFullPath(path)}");
    }

    private void ShowStudents()
    {
        Console.WriteLine("Студенты:");
        for (var i = 0; i < assessments.Students.Count; i++)
            Console.WriteLine($"{i + 1}. {assessments.Students[i].Name}");
        if (assessments.Students.Count == 0) Console.WriteLine("Список пуст.");
    }

    private void ShowCriteria()
    {
        Console.WriteLine("Критерии:");
        for (var i = 0; i < assessments.Criteria.Count; i++)
        {
            var criterion = assessments.Criteria[i];
            Console.WriteLine($"{i + 1}. {criterion.Name}: максимум {criterion.MaxScore}, вес {criterion.Weight}");
        }
        if (assessments.Criteria.Count == 0) Console.WriteLine("Рубрикатор пуст.");
    }

    private void ShowEvaluations()
    {
        Console.WriteLine("Оценки:");
        foreach (var evaluation in assessments.Evaluations)
        {
            var student = assessments.Students.Single(s => s.Id == evaluation.StudentId);
            var criterion = assessments.Criteria.Single(c => c.Id == evaluation.CriterionId);
            Console.WriteLine($"{student.Name} / {criterion.Name}: {evaluation.Score} из {criterion.MaxScore}");
        }
        if (assessments.Evaluations.Count == 0) Console.WriteLine("Оценок пока нет.");
    }

    private void ShowResults()
    {
        var results = assessments.GetResults();
        foreach (var result in results)
            Console.WriteLine($"{result.StudentName}: {result.Score?.ToString("0.00") ?? "нет итога"} / 10 ({result.Status})");
        var calculated = results.Where(r => r.Score.HasValue).ToList();
        if (calculated.Count > 0)
            Console.WriteLine($"Средний итог: {calculated.Average(r => r.Score!.Value):0.00}; рассчитано {calculated.Count} из {results.Count}.");
        if (results.Count == 0) Console.WriteLine("Студентов пока нет.");
    }

    private Guid SelectStudent()
    {
        ShowStudents();
        return assessments.Students[ReadIndex(assessments.Students.Count)].Id;
    }

    private Guid SelectCriterion()
    {
        ShowCriteria();
        return assessments.Criteria[ReadIndex(assessments.Criteria.Count)].Id;
    }

    private static int ReadIndex(int count)
    {
        if (count == 0) throw new DomainException("Сначала заполните список.");
        if (!int.TryParse(Read("Номер: "), out var number) || number < 1 || number > count)
            throw new DomainException("Такого номера нет в списке.");
        return number - 1;
    }

    private static decimal ReadDecimal(string prompt)
    {
        var text = Read(prompt).Replace(',', '.');
        if (!decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
            throw new DomainException("Введите число, например 8,5 или 8.5.");
        return value;
    }

    private static string Read(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine()?.Trim() ?? throw new EndOfStreamException();
    }
}
