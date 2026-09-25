using EmployeeManagement.Application.Common.Interfaces;

namespace EmployeeManagement.Web.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow =>
        DateTime.UtcNow;
}