using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.Departments.Models;

public class DepartmentCreateModel
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}