using Microsoft.Extensions.DependencyInjection;
using VyshKat.Composition;
using VyshKat.ConsoleUi;
using VyshKat.Services;

namespace VyshKat.Tests;

/// <summary>Сценарные тесты: консольный ввод подаётся строкой, вывод проверяется целиком.</summary>
public class ConsoleAppTests
{
    private static (string Output, Fleet Fleet) Run(string input, IServiceCenter? serviceCenter = null, bool demo = false)
    {
        var output = new StringWriter();
        var services = new ServiceCollection().AddVyshKat();
        services.AddSingleton<TextReader>(new StringReader(input));
        services.AddSingleton<TextWriter>(output);
        if (serviceCenter is not null)
        {
            services.AddSingleton(serviceCenter);
        }

        using var provider = services.BuildServiceProvider();
        var app = provider.GetRequiredService<FleetConsoleApp>();
        if (demo)
        {
            app.RunDemo();
        }
        else
        {
            app.Run();
        }

        return (output.ToString(), provider.GetRequiredService<Fleet>());
    }

    private static string Lines(params string[] lines) => string.Join(Environment.NewLine, lines) + Environment.NewLine;

    [Fact]
    public void Demo_ShowsAllScenarios()
    {
        var (output, fleet) = Run("", demo: true);

        Assert.Contains("[ПРИНЯТ]", output);
        Assert.Contains("[ОТКЛОНЁН]", output);
        Assert.Contains("износ", output);
        Assert.Contains("аномальный расход", output);
        Assert.Contains("уже занят", output);
        Assert.Contains("[НА БАЛАНСЕ]", output);
        Assert.Contains("=== Для новичков (простота ≥ 6) ===", output);
        Assert.Contains("Всего единиц:               8", output);
        Assert.Equal(4, fleet.Vehicles.Count());
        Assert.Equal(4, fleet.Equipment.Count());
    }

    [Fact]
    public void Menu_DemoTwice_DoesNotDuplicate()
    {
        var (output, fleet) = Run(Lines("6", "6", "0"));

        Assert.Contains("Демо-данные уже загружены.", output);
        Assert.Equal(8, fleet.Assets.Count);
    }

    [Fact]
    public void Menu_AcceptElectricScooter_WithInvalidInputRetries()
    {
        var input = Lines(
            "1",        // принять транспорт
            "9", "1",   // неверный вид, затем «Электросамокат»
            "abc", "500",
            "",  "Ninebot",
            "11", "7",
            "90",
            "-1", "0,8",
            "0");

        var (output, fleet) = Run(input);

        Assert.Contains("Введите целое число от 1 до 3.", output);
        Assert.Contains("Значение не может быть пустым.", output);
        Assert.Contains("Введите неотрицательное число", output);
        Assert.Contains("[ПРИНЯТ]    #500 Электросамокат «Ninebot»", output);
        var scooter = Assert.IsType<VyshKat.Domain.Scooter>(Assert.Single(fleet.Vehicles));
        Assert.Equal(0.8, scooter.DailyEnergyKwh, precision: 6);
    }

    [Fact]
    public void Menu_AcceptBicycle_DoesNotAskForEnergy_AndUsesInjectedServiceCenter()
    {
        var input = Lines("1", "2", "77", "Stels", "9", "100", "0");

        var (output, fleet) = Run(input, new FakeServiceCenter(approve: false));

        Assert.DoesNotContain("кВт·ч: ", output);
        Assert.Contains("[ОТКЛОНЁН]  #77 Велосипед «Stels»: fake: отказ", output);
        Assert.Empty(fleet.Assets);
    }

    [Fact]
    public void Menu_AddEquipment_AndShowReports()
    {
        var input = Lines(
            "2", "1", "201", "Шлем M",
            "2", "3", "301", "Шкаф", "5",
            "2", "1", "201", "Дубль",
            "3", "4", "5", "0");

        var (output, fleet) = Run(input);

        Assert.Contains("[НА БАЛАНСЕ] #201 Шлем «Шлем M»", output);
        Assert.Contains("[НА БАЛАНСЕ] #301 Зарядный шкаф «Шкаф»", output);
        Assert.Contains("[ОШИБКА]    Инвентарный номер 201 уже занят", output);
        Assert.Contains("Вещей на балансе:           2", output);
        Assert.Contains("Подходящего транспорта нет.", output);
        Assert.Contains("301  Зарядный шкаф", output);
        Assert.Equal(2, fleet.Equipment.Count());
    }

    [Fact]
    public void Menu_EmptyFleetReports_AndUnknownCommand()
    {
        var (output, _) = Run(Lines("5", "42", "0"));

        Assert.Contains("На балансе ничего нет.", output);
        Assert.Contains("Нет такого пункта меню.", output);
        Assert.EndsWith("До встречи!" + Environment.NewLine, output);
    }

    [Fact]
    public void EndOfInput_InTheMiddleOfDialog_ExitsGracefully()
    {
        var (output, fleet) = Run(Lines("1", "1", "100"));

        Assert.Contains("До встречи!", output);
        Assert.Empty(fleet.Assets);
    }
}
