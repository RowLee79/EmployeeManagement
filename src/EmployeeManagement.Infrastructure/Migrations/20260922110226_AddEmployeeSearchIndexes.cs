using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeSearchIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmploymentType",
                table: "Employees",
                column: "EmploymentType");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_HireDate",
                table: "Employees",
                column: "HireDate");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Status",
                table: "Employees",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_EmploymentType",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_HireDate",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Status",
                table: "Employees");
        }
    }
}
