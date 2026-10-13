using EquipmentBooking.Domain.Common;

namespace EquipmentBooking.Domain.Equipment;

public sealed class EquipmentItem
{
    private EquipmentItem() { }

    public EquipmentItem(string name, string category, string description)
    {
        Id = Guid.NewGuid();
        Update(name, category, description, true);
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public void Update(string name, string category, string description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new DomainRuleException("Название обязательно и должно быть не длиннее 120 символов.");
        }

        if (string.IsNullOrWhiteSpace(category) || category.Trim().Length > 60)
        {
            throw new DomainRuleException("Категория обязательна и должна быть не длиннее 60 символов.");
        }

        if (description.Length > 1000)
        {
            throw new DomainRuleException("Описание должно быть не длиннее 1000 символов.");
        }

        Name = name.Trim();
        Category = category.Trim();
        Description = description.Trim();
        IsActive = isActive;
    }
}
