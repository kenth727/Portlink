using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PortlinkApp.Core.Migrations
{
    /// <inheritdoc />
    public partial class PortlinkInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Berths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BerthCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TerminalName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MaxVesselLength = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    MaxDraft = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Facilities = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Berths", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImoNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VesselType = table.Column<int>(type: "integer", nullable: false),
                    FlagCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LengthOverall = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Beam = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Draft = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    CargoType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    OwnerCompany = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AgentEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PortCalls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VesselId = table.Column<int>(type: "integer", nullable: false),
                    BerthId = table.Column<int>(type: "integer", nullable: false),
                    EstimatedTimeOfArrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EstimatedTimeOfDeparture = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActualTimeOfArrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActualTimeOfDeparture = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CargoDescription = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CargoQuantity = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    CargoUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DelayReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PriorityLevel = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PortCalls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PortCalls_Berths_BerthId",
                        column: x => x.BerthId,
                        principalTable: "Berths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PortCalls_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Berths_BerthCode",
                table: "Berths",
                column: "BerthCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_BerthId",
                table: "PortCalls",
                column: "BerthId");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_EstimatedTimeOfArrival",
                table: "PortCalls",
                column: "EstimatedTimeOfArrival");

            migrationBuilder.CreateIndex(
                name: "IX_PortCalls_VesselId_Status",
                table: "PortCalls",
                columns: new[] { "VesselId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_ImoNumber",
                table: "Vessels",
                column: "ImoNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PortCalls");

            migrationBuilder.DropTable(
                name: "Berths");

            migrationBuilder.DropTable(
                name: "Vessels");
        }
    }
}
