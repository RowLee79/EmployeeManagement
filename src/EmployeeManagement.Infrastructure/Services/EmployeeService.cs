using EmployeeManagement.Application.Common.Interfaces;
using EmployeeManagement.Application.Common.Models;
using EmployeeManagement.Application.Employees.Models;
using EmployeeManagement.Domain.Entities;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Application.Employees.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text;
using EmployeeManagement.Infrastructure.Persistence;

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
        searchModel.PageNumber =
            Math.Max(1, searchModel.PageNumber);

        var allowedPageSizes = new[] { 10, 25, 50, 100 };

        if (!allowedPageSizes.Contains(searchModel.PageSize))
        {
            searchModel.PageSize = 10;
        }

        var query = _context.Employees
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        // Search
        if (!string.IsNullOrWhiteSpace(searchModel.Search))
        {
            var search = searchModel.Search.Trim();

            query = query.Where(x =>
                x.EmployeeNumber.Contains(search) ||
                x.FirstName.Contains(search) ||
                x.LastName.Contains(search) ||
                (x.MiddleName != null &&
                 x.MiddleName.Contains(search)) ||
                (x.Email != null &&
                 x.Email.Contains(search)));
        }

        // Department
        if (searchModel.DepartmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId ==
                searchModel.DepartmentId.Value);
        }

        // Position
        if (searchModel.PositionId.HasValue)
        {
            query = query.Where(x =>
                x.PositionId ==
                searchModel.PositionId.Value);
        }

        // Employment Type
        if (searchModel.EmploymentType.HasValue)
        {
            query = query.Where(x =>
                x.EmploymentType ==
                searchModel.EmploymentType.Value);
        }

        // Status
        if (searchModel.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status ==
                searchModel.Status.Value);
        }

        // Hire Date From
        if (searchModel.HireDateFrom.HasValue)
        {
            query = query.Where(x =>
                x.HireDate >=
                searchModel.HireDateFrom.Value.Date);
        }

        // Hire Date To
        if (searchModel.HireDateTo.HasValue)
        {
            var endDate =
                searchModel.HireDateTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.HireDate < endDate);
        }

        // Total count
        var totalCount = await query.CountAsync();

        // Sorting
        query = searchModel.SortBy?.ToLowerInvariant() switch
        {
            "employeenumber" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.EmployeeNumber)
                        .ThenByDescending(x => x.Id)
                    : query
                        .OrderBy(x => x.EmployeeNumber)
                        .ThenBy(x => x.Id),

            "firstname" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.FirstName)
                        .ThenByDescending(x => x.Id)
                    : query
                        .OrderBy(x => x.FirstName)
                        .ThenBy(x => x.Id),

            "lastname" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.LastName)
                        .ThenByDescending(x => x.Id)
                    : query
                        .OrderBy(x => x.LastName)
                        .ThenBy(x => x.Id),

            "department" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.Department.Name)
                        .ThenBy(x => x.LastName)
                    : query
                        .OrderBy(x => x.Department.Name)
                        .ThenBy(x => x.LastName),

            "position" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.Position.Name)
                        .ThenBy(x => x.LastName)
                    : query
                        .OrderBy(x => x.Position.Name)
                        .ThenBy(x => x.LastName),

            "salary" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.BasicSalary)
                        .ThenBy(x => x.LastName)
                    : query
                        .OrderBy(x => x.BasicSalary)
                        .ThenBy(x => x.LastName),

            "hiredate" =>
                searchModel.SortDescending
                    ? query
                        .OrderByDescending(x => x.HireDate)
                        .ThenBy(x => x.LastName)
                    : query
                        .OrderBy(x => x.HireDate)
                        .ThenBy(x => x.LastName),

            "employmenttype" =>
                    searchModel.SortDescending
                    ? query
                    .OrderByDescending(x => x.EmploymentType)
                    .ThenBy(x => x.LastName)
                    : query
                    .OrderBy(x => x.EmploymentType)
                    .ThenBy(x => x.LastName),

            "status" =>
                    searchModel.SortDescending
                    ? query
                    .OrderByDescending(x => x.Status)
                    .ThenBy(x => x.LastName)
                    : query
                    .OrderBy(x => x.Status)
                    .ThenBy(x => x.LastName),

            _ =>
                query
                    .OrderByDescending(x => x.HireDate)
                    .ThenBy(x => x.LastName)
        };

        // Pagination
        var items = await query
            .Skip(
                (searchModel.PageNumber - 1)
                * searchModel.PageSize)
            .Take(searchModel.PageSize)
            .Select(x => new EmployeeListModel
            {
                Id = x.Id,
                EmployeeNumber = x.EmployeeNumber,

                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,

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
            Items = items,
            PageNumber = searchModel.PageNumber,
            PageSize = searchModel.PageSize,
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
            .Where(x => x.Id == id && !x.IsDeleted)
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

                CivilStatus = (CivilStatus)x.CivilStatus,

                Email = x.Email,

                PhoneNumber = x.PhoneNumber,

                Address = x.Address,

                ProfileImage = x.ProfileImage,

                HireDate = x.HireDate,

                RegularizationDate = x.RegularizationDate,

                EmploymentType = x.EmploymentType,

                Status = x.Status,

                BasicSalary = x.BasicSalary,

                DepartmentId = x.DepartmentId,

                DepartmentName = x.Department.Name,

                PositionId = x.PositionId,

                PositionName = x.Position.Name,

                 // AUDIT
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,

                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate,

                DeletedBy = x.DeletedBy,
                DeletedDate = x.DeletedDate
            })
            .FirstOrDefaultAsync();
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<int> CreateAsync(EmployeeCreateModel model)
    {
        var now = DateTime.UtcNow;

        var employee = new Employee
        {
            EmployeeNumber = model.EmployeeNumber.Trim(),

            FirstName = model.FirstName.Trim(),

            MiddleName = string.IsNullOrWhiteSpace(model.MiddleName)
                ? null
                : model.MiddleName.Trim(),

            LastName = model.LastName.Trim(),

            Suffix = string.IsNullOrWhiteSpace(model.Suffix)
                ? null
                : model.Suffix.Trim(),

            BirthDate = model.BirthDate,

            Gender = model.Gender,

            CivilStatus = model.CivilStatus,

            Email = string.IsNullOrWhiteSpace(model.Email)
                ? null
                : model.Email.Trim(),

            PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim(),

            Address = string.IsNullOrWhiteSpace(model.Address)
                ? null
                : model.Address.Trim(),

            HireDate = model.HireDate,

            RegularizationDate = model.RegularizationDate,

            EmploymentType = model.EmploymentType,

            Status = model.Status,

            BasicSalary = model.BasicSalary,

            DepartmentId = model.DepartmentId,

            PositionId = model.PositionId,

            ProfileImage = model.ProfileImage,

            IsDeleted = false,

                // AUDIT
            CreatedBy = _currentUserService.UserName ?? "system",
            CreatedDate = now


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
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (employee == null)
        {
            return false;
        }

        employee.EmployeeNumber = model.EmployeeNumber.Trim();

        employee.FirstName = model.FirstName.Trim();

        employee.MiddleName =
            string.IsNullOrWhiteSpace(model.MiddleName)
                ? null
                : model.MiddleName.Trim();

        employee.LastName = model.LastName.Trim();

        employee.Suffix =
            string.IsNullOrWhiteSpace(model.Suffix)
                ? null
                : model.Suffix.Trim();

        employee.BirthDate = model.BirthDate;

        employee.Gender = model.Gender;

        employee.CivilStatus = model.CivilStatus;

        employee.Email =
            string.IsNullOrWhiteSpace(model.Email)
                ? null
                : model.Email.Trim();

        employee.PhoneNumber =
            string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim();

        employee.Address =
            string.IsNullOrWhiteSpace(model.Address)
                ? null
                : model.Address.Trim();

        employee.HireDate = model.HireDate;

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

        employee.ProfileImage =
            model.ProfileImage;

        // AUDIT
        employee.UpdatedBy =
            _currentUserService.UserName ?? "system";

        employee.UpdatedDate =
            DateTime.UtcNow;

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
                EscapeCsv(employee.CivilStatus.ToString()),
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
        var query = _context.Employees
            .AsNoTracking()
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchModel.Search))
        {
            var search = searchModel.Search.Trim();

            query = query.Where(x =>
                x.EmployeeNumber.Contains(search) ||
                x.FirstName.Contains(search) ||
                x.LastName.Contains(search) ||
                x.Email.Contains(search));
        }

        if (searchModel.DepartmentId.HasValue)
        {
            query = query.Where(x =>
                x.DepartmentId == searchModel.DepartmentId.Value);
        }

        if (searchModel.PositionId.HasValue)
        {
            query = query.Where(x =>
                x.PositionId == searchModel.PositionId.Value);
        }

        if (searchModel.EmploymentType.HasValue)
        {
            query = query.Where(x =>
                x.EmploymentType ==
                searchModel.EmploymentType.Value);
        }

        if (searchModel.Status.HasValue)
        {
            query = query.Where(x =>
                x.Status ==
                searchModel.Status.Value);
        }

        if (searchModel.HireDateFrom.HasValue)
        {
            query = query.Where(x =>
                x.HireDate >=
                searchModel.HireDateFrom.Value);
        }

        if (searchModel.HireDateTo.HasValue)
        {
            var endDate =
                searchModel.HireDateTo.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.HireDate < endDate);
        }

        return await query
            .OrderBy(x => x.Department.Name)
            .ThenBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Select(x => new EmployeeReportModel
            {
                Id = x.Id,

                EmployeeNumber =
                    x.EmployeeNumber,

                EmployeeName =
                    x.FirstName + " " + x.LastName,

                DepartmentName =
                    x.Department.Name,

                PositionName =
                    x.Position.Name,

                EmploymentType =
                    x.EmploymentType.ToString(),

                Status =
                    x.Status.ToString(),

                HireDate =
                    x.HireDate,

                BasicSalary =
                    x.BasicSalary
            })
            .ToListAsync();
    }
    // =========================================================
    // DELETE - SOFT DELETE
    // =========================================================


    public async Task<bool> DeleteAsync(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                !x.IsDeleted);

        if (employee == null)
        {
            return false;
        }

        employee.IsDeleted = true;

        employee.DeletedBy =
            _currentUserService.UserName ?? "system";

        employee.DeletedDate =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IReadOnlyList<EmployeeListModel>> GetDeletedAsync()
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(x => x.IsDeleted)
            .OrderByDescending(x => x.HireDate)
            .Select(x => new EmployeeListModel
            {
                Id = x.Id,
                EmployeeNumber = x.EmployeeNumber,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,

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
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsDeleted);

        if (employee == null)
        {
            return false;
        }

        employee.IsDeleted = false;

        // Clear deletion audit information
        employee.DeletedBy = null;
        employee.DeletedDate = null;

        // Track the restore as an update
        employee.UpdatedBy =
            _currentUserService.UserName ?? "system";

        employee.UpdatedDate =
            DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<EmployeeDetailsModel?> GetDeletedByIdAsync(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsDeleted)
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

                // FIX
                CivilStatus = x.CivilStatus ?? CivilStatus.Single,

                Email = x.Email,
                PhoneNumber = x.PhoneNumber,
                Address = x.Address,

                ProfileImage = x.ProfileImage,

                HireDate = x.HireDate,
                RegularizationDate = x.RegularizationDate,

                EmploymentType = x.EmploymentType,
                Status = x.Status,

                BasicSalary = x.BasicSalary,

                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department.Name,

                PositionId = x.PositionId,
                PositionName = x.Position.Name,

                // AUDIT
                CreatedBy = x.CreatedBy,
                CreatedDate = x.CreatedDate,

                UpdatedBy = x.UpdatedBy,
                UpdatedDate = x.UpdatedDate,

                DeletedBy = x.DeletedBy,
                DeletedDate = x.DeletedDate
            })
            .FirstOrDefaultAsync();
    }
    public async Task<bool> PermanentlyDeleteAsync(int id)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsDeleted);

        if (employee == null)
        {
            return false;
        }

        _context.Employees.Remove(employee);

        await _context.SaveChangesAsync();

        return true;
    }

}