
using Microsoft.EntityFrameworkCore.Migrations;
namespace EquipmentBooking.Infrastructure.Persistence.Migrations;


public partial class Initial : Migration
{
    protected override void Up(MigrationBuilder m) => m.Sql("""
        CREATE EXTENSION IF NOT EXISTS btree_gist;
        CREATE TABLE "Users" (
            "Id" uuid PRIMARY KEY, "Name" varchar(80) NOT NULL, "Email" varchar(200) NOT NULL,
            "PasswordHash" text NOT NULL, "Role" text NOT NULL);
        CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");
        CREATE TABLE "Equipment" (
            "Id" uuid PRIMARY KEY, "Name" varchar(120) NOT NULL, "Category" varchar(60) NOT NULL,
            "Description" varchar(1000) NOT NULL, "IsActive" boolean NOT NULL);
        CREATE TABLE "Bookings" (
            "Id" uuid PRIMARY KEY, "EquipmentId" uuid NOT NULL REFERENCES "Equipment" ("Id") ON DELETE RESTRICT,
            "UserId" uuid NOT NULL REFERENCES "Users" ("Id") ON DELETE RESTRICT,
            "StartsAt" timestamptz NOT NULL, "EndsAt" timestamptz NOT NULL, "IsCancelled" boolean NOT NULL,
            CONSTRAINT "CK_Booking_Interval" CHECK ("EndsAt" > "StartsAt"),
            CONSTRAINT "EX_Booking_NoOverlap" EXCLUDE USING gist
                ("EquipmentId" WITH =, tstzrange("StartsAt", "EndsAt", '[)') WITH &&)
                WHERE (NOT "IsCancelled"));
        CREATE INDEX "IX_Bookings_EquipmentId_StartsAt_EndsAt" ON "Bookings" ("EquipmentId", "StartsAt", "EndsAt");
        CREATE INDEX "IX_Bookings_UserId" ON "Bookings" ("UserId");
        """);
    protected override void Down(MigrationBuilder m) => m.Sql("DROP TABLE \"Bookings\"; DROP TABLE \"Equipment\"; DROP TABLE \"Users\";");
}
