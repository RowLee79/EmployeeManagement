using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using EmployeeManagement.CrystalReport.Models;
using EmployeeManagement.Reporting.Services;
using System;
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


        // =========================================================
        // GET: /Reports/EmployeeMasterList
        //
        // Opens the Employee Master List directly as PDF.
        // =========================================================

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


        // =========================================================
        // POST: /Reports/EmployeeMasterList
        //
        // Called by EmployeeManagement.Web.
        //
        // Supports:
        // PDF
        // EXCEL
        // =========================================================

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
                // =================================================
                // 1. LOAD REPORT
                // =================================================

                reportDocument.Load(reportPath);


                // =================================================
                // 2. APPLY DATABASE CONNECTION
                // =================================================

                _connectionService.ApplyConnection(
                    reportDocument);


                // =================================================
                // 3. APPLY USER FILTERS
                // =================================================

                ApplyFilters(
                    reportDocument,
                    request);


                // =================================================
                // 4. DETERMINE OUTPUT FORMAT
                // =================================================

                var format =
                    string.IsNullOrWhiteSpace(request.Format)
                        ? "PDF"
                        : request.Format.Trim().ToUpperInvariant();


                // =================================================
                // 5. EXPORT REPORT
                // =================================================

                switch (format)
                {
                    case "PDF":

                        return ExportPdf(
                            reportDocument);

                    case "EXCEL":

                        return ExportExcel(
                            reportDocument);

                    default:

                        return new HttpStatusCodeResult(
                            400,
                            "Unsupported report format. " +
                            "Supported formats are PDF and EXCEL.");
                }
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }


        // =========================================================
        // APPLY FILTERS
        // =========================================================

        private void ApplyFilters(
            ReportDocument report,
            EmployeeReportRequest request)
        {
            if (request == null)
            {
                return;
            }

            var filters =
                new List<string>();


            // =====================================================
            // SEARCH
            // =====================================================

            if (!string.IsNullOrWhiteSpace(
                    request.Search))
            {
                var search =
                    EscapeCrystalString(
                        request.Search.Trim());

                filters.Add(
                    "(" +
                    "{Employees.EmployeeNumber} like \"*"
                    + search +
                    "*\"" +
                    " OR " +
                    "{Employees.FirstName} like \"*"
                    + search +
                    "*\"" +
                    " OR " +
                    "{Employees.LastName} like \"*"
                    + search +
                    "*\"" +
                    " OR " +
                    "{Employees.Email} like \"*"
                    + search +
                    "*\"" +
                    ")");
            }


            // =====================================================
            // DEPARTMENT
            // =====================================================

            if (request.DepartmentId.HasValue)
            {
                filters.Add(
                    "{Employees.DepartmentId} = " +
                    request.DepartmentId.Value);
            }


            // =====================================================
            // POSITION
            // =====================================================

            if (request.PositionId.HasValue)
            {
                filters.Add(
                    "{Employees.PositionId} = " +
                    request.PositionId.Value);
            }


            // =====================================================
            // EMPLOYMENT TYPE
            // =====================================================

            if (request.EmploymentType.HasValue)
            {
                filters.Add(
                    "{Employees.EmploymentType} = " +
                    request.EmploymentType.Value);
            }


            // =====================================================
            // STATUS
            // =====================================================

            if (request.Status.HasValue)
            {
                filters.Add(
                    "{Employees.Status} = " +
                    request.Status.Value);
            }


            // =====================================================
            // HIRE DATE FROM
            // =====================================================

            if (request.HireDateFrom.HasValue)
            {
                var date =
                    request.HireDateFrom.Value;

                filters.Add(
                    "{Employees.HireDate} >= Date(" +
                    date.Year +
                    "," +
                    date.Month +
                    "," +
                    date.Day +
                    ")");
            }


            // =====================================================
            // HIRE DATE TO
            // =====================================================

            if (request.HireDateTo.HasValue)
            {
                var date =
                    request.HireDateTo.Value;

                filters.Add(
                    "{Employees.HireDate} <= Date(" +
                    date.Year +
                    "," +
                    date.Month +
                    "," +
                    date.Day +
                    ")");
            }


            // =====================================================
            // APPLY FINAL FORMULA
            // =====================================================

            if (filters.Count > 0)
            {
                report.RecordSelectionFormula =
                    string.Join(
                        " AND ",
                        filters);
            }
        }


        // =========================================================
        // EXPORT PDF
        // =========================================================

        private ActionResult ExportPdf(
            ReportDocument report)
        {
            using (var stream =
                new MemoryStream())
            {
                using (var pdfStream =
                    report.ExportToStream(
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


        // =========================================================
        // EXPORT EXCEL
        // =========================================================

        private ActionResult ExportExcel(
            ReportDocument report)
        {
            using (var stream = new MemoryStream())
            {
                using (var excelStream =
                    report.ExportToStream(
                        ExportFormatType.ExcelWorkbook))
                {
                    excelStream.CopyTo(stream);
                }

                return File(
                    stream.ToArray(),
                    "application/vnd.ms-excel",
                    "EmployeeMasterList.xls");
            }
        }


        // =========================================================
        // ESCAPE CRYSTAL STRING
        // =========================================================

        private string EscapeCrystalString(
            string value)
        {
            return value.Replace(
                "\"",
                "\"\"");
        }
    }
}