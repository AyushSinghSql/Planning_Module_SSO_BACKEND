using System.ComponentModel.DataAnnotations;
using PlanningAPI.Models;

public class Organization
{
    [Key]
    [StringLength(30)]
    public string OrgId { get; set; }

    [StringLength(100)]
    public string OrgName { get; set; }

    public int LvlNo { get; set; }

    [StringLength(100)]
    public string? L1OrgName { get; set; }

    [StringLength(100)]
    public string? L2OrgName { get; set; }

    [StringLength(100)]
    public string? L3OrgName { get; set; }

    [StringLength(100)]
    public string? L4OrgName { get; set; }

    [StringLength(100)]
    public string? L5OrgName { get; set; }

    [StringLength(100)]
    public string? L6OrgName { get; set; }

    [StringLength(100)]
    public string? L7OrgName { get; set; }

    [StringLength(100)]
    public string? L8OrgName { get; set; }

    [StringLength(100)]
    public string? L9OrgName { get; set; }

    public virtual ICollection<PlEmployee> PlEmployees { get; set; } = new List<PlEmployee>();

    public virtual ICollection<PlProject> PlProjects { get; set; } = new List<PlProject>();

    public ICollection<OrgGroupOrgMapping> OrgGroupMappings { get; set; }
    = new List<OrgGroupOrgMapping>();
}
public class OrgGroup
{
    public int OrgGroupId { get; set; }

    public string OrgGroupCode { get; set; }
    public string OrgGroupName { get; set; }
    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public string? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? ModifiedBy { get; set; }
    public DateTime? ModifiedAt { get; set; }

    // Navigation Properties
    public ICollection<OrgGroupUserMapping> UserMappings { get; set; }
        = new List<OrgGroupUserMapping>();

    public ICollection<OrgGroupOrgMapping> OrgMappings { get; set; }
        = new List<OrgGroupOrgMapping>();
}

public class OrgGroupOrgMapping
{
    public int OrgGroupId { get; set; }
    public string OrgId { get; set; }

    public OrgGroup OrgGroup { get; set; }
    public Organization Organization { get; set; }
}

public class BulkGroupOrgsToggleRequest
{
    public int GroupId { get; set; }
    public List<string> OrgIds { get; set; } = new();
}

public class BulkUserOrgsToggleRequest
{
    public int UserId { get; set; }
    public List<string> OrgIds { get; set; } = new();
}

public class OrgGroupCreateUpdateDto
{
    public string OrgGroupCode { get; set; } = null!;
    public string OrgGroupName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
