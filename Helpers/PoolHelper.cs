using PlanningAPI.DTO;
using PlanningAPI.Models;
using NPOI.SS.Formula.Functions;

public class PoolRateHelper
{
    private readonly MydatabaseContext _context;

    public PoolRateHelper(MydatabaseContext context) => _context = context;


    public StatusResponse<List<PlTemplatePoolRate>> UpsertPoolRatesForTemplate(List<PlTemplatePoolRate> plPoolRates, string updatedBy)
    {

        foreach (var rate in plPoolRates)
        {
            var existing = _context.PlTemplatePoolRates
                .FirstOrDefault(x =>
                    x.TemplateId == rate.TemplateId &&
                    x.PoolId == rate.PoolId &&
                    x.Year == rate.Year &&
                    x.Month == rate.Month);

            if (existing != null)
            {
                // Update existing record
                existing.ActualRate = rate.ActualRate;
                existing.TargetRate = rate.TargetRate;
                existing.ModifiedBy = rate.ModifiedBy;
                existing.UpdatedAt = rate.UpdatedAt;
            }
            else
            {
                // Insert new record
                _context.PlTemplatePoolRates.Add(rate);
            }
        }

        _context.SaveChanges();

        return new StatusResponse<List<PlTemplatePoolRate>>(true, "Configuration Saved", plPoolRates);
    }

    public StatusResponse<List<PlPoolRate>> ConfigurePool(PoolRateDTO dto, string updatedBy)
    {
        var poolRates = new List<PlPoolRate>();

        for (int month = 1; month <= (dto.Month == 0 ? 12 : 1); month++)
        {
            poolRates.Add(new PlPoolRate
            {
                ActualRate = dto.ActualRate,
                AccountId = dto.AcctID.ToString(),
                AccountGroupCode = dto.PoolID,
                BurdenTemplateId = dto.TemplateId,
                TargetRate = dto.TargetRate,
                OrgId = dto.OrgID.ToString(),
                Year = dto.Year ?? 0,
                Month = dto.Month == 0 ? month : dto.Month.Value
            });
        }

        _context.PlPoolRates.AddRange(poolRates);
        _context.SaveChanges();

        return new StatusResponse<List<PlPoolRate>>(true, "Configuration Saved", poolRates);
    }

    public StatusResponse<List<PlPoolRate>> GetRates(PoolRateDTO dto)
    {
        var rates = _context.PlPoolRates
            .Where(p => p.AccountGroupCode == dto.PoolID &&
                        p.Year == dto.Year &&
                        p.OrgId == dto.OrgID &&
                        p.AccountId == dto.AcctID &&
                        p.BurdenTemplateId == dto.TemplateId)
            .ToList();

        return new StatusResponse<List<PlPoolRate>>(true, "Success", rates);
    }

    public StatusResponse<PlPoolRate> UpdateRate(PoolRateDTO dto)
    {
        try
        {
            if (dto.Month == 0)
            {
                var rates = _context.PlPoolRates
                    .Where(p => p.AccountGroupCode == dto.PoolID &&
                                p.Year == dto.Year &&
                                p.OrgId == dto.OrgID &&
                                p.AccountId == dto.AcctID &&
                                p.BurdenTemplateId == dto.TemplateId)
                    .ToList();

                if (!rates.Any())
                    return NotFoundRate(dto);

                foreach (var rate in rates)
                {
                    rate.ActualRate = dto.ActualRate;
                    rate.TargetRate = dto.TargetRate;
                }

                _context.PlPoolRates.UpdateRange(rates);
            }
            else
            {
                var rate = _context.PlPoolRates.FirstOrDefault(p =>
                    p.AccountGroupCode == dto.PoolID &&
                    p.Year == dto.Year &&
                    p.Month == dto.Month &&
                    p.OrgId == dto.OrgID &&
                    p.AccountId == dto.AcctID &&
                    p.BurdenTemplateId == dto.TemplateId);

                if (rate == null)
                    return NotFoundRate(dto);

                rate.ActualRate = dto.ActualRate;
                rate.TargetRate = dto.TargetRate;

                _context.PlPoolRates.Update(rate);
            }

            _context.SaveChanges();
            return new StatusResponse<PlPoolRate>(true, "Success", new PlPoolRate { Id = dto.Id, ActualRate = dto.ActualRate, TargetRate = dto.TargetRate });
        }
        catch (Exception ex)
        {
            return new StatusResponse<PlPoolRate>(false, ex.Message, new PlPoolRate { Id = dto.Id });
        }
    }

    public StatusResponse<PlPoolRate> GetRate(PoolRateDTO dto)
    {
        try
        {
            var rate = _context.PlPoolRates.FirstOrDefault(p =>
                p.AccountGroupCode == dto.PoolID &&
                p.Year == dto.Year &&
                p.Month == dto.Month &&
                p.OrgId == dto.OrgID &&
                p.AccountId == dto.AcctID &&
                p.BurdenTemplateId == dto.TemplateId);

            if (rate == null)
                return NotFoundRate(dto);

            return new StatusResponse<PlPoolRate>(true, "Success", rate);
        }
        catch (Exception ex)
        {
            return new StatusResponse<PlPoolRate>(false, ex.Message, new PlPoolRate { Id = dto.Id });
        }
    }

    private StatusResponse<PlPoolRate> NotFoundRate(PoolRateDTO dto) =>
        new StatusResponse<PlPoolRate>(false, "Not Found", new PlPoolRate
        {
            Id = dto.Id,
            ActualRate = dto.ActualRate,
            TargetRate = dto.TargetRate
        });

    internal StatusResponse<BurdenTemplate> CreateTemplate(BurdenTemplate template, string updatedBy)
    {
        try
        {
            var entry = _context.BurdenTemplates.Add(template);
            _context.SaveChanges();
            return new StatusResponse<BurdenTemplate>(true, "Success", entry.Entity);
        }
        catch (Exception ex)
        {
            return new StatusResponse<BurdenTemplate>(false, ex.InnerException.Message, template);
        }
    }

    internal StatusResponse<BurdenTemplate> DeleteTemplate(BurdenTemplate template, string updatedBy)
    {
        try
        {
            var entry = _context.BurdenTemplates.Remove(template);
            _context.SaveChanges();
            return new StatusResponse<BurdenTemplate>(true, "Success", entry.Entity);
        }
        catch (Exception ex)
        {
            return new StatusResponse<BurdenTemplate>(false, ex.InnerException.Message, template);
        }
    }
}
