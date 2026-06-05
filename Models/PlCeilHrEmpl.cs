using System.ComponentModel.DataAnnotations.Schema;

public class PlCeilHrEmpl
{
    public string ProjectId { get; set; }

    public string EmployeeId { get; set; }

    public string LaborCategoryId { get; set; }

    public decimal HoursCeiling { get; set; } = 0;

    public char? ApplyToRbaCode { get; set; }

    [NotMapped]
    public string? EmployeeName { get; set; }
    [NotMapped]
    public string? LaborCategoryDesc { get; set; }
}
