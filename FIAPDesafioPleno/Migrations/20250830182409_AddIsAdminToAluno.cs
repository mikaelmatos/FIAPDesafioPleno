using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FIAPDesafioPleno.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminToAluno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Alunos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Alunos");
        }
    }
}
