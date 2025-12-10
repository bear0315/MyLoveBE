using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addtourmaxguid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TourDepartureId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TourDepartures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourId = table.Column<int>(type: "int", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxGuests = table.Column<int>(type: "int", nullable: false),
                    BookedGuests = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SpecialPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DefaultGuideId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourDepartures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TourDepartures_Guides_DefaultGuideId",
                        column: x => x.DefaultGuideId,
                        principalTable: "Guides",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TourDepartures_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_TourDepartureId",
                table: "Bookings",
                column: "TourDepartureId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDepartures_DefaultGuideId",
                table: "TourDepartures",
                column: "DefaultGuideId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDepartures_DepartureDate",
                table: "TourDepartures",
                column: "DepartureDate");

            migrationBuilder.CreateIndex(
                name: "IX_TourDepartures_Status",
                table: "TourDepartures",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TourDepartures_TourId",
                table: "TourDepartures",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_TourDepartures_TourId_DepartureDate",
                table: "TourDepartures",
                columns: new[] { "TourId", "DepartureDate" },
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_TourDepartures_TourDepartureId",
                table: "Bookings",
                column: "TourDepartureId",
                principalTable: "TourDepartures",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_TourDepartures_TourDepartureId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "TourDepartures");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_TourDepartureId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TourDepartureId",
                table: "Bookings");
        }
    }
}
