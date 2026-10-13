using EquipmentBooking.Api.Extensions;
using EquipmentBooking.Application;
using EquipmentBooking.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddApi();

var app = builder.Build();

app.UseApiPipeline();
await app.InitializeDatabaseAsync();

app.Run();

public partial class Program;
