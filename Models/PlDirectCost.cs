using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using PlanningAPI.Models;
using Microsoft.EntityFrameworkCore;


public class PlDct
{
    // Primary Key: dct_id
    // [Key] indicates that this property is the primary key.
    // [Column("dct_id")] explicitly maps the property to the 'dct_id' column in the database.
    [Key]
    public int DctId { get; set; }
    public int PlId { get; set; }


    [StringLength(30)] // Specifies the maximum length for VARCHAR(20)
    public string AcctId { get; set; } = string.Empty;

    [StringLength(30)] // Specifies the maximum length for VARCHAR(20)
    public string OrgId { get; set; } = string.Empty;
    public string? Type { get; set; } = string.Empty;

        // notes column
    [StringLength(255)] // Specifies the maximum length for VARCHAR(255)
    public string? Notes { get; set; } // Nullable as it's not NOT NULL in SQL

    // category column
    [StringLength(50)] // Specifies the maximum length for VARCHAR(50)
    public string? Category { get; set; } // Nullable

    // amount_type column
    [StringLength(30)] // Specifies the maximum length for VARCHAR(30)
    public string? AmountType { get; set; } // Nullable

    // id column
    [StringLength(50)] // Specifies the maximum length for VARCHAR(50)
    public string? Id { get; set; } // Nullable

    // is_rev column
    public bool IsRev { get; set; } = false; // Maps to BOOLEAN DEFAULT FALSE

    // is_brd column
    public bool IsBrd { get; set; } = false; // Maps to BOOLEAN DEFAULT FALSE

    [StringLength(20)] // Specifies the maximum length for VARCHAR(20)
    public string? PlcGlc { get; set; } // Nullable

    [StringLength(50)] // Specifies the maximum length for VARCHAR(50)
    public string? CreatedBy { get; set; } // Nullable

    // created_date column
    // Using DateTime for DATE type in SQL
    public DateTime? CreatedDate { get; set; } = DateTime.UtcNow; // Maps to DATE DEFAULT CURRENT_DATE

    // last_modified_by column
    [StringLength(50)] // Specifies the maximum length for VARCHAR(50)
    public string? LastModifiedBy { get; set; } // Nullable

    // last_modified_date column
    public DateTime? LastModifiedDate { get; set; } = DateTime.UtcNow;// Nullable
    public virtual ICollection<PlForecast>? PlForecasts { get; set; } = new List<PlForecast>();

    [JsonIgnore]
    public virtual PlProjectPlan? Pl { get; set; }

    public static PlDct CloneWithoutId(PlDct source)
    {
        if (source == null) return null;

        return new PlDct
        {
            PlcGlc = source.PlcGlc,
            Id = source.Id,
            AcctId = source.AcctId,
            OrgId = source.OrgId,
            PlForecasts = new List<PlForecast>()
        };
    }

}