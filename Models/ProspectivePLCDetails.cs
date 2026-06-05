using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class PLCDetails
{
    [Key, ForeignKey("ProspectiveEntity")]
    public int ProspectiveId { get; set; }

    public string Category { get; set; }
    public decimal HrlyRate { get; set; }

    public ProspectiveEntity? ProspectiveEntity { get; set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public string? ModifiedBy { get; internal set; }
}
