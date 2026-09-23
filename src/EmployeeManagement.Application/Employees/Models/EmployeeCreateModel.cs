using EmployeeManagement.Domain.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Application.Employees.Models;
    public class EmployeeCreateModel
    {
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
        public DateTime BirthDate { get; set; }

        [Required]
        public Gender Gender { get; set; }

        [StringLength(50)]
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

        [Range(0, 999999999)]
        public decimal BasicSalary { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int PositionId { get; set; }

        public IFormFile? ProfileImageFile { get; set; }

        public string? ProfileImage { get; set; }
    }
