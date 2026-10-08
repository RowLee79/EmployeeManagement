using System.IO.Compression;
using System.Xml.Linq;
using EmployeeManagement.Application.Reporting;
using EmployeeManagement.Web.Services;
using EmployeeManagement.Web.Services.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
namespace EmployeeManagement.IntegrationTests;
public sealed class ReportAndUploadTests
{
    [Fact]
    public void Spreadsheet_ExportsStringsAndEscapesXml()
    {
        var bytes = SpreadsheetExport.Create(["Employee"], [["""=HYPERLINK("bad")"""], ["A & B <C>"]]);
        using var zip = new ZipArchive(new MemoryStream(bytes));
        using var input = zip.GetEntry("xl/worksheets/sheet1.xml")!.Open();
        var doc = XDocument.Load(input); XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        Assert.All(doc.Descendants(ns + "c"), x => Assert.Equal("inlineStr", x.Attribute("t")!.Value));
        Assert.Empty(doc.Descendants(ns + "f"));
        Assert.Contains("A & B <C>", doc.Descendants(ns + "t").Select(x => x.Value));
    }
    [Theory]
    [InlineData("EmployeeMasterList")][InlineData("EmployeeSummary")][InlineData("EmployeesByDepartment")]
    [InlineData("EmployeeStatus")][InlineData("NewHires")]
    public async Task NativeReport_AllChoicesProduceExcel(string reportType)
    {
        using var db = TestContext.Create(); await TestContext.AddEmployeeAsync(db);
        var bytes = await new NativeReportingService(db).GenerateEmployeeMasterListAsync(new EmployeeReportRequest { ReportType = reportType }, "EXCEL");
        using var zip = new ZipArchive(new MemoryStream(bytes));
        Assert.NotNull(zip.GetEntry("xl/workbook.xml"));
    }
    [Fact]
    public async Task Upload_RejectsDisguisedImageAndTraversal()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()); Directory.CreateDirectory(root);
        try
        {
            var service = new LocalFileStorageService(new EnvironmentStub { WebRootPath = root });
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("<script>bad</script>"));
            var file = new FormFile(stream, 0, stream.Length, "photo", "photo.png");
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.SaveEmployeeProfileImageAsync(file));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync("/uploads/employees/../../appsettings.json"));
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteAsync("/css/site.css"));
            Assert.Empty(Directory.GetFiles(root, "*", SearchOption.AllDirectories));
        }
        finally { Directory.Delete(root, true); }
    }
    private sealed class EnvironmentStub : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests"; public string EnvironmentName { get; set; } = "Testing";
        public string WebRootPath { get; set; } = ""; public string ContentRootPath { get; set; } = "";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
