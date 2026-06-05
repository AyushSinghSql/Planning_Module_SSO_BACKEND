using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class VendorDetails
{
    [Key, ForeignKey("ProspectiveEntity")]
    public int ProspectiveId { get; set; }

    public string VendorId { get; set; }
    public string VendorName { get; set; }

    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public string? ModifiedBy { get; internal set; }

    public ProspectiveEntity? ProspectiveEntity { get; set; }
}
