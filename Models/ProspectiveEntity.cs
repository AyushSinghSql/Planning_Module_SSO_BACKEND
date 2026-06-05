public class ProspectiveEntity
{
    public int ProspectiveId { get; set; }

    public string Type { get; set; } // Enum: "Employee", "Vendor", etc.
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public string? ModifiedBy { get; internal set; }

    // Navigation Properties
    public EmployeeDetails? EmployeeDetails { get; set; }
    public VendorDetails? VendorDetails { get; set; }
    public VendorEmployeeDetails? VendorEmployeeDetails { get; set; }
    public PLCDetails? PLCDetails { get; set; }
    public GenericStaffDetails? GenericStaffDetails { get; set; }
}
