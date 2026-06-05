using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("chart_of_accounts", Schema = "public")]
    public class Charts_Of_Accounts
    {
        [Key]
        [Column("account_id")]
        [MaxLength(50)]
        public string AccountId { get; set; } = null!;

        [Column("account_name")]
        [MaxLength(200)]
        [Required]
        public string AccountName { get; set; } = null!;

        [Column("cost_type")]
        [MaxLength(50)]
        [Required]
        public string CostType { get; set; } = null!;

        [Column("account_type")]
        [MaxLength(50)]
        [Required]
        public string AccountType { get; set; } = null!;

        [Column("budget_sheet")]
        [MaxLength(100)]
        public string? BudgetSheet { get; set; }   // ✅ New column

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
