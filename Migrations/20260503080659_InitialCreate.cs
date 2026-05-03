using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace carpark_info_assignment.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CARPARK",
                columns: table => new
                {
                    car_park_no = table.Column<string>(type: "TEXT", nullable: false),
                    address = table.Column<string>(type: "TEXT", nullable: false),
                    x_coord = table.Column<float>(type: "REAL", nullable: false),
                    y_coord = table.Column<float>(type: "REAL", nullable: false),
                    short_term_start = table.Column<string>(type: "TEXT", nullable: false),
                    short_term_end = table.Column<string>(type: "TEXT", nullable: false),
                    night_parking = table.Column<bool>(type: "INTEGER", nullable: false),
                    decks = table.Column<int>(type: "INTEGER", nullable: false),
                    gantry_height = table.Column<float>(type: "REAL", nullable: false),
                    basement = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CARPARK", x => x.car_park_no);
                });

            migrationBuilder.CreateTable(
                name: "SYSTEM",
                columns: table => new
                {
                    system_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    system_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SYSTEM", x => x.system_id);
                });

            migrationBuilder.CreateTable(
                name: "TYPE",
                columns: table => new
                {
                    type_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    type_name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TYPE", x => x.type_id);
                });

            migrationBuilder.CreateTable(
                name: "FREE_PARK",
                columns: table => new
                {
                    free_park_instance_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    car_park_noFPcar_park_no = table.Column<string>(type: "TEXT", nullable: true),
                    day = table.Column<int>(type: "INTEGER", nullable: false),
                    start_time = table.Column<string>(type: "TEXT", nullable: false),
                    end_time = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FREE_PARK", x => x.free_park_instance_id);
                    table.ForeignKey(
                        name: "FK_FREE_PARK_CARPARK_car_park_noFPcar_park_no",
                        column: x => x.car_park_noFPcar_park_no,
                        principalTable: "CARPARK",
                        principalColumn: "car_park_no");
                });

            migrationBuilder.CreateTable(
                name: "CarParkSys",
                columns: table => new
                {
                    ParkingSystemssystem_id = table.Column<int>(type: "INTEGER", nullable: false),
                    system_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarParkSys", x => new { x.ParkingSystemssystem_id, x.system_id });
                    table.ForeignKey(
                        name: "FK_CarParkSys_CARPARK_system_id",
                        column: x => x.system_id,
                        principalTable: "CARPARK",
                        principalColumn: "car_park_no",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarParkSys_SYSTEM_ParkingSystemssystem_id",
                        column: x => x.ParkingSystemssystem_id,
                        principalTable: "SYSTEM",
                        principalColumn: "system_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarParkTyp",
                columns: table => new
                {
                    LotTypestype_id = table.Column<int>(type: "INTEGER", nullable: false),
                    type_id = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarParkTyp", x => new { x.LotTypestype_id, x.type_id });
                    table.ForeignKey(
                        name: "FK_CarParkTyp_CARPARK_type_id",
                        column: x => x.type_id,
                        principalTable: "CARPARK",
                        principalColumn: "car_park_no",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarParkTyp_TYPE_LotTypestype_id",
                        column: x => x.LotTypestype_id,
                        principalTable: "TYPE",
                        principalColumn: "type_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarParkSys_system_id",
                table: "CarParkSys",
                column: "system_id");

            migrationBuilder.CreateIndex(
                name: "IX_CarParkTyp_type_id",
                table: "CarParkTyp",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_FREE_PARK_car_park_noFPcar_park_no",
                table: "FREE_PARK",
                column: "car_park_noFPcar_park_no");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarParkSys");

            migrationBuilder.DropTable(
                name: "CarParkTyp");

            migrationBuilder.DropTable(
                name: "FREE_PARK");

            migrationBuilder.DropTable(
                name: "SYSTEM");

            migrationBuilder.DropTable(
                name: "TYPE");

            migrationBuilder.DropTable(
                name: "CARPARK");
        }
    }
}
