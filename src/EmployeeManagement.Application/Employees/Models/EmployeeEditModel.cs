using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeEditModel
{
    public string EmployeeNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    public string LastName { get; set; } = null!;

    public string? Suffix { get; set; }

    public DateTime BirthDate { get; set; }

    public Gender Gender { get; set; }

    public string? CivilStatus { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public string? ProfileImage { get; set; }

    public IFormFile? ProfileImageFile { get; set; }

    public DateTime HireDate { get; set; }

    public DateTime? RegularizationDate { get; set; }

    public EmploymentType EmploymentType { get; set; }

    public EmployeeStatus Status { get; set; }

    public decimal BasicSalary { get; set; }

    public int DepartmentId { get; set; }

    public int PositionId { get; set; }
}