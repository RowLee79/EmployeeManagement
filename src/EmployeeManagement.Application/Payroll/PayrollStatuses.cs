using System;
using System.Collections.Generic;
using System.Text;

namespace EmployeeManagement.Application.Payroll
{
    public static class PayrollStatuses
    {
        public const string Draft = "Draft";
        public const string Calculated = "Calculated";
        public const string Approved = "Approved";
        public const string Finalized = "Finalized";
        public const string Cancelled = "Cancelled";
    }
}
