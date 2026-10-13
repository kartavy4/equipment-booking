FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props NuGet.Config ./
COPY src/EquipmentBooking.Domain/EquipmentBooking.Domain.csproj src/EquipmentBooking.Domain/
COPY src/EquipmentBooking.Application/EquipmentBooking.Application.csproj src/EquipmentBooking.Application/
COPY src/EquipmentBooking.Infrastructure/EquipmentBooking.Infrastructure.csproj src/EquipmentBooking.Infrastructure/
COPY src/EquipmentBooking.Api/EquipmentBooking.Api.csproj src/EquipmentBooking.Api/
RUN dotnet restore src/EquipmentBooking.Api
COPY src/ src/
RUN dotnet publish src/EquipmentBooking.Api -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "EquipmentBooking.Api.dll"]
