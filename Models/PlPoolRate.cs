using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    public class PlPoolRate
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(20)]
        public string OrgId { get; set; }

        [MaxLength(20)]
        public string AccountId { get; set; }

        [MaxLength(20)]
        public string? AccountGroupCode { get; set; }

        [MaxLength(20)]
        public string? AccountType { get; set; }

        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        public decimal? ActualRate { get; set; }

        public decimal? TargetRate { get; set; }

        [MaxLength(30)]
        public string? ModifiedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int? BurdenTemplateId { get; set; }
        [JsonIgnore]
        public OrgAccount OrgAccount { get; set; }
        [JsonIgnore]
        public Pools? AccountGroup { get; set; }
        [JsonIgnore]
        public BurdenTemplate? BurdenTemplate { get; set; }


        public PlPoolRate CloneWithoutId()
        {
            return new PlPoolRate
            {
                // Do NOT set Id (let DB generate it)
                OrgId = this.OrgId,
                AccountId = this.AccountId,
                AccountGroupCode = this.AccountGroupCode,
                AccountType = this.AccountType,
                Year = this.Year,
                Month = this.Month,
                ActualRate = this.ActualRate,
                TargetRate = this.TargetRate,
                ModifiedBy = this.ModifiedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                BurdenTemplateId = this.BurdenTemplateId,

                // Navigation properties are ignored
            };
        }
    }

    public class PoolBaseAccount
    {
        public int AllocGrpNo { get; set; }
        public int FyCd { get; set; }
        public int PoolNo { get; set; }
        public string OrgId { get; set; } = string.Empty;
        public string AcctId { get; set; } = string.Empty;
        public string? AllocOrgId { get; set; } = string.Empty;
        public string? AllocAcctId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }
    public class PoolCostAccount
    {
        public int AllocGrpNo { get; set; }
        public int FyCd { get; set; }
        public int PoolNo { get; set; }
        public string OrgId { get; set; } = string.Empty;
        public string AcctId { get; set; } = string.Empty;
        //public string AllocOrgId { get; set; } = string.Empty;
        //public string AllocAcctId { get; set; } = string.Empty;
        public int CompanyId { get; set; }
    }

    public class PoolAllocationRawDto
    {
        public int BasePoolNo { get; set; }
        public string BaseAcctId { get; set; } = null!;
        public string BaseOrgId { get; set; } = null!;
        public int AllocPoolNo { get; set; }
        public decimal? CurAllocAmt { get; set; }
    }



}
