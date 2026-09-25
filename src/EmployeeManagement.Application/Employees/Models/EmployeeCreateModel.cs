using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Common;
using EmployeeManagement.Domain.Enums;

namespace EmployeeManagement.Application.Employees.Models;

public class EmployeeCreateModel : AuditableEntity
{
    [Required]
    [Display(Name = "Employee Number")]
    [StringLength(50)]
    public string EmployeeNumber { get; set; } = string.Empty;


    [Required]
    [Display(Name = "First Name")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;


    [Display(Name = "Middle Name")]
    [StringLength(100)]
    public string? MiddleName { get; set; }


    [Required]
    [Display(Name = "Last Name")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;


    [Display(Name = "Suffix")]
    [StringLength(20)]
    public string? Suffix { get; set; }


    [Required]
    [Display(Name = "Birth Date")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }


    [Required]
    public Gender Gender { get; set; }


    [Required]
    [Display(Name = "Civil Status")]
    public CivilStatus CivilStatus { get; set; }


    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }


    [Display(Name = "Phone Number")]
    [StringLength(50)]
    public string? PhoneNumber { get; set; }


    [StringLength(500)]
    public string? Address { get; set; }


    [Required]
    [Display(Name = "Hire Date")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }


    [Display(Name = "Regularization Date")]
    [DataType(DataType.Date)]
    public DateTime? RegularizationDate { get; set; }


    [Required]
    [Display(Name = "Employment Type")]
    public EmploymentType EmploymentType { get; set; }


    [Required]
    public EmployeeStatus Status { get; set; }


    [Required]
    [Range(0, 999999999)]
    [Display(Name = "Basic Salary")]
    public decimal BasicSalary { get; set; }


    [Required]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }


    [Required]
    [Display(Name = "Position")]
    public int PositionId { get; set; }


    [Display(Name = "Profile Image")]
    [StringLength(500)]
    public string? ProfileImage { get; set; }
}