using VyshKat.Domain;
using VyshKat.Services;

namespace VyshKat.ConsoleUi;

/// <summary>Консольное меню учёта парка. Только ввод/вывод — бизнес-правила живут в сервисах.</summary>
public sealed class FleetConsoleApp
{
    private readonly Fleet _fleet;
    private readonly FleetReportService _reports;
    private readonly IReadOnlyList<VehicleKind> _vehicleKinds;
    private readonly IReadOnlyList<EquipmentKind> _equipmentKinds;
    private readonly ConsoleIo _io;
    private bool _demoLoaded;

    public FleetConsoleApp(
        Fleet fleet,
        FleetReportService reports,
        IEnumerable<VehicleKind> vehicleKinds,
        IEnumerable<EquipmentKind> equipmentKinds,
        ConsoleIo io)
    {
        _fleet = fleet;
        _reports = reports;
        _vehicleKinds = vehicleKinds.ToList();
        _equipmentKinds = equipmentKinds.ToList();
        _io = io;
    }

    public void Run()
    {
        _io.WriteLine("ВышКат — учёт парка. «Доедем до дедлайна»");

        try
        {
            while (true)
            {
                PrintMenu();
                var choice = _io.ReadLine("> ");
                if (choice == "0")
                {
                    break;
                }

                Execute(choice);
            }
        }
        catch (EndOfInputException)
        {
            // Ввод закончился — штатный выход.
        }

        _io.WriteLine("До встречи!");
    }

    /// <summary>Загружает пилотный парк и прогоняет все отчёты — все сценарии без ручного ввода.</summary>
    public void RunDemo()
    {
        _io.WriteLine("=== Демо-сценарий ===");
        if (_demoLoaded)
        {
            _io.WriteLine("Демо-данные уже загружены.");
            return;
        }

        _demoLoaded = true;

        _io.WriteLine("-- Приёмка транспорта через техосмотр --");
        foreach (var vehicle in DemoData.Vehicles())
        {
            TryAccept(vehicle);
        }

        _io.WriteLine("-- Повторный инвентарный номер --");
        TryAccept(DemoData.Duplicate());

        _io.WriteLine("-- Некорректные данные --");
        TryRun(() => new Scooter(107, "Самокат-мечта", simplicity: 11, conditionPercent: 100, dailyEnergyKwh: 0.5));

        _io.WriteLine("-- Постановка вещей на баланс --");
        foreach (var equipment in DemoData.Equipment())
        {
            TryRun(() => AddEquipment(equipment));
        }

        _io.WriteLine();
        PrintSummary();
        PrintBeginnerVehicles();
        PrintInventory();
    }

    private void PrintMenu()
    {
        _io.WriteLine();
        _io.WriteLine("1. Принять транспорт (с техосмотром)");
        _io.WriteLine("2. Поставить вещь на баланс");
        _io.WriteLine("3. Отчёт: количество и суммарный расход энергии");
        _io.WriteLine("4. Транспорт для новичков");
        _io.WriteLine("5. Инвентарная ведомость");
        _io.WriteLine("6. Демо-сценарий");
        _io.WriteLine("0. Выход");
    }

    private void Execute(string choice)
    {
        switch (choice)
        {
            case "1": TryRun(AcceptVehicleInteractive); break;
            case "2": TryRun(AddEquipmentInteractive); break;
            case "3": PrintSummary(); break;
            case "4": PrintBeginnerVehicles(); break;
            case "5": PrintInventory(); break;
            case "6": RunDemo(); break;
            default: _io.WriteLine("Нет такого пункта меню."); break;
        }
    }

    private void AcceptVehicleInteractive()
    {
        var kind = Choose("Вид транспорта", _vehicleKinds, k => k.Title);
        var number = _io.ReadInt("Инвентарный номер: ", 1, int.MaxValue);
        var name = _io.ReadNonEmpty("Наименование (модель): ");
        var simplicity = _io.ReadInt(
            $"Простота для новичка ({Vehicle.MinSimplicity}–{Vehicle.MaxSimplicity}): ",
            Vehicle.MinSimplicity, Vehicle.MaxSimplicity);
        var condition = _io.ReadInt("Техническое состояние, % (0–100): ", 0, 100);
        var energy = kind.IsElectric ? _io.ReadNonNegativeDouble("Суточный расход, кВт·ч: ") : 0;

        TryAccept(kind.Create(new VehicleSpec(number, name, simplicity, condition, energy)));
    }

    private void AddEquipmentInteractive()
    {
        var kind = Choose("Вид вещи", _equipmentKinds, k => k.Title);
        var number = _io.ReadInt("Инвентарный номер: ", 1, int.MaxValue);
        var name = _io.ReadNonEmpty("Наименование: ");
        var energy = kind.IsPowered ? _io.ReadNonNegativeDouble("Суточный расход, кВт·ч: ") : 0;

        AddEquipment(kind.Create(new EquipmentSpec(number, name, energy)));
    }

    private T Choose<T>(string title, IReadOnlyList<T> options, Func<T, string> label)
    {
        _io.WriteLine($"{title}:");
        for (var i = 0; i < options.Count; i++)
        {
            _io.WriteLine($"  {i + 1}. {label(options[i])}");
        }

        return options[_io.ReadInt("Выбор: ", 1, options.Count) - 1];
    }

    private void TryAccept(Vehicle vehicle) => TryRun(() =>
    {
        var result = _fleet.AcceptVehicle(vehicle);
        _io.WriteLine(result.Approved
            ? $"[ПРИНЯТ]    {vehicle}: {result.Reason}"
            : $"[ОТКЛОНЁН]  {vehicle}: {result.Reason}");
    });

    private void AddEquipment(Equipment equipment)
    {
        _fleet.AddEquipment(equipment);
        _io.WriteLine($"[НА БАЛАНСЕ] {equipment}");
    }

    /// <summary>Ошибки ввода и бизнес-правил показываем пользователю, а не роняем приложение.</summary>
    private void TryRun(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            _io.WriteLine($"[ОШИБКА]    {ex.Message}");
        }
    }

    private void PrintSummary()
    {
        var summary = _reports.GetSummary();
        _io.WriteLine("=== Отчёт по парку ===");
        _io.WriteLine($"Транспорт в активном парке: {summary.VehicleCount}");
        _io.WriteLine($"Вещей на балансе:           {summary.EquipmentCount}");
        _io.WriteLine($"Всего единиц:               {summary.TotalCount}");
        _io.WriteLine($"Суммарный расход энергии:   {summary.TotalDailyEnergyKwh:0.##} кВт·ч/сут");
    }

    private void PrintBeginnerVehicles()
    {
        var vehicles = _reports.GetBeginnerFriendlyVehicles();
        _io.WriteLine($"=== Для новичков ({_reports.BeginnerRule}) ===");
        if (vehicles.Count == 0)
        {
            _io.WriteLine("Подходящего транспорта нет.");
            return;
        }

        foreach (var vehicle in vehicles)
        {
            _io.WriteLine($"{vehicle}, простота {vehicle.Simplicity}");
        }
    }

    private void PrintInventory()
    {
        var assets = _reports.GetInventoryList();
        _io.WriteLine("=== Инвентарная ведомость ===");
        if (assets.Count == 0)
        {
            _io.WriteLine("На балансе ничего нет.");
            return;
        }

        foreach (var asset in assets)
        {
            _io.WriteLine($"{asset.InventoryNumber,6}  {asset.Kind,-17} {asset.Name}");
        }
    }
}
