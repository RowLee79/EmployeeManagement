using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Employees.Interfaces;
using EmployeeManagement.Application.Employees.Models;
using EmployeeManagement.Application.Employees.Reports;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EmployeeManagement.Infrastructure.Services;

public class EmployeeService : IEmployeeService
{
    private readonly EmployeeDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeService(
        EmployeeDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    // =========================================================
    // GET PAGED EMPLOYEES
    // =========================================================

    public async Task<PagedResult<EmployeeListModel>> GetPagedAsync(
        EmployeeSearchModel searchModel)
    {
        var query = BuildEmployeeQuery(searchModel);

        query = ApplySorting(
            query,
            searchModel.SortBy,
            searchModel.SortDescending);

         // -----------------------------------------------------
        // Search
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(searchModel.Search))
        {
            var search = searchModel.Search.Trim();

            query = query.Where(x =>
                x.EmployeeNumber.Contains(search) ||
                x.FirstName.Contains(search) ||
                (x.MiddleName != null &&
                 x.MiddleName.Contains(search)) ||
                x.LastName.Contains(search) ||
                (x.Email != null &&
                 x.Email.Contains(search)) ||
                (x.PhoneNumber != null &&
                 x.PhoneNumber.Contains(search)));
        }

        // -----------------------------------------------------
        // Department
        // -----------------------------------------------------

        if (searchModel.DepartmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId == searchModel.DepartmentId.Value);
        }

        // -----------------------------------------------------
        // Position
        // -----------------------------------------------------

        if (searchModel.PositionId.HasValue)
        {
            query = query.Where(x =>
                x.PositionId == searchModel.PositionId.Value);
        }

        // -----------------------------------------------------
        // Status
        // -----------------------------------------------------

        if (searchModel.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status == searchModel.Status.Value);
        }

        // -----------------------------------------------------
        // Employment Type
        // -----------------------------------------------------

        if (searchModel.EmploymentType.HasValue)
        {
            query = query.Where(x =>
                x.EmploymentType == searchModel.EmploymentType.Value);
        }

        // -----------------------------------------------------
        // Hire Date From
        // -----------------------------------------------------

        if (searchModel.HireDateFrom.HasValue)
        {
            query = query.Where(x =>
                x.HireDate >= searchModel.HireDateFrom.Value.Date);
        }

        // -----------------------------------------------------
        // Hire Date To
        // -----------------------------------------------------

        if (searchModel.HireDateTo.HasValue)
        {
            var dateToExclusive =
                searchModel.HireDateTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.HireDate < dateToExclusive);
        }

        // -----------------------------------------------------
        // Sorting
        // -----------------------------------------------------

        query = ApplySorting(
            query,
            searchModel.SortBy,
            searchModel.SortDescending);

        // -----------------------------------------------------
        // Page Size
        // -----------------------------------------------------

        var allowedPageSizes = new[] { 10, 25, 50, 100 };

        var pageSize = allowedPageSizes.Contains(searchModel.PageSize)
            ? searchModel.PageSize
            : 10;

        var pageNumber = searchModel.PageNumber < 1
            ? 1
            : searchModel.PageNumber;

        // -----------------------------------------------------
        // Total Count
        // -----------------------------------------------------

        var totalCount = await query.CountAsync();

        // -----------------------------------------------------
        // Prevent invalid page number
        // -----------------------------------------------------

        var totalPages = (int)Math.Ceiling(
            totalCount / (double)pageSize);

        if (totalPages > 0 && pageNumber > totalPages)
        {
            pageNumber = totalPages;
        }

        // -----------------------------------------------------
        // Server-side pagination
        // -----------------------------------------------------

        var employees = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EmployeeListModel
            {
                Id = x.Id,

                EmployeeNumber = x.EmployeeNumber,

                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                Suffix = x.Suffix,

                Email = x.Email,
                PhoneNumber = x.PhoneNumber,

                HireDate = x.HireDate,

                Gender = x.Gender,
                EmploymentType = x.EmploymentType,
                Status = x.Status,

                BasicSalary = x.BasicSalary,

                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.Name,

                PositionId = x.PositionId,
                PositionName = x.Position.Name,

                ProfileImage = x.ProfileImage
            })
            .ToListAsync();

        return new PagedResult<EmployeeListModel>
        {
            Items = employees,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    // =========================================================
    // SORTING
    // =========================================================

    private static IQueryable<Employee> ApplySorting(
        IQueryable<Employee> query,
        string? sortBy,
        bool sortDescending)
    {
        var key = sortBy?
            .Trim()
            .ToLowerInvariant();

        if (sortDescending)
        {
            return key switch
            {
                "employeenumber" =>
                    query.OrderByDescending(
                        x => x.EmployeeNumber),

                "name" =>
                    query
                        .OrderByDescending(x => x.LastName)
                        .ThenByDescending(x => x.FirstName),

                "department" =>
                    query.OrderByDescending(
                        x => x.Department.Name),

                "position" =>
                    query.OrderByDescending(
                        x => x.Position.Name),

                "status" =>
                    query.OrderByDescending(
                        x => x.Status),

                "salary" =>
                    query.OrderByDescending(
                        x => x.BasicSalary),

                "hiredate" =>
                    query.OrderByDescending(
                        x => x.HireDate),

                _ =>
                    query.OrderByDescending(
                        x => x.HireDate)
            };
        }

        return key switch
        {
            "employeenumber" =>
                query.OrderBy(
                    x => x.EmployeeNumber),

            "name" =>
                query
                    .OrderBy(x => x.LastName)
                    .ThenBy(x => x.FirstName),

            "department" =>
                query.OrderBy(
                    x => x.Department.Name),

            "position" =>
                query.OrderBy(
                    x => x.Position.Name),

            "status" =>
                query.OrderBy(
                    x => x.Status),

            "salary" =>
                query.OrderBy(
                    x => x.BasicSalary),

            "hiredate" =>
                query.OrderBy(
                    x => x.HireDate),

            _ =>
                query.OrderBy(
                    x => x.HireDate)
        };
    }

    // =========================================================
    // EMPLOYEE LOOKUP
    // =========================================================

    public async Task<IReadOnlyList<EmployeeLookupModel>>
        GetLookupAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.Status == EmployeeStatus.Active)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeLookupModel
            {
                Id = x.Id,

                EmployeeNumber = x.EmployeeNumber,

                FullName =
                    x.FirstName +
                    " " +
                    x.LastName
            })
            .ToListAsync();
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    public async Task<EmployeeDetailsModel?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(x =>
                x.Id == id &&
                !x.IsDeleted)
            .Select(x => new EmployeeDetailsModel
            {
                Id = x.Id,

                EmployeeNumber = x.EmployeeNumber,

                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                Suffix = x.Suffix,

                BirthDate = x.BirthDate,

                Gender = x.Gender,

                CivilStatus = x.CivilStatus,

                Email = x.Email,

                PhoneNumber = x.PhoneNumber,

                Address = x.Address,

                ProfileImage = x.ProfileImage,

                HireDate = x.HireDate,

                RegularizationDate =
                    x.RegularizationDate,

                EmploymentType =
                    x.EmploymentType,

                Status = x.Status,

                BasicSalary = x.BasicSalary,

                DepartmentId = x.DepartmentId,

                DepartmentName =
                    x.Department.Name,

                PositionId = x.PositionId,

                PositionName =
                    x.Position.Name,

                CreatedDate = x.CreatedDate,

                CreatedBy = x.CreatedBy,

                UpdatedDate = x.UpdatedDate,

                UpdatedBy = x.UpdatedBy
            })
            .FirstOrDefaultAsync();
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<int> CreateAsync(
        EmployeeCreateModel model)
    {
        // -----------------------------------------------------
        // Validate Department
        // -----------------------------------------------------

        var departmentExists =
            await _context.Departments.AnyAsync(x =>
                x.Id == model.DepartmentId &&
                !x.IsDeleted &&
                x.IsActive);

        if (!departmentExists)
        {
            throw new InvalidOperationException(
                "Selected department does not exist or is inactive.");
        }

        // -----------------------------------------------------
        // Validate Position
        // -----------------------------------------------------

        var positionExists =
            await _context.Positions.AnyAsync(x =>
                x.Id == model.PositionId &&
                !x.IsDeleted &&
                x.IsActive &&
                x.DepartmentId == model.DepartmentId);

        if (!positionExists)
        {
            throw new InvalidOperationException(
                "Selected position does not exist, is inactive, " +
                "or does not belong to the selected department.");
        }

        // -----------------------------------------------------
        // Validate Email
        // -----------------------------------------------------

        var email = model.Email.Trim();

        var emailExists =
            await _context.Employees.AnyAsync(x =>
                !x.IsDeleted &&
                x.Email == email);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "Email address already exists.");
        }

        // -----------------------------------------------------
        // Generate Employee Number
        // -----------------------------------------------------

        var employeeNumber =
            await GenerateEmployeeNumberAsync();

        // -----------------------------------------------------
        // Create Employee
        // -----------------------------------------------------

        var employee = new Employee
        {
            EmployeeNumber = employeeNumber,

            FirstName = model.FirstName.Trim(),

            MiddleName =
                string.IsNullOrWhiteSpace(model.MiddleName)
                    ? null
                    : model.MiddleName.Trim(),

            LastName = model.LastName.Trim(),

            Suffix =
                string.IsNullOrWhiteSpace(model.Suffix)
                    ? null
                    : model.Suffix.Trim(),

            BirthDate = model.BirthDate,

            Gender = model.Gender,

            CivilStatus =
                string.IsNullOrWhiteSpace(model.CivilStatus)
                    ? null
                    : model.CivilStatus.Trim(),

            Email = email,

            PhoneNumber =
                string.IsNullOrWhiteSpace(model.PhoneNumber)
                    ? null
                    : model.PhoneNumber.Trim(),

            Address =
                string.IsNullOrWhiteSpace(model.Address)
                    ? null
                    : model.Address.Trim(),

            ProfileImage = model.ProfileImage,

            HireDate = model.HireDate,

            RegularizationDate =
                model.RegularizationDate,

            EmploymentType =
                model.EmploymentType,

            Status =
                model.Status,

            BasicSalary =
                model.BasicSalary,

            DepartmentId =
                model.DepartmentId,

            PositionId =
                model.PositionId,

            CreatedDate =
                DateTime.UtcNow,

            CreatedBy =
                _currentUserService.Email ?? "System",

            IsDeleted = false
        };

        _context.Employees.Add(employee);

        await _context.SaveChangesAsync();

        return employee.Id;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<bool> UpdateAsync(
        int id,
        EmployeeEditModel model)
    {
        var employee =
            await _context.Employees.FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (employee == null)
        {
            return false;
        }

        // -----------------------------------------------------
        // Validate Department
        // -----------------------------------------------------

        var departmentExists =
            await _context.Departments.AnyAsync(x =>
                x.Id == model.DepartmentId &&
                !x.IsDeleted &&
                x.IsActive);

        if (!departmentExists)
        {
            throw new InvalidOperationException(
                "Selected department does not exist or is inactive.");
        }

        // -----------------------------------------------------
        // Validate Position
        // -----------------------------------------------------

        var positionExists =
            await _context.Positions.AnyAsync(x =>
                x.Id == model.PositionId &&
                !x.IsDeleted &&
                x.IsActive &&
                x.DepartmentId == model.DepartmentId);

        if (!positionExists)
        {
            throw new InvalidOperationException(
                "Selected position does not exist, is inactive, " +
                "or does not belong to the selected department.");
        }

        // -----------------------------------------------------
        // Validate Email
        // -----------------------------------------------------

        var email = model.Email.Trim();

        var duplicateEmail =
            await _context.Employees.AnyAsync(x =>
                x.Id != id &&
                !x.IsDeleted &&
                x.Email == email);

        if (duplicateEmail)
        {
            throw new InvalidOperationException(
                "Email address already exists.");
        }

        // -----------------------------------------------------
        // Update Employee
        // -----------------------------------------------------

        employee.EmployeeNumber =
            model.EmployeeNumber.Trim();

        employee.FirstName =
            model.FirstName.Trim();

        employee.MiddleName =
            string.IsNullOrWhiteSpace(model.MiddleName)
                ? null
                : model.MiddleName.Trim();

        employee.LastName =
            model.LastName.Trim();

        employee.Suffix =
            string.IsNullOrWhiteSpace(model.Suffix)
                ? null
                : model.Suffix.Trim();

        employee.BirthDate =
            model.BirthDate;

        employee.Gender =
            model.Gender;

        employee.CivilStatus =
            string.IsNullOrWhiteSpace(model.CivilStatus)
                ? null
                : model.CivilStatus.Trim();

        employee.Email =
            email;

        employee.PhoneNumber =
            string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim();

        employee.Address =
            string.IsNullOrWhiteSpace(model.Address)
                ? null
                : model.Address.Trim();

        // -----------------------------------------------------
        // Profile Image
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(model.ProfileImage))
        {
            employee.ProfileImage =
                model.ProfileImage;
        }

        employee.HireDate =
            model.HireDate;

        employee.RegularizationDate =
            model.RegularizationDate;

        employee.EmploymentType =
            model.EmploymentType;

        employee.Status =
            model.Status;

        employee.BasicSalary =
            model.BasicSalary;

        employee.DepartmentId =
            model.DepartmentId;

        employee.PositionId =
            model.PositionId;

        // -----------------------------------------------------
        // Audit
        // -----------------------------------------------------

        employee.UpdatedDate =
            DateTime.UtcNow;

        employee.UpdatedBy =
            _currentUserService.Email ?? "System";

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // DELETE - SOFT DELETE
    // =========================================================

    public async Task<bool> DeleteAsync(int id)
    {
        var employee =
            await _context.Employees.FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (employee == null)
        {
            return false;
        }

        employee.IsDeleted = true;

        employee.UpdatedDate =
            DateTime.UtcNow;

        employee.UpdatedBy =
            _currentUserService.Email ?? "System";

        await _context.SaveChangesAsync();

        return true;
    }

    // =========================================================
    // GENERATE EMPLOYEE NUMBER
    // =========================================================

    private async Task<string> GenerateEmployeeNumberAsync()
    {
        var lastEmployeeNumber =
            await _context.Employees
                .IgnoreQueryFilters()
                .Where(x =>
                    x.EmployeeNumber.StartsWith("EMP-"))
                .OrderByDescending(x => x.Id)
                .Select(x => x.EmployeeNumber)
                .FirstOrDefaultAsync();

        var nextNumber = 1;

        if (!string.IsNullOrWhiteSpace(lastEmployeeNumber))
        {
            var numberPart =
                lastEmployeeNumber.Replace("EMP-", "");

            if (int.TryParse(
                    numberPart,
                    out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        return $"EMP-{nextNumber:D6}";
    }

    private IQueryable<Employee> BuildEmployeeQuery(
    EmployeeSearchModel searchModel)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchModel.Search))
        {
            var search = searchModel.Search.Trim();

            query = query.Where(x =>
                x.EmployeeNumber.Contains(search) ||
                x.FirstName.Contains(search) ||
                (x.MiddleName != null &&
                 x.MiddleName.Contains(search)) ||
                x.LastName.Contains(search) ||
                (x.Email != null &&
                 x.Email.Contains(search)) ||
                (x.PhoneNumber != null &&
                 x.PhoneNumber.Contains(search)));
        }

        if (searchModel.DepartmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId ==
                searchModel.DepartmentId.Value);
        }

        if (searchModel.PositionId.HasValue)
        {
            query = query.Where(x =>
                x.PositionId ==
                searchModel.PositionId.Value);
        }

        if (searchModel.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status ==
                searchModel.Status.Value);
        }

        if (searchModel.EmploymentType.HasValue)
        {
            query = query.Where(x =>
                x.EmploymentType ==
                searchModel.EmploymentType.Value);
        }

        if (searchModel.HireDateFrom.HasValue)
        {
            query = query.Where(x =>
                x.HireDate >=
                searchModel.HireDateFrom.Value.Date);
        }

        if (searchModel.HireDateTo.HasValue)
        {
            var dateToExclusive =
                searchModel.HireDateTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.HireDate < dateToExclusive);
        }

        return query;
    }

    public async Task<byte[]> ExportCsvAsync(
    EmployeeSearchModel searchModel)
    {
        var query = BuildEmployeeQuery(searchModel);

        query = ApplySorting(
            query,
            searchModel.SortBy,
            searchModel.SortDescending);

        var employees = await query
            .Select(x => new
            {
                x.EmployeeNumber,
                x.FirstName,
                x.MiddleName,
                x.LastName,
                x.Suffix,
                x.Email,
                x.PhoneNumber,
                x.Gender,
                x.CivilStatus,
                x.HireDate,
                x.EmploymentType,
                x.Status,
                x.BasicSalary,
                DepartmentName = x.Department.Name,
                PositionName = x.Position.Name
            })
            .ToListAsync();

        var csv = new StringBuilder();

        csv.AppendLine(
            "Employee Number,First Name,Middle Name,Last Name," +
            "Suffix,Email,Phone Number,Gender,Civil Status," +
            "Hire Date,Employment Type,Status,Basic Salary," +
            "Department,Position");

        foreach (var employee in employees)
        {
            csv.AppendLine(string.Join(",",
                EscapeCsv(employee.EmployeeNumber),
                EscapeCsv(employee.FirstName),
                EscapeCsv(employee.MiddleName),
                EscapeCsv(employee.LastName),
                EscapeCsv(employee.Suffix),
                EscapeCsv(employee.Email),
                EscapeCsv(employee.PhoneNumber),
                EscapeCsv(employee.Gender.ToString()),
                EscapeCsv(employee.CivilStatus),
                EscapeCsv(employee.HireDate.ToString("yyyy-MM-dd")),
                EscapeCsv(employee.EmploymentType.ToString()),
                EscapeCsv(employee.Status.ToString()),
                EscapeCsv(employee.BasicSalary.ToString("N2")),
                EscapeCsv(employee.DepartmentName),
                EscapeCsv(employee.PositionName)
            ));
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        if (value.Contains(',') ||
            value.Contains('"') ||
            value.Contains('\n') ||
            value.Contains('\r'))
        {
            return "\"" +
                   value.Replace("\"", "\"\"") +
                   "\"";
        }

        return value;
    }

    public async Task<IReadOnlyList<EmployeeReportModel>>
    GetEmployeeReportAsync(
        EmployeeSearchModel searchModel)
    {
        var query = BuildEmployeeQuery(searchModel);

        return await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeReportModel
            {
                Id = x.Id,

                EmployeeNumber =
                    x.EmployeeNumber,

                FullName =
                    x.FirstName +
                    " " +
                    (string.IsNullOrWhiteSpace(x.MiddleName)
                        ? ""
                        : x.MiddleName + " ") +
                    x.LastName +
                    (string.IsNullOrWhiteSpace(x.Suffix)
                        ? ""
                        : " " + x.Suffix),

                Email = x.Email,

                PhoneNumber =
                    x.PhoneNumber,

                DepartmentName =
                    x.Department.Name,

                PositionName =
                    x.Position.Name,

                EmploymentType =
                    x.EmploymentType.ToString(),

                Status =
                    x.Status.ToString(),

                BasicSalary =
                    x.BasicSalary,

                HireDate =
                    x.HireDate,

                RegularizationDate =
                    x.RegularizationDate,

                ProfileImage =
                    x.ProfileImage
            })
            .ToListAsync();
    }
}