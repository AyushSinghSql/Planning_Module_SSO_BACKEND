using System.ComponentModel.DataAnnotations.Schema;

public class PlCeilBurden
{
    public string ProjectId { get; set; }

    public string FiscalYear { get; set; }

    public string AccountId { get; set; }

    [NotMapped]
    public string AccountDesc { get; set; }

    public string PoolCode { get; set; }

    public decimal? RateCeiling { get; set; }

    public string RateFormat { get; set; }

    public decimal? ComCeiling { get; set; }

    public string ComFormat { get; set; }

    public char? CeilingMethodCode { get; set; }

    public char? ApplyToRbaCode { get; set; }
}
