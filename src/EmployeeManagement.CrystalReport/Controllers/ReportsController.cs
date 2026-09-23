using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using EmployeeManagement.CrystalReport.Models;
using EmployeeManagement.Reporting.Services;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;

namespace EmployeeManagement.Reporting.Controllers
{
    public class ReportsController : Controller
    {
        private readonly CrystalReportConnectionService
            _connectionService;

        public ReportsController()
        {
            _connectionService =
                new CrystalReportConnectionService();
        }

        [HttpGet]
        public ActionResult EmployeeMasterList()
        {
            var reportPath = Server.MapPath(
                "~/Reports/EmployeeMasterList.rpt");

            if (!System.IO.File.Exists(reportPath))
            {
                return HttpNotFound(
                    "EmployeeMasterList.rpt was not found.");
            }

            var reportDocument = new ReportDocument();

            try
            {
                reportDocument.Load(reportPath);

                _connectionService.ApplyConnection(
                    reportDocument);

                using (var stream = new MemoryStream())
                {
                    using (var pdfStream =
                           reportDocument.ExportToStream(
                               ExportFormatType.PortableDocFormat))
                    {
                        pdfStream.CopyTo(stream);
                    }

                    return File(
                        stream.ToArray(),
                        "application/pdf",
                        "EmployeeMasterList.pdf");
                }
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }
        [HttpPost]
        public ActionResult EmployeeMasterList(
    EmployeeReportRequest request)
        {
            var reportPath = Server.MapPath(
                "~/Reports/EmployeeMasterList.rpt");

            if (!System.IO.File.Exists(reportPath))
            {
                return HttpNotFound(
                    "EmployeeMasterList.rpt was not found.");
            }

            var reportDocument = new ReportDocument();

            try
            {
                reportDocument.Load(reportPath);

                _connectionService.ApplyConnection(
                    reportDocument);

                ApplyFilters(
                    reportDocument,
                    request);

                using (var stream = new MemoryStream())
                {
                    using (var pdfStream =
                        reportDocument.ExportToStream(
                            ExportFormatType.PortableDocFormat))
                    {
                        pdfStream.CopyTo(stream);
                    }

                    return File(
                        stream.ToArray(),
                        "application/pdf",
                        "EmployeeMasterList.pdf");
                }
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }


        private void ApplyFilters(
        ReportDocument report,
        EmployeeReportRequest request)
        {
            var filters = new List<string>();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var search = EscapeCrystalString(
                    request.Search.Trim());

                filters.Add(
                    "(" +
                    "{Employees.EmployeeNumber} like \"*" + search + "*\"" +
                    " OR " +
                    "{Employees.FirstName} like \"*" + search + "*\"" +
                    " OR " +
                    "{Employees.LastName} like \"*" + search + "*\"" +
                    " OR " +
                    "{Employees.Email} like \"*" + search + "*\"" +
                    ")");
            }

            if (request.DepartmentId.HasValue)
            {
                filters.Add(
                    "{Employees.DepartmentId} = " +
                    request.DepartmentId.Value);
            }

            if (request.PositionId.HasValue)
            {
                filters.Add(
                    "{Employees.PositionId} = " +
                    request.PositionId.Value);
            }

            if (request.EmploymentType.HasValue)
            {
                filters.Add(
                    "{Employees.EmploymentType} = " +
                    request.EmploymentType.Value);
            }

            if (request.Status.HasValue)
            {
                filters.Add(
                    "{Employees.Status} = " +
                    request.Status.Value);
            }

            if (request.HireDateFrom.HasValue)
            {
                filters.Add(
                    "{Employees.HireDate} >= Date(" +
                    request.HireDateFrom.Value.Year + "," +
                    request.HireDateFrom.Value.Month + "," +
                    request.HireDateFrom.Value.Day +
                    ")");
            }

            if (request.HireDateTo.HasValue)
            {
                filters.Add(
                    "{Employees.HireDate} <= Date(" +
                    request.HireDateTo.Value.Year + "," +
                    request.HireDateTo.Value.Month + "," +
                    request.HireDateTo.Value.Day +
                    ")");
            }

            if (filters.Count > 0)
            {
                report.RecordSelectionFormula =
                    string.Join(" AND ", filters);
            }
        }
        private string EscapeCrystalString(string value)
        {
            return value.Replace("\"", "\"\"");
        }
    }
}