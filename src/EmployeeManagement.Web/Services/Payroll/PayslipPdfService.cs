using EmployeeManagement.Application.Payroll.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EmployeeManagement.Web.Services.Payroll;

public class PayslipPdfService : IPayslipPdfService
{
    public byte[] GeneratePayslip(
        PayslipModel payslip)
    {
        if (payslip == null)
        {
            throw new ArgumentNullException(nameof(payslip));
        }

        var document =
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(40);

                    page.DefaultTextStyle(
                        x => x.FontSize(10));

                    page.Header()
                        .Element(header =>
                            ComposeHeader(
                                header,
                                payslip));

                    page.Content()
                        .Element(content =>
                            ComposeContent(
                                content,
                                payslip));

                    page.Footer()
                        .Element(footer =>
                            ComposeFooter(footer));
                });
            });

        return document.GeneratePdf();
    }

    // ============================================================
    // HEADER
    // ============================================================

    private void ComposeHeader(
        IContainer container,
        PayslipModel payslip)
    {
        container.Column(column =>
        {
            column.Spacing(4);

            column.Item()
                .AlignCenter()
                .Text("EMPLOYEE MANAGEMENT SYSTEM")
                .Bold()
                .FontSize(18);

            column.Item()
                .AlignCenter()
                .Text("PAYSLIP")
                .Bold()
                .FontSize(14);

            column.Item()
                .PaddingTop(8)
                .LineHorizontal(1);
        });
    }

    // ============================================================
    // CONTENT
    // ============================================================

    private void ComposeContent(
        IContainer container,
        PayslipModel payslip)
    {
        container.Column(column =>
        {
            column.Spacing(15);

            ComposeEmployeeInformation(
                column.Item(),
                payslip);

            ComposeEarnings(
                column.Item(),
                payslip);

            ComposeDeductions(
                column.Item(),
                payslip);

            ComposeNetSalary(
                column.Item(),
                payslip);

            ComposeSignatures(
                column.Item());
        });
    }

    // ============================================================
    // EMPLOYEE INFORMATION
    // ============================================================

    private void ComposeEmployeeInformation(
        IContainer container,
        PayslipModel payslip)
    {
        container.Column(column =>
        {
            column.Item()
                .Text("EMPLOYEE INFORMATION")
                .Bold()
                .FontSize(12);

            column.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                        columns.RelativeColumn();
                    });

                    table.Cell()
                        .Element(CellStyle)
                        .Text("Employee Number");

                    table.Cell()
                        .Element(CellStyle)
                        .Text(
                            payslip.EmployeeNumber);

                    table.Cell()
                        .Element(CellStyle)
                        .Text("Payroll Date");

                    table.Cell()
                        .Element(CellStyle)
                        .Text(
                            payslip.PayrollDate
                                .ToString("dd-MMM-yyyy"));

                    table.Cell()
                        .Element(CellStyle)
                        .Text("Employee Name");

                    table.Cell()
                        .Element(CellStyle)
                        .Text(
                            payslip.EmployeeName);

                    table.Cell()
                        .Element(CellStyle)
                        .Text("Status");

                    table.Cell()
                        .Element(CellStyle)
                        .Text(
                            payslip.Status);

                    table.Cell()
                        .Element(CellStyle)
                        .Text("Department");

                    table.Cell()
                        .Element(CellStyle)
                        .Text(
                            string.IsNullOrWhiteSpace(
                                payslip.DepartmentName)
                                ? "N/A"
                                : payslip.DepartmentName);

                    table.Cell()
                        .Element(CellStyle)
                        .Text("Position");

                    table.Cell()
                        .Element(CellStyle)
                        .Text(
                            string.IsNullOrWhiteSpace(
                                payslip.PositionName)
                                ? "N/A"
                                : payslip.PositionName);
                });
        });
    }

    // ============================================================
    // EARNINGS
    // ============================================================

    private void ComposeEarnings(
        IContainer container,
        PayslipModel payslip)
    {
        container.Column(column =>
        {
            column.Item()
                .Text("EARNINGS")
                .Bold()
                .FontSize(12);

            column.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell()
                            .Element(HeaderStyle)
                            .Text("Description");

                        header.Cell()
                            .Element(HeaderStyle)
                            .AlignRight()
                            .Text("Amount");
                    });

                    AddAmountRow(
                        table,
                        "Basic Salary",
                        payslip.BasicSalary);

                    AddAmountRow(
                        table,
                        "Overtime",
                        payslip.Overtime);

                    AddAmountRow(
                        table,
                        "Allowances",
                        payslip.Allowances);

                    // Gross Salary
                    table.Cell()
                        .Element(TotalLabelStyle)
                        .Text("GROSS SALARY")
                        .Bold();

                    table.Cell()
                        .Element(TotalAmountStyle)
                        .AlignRight()
                        .Text(
                            FormatCurrency(
                                payslip.GrossSalary))
                        .Bold();
                });
        });
    }

    // ============================================================
    // DEDUCTIONS
    // ============================================================

    private void ComposeDeductions(
        IContainer container,
        PayslipModel payslip)
    {
        container.Column(column =>
        {
            column.Item()
                .Text("DEDUCTIONS")
                .Bold()
                .FontSize(12);

            column.Item()
                .PaddingTop(5)
                .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(1);
                    });

                    table.Header(header =>
                    {
                        header.Cell()
                            .Element(HeaderStyle)
                            .Text("Description");

                        header.Cell()
                            .Element(HeaderStyle)
                            .AlignRight()
                            .Text("Amount");
                    });

                    // SSS
                    AddAmountRow(
                        table,
                        "SSS",
                        payslip.Deductions.Sss);

                    // PhilHealth
                    AddAmountRow(
                        table,
                        "PhilHealth",
                        payslip.Deductions.PhilHealth);

                    // Pag-IBIG
                    AddAmountRow(
                        table,
                        "Pag-IBIG",
                        payslip.Deductions.PagIbig);

                    // Withholding Tax
                    AddAmountRow(
                        table,
                        "Withholding Tax",
                        payslip.Deductions.WithholdingTax);

                    // Other deductions
                    AddAmountRow(
                        table,
                        "Other Deductions",
                        payslip.Deductions.OtherDeductions);

                    // Total deductions
                    table.Cell()
                        .Element(TotalLabelStyle)
                        .Text("TOTAL DEDUCTIONS")
                        .Bold();

                    table.Cell()
                        .Element(TotalAmountStyle)
                        .AlignRight()
                        .Text(
                            FormatCurrency(
                                payslip
                                    .Deductions
                                    .TotalDeductions))
                        .Bold();
                });
        });
    }

    // ============================================================
    // NET SALARY
    // ============================================================

    private void ComposeNetSalary(
        IContainer container,
        PayslipModel payslip)
    {
        container
            .Border(1)
            .Padding(12)
            .Column(column =>
            {
                column.Item()
                    .Text("NET SALARY")
                    .Bold()
                    .FontSize(14);

                column.Item()
                    .PaddingTop(8)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Gross Salary");

                        row.RelativeItem()
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    payslip.GrossSalary));
                    });

                column.Item()
                    .PaddingTop(5)
                    .LineHorizontal(0.5f);

                column.Item()
                    .PaddingTop(5)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("Total Deductions");

                        row.RelativeItem()
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    payslip
                                        .Deductions
                                        .TotalDeductions));
                    });

                column.Item()
                    .PaddingTop(10)
                    .BorderTop(1)
                    .PaddingTop(8)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("NET SALARY")
                            .Bold()
                            .FontSize(14);

                        row.RelativeItem()
                            .AlignRight()
                            .Text(
                                FormatCurrency(
                                    payslip.NetSalary))
                            .Bold()
                            .FontSize(14);
                    });
            });
    }

    // ============================================================
    // SIGNATURES
    // ============================================================

    private void ComposeSignatures(
        IContainer container)
    {
        container
            .PaddingTop(35)
            .Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell()
                    .PaddingRight(20)
                    .Column(column =>
                    {
                        column.Item()
                            .PaddingTop(30)
                            .BorderTop(1);

                        column.Item()
                            .AlignCenter()
                            .Text("Employee Signature");
                    });

                table.Cell()
                    .PaddingLeft(20)
                    .Column(column =>
                    {
                        column.Item()
                            .PaddingTop(30)
                            .BorderTop(1);

                        column.Item()
                            .AlignCenter()
                            .Text("Authorized Signature");
                    });
            });
    }

    // ============================================================
    // FOOTER
    // ============================================================

    private void ComposeFooter(
        IContainer container)
    {
        container
            .AlignCenter()
            .Text(text =>
            {
                text.Span(
                    "Generated by Employee Management System");

                text.Span(" | ");

                text.CurrentPageNumber();

                text.Span(" / ");

                text.TotalPages();
            });
    }

    // ============================================================
    // AMOUNT ROW
    // ============================================================

    private void AddAmountRow(
        TableDescriptor table,
        string description,
        decimal amount)
    {
        table.Cell()
            .Element(CellStyle)
            .Text(description);

        table.Cell()
            .Element(CellStyle)
            .AlignRight()
            .Text(
                FormatCurrency(amount));
    }

    // ============================================================
    // STYLES
    // ============================================================

    private static IContainer CellStyle(
        IContainer container)
    {
        return container
            .BorderBottom(0.5f)
            .PaddingVertical(5)
            .PaddingHorizontal(6);
    }

    private static IContainer HeaderStyle(
        IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten2)
            .BorderBottom(1)
            .Padding(6);
    }

    private static IContainer TotalLabelStyle(
        IContainer container)
    {
        return container
            .BorderTop(1)
            .Padding(6);
    }

    private static IContainer TotalAmountStyle(
        IContainer container)
    {
        return container
            .BorderTop(1)
            .Padding(6);
    }

    // ============================================================
    // HELPERS
    // ============================================================

    private static string FormatCurrency(
        decimal amount)
    {
        return $"₱{amount:N2}";
    }
}