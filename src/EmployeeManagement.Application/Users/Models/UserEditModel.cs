using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.Users.Models;

public class UserEditModel
{
    public string Id { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? PhoneNumber { get; set; }

    [Required]
    public string Role { get; set; } = "Employee";

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; } = true;
}