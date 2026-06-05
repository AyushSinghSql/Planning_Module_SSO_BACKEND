using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class GenericStaffDetails
{
    [Key, ForeignKey("ProspectiveEntity")]
    public int ProspectiveId { get; set; }

    public decimal HrlyRate { get; set; }
    public string PLC { get; set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public string? ModifiedBy { get; internal set; }
    public ProspectiveEntity? ProspectiveEntity { get; set; }
}
