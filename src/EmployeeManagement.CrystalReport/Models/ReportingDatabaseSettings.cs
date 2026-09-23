namespace EmployeeManagement.Reporting.Models
{
    public class ReportingDatabaseSettings
    {
        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public string UserId { get; set; }

        public string Password { get; set; }

        public bool UseIntegratedSecurity { get; set; }
    }
}