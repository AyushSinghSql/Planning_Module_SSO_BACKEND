using Microsoft.EntityFrameworkCore;
using PlanningAPI.DTO;
using PlanningAPI.Models;

namespace PlanningAPI.Helpers
{

    public class RateCalculator
    {
        private readonly MydatabaseContext _context;
        public RateCalculator(MydatabaseContext context)
        {
            _context = context;
        }


        public List<PoolOrgCostFinancialDetail> GetCostHRDetails(string fycd)
        {
            var fringeCostAccounts = _context.PoolCostAccounts.Where(p => p.PoolNo == 12).ToList();

            var GLReport = _context.PlFinancialTransactions
                .Where(x => fringeCostAccounts.Select(f => f.AcctId).Contains(x.AcctId) && x.FyCd == fycd)
                .GroupBy(x => new { x.AcctId, x.OrgId, x.PdNo, x.FyCd })
                .Select(g => new PlFinancialTransaction
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    FyCd = g.Key.FyCd,
                    PdNo = g.Key.PdNo,
                    Amt1 = g.Sum(x => x.Amt1 ?? 0)
                })
                .ToList();

            var result = _context.PlForecasts
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => fringeCostAccounts.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.DctId != null)
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();

            //var forecastDict = result
            //        .ToDictionary(
            //            x => (x.AcctId, x.OrgId, x.Month, x.Year),
            //            x => x.TotalForecastedAmt
            //        );

            var forecastDict = result.ToDictionary(
                        x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
                        x => x.TotalForecastedAmt
                    );

            return GLReport
            .Where(t => t.OrgId != null && t.AcctId != null)
            .GroupBy(t => new
            {
                t.OrgId,
                t.AcctId,
            })
            .Select(g => new PoolOrgCostFinancialDetail
            {
                OrgId = g.Key.OrgId,
                AcctId = g.Key.AcctId,

                YTDActualAmt = g.Sum(x => x.Amt1 ?? 0),
                YTDBudgetedAmt = g.Sum(x => x.Amt ?? 0),

                PeriodDetails = g
                    .GroupBy(x => x.PdNo ?? 0)
                    .Select(p => new PeriodCostFinancialDetail
                    {
                        Period = p.Key,
                        Actualamt = p.Sum(x => x.Amt1 ?? 0),
                        BudgetedAmt = p.Sum(x => x.Amt ?? 0)
                    })
                    .OrderBy(p => p.Period)
                    .ToList()
            })
            .ToList();
        }
        public List<PoolOrgCostFinancialDetail> GetCostFringeDetails(string fycd)
        {
            var fringeCostAccounts = _context.PoolCostAccounts.Where(p => p.PoolNo == 11).ToList();

            var GLReport = _context.PlFinancialTransactions
                .Where(x => fringeCostAccounts.Select(f => f.AcctId).Contains(x.AcctId) && x.FyCd == fycd)
                .GroupBy(x => new { x.AcctId, x.OrgId, x.PdNo, x.FyCd })
                .Select(g => new PlFinancialTransaction
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    FyCd = g.Key.FyCd,
                    PdNo = g.Key.PdNo,
                    Amt1 = g.Sum(x => x.Amt1 ?? 0)
                })
                .ToList();

            var result = _context.PlForecasts
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => fringeCostAccounts.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.DctId != null)
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();

            //var forecastDict = result
            //        .ToDictionary(
            //            x => (x.AcctId, x.OrgId, x.Month, x.Year),
            //            x => x.TotalForecastedAmt
            //        );

            var forecastDict = result.ToDictionary(
                        x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
                        x => x.TotalForecastedAmt
                    );

            return GLReport
            .Where(t => t.OrgId != null && t.AcctId != null)
            .GroupBy(t => new
            {
                t.OrgId,
                t.AcctId,
            })
            .Select(g => new PoolOrgCostFinancialDetail
            {
                OrgId = g.Key.OrgId,
                AcctId = g.Key.AcctId,

                YTDActualAmt = g.Sum(x => x.Amt1 ?? 0),
                YTDBudgetedAmt = g.Sum(x => x.Amt ?? 0),

                PeriodDetails = g
                    .GroupBy(x => x.PdNo ?? 0)
                    .Select(p => new PeriodCostFinancialDetail
                    {
                        Period = p.Key,
                        Actualamt = p.Sum(x => x.Amt1 ?? 0),
                        BudgetedAmt = p.Sum(x => x.Amt ?? 0)
                    })
                    .OrderBy(p => p.Period)
                    .ToList()
            })
            .ToList();
        }

        public List<PoolOrgBaseFinancialDetail> GetBaseLaborAccountDetails(string fycd)
        {
            var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == 11).ToList();

            var GLReport = _context.PlFinancialTransactions
                .Where(x => baseLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.FyCd == fycd)
                .GroupBy(x => new { x.AcctId, x.OrgId, x.PdNo, x.FyCd })
                .Select(g => new PlFinancialTransaction
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    FyCd = g.Key.FyCd,
                    PdNo = g.Key.PdNo,
                    Amt1 = g.Sum(x => x.Amt1 ?? 0)
                })
                .ToList();

            var result = _context.PlForecasts
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => baseLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.DctId != null)
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();

            //var forecastDict = result
            //        .ToDictionary(
            //            x => (x.AcctId, x.OrgId, x.Month, x.Year),
            //            x => x.TotalForecastedAmt
            //        );

            var forecastDict = result.ToDictionary(
                        x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
                        x => x.TotalForecastedAmt
                    );

            return GLReport
            .Where(t => t.OrgId != null && t.AcctId != null)
            .GroupBy(t => new
            {
                t.OrgId,
                t.AcctId,
            })
            .Select(g => new PoolOrgBaseFinancialDetail
            {
                OrgId = g.Key.OrgId,
                AcctId = g.Key.AcctId,

                YTDActualAmt = g.Sum(x => x.Amt1 ?? 0),
                YTDBudgetedAmt = g.Sum(x => x.Amt ?? 0),

                PeriodDetails = g
                    .GroupBy(x => x.PdNo ?? 0)
                    .Select(p => new PeriodbaseFinancialDetail
                    {
                        Period = p.Key,
                        baseAmt = p.Sum(x => x.Amt1 ?? 0)
                    })
                    .OrderBy(p => p.Period)
                    .ToList()
            })
            .ToList();
        }


        public List<PoolOrgCostFinancialDetail> GetCostFringeDetails(string fycd, int PoolNo)
        {
            List<PoolOrgBaseFinancialDetail> fringAllocation = new List<PoolOrgBaseFinancialDetail>();

            if (PoolNo != 11)
            {
                fringAllocation = GetBaseLaborAccountDetails(fycd, 11);
            }


            var fringeCostAccounts = _context.PoolCostAccounts.Where(p => p.PoolNo == PoolNo).ToList();

            var GLReport = _context.PlFinancialTransactions
                .Where(x => fringeCostAccounts.Select(f => f.AcctId).Contains(x.AcctId) && x.FyCd == fycd)
                .GroupBy(x => new { x.AcctId, x.OrgId, x.PdNo, x.FyCd })
                .Select(g => new PlFinancialTransaction
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    FyCd = g.Key.FyCd,
                    PdNo = g.Key.PdNo,
                    Amt1 = g.Sum(x => x.Amt1 ?? 0)
                })
                .ToList();

            var AllocationAccountsRecords = fringAllocation.Where(p => GLReport.Select(q => q.AcctId).Contains(p.AcctId)).ToList();


            var result = _context.PlForecasts.Include(s => s.Pl)
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => fringeCostAccounts.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.DctId != null && x.Pl.FinalVersion == true)
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();

            //var forecastDict = result
            //        .ToDictionary(
            //            x => (x.AcctId, x.OrgId, x.Month, x.Year),
            //            x => x.TotalForecastedAmt
            //        );

            var forecastDict = result.ToDictionary(
                        x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
                        x => x.TotalForecastedAmt
                    );

            var finalResult = GLReport
            .Where(t => t.OrgId != null && t.AcctId != null)
            .GroupBy(t => new
            {
                t.OrgId,
                t.AcctId,
            })
            .Select(g => new PoolOrgCostFinancialDetail
            {
                OrgId = g.Key.OrgId,
                AcctId = g.Key.AcctId,

                YTDActualAmt = g.Sum(x => x.Amt1 ?? 0),
                YTDBudgetedAmt = g.Sum(x => x.Amt ?? 0),

                PeriodDetails = g
                    .GroupBy(x => x.PdNo ?? 0)
                    .Select(p => new PeriodCostFinancialDetail
                    {
                        Period = p.Key,
                        Actualamt = p.Sum(x => x.Amt1 ?? 0),
                        BudgetedAmt = p.Sum(x => x.Amt ?? 0)
                    })
                    .OrderBy(p => p.Period)
                    .ToList()
            })
            .ToList();



            return finalResult;
        }

        public List<PoolOrgBaseFinancialDetail> GetBaseLaborAccountDetails(string fycd, int PoolNo)
        {
            var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == PoolNo).ToList();

            var GLReport = _context.PlFinancialTransactions
                .Where(x => baseLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.FyCd == fycd)
                .GroupBy(x => new { x.AcctId, x.OrgId, x.PdNo, x.FyCd })
                .Select(g => new PlFinancialTransaction
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    FyCd = g.Key.FyCd,
                    PdNo = g.Key.PdNo,
                    Amt1 = g.Sum(x => x.Amt1 ?? 0),

                })
                .ToList();

            var result = _context.PlForecasts.Include(s => s.Pl)
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => baseLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.DctId != null && x.Pl.FinalVersion == true)
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();

            //var forecastDict = result
            //        .ToDictionary(
            //            x => (x.AcctId, x.OrgId, x.Month, x.Year),
            //            x => x.TotalForecastedAmt
            //        );

            var forecastDict = result.ToDictionary(
                        x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
                        x => x.TotalForecastedAmt
                    );

            return GLReport
                        .Where(t => t.OrgId != null && t.AcctId != null)
                        .GroupBy(t => new
                        {
                            t.OrgId,
                            t.AcctId,
                        })
                        .Select(g => new PoolOrgBaseFinancialDetail
                        {
                            OrgId = g.Key.OrgId,
                            AcctId = g.Key.AcctId,

                            AllocationAcctId = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == PoolNo && p.AcctId == g.Key.AcctId)
                                .Select(p => p.AllocAcctId)
                                .FirstOrDefault(),
                            AllocationOrgId = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == PoolNo && p.AcctId == g.Key.AcctId)
                                .Select(p => p.AllocOrgId)
                                .FirstOrDefault(),
                            YTDActualAmt = g.Sum(x => x.Amt1 ?? 0),
                            YTDBudgetedAmt = g.Sum(x => x.Amt ?? 0),

                            PeriodDetails = g
                                .GroupBy(x => x.PdNo ?? 0)
                                .Select(p => new PeriodbaseFinancialDetail
                                {
                                    Period = p.Key,
                                    baseAmt = p.Sum(x => x.Amt1 ?? 0)
                                })
                                .OrderBy(p => p.Period)
                                .ToList()
                        })
                        .ToList();



            //return GLReport
            //.Where(t => t.OrgId != null && t.AcctId != null)
            //.GroupBy(t => new
            //{
            //    t.OrgId,
            //    t.AcctId,
            //})
            //.Select(g => new PoolOrgBaseFinancialDetail
            //{
            //    OrgId = g.Key.OrgId,
            //    AcctId = g.Key.AcctId,
            //    //AllocationAcctId = baseLaborAccountFinancialDetails.Where(p=>p.PoolNo == PoolNo && p.AcctId == g.Key.AcctId)

            //    YTDActualAmt = g.Sum(x => x.Amt1 ?? 0),
            //    YTDBudgetedAmt = g.Sum(x => x.Amt ?? 0),


            //    PeriodDetails = g
            //        .GroupBy(x => x.PdNo ?? 0)
            //        .Select(p => new PeriodbaseFinancialDetail
            //        {
            //            Period = p.Key,
            //            baseAmt = p.Sum(x => x.Amt1 ?? 0)
            //        })
            //        .OrderBy(p => p.Period)
            //        .ToList()
            //})
            //.ToList();
        }

        public class ForecastSummaryDto
        {
            public string AcctId { get; set; }
            public string OrgId { get; set; }
            public int Month { get; set; }
            public int Year { get; set; }
            public decimal TotalForecastedAmt { get; set; }
        }


        public List<ForecastSummaryDto> GetCostForecasts(string fycd, int PoolNo)
        {
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
            //var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == PoolNo).ToList();
            var costLaborAccountFinancialDetails = _context.PoolCostAccounts.Where(p => p.PoolNo == PoolNo).ToList();

            var result = _context.PlForecasts.Include(s => s.Pl)
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => costLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.Month >= ClosedPeriod.Month && x.Pl.FinalVersion == true && x.Pl.PlType.ToUpper().Equals("EAC"))
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new ForecastSummaryDto
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();

            //var result = _context.PlForecasts.Include(s => s.Pl)
            //    //.Where(f => f.DctId != null)     // <-- added condition
            //    .Where(x => costLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.Month >= ClosedPeriod.Month && x.Pl.FinalVersion == true && x.Pl.PlType.ToUpper().Equals("EAC"))
            //    .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
            //    .Select(g => new ForecastSummaryDto
            //    {
            //        AcctId = g.Key.AcctId,
            //        OrgId = g.Key.OrgId,
            //        Month = g.Key.Month,
            //        Year = g.Key.Year,
            //        TotalForecastedAmt = g.Sum(x => x.Cost)
            //    })
            //    .ToList();

            return result;
            //var forecastDict = result.ToDictionary(
            //            x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
            //            x => x.TotalForecastedAmt
            //        );


        }

        public List<ForecastSummaryDto> GetBaseForecasts(string fycd, int PoolNo)
        {
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
            var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == PoolNo).ToList();

            var result = _context.PlForecasts.Include(s => s.Pl)
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => baseLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.Month >= ClosedPeriod.Month && x.Pl.FinalVersion == true )
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new ForecastSummaryDto
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Cost)
                })
                .ToList();

            return result;
            //var forecastDict = result.ToDictionary(
            //            x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
            //            x => x.TotalForecastedAmt
            //        );


        }


        public void GetFringeForForecast(string fycd)
        { 
            int PoolNo = 11;
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
            //var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == PoolNo).ToList();
            var costLaborAccountFinancialDetails = _context.PoolCostAccounts.Where(p => p.PoolNo == PoolNo).ToList();

            var result = _context.PlForecasts.Include(s => s.Pl)
                //.Where(f => f.DctId != null)     // <-- added condition
                .Where(x => costLaborAccountFinancialDetails.Select(f => f.AcctId).Contains(x.AcctId) && x.Year.ToString() == fycd && x.Month >= ClosedPeriod.Month && x.DctId != null && x.Pl.FinalVersion == true)
                .GroupBy(f => new { f.AcctId, f.OrgId, f.Month, f.Year })
                .Select(g => new ForecastSummaryDto
                {
                    AcctId = g.Key.AcctId,
                    OrgId = g.Key.OrgId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    TotalForecastedAmt = g.Sum(x => x.Forecastedamt ?? 0)
                })
                .ToList();


            //var forecastDict = result.ToDictionary(
            //            x => (x.AcctId, x.OrgId, (int?)x.Month, x.Year.ToString()),
            //            x => x.TotalForecastedAmt
            //        );


        }

    }
}
