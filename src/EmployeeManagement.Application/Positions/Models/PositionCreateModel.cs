using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.Positions.Models;

public class PositionCreateModel
{
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, 999999999)]
    public decimal? MinimumSalary { get; set; }

    [Range(0, 999999999)]
    public decimal? MaximumSalary { get; set; }

    public bool IsActive { get; set; } = true;

    [Required]
    public int DepartmentId { get; set; }
}