using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.ConsoleUi;
using Pos.Domain;
using Pos.Infrastructure;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
var arguments = args.ToList();
var complete = arguments.Remove("--complete");
var demo = arguments.Count > 0 && arguments[0] == "--demo";
if ((!demo && arguments.Count != 0) || (demo && arguments.Count > 2))
{
    Console.Error.WriteLine("Запуск: [--complete] [--demo [путь.csv]]");
    return 1;
}

var services = new ServiceCollection();
services.AddSingleton<ICourseRepository, InMemoryCourseRepository>();
services.AddSingleton<IResultExporter, CsvResultExporter>();
services.AddSingleton<IFinalScoreStrategy>(complete
    ? new CompleteScoreStrategy()
    : new WeightedScoreStrategy());
services.AddSingleton<FinalScoreService>();
services.AddSingleton<AssessmentFacade>();
services.AddSingleton<AssessmentConsole>();
using var provider = services.BuildServiceProvider();
var app = provider.GetRequiredService<AssessmentConsole>();
Console.WriteLine($"ПОС. Режим: {(complete ? "только полные оценки" : "пропуски считаются нулём")}.");
try
{
    if (demo)
        app.RunDemo(arguments.Count == 2 ? arguments[1] : "pos-results.csv");
    else
        app.Run();
    return 0;
}
catch (Exception exception) when (exception is DomainException or IOException or UnauthorizedAccessException or ArgumentException)
{
    Console.Error.WriteLine($"Ошибка: {exception.Message}");
    return 1;
}
