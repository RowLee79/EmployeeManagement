using System.ComponentModel.DataAnnotations;
using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EmployeeManagement.Web.Models.Employees;

public class EmployeeEditViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = null!;

    [StringLength(100)]
    public string? MiddleName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = null!;

    [StringLength(20)]
    public string? Suffix { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required]
    public Gender Gender { get; set; }

    [StringLength(50)]
    public string? CivilStatus { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = null!;

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? RegularizationDate { get; set; }

    [Required]
    public EmploymentType EmploymentType { get; set; }

    [Required]
    public EmployeeStatus Status { get; set; }

    [Range(0, 999999999)]
    public decimal BasicSalary { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public int PositionId { get; set; }

    // Existing image stored in database
    public string? ExistingProfileImage { get; set; }

    // New uploaded image
    public IFormFile? ProfileImage { get; set; }
}