using System.Globalization;
using EmployeeManagement.Application.Reporting;
using EmployeeManagement.Domain.Enums;
using EmployeeManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
namespace EmployeeManagement.Web.Services.Reporting;

public sealed class NativeReportingService(EmployeeDbContext context) : IReportingService
{
    public async Task<byte[]> GenerateEmployeeMasterListAsync(EmployeeReportRequest request, string format)
    {
        if (request.HireDateFrom > request.HireDateTo)
            throw new InvalidOperationException("Hire date From cannot be later than To.");
        var query = context.Employees.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.EmployeeNumber.Contains(search) || x.FirstName.Contains(search) ||
                x.LastName.Contains(search) || x.Email.Contains(search));
        }
        if (request.DepartmentId.HasValue) query = query.Where(x => x.DepartmentId == request.DepartmentId);
        if (request.PositionId.HasValue) query = query.Where(x => x.PositionId == request.PositionId);
        if (request.EmploymentType.HasValue) query = query.Where(x => (int)x.EmploymentType == request.EmploymentType);
        if (request.Status.HasValue) query = query.Where(x => (int)x.Status == request.Status);
        if (request.HireDateFrom.HasValue) query = query.Where(x => x.HireDate >= request.HireDateFrom.Value.Date);
        if (request.HireDateTo.HasValue) query = query.Where(x => x.HireDate < request.HireDateTo.Value.Date.AddDays(1));
        if (request.ReportType == "NewHires" && !request.HireDateFrom.HasValue)
        {
            var from = DateTime.Today.AddDays(-30);
            query = query.Where(x => x.HireDate >= from);
        }
        var employees = await query.OrderBy(x => x.Department.Name).ThenBy(x => x.LastName).ThenBy(x => x.Id)
            .Select(x => new { x.EmployeeNumber, Name = x.FirstName + " " + x.LastName,
                Department = x.Department.Name, Position = x.Position.Name, x.Status, x.HireDate }).ToListAsync();
        string title;
        string[] headers;
        List<string[]> rows;
        switch (request.ReportType)
        {
            case "EmployeesByDepartment":
                title = "Employees by Department"; headers = ["Department", "Employees"];
                rows = employees.GroupBy(x => x.Department).Select(x => new[] { x.Key, x.Count().ToString() }).ToList();
                break;
            case "EmployeeStatus":
                title = "Employee Status"; headers = ["Status", "Employees"];
                rows = employees.GroupBy(x => x.Status).Select(x => new[] { x.Key.ToString(), x.Count().ToString() }).ToList();
                break;
            case "EmployeeSummary":
                title = "Employee Summary"; headers = ["Department", "Status", "Employees"];
                rows = employees.GroupBy(x => new { x.Department, x.Status })
                    .Select(x => new[] { x.Key.Department, x.Key.Status.ToString(), x.Count().ToString() }).ToList();
                break;
            case "NewHires":
            case "EmployeeMasterList":
                title = request.ReportType == "NewHires" ? "New Hires" : "Employee Master List";
                headers = ["Number", "Employee", "Department", "Position", "Status", "Hire Date"];
                rows = employees.Select(x => new[] { x.EmployeeNumber, x.Name, x.Department, x.Position,
                    x.Status.ToString(), x.HireDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }).ToList();
                break;
            default: throw new InvalidOperationException("Select a supported report type.");
        }
        if (format.Equals("EXCEL", StringComparison.OrdinalIgnoreCase))
            return SpreadsheetExport.Create(headers, rows);
        if (!format.Equals("PDF", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Select PDF or Excel.");
        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape()); page.Margin(25); page.DefaultTextStyle(x => x.FontSize(9));
            page.Header().PaddingBottom(12).Text(title).FontSize(20).Bold();
            page.Content().Column(column =>
            {
                column.Item().PaddingBottom(8).Text($"Matching employees: {employees.Count}");
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => { foreach (var _ in headers) columns.RelativeColumn(); });
                    table.Header(header => { foreach (var text in headers) header.Cell().Padding(4).Text(text).Bold(); });
                    foreach (var row in rows)
                        foreach (var text in row) table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(text);
                });
                if (rows.Count == 0) column.Item().PaddingTop(10).Text("No matching records.");
            });
            page.Footer().AlignRight().Text(text => { text.Span("Page "); text.CurrentPageNumber(); });
        })).GeneratePdf();
    }
}
