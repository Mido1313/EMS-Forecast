using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class V1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    DistrictName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IncidentTypeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SeverityMean = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    SeverityMin = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    SeverityMax = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LocationTypeName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSchoolBreak = table.Column<bool>(type: "boolean", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicHolidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccidentHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    AreaId = table.Column<int>(type: "integer", nullable: true),
                    PostalCodeCount = table.Column<int>(type: "integer", nullable: true),
                    DistrictBasis = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    RatePer10000Weighted = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    PopulationEstimated = table.Column<int>(type: "integer", nullable: true),
                    AccidentsTotalEstimated = table.Column<int>(type: "integer", nullable: true),
                    HotspotFactor = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    AccidentsHotspotAdjusted = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    InjuredEstimated = table.Column<int>(type: "integer", nullable: true),
                    FatalitiesEstimated = table.Column<int>(type: "integer", nullable: true),
                    ReferenceYear = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccidentHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccidentHistories_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PostalCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Plz = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    CityName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostalCodes", x => x.Id);
                    table.UniqueConstraint("AK_PostalCodes_Plz", x => x.Plz);
                    table.ForeignKey(
                        name: "FK_PostalCodes_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    WindowFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    WindowTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RiskScore = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    RiskLevel = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ScoreTraffic = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    ScoreAccident = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    ScoreWeather = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    ScoreHoliday = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    ScoreEvent = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    Explanation = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModelVersion = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Confidence = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Results_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrafficHotspots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LinkId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    HotspotName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RoadType = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CriticalityWeight = table.Column<decimal>(type: "numeric(8,4)", nullable: true),
                    FreeFlowSpeedKph = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    LengthKm = table.Column<decimal>(type: "numeric(8,3)", nullable: true),
                    IsTouristic = table.Column<bool>(type: "boolean", nullable: true),
                    IsCommuter = table.Column<bool>(type: "boolean", nullable: true),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficHotspots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficHotspots_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Attractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostalCodeId = table.Column<string>(type: "character varying(10)", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RiskSummer = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    RiskWinter = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attractions_PostalCodes_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalTable: "PostalCodes",
                        principalColumn: "Plz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostalCodeId = table.Column<string>(type: "character varying(10)", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpectedVisitors = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Events_PostalCodes_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalTable: "PostalCodes",
                        principalColumn: "Plz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Municipalities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostalCodeId = table.Column<string>(type: "character varying(10)", nullable: false),
                    MunicipalityName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Municipalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Municipalities_PostalCodes_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalTable: "PostalCodes",
                        principalColumn: "Plz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NursingHomes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostalCodeId = table.Column<string>(type: "character varying(10)", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    BedCount = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NursingHomes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NursingHomes_PostalCodes_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalTable: "PostalCodes",
                        principalColumn: "Plz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Weathers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostalCodeId = table.Column<string>(type: "character varying(10)", nullable: false),
                    MeasurementDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Precipitation = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Snow = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    WindSpeed = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    AirQuality = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    PollenTotal = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    PollenBirch = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    PollenGrass = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    ParticulateMatter = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    Visibility = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    IsForecast = table.Column<bool>(type: "boolean", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Weathers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Weathers_PostalCodes_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalTable: "PostalCodes",
                        principalColumn: "Plz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrafficAccidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    SegmentId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficAccidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficAccidents_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrafficAccidents_TrafficHotspots_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "TrafficHotspots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrafficConstructions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    SegmentId = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrafficConstructions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrafficConstructions_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrafficConstructions_TrafficHotspots_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "TrafficHotspots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Traffics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SegmentId = table.Column<int>(type: "integer", nullable: false),
                    DistrictId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AverageVehicleSpeed = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    TravelTime = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    TrafficStatus = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traffics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Traffics_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Traffics_TrafficHotspots_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "TrafficHotspots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PostalCodeId = table.Column<string>(type: "character varying(10)", nullable: false),
                    LocationTypeId = table.Column<int>(type: "integer", nullable: false),
                    IncidentTypeId = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    AttractionId = table.Column<int>(type: "integer", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidents_Attractions_AttractionId",
                        column: x => x.AttractionId,
                        principalTable: "Attractions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_IncidentTypes_IncidentTypeId",
                        column: x => x.IncidentTypeId,
                        principalTable: "IncidentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_LocationTypes_LocationTypeId",
                        column: x => x.LocationTypeId,
                        principalTable: "LocationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Incidents_PostalCodes_PostalCodeId",
                        column: x => x.PostalCodeId,
                        principalTable: "PostalCodes",
                        principalColumn: "Plz",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Populations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MunicipalityId = table.Column<int>(type: "integer", nullable: false),
                    ResidentCount = table.Column<int>(type: "integer", nullable: false),
                    AgeStructure = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Populations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Populations_Municipalities_MunicipalityId",
                        column: x => x.MunicipalityId,
                        principalTable: "Municipalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccidentHistories_DistrictId",
                table: "AccidentHistories",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Attractions_PostalCodeId",
                table: "Attractions",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DistrictId",
                table: "Districts",
                column: "DistrictId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DistrictName",
                table: "Districts",
                column: "DistrictName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_PostalCodeId",
                table: "Events",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_AttractionId",
                table: "Incidents",
                column: "AttractionId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_IncidentTypeId",
                table: "Incidents",
                column: "IncidentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_LocationTypeId",
                table: "Incidents",
                column: "LocationTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_PostalCodeId",
                table: "Incidents",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentTypes_IncidentTypeName",
                table: "IncidentTypes",
                column: "IncidentTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationTypes_LocationTypeName",
                table: "LocationTypes",
                column: "LocationTypeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Municipalities_PostalCodeId",
                table: "Municipalities",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_NursingHomes_PostalCodeId",
                table: "NursingHomes",
                column: "PostalCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Populations_MunicipalityId",
                table: "Populations",
                column: "MunicipalityId");

            migrationBuilder.CreateIndex(
                name: "IX_PostalCodes_DistrictId",
                table: "PostalCodes",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_PostalCodes_Plz",
                table: "PostalCodes",
                column: "Plz",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Results_DistrictId",
                table: "Results",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficAccidents_DistrictId",
                table: "TrafficAccidents",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficAccidents_SegmentId",
                table: "TrafficAccidents",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficConstructions_DistrictId",
                table: "TrafficConstructions",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficConstructions_SegmentId",
                table: "TrafficConstructions",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficHotspots_DistrictId",
                table: "TrafficHotspots",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_TrafficHotspots_LinkId",
                table: "TrafficHotspots",
                column: "LinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Traffics_DistrictId",
                table: "Traffics",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Traffics_SegmentId",
                table: "Traffics",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Weathers_PostalCodeId",
                table: "Weathers",
                column: "PostalCodeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccidentHistories");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "NursingHomes");

            migrationBuilder.DropTable(
                name: "Populations");

            migrationBuilder.DropTable(
                name: "PublicHolidays");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "TrafficAccidents");

            migrationBuilder.DropTable(
                name: "TrafficConstructions");

            migrationBuilder.DropTable(
                name: "Traffics");

            migrationBuilder.DropTable(
                name: "Weathers");

            migrationBuilder.DropTable(
                name: "Attractions");

            migrationBuilder.DropTable(
                name: "IncidentTypes");

            migrationBuilder.DropTable(
                name: "LocationTypes");

            migrationBuilder.DropTable(
                name: "Municipalities");

            migrationBuilder.DropTable(
                name: "TrafficHotspots");

            migrationBuilder.DropTable(
                name: "PostalCodes");

            migrationBuilder.DropTable(
                name: "Districts");
        }
    }
}
