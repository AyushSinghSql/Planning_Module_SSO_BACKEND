using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;
using System.Text;

namespace PlanningAPI.Helpers
{
    public class ConfigurationHelper
    {
        private readonly MydatabaseContext _context;

        public ConfigurationHelper()
        {
        }

        public ConfigurationHelper(MydatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<PlConfigValue>> GetAllConfigValuesByProject(string projID)
        {
            return await _context.PlConfigValues.Where(p => p.ProjId.Trim().ToUpper() == projID.Trim().ToUpper()).ToListAsync();
        }


        public async Task<PlConfigValue> GetConfigValueByName(string name)
        {
            var config = await _context.PlConfigValues
                .FirstOrDefaultAsync(p => p.Name == name);

            if (config == null)
            {
                return null;
            }

            return config;
        }

        public async Task<PlConfigValue> AddConfigValue(PlConfigValue plConfigValue)
        {
            _context.PlConfigValues.Add(plConfigValue);
            await _context.SaveChangesAsync();
            return plConfigValue;
        }


        public async Task<bool> DeleteConfigValueByName(string name)
        {
            var config = await _context.PlConfigValues.FindAsync(name);
            if (config == null)
                return false;

            _context.PlConfigValues.Remove(config);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteConfigValueById(int id)
        {
            var config = await _context.PlConfigValues.FirstOrDefaultAsync(p => p.Id == id);
            if (config == null)
                return false;

            _context.PlConfigValues.Remove(config);
            await _context.SaveChangesAsync();
            return true;
        }
        //public async Task<bool> UpdateConfigValue(PlConfigValue plConfigValue)
        //{
        //    var existing = await _context.PlConfigValues.FirstOrDefaultAsync(p => p.Id == plConfigValue.Id);
        //    if (existing == null)
        //        return false;

        //    // Update fields
        //    existing.Value = plConfigValue.Value;
        //    _context.PlConfigValues.Update(existing);

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //        return true;
        //    }
        //    catch (DbUpdateException ex)
        //    {
        //        return false;
        //    }
        //}
        //public async Task<bool> UpdateConfigValuesAsync(List<PlConfigValue> configValues)
        //{
        //    const int maxRetries = 3;

        //    for (int attempt = 0; attempt < maxRetries; attempt++)
        //    {
        //        try
        //        {
        //            // Load all existing records that match the IDs in the input list
        //            var ids = configValues.Select(c => c.Id).ToList();
        //            var existingConfigs = await _context.PlConfigValues
        //                                                .Where(c => ids.Contains(c.Id))
        //                                                .ToListAsync();

        //            if (!existingConfigs.Any())
        //                return false; // No matching records found

        //            // Update each existing record
        //            foreach (var existing in existingConfigs)
        //            {
        //                var updatedConfig = configValues.First(c => c.Id == existing.Id);
        //                existing.Value = updatedConfig.Value;

        //                // Extend here for other fields if needed
        //                // existing.Description = updatedConfig.Description;
        //            }

        //            // Save all updates in a single batch
        //            await _context.SaveChangesAsync();
        //            return true; // Success
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            // Concurrency conflict occurred; retry the loop
        //            // Optionally refresh the context
        //            foreach (var entry in _context.ChangeTracker.Entries())
        //            {
        //                await entry.ReloadAsync();
        //            }
        //        }
        //        catch (DbUpdateException)
        //        {
        //            // Handle other DB errors (e.g., unique constraint violation)
        //            return false;
        //        }
        //    }

        //    return false; // Failed after retries
        //}


        public async Task<bool> UpdateConfigValuesAsync(List<PlConfigValue> configValues)
        {
            if (configValues == null || !configValues.Any())
                return false;

            // Build parameterized VALUES list
            var parameters = new List<Npgsql.NpgsqlParameter>();
            var valueRows = new List<string>();
            int index = 0;

            foreach (var config in configValues)
            {
                var nameParam = new Npgsql.NpgsqlParameter($"@p{index}_name", config.Name);
                var valueParam = new Npgsql.NpgsqlParameter($"@p{index}_value", (object?)config.Value ?? DBNull.Value);
                var createdAtParam = new Npgsql.NpgsqlParameter($"@p{index}_created_at", (object?)config.CreatedAt ?? DateTime.UtcNow);
                var projIdParam = new Npgsql.NpgsqlParameter($"@p{index}_proj_id", config.ProjId);

                parameters.AddRange(new[] { nameParam, valueParam, createdAtParam, projIdParam });

                valueRows.Add($"({nameParam.ParameterName}, {valueParam.ParameterName}, {createdAtParam.ParameterName}, {projIdParam.ParameterName})");

                index++;
            }

            var sql = $@"
                        INSERT INTO public.pl_config_values (name, value, created_at, proj_id)
                        VALUES {string.Join(", ", valueRows)}
                        ON CONFLICT (name, proj_id) 
                        DO UPDATE SET
                            value = EXCLUDED.value,
                            created_at = EXCLUDED.created_at;";

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }


        public async Task<bool> UpdateConfigValue(PlConfigValue plConfigValue)
        {
            for (int i = 0; i < 3; i++) // Retry up to 3 times
            {
                try
                {
                    var existing = await _context.PlConfigValues
                                                 .FirstOrDefaultAsync(p => p.Id == plConfigValue.Id);
                    if (existing == null)
                        return false; // Record deleted

                    existing.Value = plConfigValue.Value;
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Refresh context and retry
                    _context.Entry(plConfigValue).Reload();
                }
            }

            return false; // Failed after retries
        }

        public async Task BulkUpsertAsync(List<PlConfigValue> items)
        {
            if (!items.Any()) return;

            var sql = new StringBuilder();
            var parameters = new List<NpgsqlParameter>();

            sql.AppendLine("INSERT INTO pl_config_values (name, value, proj_id) VALUES");

            for (int i = 0; i < items.Count; i++)
            {
                sql.Append($"(@name{i}, @value{i}, @proj{i})");

                if (i < items.Count - 1)
                    sql.Append(",");

                parameters.Add(new NpgsqlParameter($"name{i}", items[i].Name));
                parameters.Add(new NpgsqlParameter($"value{i}", items[i].Value));
                parameters.Add(new NpgsqlParameter($"proj{i}", items[i].ProjId));
            }

            sql.AppendLine(@"
        ON CONFLICT (name, proj_id)
        DO UPDATE SET
            value = EXCLUDED.value,
            created_at = CURRENT_TIMESTAMP;
    ");
            try
            {

                await _context.Database.ExecuteSqlRawAsync(sql.ToString(), parameters);
            }
            catch (Exception ex)
            {

            }
        }


        public async Task<IEnumerable<Holidaycalender>> GetAllHolidayCalenderAsync()
        {
            return await _context.Holidaycalenders.OrderBy(p => p.Date).ToListAsync();
        }

        public async Task<Holidaycalender?> GetHolidayCalenderByIdAsync(int id)
        {
            return await _context.Holidaycalenders.FindAsync(id);
        }

        public async Task<Holidaycalender> AddHolidayCalenderAsync(Holidaycalender holiday)
        {
            var entry = await _context.Holidaycalenders.AddAsync(holiday);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }

        public async Task<bool> UpdateHolidayCalenderAsync(Holidaycalender holiday)
        {
            var existing = await _context.Holidaycalenders.FindAsync(holiday.Id);
            if (existing == null) return false;

            existing.Date = holiday.Date;
            existing.Type = holiday.Type;
            existing.Name = holiday.Name;
            existing.Ispublicholiday = holiday.Ispublicholiday;
            existing.State = holiday.State;
            existing.Remarks = holiday.Remarks;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteHolidayCalenderAsync(int id)
        {
            var holiday = await _context.Holidaycalenders.FindAsync(id);
            if (holiday == null) return false;

            _context.Holidaycalenders.Remove(holiday);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
