using System.ComponentModel.DataAnnotations.Schema;

public class PlCeilDirCst
{
    public string ProjectId { get; set; }

    public string AccountId { get; set; }

    public decimal CeilingAmountFunc { get; set; } = 0;

    public decimal? CeilingAmountBilling { get; set; }

    public char? ApplyToRbaCode { get; set; }
    [NotMapped]
    public string? AccountDesc { get; set; }
}
