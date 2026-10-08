using Microsoft.EntityFrameworkCore.Migrations;
namespace EmployeeManagement.Infrastructure.Migrations;
public partial class RepairEmployeeRelationships : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Fail rather than silently delete duplicate business records.
        migrationBuilder.Sql("""
            IF EXISTS (SELECT 1 FROM Attendances WHERE IsDeleted = 0
                GROUP BY EmployeeId, AttendanceDate HAVING COUNT(*) > 1)
                THROW 51000, 'Resolve duplicate active attendance records before migration.', 1;
            IF EXISTS (SELECT 1 FROM LeaveBalances WHERE IsDeleted = 0
                GROUP BY EmployeeId, Year, LeaveType HAVING COUNT(*) > 1)
                THROW 51001, 'Resolve duplicate active leave balances before migration.', 1;
            IF EXISTS (SELECT 1 FROM Payrolls WHERE IsDeleted = 0 AND PayrollPeriodId IS NOT NULL
                GROUP BY EmployeeId, PayrollPeriodId HAVING COUNT(*) > 1)
                THROW 51002, 'Resolve duplicate active payrolls per period before migration.', 1;
            """);
        foreach (var column in new[] { "DepartmentId1", "PositionId1" })
        {
            var table = column == "DepartmentId1" ? "Departments" : "Positions";
            migrationBuilder.DropForeignKey($"FK_Employees_{table}_{column}", "Employees");
            migrationBuilder.DropIndex($"IX_Employees_{column}", "Employees");
            migrationBuilder.DropColumn(column, "Employees");
        }
        migrationBuilder.DropIndex("IX_LeaveBalances_EmployeeId", "LeaveBalances");
        migrationBuilder.CreateIndex("IX_LeaveBalances_EmployeeId_Year_LeaveType", "LeaveBalances",
            new[] { "EmployeeId", "Year", "LeaveType" }, unique: true, filter: "[IsDeleted] = 0");
        migrationBuilder.CreateIndex("IX_Payrolls_EmployeeId_PayrollPeriodId", "Payrolls",
            new[] { "EmployeeId", "PayrollPeriodId" }, unique: true, filter: "[IsDeleted] = 0 AND [PayrollPeriodId] IS NOT NULL");
        migrationBuilder.DropIndex("IX_Attendances_EmployeeId", "Attendances");
        migrationBuilder.CreateIndex("IX_Attendances_EmployeeId_AttendanceDate", "Attendances",
            new[] { "EmployeeId", "AttendanceDate" }, unique: true, filter: "[IsDeleted] = 0");
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex("IX_LeaveBalances_EmployeeId_Year_LeaveType", "LeaveBalances");
        migrationBuilder.CreateIndex("IX_LeaveBalances_EmployeeId", "LeaveBalances", "EmployeeId");
        migrationBuilder.DropIndex("IX_Payrolls_EmployeeId_PayrollPeriodId", "Payrolls");
        migrationBuilder.DropIndex("IX_Attendances_EmployeeId_AttendanceDate", "Attendances");
        migrationBuilder.CreateIndex("IX_Attendances_EmployeeId", "Attendances", "EmployeeId");
        foreach (var column in new[] { "DepartmentId1", "PositionId1" })
        {
            var table = column == "DepartmentId1" ? "Departments" : "Positions";
            migrationBuilder.AddColumn<int>(column, "Employees", type: "int", nullable: true);
            migrationBuilder.CreateIndex($"IX_Employees_{column}", "Employees", column);
            migrationBuilder.AddForeignKey($"FK_Employees_{table}_{column}", "Employees", column,
                table, principalColumn: "Id");
        }
    }
}
