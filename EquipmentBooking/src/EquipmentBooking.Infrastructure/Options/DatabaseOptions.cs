using System.ComponentModel.DataAnnotations;

namespace EquipmentBooking.Infrastructure.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    [RegularExpression("^(Postgres|Sqlite)$")]
    public string Provider { get; init; } = "Postgres";

    public bool InitializeOnStartup { get; init; } = true;
}
