using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Configuration;

namespace EmployeeManagement.Reporting.Services
{
    public class CrystalReportConnectionService
    {
        public void ApplyConnection(ReportDocument report)
        {
            var connectionString =
                ConfigurationManager
                    .ConnectionStrings["EmployeeManagementDb"]
                    .ConnectionString;

            var builder =
                new System.Data.SqlClient.SqlConnectionStringBuilder(
                    connectionString);

            var connectionInfo = new ConnectionInfo
            {
                ServerName = builder.DataSource,
                DatabaseName = builder.InitialCatalog,
                IntegratedSecurity = builder.IntegratedSecurity,
                UserID = builder.UserID,
                Password = builder.Password
            };

            ApplyConnection(report, connectionInfo);
        }

        private void ApplyConnection(
            ReportDocument report,
            ConnectionInfo connectionInfo)
        {
            foreach (Table table in report.Database.Tables)
            {
                var logonInfo = table.LogOnInfo;

                logonInfo.ConnectionInfo = connectionInfo;

                table.ApplyLogOnInfo(logonInfo);
            }

            foreach (Section section in report.ReportDefinition.Sections)
            {
                foreach (ReportObject reportObject
                         in section.ReportObjects)
                {
                    if (reportObject.Kind ==
                        ReportObjectKind.SubreportObject)
                    {
                        var subreportObject =
                            (SubreportObject)reportObject;

                        var subreport =
                            report.OpenSubreport(
                                subreportObject.SubreportName);

                        ApplyConnection(
                            subreport,
                            connectionInfo);
                    }
                }
            }
        }
    }
}