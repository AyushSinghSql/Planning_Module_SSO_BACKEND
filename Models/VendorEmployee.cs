using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PlanningAPI.Models
{
    public class VendorEmployee
    {
        [Key, Column(Order = 0)]
        public string VendEmplId { get; set; }

        [Key, Column(Order = 1)]
        public string? VendId { get; set; }

        public string? CompanyId { get; set; }
        public string? DfGenlLabCatCd { get; set; }
        public string? ModifiedBy { get; set; }
        public int? Rowversion { get; set; } = 1;
        public DateTime? TimeStamp { get; set; } = DateTime.UtcNow;
        public string? VendEmplName { get; set; }
        public string? DfBillLabCatCd { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? MidName { get; set; }
        public string? VendEmplStatus { get; set; }
        public string? SubctrId { get; set; }
        public string? TeEmplId { get; set; }
        public string? VendEmplAprvrId { get; set; }
        public DateTime? VendEmplAprvlDt { get; set; }
        public string? VendEmplAprvlCd { get; set; }
        public string? IntEmail { get; set; }
        public string? ExtEmail { get; set; }
        public string? IntPhone { get; set; }
        public string? ExtPhone { get; set; }
        public string? CellPhone { get; set; }
        public string? Cont1Name { get; set; }
        public string? Cont1Rel { get; set; }
        public string? Cont1Phone1 { get; set; }
        public string? Cont1Phone2 { get; set; }
        public string? Cont1Phone3 { get; set; }
        public string? Cont2Name { get; set; }
        public string? Cont2Rel { get; set; }
        public string? Cont2Phone1 { get; set; }
        public string? Cont2Phone2 { get; set; }
        public string? Cont2Phone3 { get; set; }
        public bool? UsCitizenFl { get; set; }
        public string? ItarStatus { get; set; }
    }

}
