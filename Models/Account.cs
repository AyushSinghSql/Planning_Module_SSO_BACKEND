using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Account
{
    [Key]
    [StringLength(30)]
    public string AcctId { get; set; }

    [StringLength(100)]
    public string AcctName { get; set; }
    public string? ActiveFlag { get; set; }


    public string? L1AcctName { get; set; }
    public string? L2AcctName { get; set; }
    public string? L3AcctName { get; set; }
    public string? L4AcctName { get; set; }
    public string? L5AcctName { get; set; }
    public string? L6AcctName { get; set; }
    public string? L7AcctName { get; set; }
    [NotMapped]
    public string? ModifiedBy { get; set; }
    [NotMapped]
    public DateTime? Createdat { get; set; }
    [NotMapped]
    public DateTime? Updatedat { get; set; }
    public int? LvlNo { get; set; } = 0;
    public string? SAcctTypeCd { get; set; }

    [JsonIgnore]
    public virtual AccountGroupSetup? AccountGroupSetup { get; set; }
}


[Table("chart_of_accounts")]
public class ChartOfAccount
{
    [Key]
    [Column("account_id")]
    [MaxLength(50)]
    public string AccountId { get; set; } = null!;

    [Required]
    [Column("account_name")]
    [MaxLength(200)]
    public string? AccountName { get; set; } = null!;

    [Required]
    [Column("cost_type")]
    [MaxLength(50)]
    public string? CostType { get; set; } = null!;

    [Required]
    [Column("account_type")]
    [MaxLength(100)]
    public string? AccountType { get; set; } = null!;

    [Column("budget_sheet")]
    [MaxLength(50)]
    public string? BudgetSheet { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}

//public class LaborProjectAccount
//{
//    public string? LaborGroup { get; set; }
//    public string? LaborGroupDescription { get; set; }
//    public string? ProjectAccountGroup { get; set; }
//    public string? ProjectAccountGroupDescription { get; set; }
//    public string? Account { get; set; }
//    public string? AccountName { get; set; }
//}

[Table("labor_project_accounts")]
public class LaborProjectAccount
{
    [Column("labor_group")]
    public string? LaborGroup { get; set; }

    [Column("labor_group_description")]
    public string? LaborGroupDescription { get; set; }

    [Column("project_account_group")]
    public string? ProjectAccountGroup { get; set; }

    [Column("project_account_group_description")]
    public string? ProjectAccountGroupDescription { get; set; }

    [Column("account")]
    public string? Account { get; set; }

    [Column("account_name")]
    public string? AccountName { get; set; }
}