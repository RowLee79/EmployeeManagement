namespace EmployeeManagement.Application.Payroll.Models;

public class PayrollCreateModel
{
    public int EmployeeId { get; set; }

    public int? PayrollPeriodId { get; set; }

    public DateTime PayrollDate { get; set; }
        = DateTime.Today;

    public decimal BasicSalary { get; set; }

    public decimal Overtime { get; set; }

    public decimal Allowances { get; set; }

    public decimal OtherDeductions { get; set; }

    public string Status { get; set; } = "Draft";
}