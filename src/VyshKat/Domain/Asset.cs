namespace VyshKat.Domain;

/// <summary>
/// Всё, что ставится на баланс «ВышКат»: транспорт и вещи.
/// Сотрудники сюда не входят — у них нет инвентарного номера.
/// </summary>
public abstract class Asset : IInventoried
{
    protected Asset(int inventoryNumber, string name)
    {
        if (inventoryNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inventoryNumber), $"Инвентарный номер должен быть положительным, получено {inventoryNumber}.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Наименование не может быть пустым.", nameof(name));
        }

        InventoryNumber = inventoryNumber;
        Name = name.Trim();
    }

    public int InventoryNumber { get; }

    public string Name { get; }

    /// <summary>Человекочитаемый вид объекта («Электросамокат», «Шлем»…).</summary>
    public abstract string Kind { get; }

    public override string ToString() => $"#{InventoryNumber} {Kind} «{Name}»";
}
