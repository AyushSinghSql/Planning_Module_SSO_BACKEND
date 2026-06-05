using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NPOI.SS.Formula.Functions;
using PlanningAPI.DTO;
using PlanningAPI.Models;
using System.Dynamic;
using System.Text.Json;
using WebApi.Controllers;
using WebApi.Helpers;

namespace PlanningAPI.Helpers
{
    public class Account_Org_Helpercs
    {
        MydatabaseContext _context;
        private readonly ILogger<OrgnizationController> _logger;

        public Account_Org_Helpercs(MydatabaseContext context, ILogger<OrgnizationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Account_Org_Helpercs(MydatabaseContext context)
        {
            _context = context;
        }
        public async Task BulkUpsertEfLoopAsync(JsonElement recordss)
        {
            List<ExpandoObject> records = new List<ExpandoObject>();
            var list = JsonExtensions.JsonElementToObject(recordss) as List<object?>;
            Helper helper = new Helper();

            foreach (var item in list)
            {
                if (item is ExpandoObject expando)
                {
                    records.Add(expando);
                }
            }

            var flatList = records.SelectMany(item =>
            {
                var entries = new List<PlOrgAcctPoolMapping>();
                var dict = (IDictionary<string, object?>)item;

                string? acctId = helper.GetValue<string>(dict, "acctId");
                string? orgId = helper.GetValue<string>(dict, "orgId");

                int year = 0;
                if (dict.TryGetValue("Year", out var yearVal) && yearVal != null)
                {
                    if (yearVal is long l)
                        year = (int)l;
                    else if (yearVal is int i)
                        year = i;
                    else if (int.TryParse(yearVal.ToString(), out var parsed))
                        year = parsed;
                }

                foreach (var kv in dict)
                {
                    string key = kv.Key;
                    var value = kv.Value;

                    if (value is bool b && b)
                    {
                        entries.Add(new PlOrgAcctPoolMapping
                        {
                            PoolId = key,
                            AccountId = acctId,
                            OrgId = orgId,
                            Year = year
                        });
                    }
                }

                return entries;
            }).ToList();


            //var flatList = records.SelectMany(item =>
            //{
            //    var entries = new List<PlOrgAcctPoolMapping>();
            //    var dict = (IDictionary<string, object?>)item;

            //    foreach (var kv in (IDictionary<string, object?>)item)
            //    {
            //        string key = kv.Key;
            //        var value = kv.Value;

            //        if (value is bool b)
            //        {
            //            if (Convert.ToBoolean(value))
            //                entries.Add(new PlOrgAcctPoolMapping { PoolId = key, AccountId = helper.GetValue<string>(dict, "acctId"), OrgId = helper.GetValue<string>(dict, "orgId"), Year = helper.GetValue<int>(dict, "Year") });
            //        }

            //    }

            //    return entries;
            //}).ToList();

            var removeList = records.SelectMany(item =>
            {
                var entries = new List<PlOrgAcctPoolMapping>();

                var dict = (IDictionary<string, object?>)item;

                int year = 0;
                if (dict.TryGetValue("Year", out var yearVal) && yearVal != null)
                {
                    if (yearVal is long l)
                        year = (int)l;
                    else if (yearVal is int i)
                        year = i;
                    else if (int.TryParse(yearVal.ToString(), out var parsed))
                        year = parsed;
                }

                foreach (var kv in (IDictionary<string, object?>)item)
                {
                    string key = kv.Key;
                    var value = kv.Value;

                    if (value is bool b)
                    {
                        if (!Convert.ToBoolean(value))
                            entries.Add(new PlOrgAcctPoolMapping { PoolId = key, AccountId = helper.GetValue<string>(dict, "acctId"), OrgId = helper.GetValue<string>(dict, "orgId"), Year = year });
                    }
                }

                return entries;
            }).ToList();

            foreach (var item in removeList)
            {
                // Check for existing record using the unique key
                var existing = _context.PlOrgAcctPoolMappings
                .FirstOrDefault(x => x.OrgId == item.OrgId
                                       && x.AccountId == item.AccountId
                                       && x.PoolId == item.PoolId && x.Year == item.Year);

                if (existing != null)
                {
                    _context.PlOrgAcctPoolMappings.Remove(existing);
                }
            }

            foreach (var item in flatList)
            {
                // Check for existing record using the unique key
                var existing = _context.PlOrgAcctPoolMappings
                .FirstOrDefault(x => x.OrgId == item.OrgId
                                       && x.AccountId == item.AccountId
                                       && x.PoolId == item.PoolId && x.Year == item.Year);

                if (existing == null)
                {
                    // Ensure required fields like 'ModifiedBy' are not null
                    item.ModifiedBy ??= "system";
                    item.CreatedAt = DateTime.UtcNow;
                    item.UpdatedAt = DateTime.UtcNow;

                    _context.PlOrgAcctPoolMappings.Add(item);
                }

            }
            await _context.SaveChangesAsync();
        }

        public List<dynamic> GetAccountPools(int Year)
        {

            //var grps = _context.OrgAccounts.Select(e => new PlOrgAcctPoolMapping
            //{
            //    PoolId ="",
            //    AccountId = e.AcctId,
            //    OrgId = e.OrgId
            //});

            var grps = _context.OrgAccounts
                .Select(e => new PlOrgAcctPoolMapping
                {
                    PoolId = "",                  // Empty or default string
                    AccountId = e.AcctId,
                    OrgId = e.OrgId
                })
                .Select(e => new
                {
                    e.PoolId,
                    e.AccountId,
                    e.OrgId
                });
            var templates = _context.PlOrgAcctPoolMappings.Where(p => p.Year == Year).ToList().Select(e => new
            {
                e.PoolId,
                e.AccountId,
                e.OrgId
            }).Distinct().ToList();

            var poolKeys = _context.AccountGroups.Select(p => p.Code).ToList();

            var mergedDistinct = templates.Union(grps.ToList()).ToList();

            //var result = templates
            //    .GroupBy(x => new { x.OrgId, x.AccountId })
            //    .Select(g =>
            //    {
            //        dynamic expando = new ExpandoObject();
            //        var dict = (IDictionary<string, object?>)expando;

            //        // Add static fields
            //        dict["orgId"] = g.Key.OrgId;
            //        dict["acctId"] = g.Key.AccountId;

            //        // Add dynamic pool flags
            //        foreach (var pool in poolKeys)
            //        {
            //            dict[pool] = g.Any(x => x.PoolId == pool);
            //        }

            //        return expando;
            //    })
            //    .ToList();

            //var result = mergedDistinct
            //    .GroupBy(x => new { x.OrgId, x.AccountId })
            //    .Select(g =>
            //    {
            //        dynamic expando = new ExpandoObject();
            //        var dict = (IDictionary<string, object?>)expando;

            //        // Add static fields
            //        dict["orgId"] = g.Key.OrgId;
            //        dict["acctId"] = g.Key.AccountId;

            //        // Add dynamic pool flags
            //        foreach (var pool in poolKeys)
            //        {
            //            dict[pool] = g.Any(x => x.PoolId == pool);
            //        }

            //        return expando;
            //    })
            //    .ToList();

            var result =
                (from x in mergedDistinct
                 join a in _context.Accounts
                    on x.AccountId equals a.AcctId into acc
                 from a in acc.DefaultIfEmpty()
                 select new
                 {
                     x.OrgId,
                     x.AccountId,
                     x.PoolId,
                     AccountName = a != null ? a.AcctName : null
                 })
                .GroupBy(x => new
                {
                    x.OrgId,
                    x.AccountId,
                    x.AccountName
                })
                .Select(g =>
                {
                    dynamic expando = new ExpandoObject();
                    var dict = (IDictionary<string, object?>)expando;

                    dict["orgId"] = g.Key.OrgId;
                    dict["acctId"] = g.Key.AccountId;
                    dict["accountName"] = g.Key.AccountName;

                    foreach (var pool in poolKeys)
                    {
                        dict[pool] = g.Any(x => x.PoolId == pool);
                    }

                    return expando;
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} active pools at {Time}", templates.Count, DateTime.UtcNow);
            return result;
        }

        public List<dynamic> GetAccountPoolsV1(int? Year, string? acctId, string? orgId)
        {

            //var grps = _context.OrgAccounts.Select(e => new PlOrgAcctPoolMapping
            //{
            //    PoolId ="",
            //    AccountId = e.AcctId,
            //    OrgId = e.OrgId
            //});

            var grps = _context.OrgAccounts
                .Select(e => new PlOrgAcctPoolMapping
                {
                    PoolId = "",                  // Empty or default string
                    AccountId = e.AcctId,
                    OrgId = e.OrgId
                })
                .Select(e => new
                {
                    e.PoolId,
                    e.AccountId,
                    e.OrgId
                });

            var templates = _context.PlOrgAcctPoolMappings
                            .Where(p =>
                                (p.Year != null && p.Year == Year)

                            )
                            .Select(e => new { e.PoolId, e.AccountId, e.OrgId })
                            .Distinct()
                            .ToList();

            //var templates = _context.PlOrgAcctPoolMappings.Where(p => p.Year == Year && p.AccountId.StartsWith(acctId) && p.OrgId.StartsWith(orgId)).ToList().Select(e => new
            //{
            //    e.PoolId,
            //    e.AccountId,
            //    e.OrgId
            //}).Distinct().ToList();

            var poolKeys = _context.AccountGroups.Select(p => p.Code).ToList();

            var mergedDistinct = templates.Union(grps.ToList()).ToList();

            var result = mergedDistinct
                .GroupBy(x => new { x.OrgId, x.AccountId })
                .Select(g =>
                {
                    dynamic expando = new ExpandoObject();
                    var dict = (IDictionary<string, object?>)expando;

                    // Add static fields
                    dict["orgId"] = g.Key.OrgId;
                    dict["acctId"] = g.Key.AccountId;

                    // Add dynamic pool flags
                    foreach (var pool in poolKeys)
                    {
                        dict[pool] = g.Any(x => x.PoolId == pool);
                    }

                    return expando;
                })
                .ToList().Where(g => acctId != null || g.Key.AccountId.StartsWith(acctId)).ToList();



            _logger.LogInformation("Retrieved {Count} active pools at {Time}", templates.Count, DateTime.UtcNow);
            return result;
        }

        internal List<PoolInfo> GetPoolsByTemplateId(int templateId)
        {
            var result = _context.PlTemplatePoolMappings
                        .Include(p => p.Pool)
                        .Where(p => p.TemplateId == templateId && p.Pool != null)
                        .OrderBy(p => p.Pool.Sequence)
                        .Select(e => new PoolInfo
                        {
                            PoolId = e.PoolId,
                            TemplateId = e.TemplateId,
                            GroupName = e.Pool.Name,
                            Sequence = e.Pool.Sequence
                        })
                        .ToList();

            return result;
        }

        public async Task BulkUpsertTemplatePoolRatesAsync(List<PlTemplatePoolRate> items)
        {
            if (items == null || items.Count == 0)
                return;

            var sql = @"
        INSERT INTO pl_template_pool_rates
        (template_id, pool_id, year, month, actual_rate, target_rate, modified_by, created_at, updated_at)
        VALUES ";

            var parameters = new List<NpgsqlParameter>();
            var valueLines = new List<string>();

            int i = 0;

            foreach (var item in items)
            {
                string line = $"(@t{i}, @p{i}, @y{i}, @m{i}, @ar{i}, @tr{i}, @mb{i}, @ca{i}, @ua{i})";
                valueLines.Add(line);

                //        parameters.AddRange(new[]
                //        {
                //    new NpgsqlParameter($"t{i}", item.TemplateId),
                //    new NpgsqlParameter($"p{i}", item.PoolId),
                //    new NpgsqlParameter($"y{i}", item.Year),
                //    new NpgsqlParameter($"m{i}", item.Month),
                //    new NpgsqlParameter($"ar{i}", (object?)item.ActualRate ?? DBNull.Value),
                //    new NpgsqlParameter($"tr{i}", (object?)item.TargetRate ?? DBNull.Value),
                //    new NpgsqlParameter($"mb{i}", item.ModifiedBy),
                //    new NpgsqlParameter($"ca{i}", item.CreatedAt),
                //    new NpgsqlParameter($"ua{i}", item.UpdatedAt)
                //});
                parameters.AddRange(new[]
                        {
                    new NpgsqlParameter($"t{i}", item.TemplateId),
                    new NpgsqlParameter($"p{i}", item.PoolId),
                    new NpgsqlParameter($"y{i}", item.Year),
                    new NpgsqlParameter($"m{i}", item.Month),
                    new NpgsqlParameter($"ar{i}", (object?)item.ActualRate ?? DBNull.Value),
                    new NpgsqlParameter($"tr{i}", (object?)item.TargetRate ?? DBNull.Value),
                    new NpgsqlParameter($"mb{i}", (object?)item.ModifiedBy ?? DBNull.Value),
                    new NpgsqlParameter($"ca{i}", (object?)item.CreatedAt ?? DBNull.Value),
                    new NpgsqlParameter($"ua{i}", (object?)item.UpdatedAt ?? DBNull.Value)
                });


                i++;
            }

            sql += string.Join(",", valueLines);

            sql += @"
        ON CONFLICT (template_id, pool_id, year, month)
        DO UPDATE SET
            actual_rate = EXCLUDED.actual_rate,
            target_rate = EXCLUDED.target_rate,
            modified_by = EXCLUDED.modified_by,
            updated_at = EXCLUDED.updated_at;";

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }

        //internal List<string> GetPoolsByTemplateId(int templateId)
        //{
        //    var pools = _context.PlTemplatePoolMappings.Include(p => p.Pool)
        //        .Where(p => p.TemplateId == templateId)
        //        .OrderBy(p => p.Pool.Sequence)
        //        .Select(e => e.PoolId)
        //        .ToList();

        //    return pools;
        //}

        internal List<PlTemplatePoolRate> GetRatesByPoolsTemplateId(int templateId, string poolId, int year)
        {
            var pools = _context.PlTemplatePoolRates
                        .Where(p => p.TemplateId == templateId && p.PoolId == poolId && p.Year == year).ToList();

            return pools;
        }

        internal void BulkUpSertTemplatePoolMapping(JsonElement recordss)
        {
            List<ExpandoObject> records = new List<ExpandoObject>();
            var list = JsonExtensions.JsonElementToObject(recordss) as List<object?>;
            Helper helper = new Helper();

            foreach (var item in list)
            {
                if (item is ExpandoObject expando)
                {
                    records.Add(expando);
                }
            }

            var flatList = records.SelectMany(item =>
            {
                var entries = new List<PlTemplatePoolMapping>();
                var dict = (IDictionary<string, object?>)item;

                foreach (var kv in (IDictionary<string, object?>)item)
                {
                    string key = kv.Key;
                    var value = kv.Value;

                    if (value is bool b)
                    {
                        if (Convert.ToBoolean(value))
                            entries.Add(new PlTemplatePoolMapping { PoolId = key, TemplateId = Convert.ToInt16(helper.GetValues<int>(dict, "templateId")) });
                    }

                }

                return entries;
            }).ToList();

            var removeList = records.SelectMany(item =>
            {
                var entries = new List<PlTemplatePoolMapping>();

                var dict = (IDictionary<string, object?>)item;

                foreach (var kv in (IDictionary<string, object?>)item)
                {
                    string key = kv.Key;
                    var value = kv.Value;

                    if (value is bool b)
                    {
                        if (!Convert.ToBoolean(value))
                            entries.Add(new PlTemplatePoolMapping { PoolId = key, TemplateId = Convert.ToInt16(helper.GetValues<int>(dict, "templateId")) });
                    }
                }

                return entries;
            }).ToList();

            foreach (var item in removeList)
            {
                // Check for existing record using the unique key
                var existing = _context.PlTemplatePoolMappings
                .FirstOrDefault(x => x.TemplateId == item.TemplateId
                                       && x.PoolId == item.PoolId);

                if (existing != null)
                {
                    _context.PlTemplatePoolMappings.Remove(existing);
                    var poolratesToDelete = _context.PlTemplatePoolRates.Where(p => p.PoolId == item.PoolId && p.TemplateId == item.TemplateId).ToList();
                    _context.PlTemplatePoolRates.RemoveRange(poolratesToDelete);
                }
            }

            foreach (var item in flatList)
            {
                // Check for existing record using the unique key
                var existing = _context.PlTemplatePoolMappings
                .FirstOrDefault(x => x.TemplateId == item.TemplateId
                                       && x.PoolId == item.PoolId);

                if (existing == null)
                {
                    // Ensure required fields like 'ModifiedBy' are not null
                    item.ModifiedBy ??= "system";
                    item.CreatedAt = DateTime.UtcNow;

                    _context.PlTemplatePoolMappings.Add(item);
                }
            }
            _context.SaveChanges();
        }

        internal List<PoolInfo> GetPoolsByOrgAccount(string accountId, string orgId)
        {
            var result = _context.PlOrgAcctPoolMappings.Where(p => p.AccountId == accountId && p.OrgId == orgId).Select(e => new PoolInfo
            {
                PoolId = e.PoolId,
                GroupName = e.Pool.Name,
                Sequence = e.Pool.Sequence
            }).ToList();

            return result;
        }

        //public List<dynamic> GetAccountPools()
        //{
        //    var templates = _context.PlOrgAcctPoolMappings.Select(e => new
        //    {
        //        e.PoolId,
        //        e.AccountId,
        //        e.OrgId
        //    }).Distinct().ToList();

        //    var poolKeys = _context.AccountGroups.Select(p => p.GroupCode).ToList();

        //    var result = templates
        //        .GroupBy(x => new { x.OrgId, x.AccountId })
        //        .Select(g =>
        //        {
        //            dynamic expando = new ExpandoObject();
        //            var dict = (IDictionary<string, object?>)expando;

        //            // Add static fields
        //            dict["orgId"] = g.Key.OrgId;
        //            dict["acctId"] = g.Key.AccountId;

        //            // Add dynamic pool flags
        //            foreach (var pool in poolKeys)
        //            {
        //                dict[pool] = g.Any(x => x.PoolId == pool);
        //            }

        //            return expando;
        //        })
        //        .ToList();
        //    _logger.LogInformation("Retrieved {Count} active pools at {Time}", templates.Count, DateTime.UtcNow);
        //    return result;
        //}
    }
}
