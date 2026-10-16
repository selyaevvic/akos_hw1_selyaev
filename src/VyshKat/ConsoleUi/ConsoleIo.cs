using System.Globalization;

namespace VyshKat.ConsoleUi;

/// <summary>Ввод закончился (Ctrl+Z / конец перенаправленного файла).</summary>
public sealed class EndOfInputException : Exception
{
    public EndOfInputException()
        : base("Ввод завершён.")
    {
    }
}

/// <summary>Ввод/вывод с валидацией: повторяет вопрос, пока пользователь не введёт корректное значение.</summary>
public sealed class ConsoleIo
{
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public ConsoleIo(TextReader input, TextWriter output)
    {
        _input = input;
        _output = output;
    }

    public void WriteLine(string text = "") => _output.WriteLine(text);

    public string ReadLine(string prompt)
    {
        _output.Write(prompt);
        var line = _input.ReadLine() ?? throw new EndOfInputException();
        return line.Trim();
    }

    public string ReadNonEmpty(string prompt)
    {
        while (true)
        {
            var line = ReadLine(prompt);
            if (line.Length > 0)
            {
                return line;
            }

            WriteLine("  Значение не может быть пустым.");
        }
    }

    public int ReadInt(string prompt, int min, int max)
    {
        while (true)
        {
            var line = ReadLine(prompt);
            if (int.TryParse(line, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                && value >= min && value <= max)
            {
                return value;
            }

            WriteLine($"  Введите целое число от {min} до {max}.");
        }
    }

    /// <summary>Читает неотрицательное дробное число; принимает и точку, и запятую.</summary>
    public double ReadNonNegativeDouble(string prompt)
    {
        while (true)
        {
            var line = ReadLine(prompt).Replace(',', '.');
            if (double.TryParse(line, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                && double.IsFinite(value) && value >= 0)
            {
                return value;
            }

            WriteLine("  Введите неотрицательное число, например 0.8");
        }
    }
}
