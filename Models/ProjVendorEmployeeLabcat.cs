using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PlanningAPI.Models
{
    [Table("proj_vendor_employee_labcat", Schema = "public")]
    public class ProjVendorEmployeeLabcat
    {
        [Key, Column(Order = 0)]
        [MaxLength(50)]
        public string ProjId { get; set; } = null!;

        [Key, Column(Order = 1)]
        [MaxLength(50)]
        public string VendId { get; set; } = null!;

        [Key, Column(Order = 2)]
        [MaxLength(50)]
        public string VendEmplId { get; set; } = null!;

        [Key, Column(Order = 3)]
        [MaxLength(50)]
        public string BillLabCatCd { get; set; } = null!;

        [MaxLength(1)]
        public string DfltFl { get; set; } = "N";   // "Y" or "N"

        public DateTime? StartDt { get; set; }

        public DateTime? EndDt { get; set; }
    }
}
