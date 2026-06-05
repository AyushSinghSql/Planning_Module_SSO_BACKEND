namespace PlanningAPI.Models
{
    using System;

    public class ProjBillHs
    {
        public string ProjId { get; set; }
        public string FyCd { get; set; }
        public int PdNo { get; set; }
        public int SubPdNo { get; set; }
        public decimal BilledAmt { get; set; }
        public decimal RtngeAmt { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;
        public decimal DelAmt { get; set; }
        public long RowVersion { get; set; } = 0;
        public decimal BillWhAmt { get; set; }
        public decimal BillWhRelAmt { get; set; }
        public decimal MuBilledAmt { get; set; }
        public decimal MuRtngeAmt { get; set; }
        public decimal MuBillWhAmt { get; set; }
        public decimal MuBillWhRelAmt { get; set; }
    }

}
