using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WetterdatenImporter.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EnvironmentDaily",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GebietId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    EuropeanAqi = table.Column<double>(type: "REAL", nullable: true),
                    Pm10 = table.Column<double>(type: "REAL", nullable: true),
                    Pm2_5 = table.Column<double>(type: "REAL", nullable: true),
                    NitrogenDioxide = table.Column<double>(type: "REAL", nullable: true),
                    Ozone = table.Column<double>(type: "REAL", nullable: true),
                    AlderPollen = table.Column<double>(type: "REAL", nullable: true),
                    BirchPollen = table.Column<double>(type: "REAL", nullable: true),
                    GrassPollen = table.Column<double>(type: "REAL", nullable: true),
                    MugwortPollen = table.Column<double>(type: "REAL", nullable: true),
                    RagweedPollen = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnvironmentDaily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeasurementPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GebietId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeasurementPoint", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherForecastDaily",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GebietId = table.Column<int>(type: "INTEGER", nullable: false),
                    ForecastDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ForecastRunAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TemperatureMean = table.Column<double>(type: "REAL", nullable: true),
                    TemperatureMax = table.Column<double>(type: "REAL", nullable: true),
                    TemperatureMin = table.Column<double>(type: "REAL", nullable: true),
                    PrecipitationSum = table.Column<double>(type: "REAL", nullable: true),
                    WindSpeedMax = table.Column<double>(type: "REAL", nullable: true),
                    RelativeHumidityMean = table.Column<double>(type: "REAL", nullable: true),
                    WeatherCode = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherForecastDaily", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WeatherObservedDaily",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GebietId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TemperatureMean = table.Column<double>(type: "REAL", nullable: true),
                    TemperatureMax = table.Column<double>(type: "REAL", nullable: true),
                    TemperatureMin = table.Column<double>(type: "REAL", nullable: true),
                    PrecipitationSum = table.Column<double>(type: "REAL", nullable: true),
                    WindSpeedMax = table.Column<double>(type: "REAL", nullable: true),
                    RelativeHumidityMean = table.Column<double>(type: "REAL", nullable: true),
                    WeatherCode = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherObservedDaily", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EnvironmentDaily_GebietId_Date_Source",
                table: "EnvironmentDaily",
                columns: new[] { "GebietId", "Date", "Source" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MeasurementPoint_GebietId",
                table: "MeasurementPoint",
                column: "GebietId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherForecastDaily_GebietId_ForecastDate_Source_ForecastRunAt",
                table: "WeatherForecastDaily",
                columns: new[] { "GebietId", "ForecastDate", "Source", "ForecastRunAt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WeatherObservedDaily_GebietId_Date_Source",
                table: "WeatherObservedDaily",
                columns: new[] { "GebietId", "Date", "Source" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EnvironmentDaily");

            migrationBuilder.DropTable(
                name: "MeasurementPoint");

            migrationBuilder.DropTable(
                name: "WeatherForecastDaily");

            migrationBuilder.DropTable(
                name: "WeatherObservedDaily");
        }
    }
}
