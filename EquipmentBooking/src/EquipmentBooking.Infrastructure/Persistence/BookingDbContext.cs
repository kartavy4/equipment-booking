using EquipmentBooking.Application.Abstractions;
using EquipmentBooking.Application.Common.Errors;
using EquipmentBooking.Domain.Bookings;
using EquipmentBooking.Domain.Equipment;
using EquipmentBooking.Domain.Users;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EquipmentBooking.Infrastructure.Persistence;

public sealed class BookingDbContext(DbContextOptions<BookingDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<EquipmentItem> Equipment => Set<EquipmentItem>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingDbContext).Assembly);

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.ExclusionViolation,
                ConstraintName: "EX_Booking_NoOverlap"
            })
        {
            throw new AppException(ErrorType.Conflict, "Это время уже занято. Выберите другой интервал.", exception);
        }
        catch (DbUpdateException exception) when (IsDuplicateEmail(exception))
        {
            throw new AppException(ErrorType.Conflict, "Этот email уже зарегистрирован.", exception);
        }
    }

    private static bool IsDuplicateEmail(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_Users_Email"
        } ||
        exception.InnerException is SqliteException { SqliteExtendedErrorCode: 2067 } sqlite &&
        sqlite.Message.Contains("Users.Email", StringComparison.Ordinal);
}
