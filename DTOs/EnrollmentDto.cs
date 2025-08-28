namespace Tms.Api.Dtos;

public record EnrollmentDto(
    int EnrollmentId,
    string EmployeeName,
    string CourseName,
    string BatchName,
    string Status,
    string? ApprovedBy
);
