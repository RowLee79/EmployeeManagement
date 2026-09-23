using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Domain.Entities;

public class Employee : AuditableEntity
{
    public string EmployeeNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? Suffix { get; set; }

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    //public string? CivilStatus { get; set; }
    public CivilStatus? CivilStatus { get; set; }
    public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? ProfileImage { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? RegularizationDate { get; set; }

    public EmploymentType EmploymentType { get; set; }

    public EmployeeStatus Status { get; set; }

    public decimal BasicSalary { get; set; }

    public int DepartmentId { get; set; }

    public int PositionId { get; set; }

    public Department Department { get; set; } = null!;

    public Position Position { get; set; } = null!;

    public ICollection<Attendance> Attendances { get; set; }
        = new List<Attendance>();

    public ICollection<LeaveRequest> LeaveRequests { get; set; }
        = new List<LeaveRequest>();

    public ICollection<Payroll> Payrolls { get; set; }
        = new List<Payroll>();
}