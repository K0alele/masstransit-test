using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MassTransitTest.Migrations
{
    /// <inheritdoc />
    public partial class AddProcessFileSaga : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileSaga",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    PrevSequece = table.Column<long>(type: "bigint", nullable: false),
                    FileSent = table.Column<bool>(type: "bit", nullable: false),
                    WorkflowCreated = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("FileSagaData", x => x.CorrelationId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileSaga");
        }
    }
}
