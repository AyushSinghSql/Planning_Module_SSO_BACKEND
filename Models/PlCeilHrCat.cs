using System.ComponentModel.DataAnnotations.Schema;

public class PlCeilHrCat
{
    public string ProjectId { get; set; }

    public string LaborCategoryId { get; set; }

    [NotMapped]
    public string LaborCategoryDesc{ get; set; }
    public decimal HoursCeiling { get; set; } = 0;
    public char? ApplyToRbaCode { get; set; }
}
