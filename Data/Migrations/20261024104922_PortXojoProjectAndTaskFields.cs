using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Data.Migrations
{
    /// <summary>
    /// Adds the fields of the Xojo TimeTracker3 data model.
    /// Hand-edited after scaffolding: EF Core guessed Name → HourlyRate as a rename (both are
    /// TEXT in SQLite) and gave the new flags a default of false. Existing projects keep their
    /// name as the new Code, and existing rows stay billable and productive.
    /// </summary>
    public partial class PortXojoProjectAndTaskFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Projects ---

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Projects",
                newName: "Code");

            migrationBuilder.AddColumn<string>(
                name: "Customer",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Projects",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProductive",
                table: "Projects",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            // --- ProjectTasks ---

            migrationBuilder.AddColumn<double>(
                name: "EstimatedHours",
                table: "ProjectTasks",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "ProjectTasks",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                table: "ProjectTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProductive",
                table: "ProjectTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EstimatedHours", table: "ProjectTasks");
            migrationBuilder.DropColumn(name: "HourlyRate",     table: "ProjectTasks");
            migrationBuilder.DropColumn(name: "IsBillable",     table: "ProjectTasks");
            migrationBuilder.DropColumn(name: "IsProductive",   table: "ProjectTasks");

            migrationBuilder.DropColumn(name: "Customer",     table: "Projects");
            migrationBuilder.DropColumn(name: "HourlyRate",   table: "Projects");
            migrationBuilder.DropColumn(name: "IsBillable",   table: "Projects");
            migrationBuilder.DropColumn(name: "IsProductive", table: "Projects");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "Projects",
                newName: "Name");
        }
    }
}
