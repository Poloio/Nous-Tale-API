using Microsoft.EntityFrameworkCore.Migrations;

namespace Nous_Tale_API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxPlayers = table.Column<int>(type: "int", nullable: false),
                    ReadyCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHost = table.Column<bool>(type: "bit", nullable: false),
                    Emoji = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Players_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tales",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tales", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tales_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chapters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaleID = table.Column<int>(type: "int", nullable: false),
                    PlayerID = table.Column<int>(type: "int", nullable: true),
                    OrderNo = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chapters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Chapters_Players_PlayerID",
                        column: x => x.PlayerID,
                        principalTable: "Players",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Chapters_Tales_TaleID",
                        column: x => x.TaleID,
                        principalTable: "Tales",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_PlayerID",
                table: "Chapters",
                column: "PlayerID");

            migrationBuilder.CreateIndex(
                name: "IX_Chapters_TaleID",
                table: "Chapters",
                column: "TaleID");

            migrationBuilder.CreateIndex(
                name: "IX_Players_RoomID",
                table: "Players",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Tales_RoomID",
                table: "Tales",
                column: "RoomID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Chapters");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Tales");

            migrationBuilder.DropTable(
                name: "Rooms");
        }
    }
}
