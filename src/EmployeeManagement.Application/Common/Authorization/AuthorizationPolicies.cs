namespace EmployeeManagement.Application.Common.Authorization;

public static class AuthorizationPolicies
{
    public const string CanViewEmployees =
        "CanViewEmployees";

    public const string CanManageEmployees =
        "CanManageEmployees";

    public const string CanManageDepartments =
        "CanManageDepartments";

    public const string CanManagePositions =
        "CanManagePositions";

    public const string CanManageUsers =
        "CanManageUsers";

    public const string CanViewReports =
        "CanViewReports";

    public const string CanPermanentlyDeleteEmployees =
        "CanPermanentlyDeleteEmployees";
}

//Then:

//using EmployeeManagement.Application.Common.Authorization;

//and:

//[Authorize(
//    Policy = AuthorizationPolicies.CanManageEmployees)]