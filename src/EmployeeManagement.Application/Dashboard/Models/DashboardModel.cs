namespace EmployeeManagement.Application.Dashboard.Models;

public class DashboardModel
{
    // Employees

    public int TotalEmployees { get; set; }

    public int ActiveEmployees { get; set; }

    public int InactiveEmployees { get; set; }

    public int OnLeaveEmployees { get; set; }


    // Organization

    public int TotalDepartments { get; set; }

    public int TotalPositions { get; set; }


    // Leave

    public int PendingLeaveRequests { get; set; }

    public int ApprovedLeaveRequests { get; set; }

    public int RejectedLeaveRequests { get; set; }


    // Today's Attendance

    public int PresentToday { get; set; }

    public int LateToday { get; set; }

    public int AbsentToday { get; set; }

    public int HalfDayToday { get; set; }

    public int OnLeaveToday { get; set; }


    // Charts

    public IReadOnlyList<DepartmentEmployeeCountModel>
        EmployeesByDepartment
    { get; set; }
        = Array.Empty<DepartmentEmployeeCountModel>();


    // Recent Employees

    public IReadOnlyList<RecentEmployeeModel>
        RecentEmployees
    { get; set; }
        = Array.Empty<RecentEmployeeModel>();
}