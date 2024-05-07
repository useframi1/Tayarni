using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace API.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class PostgresInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airlines",
                columns: table => new
                {
                    AirlineName = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    UniqueCarrier = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Airlines__E6B3FF59897A8205", x => x.AirlineName);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Airport_Name = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Airport_Code = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Airports__34A7AB6E169FA657", x => x.Airport_Name);
                });

            migrationBuilder.CreateTable(
                name: "Tail_Numbers",
                columns: table => new
                {
                    TailNum = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Tail_Num__90AA504830AF2851", x => x.TailNum);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Username = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Users__536C85E50075D7BD", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "Training",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    AirlineName = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    TailNum = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    ScheduledDepTime = table.Column<int>(type: "integer", nullable: false),
                    ScheduledArrTime = table.Column<int>(type: "integer", nullable: false),
                    Org_Airport = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Dest_Airport = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    DepTemperature = table.Column<double>(type: "double precision", nullable: false),
                    DepWindSpeed = table.Column<double>(type: "double precision", nullable: false),
                    DepWindDirection = table.Column<double>(type: "double precision", nullable: false),
                    DepPrecipitation = table.Column<double>(type: "double precision", nullable: false),
                    DepRain = table.Column<double>(type: "double precision", nullable: false),
                    DepSnowFall = table.Column<double>(type: "double precision", nullable: false),
                    ArrTemperature = table.Column<double>(type: "double precision", nullable: false),
                    ArrWindSpeed = table.Column<double>(type: "double precision", nullable: false),
                    ArrWindDirection = table.Column<double>(type: "double precision", nullable: false),
                    ArrPrecipitation = table.Column<double>(type: "double precision", nullable: false),
                    ArrRain = table.Column<double>(type: "double precision", nullable: false),
                    ArrSnowFall = table.Column<double>(type: "double precision", nullable: false),
                    IsDelayed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Training__39DE829EBC998803", x => new { x.ScheduledDepTime, x.AirlineName, x.TailNum, x.Date });
                    table.ForeignKey(
                        name: "FK__Training__Airlin__40F9A68C",
                        column: x => x.AirlineName,
                        principalTable: "Airlines",
                        principalColumn: "AirlineName");
                    table.ForeignKey(
                        name: "FK__Training__Dest_A__43D61337",
                        column: x => x.Dest_Airport,
                        principalTable: "Airports",
                        principalColumn: "Airport_Name");
                    table.ForeignKey(
                        name: "FK__Training__Org_Ai__42E1EEFE",
                        column: x => x.Org_Airport,
                        principalTable: "Airports",
                        principalColumn: "Airport_Name");
                    table.ForeignKey(
                        name: "FK__Training__TailNu__41EDCAC5",
                        column: x => x.TailNum,
                        principalTable: "Tail_Numbers",
                        principalColumn: "TailNum");
                });

            migrationBuilder.CreateTable(
                name: "User_Predictions",
                columns: table => new
                {
                    Username = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ScheduledDepTime = table.Column<int>(type: "integer", nullable: false),
                    ScheduledArrTime = table.Column<int>(type: "integer", nullable: false),
                    AirlineName = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    TailNum = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Org_Airport = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Dest_Airport = table.Column<string>(type: "character varying(255)", unicode: false, maxLength: 255, nullable: false),
                    Distance = table.Column<int>(type: "integer", nullable: false),
                    DepTemperature = table.Column<double>(type: "double precision", nullable: false),
                    DepWindSpeed = table.Column<double>(type: "double precision", nullable: false),
                    DepWindDirection = table.Column<double>(type: "double precision", nullable: false),
                    DepPrecipitation = table.Column<double>(type: "double precision", nullable: false),
                    DepRain = table.Column<double>(type: "double precision", nullable: false),
                    DepSnowFall = table.Column<double>(type: "double precision", nullable: false),
                    ArrTemperature = table.Column<double>(type: "double precision", nullable: false),
                    ArrWindSpeed = table.Column<double>(type: "double precision", nullable: false),
                    ArrWindDirection = table.Column<double>(type: "double precision", nullable: false),
                    ArrPrecipitation = table.Column<double>(type: "double precision", nullable: false),
                    ArrRain = table.Column<double>(type: "double precision", nullable: false),
                    ArrSnowFall = table.Column<double>(type: "double precision", nullable: false),
                    IsDelayedPredicted = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelayedActual = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User_Pre__86FE9BD31D1DBDAC", x => new { x.Username, x.ScheduledDepTime, x.Date });
                    table.ForeignKey(
                        name: "FK__User_Pred__Airli__634EBE90",
                        column: x => x.AirlineName,
                        principalTable: "Airlines",
                        principalColumn: "AirlineName");
                    table.ForeignKey(
                        name: "FK__User_Pred__Dest___662B2B3B",
                        column: x => x.Dest_Airport,
                        principalTable: "Airports",
                        principalColumn: "Airport_Name");
                    table.ForeignKey(
                        name: "FK__User_Pred__Org_A__65370702",
                        column: x => x.Org_Airport,
                        principalTable: "Airports",
                        principalColumn: "Airport_Name");
                    table.ForeignKey(
                        name: "FK__User_Pred__TailN__6442E2C9",
                        column: x => x.TailNum,
                        principalTable: "Tail_Numbers",
                        principalColumn: "TailNum");
                    table.ForeignKey(
                        name: "FK__User_Pred__Usern__625A9A57",
                        column: x => x.Username,
                        principalTable: "Users",
                        principalColumn: "Username");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Training_AirlineName",
                table: "Training",
                column: "AirlineName");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Dest_Airport",
                table: "Training",
                column: "Dest_Airport");

            migrationBuilder.CreateIndex(
                name: "IX_Training_Org_Airport",
                table: "Training",
                column: "Org_Airport");

            migrationBuilder.CreateIndex(
                name: "IX_Training_TailNum",
                table: "Training",
                column: "TailNum");

            migrationBuilder.CreateIndex(
                name: "IX_User_Predictions_AirlineName",
                table: "User_Predictions",
                column: "AirlineName");

            migrationBuilder.CreateIndex(
                name: "IX_User_Predictions_Dest_Airport",
                table: "User_Predictions",
                column: "Dest_Airport");

            migrationBuilder.CreateIndex(
                name: "IX_User_Predictions_Org_Airport",
                table: "User_Predictions",
                column: "Org_Airport");

            migrationBuilder.CreateIndex(
                name: "IX_User_Predictions_TailNum",
                table: "User_Predictions",
                column: "TailNum");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Training");

            migrationBuilder.DropTable(
                name: "User_Predictions");

            migrationBuilder.DropTable(
                name: "Airlines");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "Tail_Numbers");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
