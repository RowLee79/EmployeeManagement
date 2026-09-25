using System;
using EmployeeManagement.CrystalReport.Models;

namespace EmployeeManagement.CrystalReport.Services
{
    public class EmployeeReportFilterBuilder
    {
        public string Build(EmployeeReportRequest request)
        {
            var filters = new System.Collections.Generic.List<string>();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = EscapeCrystalString(
                    request.Search.Trim());

                filters.Add(
                    $"{{Employees.EmployeeNumber}} like '*{search}*' " +
                    $"or {{Employees.FirstName}} like '*{search}*' " +
                    $"or {{Employees.LastName}} like '*{search}*' " +
                    $"or {{Employees.Email}} like '*{search}*'");
            }

            if (request.DepartmentId.HasValue)
            {
                filters.Add(
                    $"{{Employees.DepartmentId}} = " +
                    $"{request.DepartmentId.Value}");
            }

            if (request.PositionId.HasValue)
            {
                filters.Add(
                    $"{{Employees.PositionId}} = " +
                    $"{request.PositionId.Value}");
            }

            if (request.EmploymentType.HasValue)
            {
                filters.Add(
                    $"{{Employees.EmploymentType}} = " +
                    $"{request.EmploymentType.Value}");
            }

            if (request.Status.HasValue)
            {
                filters.Add(
                    $"{{Employees.Status}} = " +
                    $"{request.Status.Value}");
            }

            if (request.HireDateFrom.HasValue)
            {
                var from =
                    request.HireDateFrom.Value.Date
                        .ToString("yyyy-MM-dd");

                filters.Add(
                    $"{{Employees.HireDate}} >= Date({from})");
            }

            if (request.HireDateTo.HasValue)
            {
                var to =
                    request.HireDateTo.Value.Date
                        .ToString("yyyy-MM-dd");

                filters.Add(
                    $"{{Employees.HireDate}} <= Date({to})");
            }

            if (filters.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" and ", filters);
        }

        private static string EscapeCrystalString(string value)
        {
            return value.Replace("'", "''");
        }
    }
}