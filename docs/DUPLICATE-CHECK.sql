-- Read-only checks to run before upgrading an existing database.
SELECT EmployeeId, AttendanceDate, COUNT(*) AS ActiveRecords
FROM Attendances WHERE IsDeleted = 0
GROUP BY EmployeeId, AttendanceDate HAVING COUNT(*) > 1;
SELECT EmployeeId, [Year], LeaveType, COUNT(*) AS ActiveRecords
FROM LeaveBalances WHERE IsDeleted = 0
GROUP BY EmployeeId, [Year], LeaveType HAVING COUNT(*) > 1;
SELECT EmployeeId, PayrollPeriodId, COUNT(*) AS ActiveRecords
FROM Payrolls WHERE IsDeleted = 0 AND PayrollPeriodId IS NOT NULL
GROUP BY EmployeeId, PayrollPeriodId HAVING COUNT(*) > 1;
SELECT EmployeeNumber, COUNT(*) AS Records
FROM Employees GROUP BY EmployeeNumber HAVING COUNT(*) > 1;
