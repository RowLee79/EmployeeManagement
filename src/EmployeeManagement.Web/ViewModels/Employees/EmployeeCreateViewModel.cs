using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EmployeeManagement.Web.ViewModels.Employees;

public class EmployeeCreateViewModel
{
    [Required]
    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    [Required]
    public string LastName { get; set; } = null!;

    public string? Suffix { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public Gender Gender { get; set; }

    public string? CivilStatus { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    [Required]
    public DateTime HireDate { get; set; }

    public DateTime? RegularizationDate { get; set; }

    [Required]
    public EmploymentType EmploymentType { get; set; }

    [Required]
    public EmployeeStatus Status { get; set; }

    [Range(0, 100000000)]
    public decimal BasicSalary { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int PositionId { get; set; }

    public List<SelectListItem> Departments { get; set; }
        = [];

    public List<SelectListItem> Positions { get; set; }
        = [];
}