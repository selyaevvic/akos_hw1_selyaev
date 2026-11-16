using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public sealed class CsvResultExporter : IResultExporter
{
    public void Export(IReadOnlyList<ResultRow> results, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Укажите путь к CSV-файлу.");
        // CreateNew защищает существующий файл от случайного перезаписывания.
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
        using var writer = new StreamWriter(stream, new UTF8Encoding(true));
        writer.WriteLine("student_id;student_name;score;status");
        foreach (var row in results)
            writer.WriteLine($"{row.StudentId};{Escape(row.StudentName)};{row.Score?.ToString("0.00", CultureInfo.InvariantCulture)};{Escape(row.Status)}");
    }

    private static string Escape(string value)
    {
        // Имена рассматриваются таблицей как текст, даже если начинаются с формулы.
        if (value.Length > 0 && "=+-@\t\r\n".Contains(value[0]))
            value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }
}
