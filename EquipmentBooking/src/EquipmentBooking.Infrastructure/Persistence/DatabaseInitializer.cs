using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Domain.Equipment;
using EquipmentBooking.Domain.Users;
using EquipmentBooking.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EquipmentBooking.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    BookingDbContext db,
    IPasswordService passwords,
    IOptions<DatabaseOptions> databaseOptions,
    IOptions<SeedOptions> seedOptions)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (!databaseOptions.Value.InitializeOnStartup)
        {
            return;
        }

        if (db.Database.IsNpgsql())
        {
            await db.Database.MigrateAsync(cancellationToken);
        }
        else
        {
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        await SeedEquipmentAsync(cancellationToken);
        await SeedAdminAsync(cancellationToken);
    }

    private async Task SeedEquipmentAsync(CancellationToken cancellationToken)
    {
        if (await db.Equipment.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Equipment.AddRange(
            new EquipmentItem("Sony Alpha A7 IV", "Камеры", "Полнокадровая камера для интервью и контента. Объектив 24–70 мм в комплекте."),
            new EquipmentItem("Epson EB-FH52", "Презентации", "Full HD проектор для встреч. HDMI-кабель и пульт в комплекте."),
            new EquipmentItem("Rode Wireless GO II", "Аудио", "Два беспроводных микрофона для интервью и подкастов."),
            new EquipmentItem("Wacom Intuos Pro", "Дизайн", "Графический планшет размера M с пером для иллюстраций и ретуши."));

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedAdminAsync(CancellationToken cancellationToken)
    {
        var options = seedOptions.Value;

        if (string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            return;
        }

        var email = options.AdminEmail.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            return;
        }

        var admin = new AppUser("Администратор", email, Roles.Admin);
        admin.SetPasswordHash(passwords.Hash(admin, options.AdminPassword));
        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}
