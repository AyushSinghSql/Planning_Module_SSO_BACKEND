using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("nbi_prmtrc_rt")]
    public class NbiPrmtrcRt
    {
        [Key]
        [Column("para_id")]
        public int ParaId { get; set; }

        [Column("contract_type")]
        [StringLength(20)]
        public string ContractType { get; set; }

        [Column("driver_grp")]
        [StringLength(50)]
        public string DriverGrp { get; set; }

        [Column("proj_id")]
        [StringLength(50)]
        public string ProjId { get; set; }

        [Column("lab_onste", TypeName = "numeric(18,4)")]
        public decimal? LabOnste { get; set; }

        [Column("lab_offste", TypeName = "numeric(18,4)")]
        public decimal? LabOffste { get; set; }

        [Column("non_lab", TypeName = "numeric(18,4)")]
        public decimal? NonLab { get; set; }

        [Column("sub_lab", TypeName = "numeric(18,4)")]
        public decimal? SubLab { get; set; }

        [Column("sub_non_lab", TypeName = "numeric(18,4)")]
        public decimal? SubNonLab { get; set; }

        [Column("cls_pd")]
        public DateTime? ClsPd { get; set; }

        [Column("fy_cd")]
        [StringLength(4)]
        public string FyCd { get; set; }

        [Column("modified_by")]
        [StringLength(100)]
        public string ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }
    }
}


