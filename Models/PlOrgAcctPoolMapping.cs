using System;
using System.ComponentModel.DataAnnotations;
using PlanningAPI.Models;

public class PlOrgAcctPoolMapping
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string OrgId { get; set; }

    [Required]
    [MaxLength(20)]
    public string AccountId { get; set; }

    [MaxLength(20)]
    public string PoolId { get; set; }

    [MaxLength(30)]
    public string? ModifiedBy { get; set; }

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? Year { get; set; }

    // Navigation properties
    public OrgAccount OrgAccount { get; set; }
    public Pools Pool { get; set; }
}
