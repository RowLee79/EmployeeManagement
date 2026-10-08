using System.ComponentModel.DataAnnotations;
namespace EmployeeManagement.Web.Models.Account;
public class ProfileViewModel
{
    [StringLength(100)] public string? FirstName { get; set; }
    [StringLength(100)] public string? LastName { get; set; }
    [Phone, StringLength(50)] public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
}
