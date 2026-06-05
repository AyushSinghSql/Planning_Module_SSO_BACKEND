using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PlanningAPI.Models;
using SkiaSharp;
using SQLitePCL;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using WebApi.DTO;

public static class PlanningBulkInsertHelper
{
    public static async Task BulkInsertPlanningDataParallelWithTransactionsAsync<TContext>(
        TContext context,
        IConfiguration configuration,
        List<PlEmployeee> employees,
        List<PlDct> directCosts,
        Func<PlEmployeee, IEnumerable<PlForecast>> getForecastsForEmployee,
        Func<PlDct, IEnumerable<PlForecast>>? getForecastsForDct = null,
        int maxParallelTasks = 4,
        int forecastChunkSize = 4000
    ) where TContext : DbContext
    {
        var swTotal = Stopwatch.StartNew();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        int batchSize = configuration.GetValue<int>("BatchSize", 300);
        int ParallelTasks = configuration.GetValue<int>("ParallelTasks", 1);
        // ============================
        // STEP 0: Clone Employees & DCT for Forecast Extraction
        // ============================
        var employeeClone = employees
            .Select(e => new
            {
                Employee = e,
                Forecasts = e.PlForecasts?.ToList() ?? new List<PlForecast>()
            })
            .ToList();

        var dctClone = directCosts?
            .Select(d => new
            {
                Dct = d,
                Forecasts = d.PlForecasts?.ToList() ?? new List<PlForecast>()
            })
            .ToList();

        // ============================
        // STEP 1: Bulk Insert Employees
        // ============================
        foreach (var emp in employees)
        {
            emp.PlForecasts = null;
            emp.Organization = null;
            emp.PlProjectPlan = null;
        }

        var swEmp = Stopwatch.StartNew();
        await context.BulkInsertAsync(employees, new BulkConfig { SetOutputIdentity = true });
        await context.SaveChangesAsync();
        swEmp.Stop();
        Console.WriteLine($"✅ Inserted {employees.Count} employees in {swEmp.Elapsed.TotalSeconds:F2}s");

        // ============================
        // STEP 2: Bulk Insert Direct Costs
        // ============================
        if (directCosts != null && directCosts.Count > 0)
        {
            foreach (var dct in directCosts)
                dct.PlForecasts = null;

            var swDct = Stopwatch.StartNew();
            await context.BulkInsertAsync(directCosts, new BulkConfig { SetOutputIdentity = true });
            await context.SaveChangesAsync();
            swDct.Stop();
            Console.WriteLine($"✅ Inserted {directCosts.Count} direct costs in {swDct.Elapsed.TotalSeconds:F2}s");
        }

        // ============================
        // STEP 3: Prepare All Forecasts
        // ============================
        var allForecasts = new List<PlForecast>();

        foreach (var item in employeeClone)
        {
            var emp = item.Employee;
            foreach (var forecast in item.Forecasts)
            {
                CleanForecastForInsert(forecast, emp.Id, null);
                allForecasts.Add(forecast);
            }
        }

        if (dctClone != null)
        {
            foreach (var item in dctClone)
            {
                var dct = item.Dct;
                foreach (var forecast in item.Forecasts)
                {
                    CleanForecastForInsert(forecast, null, dct.DctId);
                    allForecasts.Add(forecast);
                }
            }
        }

        Console.WriteLine($"📊 Total forecasts prepared: {allForecasts.Count}");




        //// insert 500 records per chunk using 4 parallel tasks (-- Working --)
        await InsertPlForecastsParallelAsync(
            allForecasts,
            chunkSize: batchSize,
            maxParallelTasks: 1,
            connectionString: connectionString
        );

        //// insert 500 records per chunk using 4 parallel tasks (-- Working --)
        //await InsertEntitiesCopyAsync<MydatabaseContext, PlForecast>(
        //    allForecasts,
        //    () => new MydatabaseContext(
        //        new DbContextOptionsBuilder<MydatabaseContext>()
        //            .UseNpgsql(connectionString, o =>
        //            {
        //                o.CommandTimeout(300);
        //                o.EnableRetryOnFailure(5);
        //            })
        //            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        //            .Options),
        //    connectionString,
        //    chunkSize: batchSize,       // ✅ Small batch
        //    maxParallelTasks: ParallelTasks); // ✅ No parallelism





        // ============================
        // STEP 4: Parallel Insert of Forecasts
        // ============================
        //await InsertPlForecastsInParallelAsync(allForecasts, forecastChunkSize, context, maxParallelTasks);

        swTotal.Stop();
        Console.WriteLine($"🎯 Bulk insert completed in {swTotal.Elapsed.TotalSeconds:F2}s");
    }


    public static async Task InsertForecastsSequentialAsync(
    DbContext context,
    List<PlForecast> forecasts,
    int chunkSize = 500)
    {
        if (forecasts == null || forecasts.Count == 0)
            return;

        // Disable change tracking to save memory
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        for (int i = 0; i < forecasts.Count; i += chunkSize)
        {
            var chunk = forecasts.Skip(i).Take(chunkSize).ToList();

            // Optional: clean navigations to avoid EF trying to insert related entities
            foreach (var f in chunk)
            {
                f.Empl = null;
                f.Emple = null;
                f.Pl = null;
                f.Proj = null;
                f.DirectCost = null;

                // Convert UTC DateTime to Unspecified to match timestamp without time zone
                if (f.Createdat.HasValue)
                    f.Createdat = DateTime.SpecifyKind(f.Createdat.Value, DateTimeKind.Unspecified);
                if (f.Updatedat.HasValue)
                    f.Updatedat = DateTime.SpecifyKind(f.Updatedat.Value, DateTimeKind.Unspecified);
            }

            await context.Set<PlForecast>().AddRangeAsync(chunk);
            await context.SaveChangesAsync();

            // Clear the ChangeTracker to free memory
            context.ChangeTracker.Clear();
        }

        // Re-enable change tracking
        context.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    //Worked only once
    //public static async Task InsertPlForecastsParallelAsync(
    //List<PlForecast> forecasts,
    //int chunkSize,
    //int maxParallelTasks,
    //string connectionString)
    //{
    //    if (forecasts == null || forecasts.Count == 0)
    //        return;

    //    var chunks = forecasts.Chunk(chunkSize).ToList();
    //    var throttler = new SemaphoreSlim(maxParallelTasks);

    //    var tasks = chunks.Select(async (chunk, index) =>
    //    {
    //        await throttler.WaitAsync();
    //        try
    //        {
    //            await using var conn = new NpgsqlConnection(connectionString);
    //            await conn.OpenAsync();

    //            // 🧭 Use COPY for fast insert
    //            var copyCommand = @"
    //                COPY pl_forecast (
    //                    forecastedamt,
    //                    proj_id,
    //                    pl_id,
    //                    empl_id,
    //                    month,
    //                    year,
    //                    forecastedhours,
    //                    dct_id,
    //                    acct_id,
    //                    org_id,
    //                    hrly_rate,
    //                    plc,
    //                    actualamt,
    //                    actualhours,
    //                    forecastedcost
    //                )
    //                FROM STDIN (FORMAT BINARY)";

    //            await using var writer = await conn.BeginBinaryImportAsync(copyCommand);

    //            foreach (var f in chunk)
    //            {
    //                var created = f.Createdat ?? DateTime.UtcNow;
    //                created = DateTime.SpecifyKind(created, DateTimeKind.Unspecified);

    //                var updated = f.Updatedat ?? DateTime.UtcNow;
    //                updated = DateTime.SpecifyKind(updated, DateTimeKind.Unspecified);

    //                DateTime? effectDtValue = f.EffectDt?.ToDateTime(TimeOnly.MinValue);

    //                await writer.StartRowAsync();
    //                await writer.WriteAsync(f.Forecastedamt, NpgsqlTypes.NpgsqlDbType.Integer);
    //                await writer.WriteAsync(f.ProjId, NpgsqlTypes.NpgsqlDbType.Varchar);
    //                await writer.WriteAsync(f.PlId, NpgsqlTypes.NpgsqlDbType.Integer);
    //                await writer.WriteAsync(f.EmplId, NpgsqlTypes.NpgsqlDbType.Varchar);
    //                await writer.WriteAsync(f.Month, NpgsqlTypes.NpgsqlDbType.Integer);
    //                await writer.WriteAsync(f.Year, NpgsqlTypes.NpgsqlDbType.Integer);
    //                await writer.WriteAsync(f.Forecastedhours, NpgsqlTypes.NpgsqlDbType.Numeric);
    //                await writer.WriteAsync(f.DctId, NpgsqlTypes.NpgsqlDbType.Integer);
    //                await writer.WriteAsync(f.AcctId, NpgsqlTypes.NpgsqlDbType.Varchar);
    //                await writer.WriteAsync(f.OrgId, NpgsqlTypes.NpgsqlDbType.Varchar);
    //                await writer.WriteAsync(f.HrlyRate, NpgsqlTypes.NpgsqlDbType.Numeric);
    //                await writer.WriteAsync(f.Plc, NpgsqlTypes.NpgsqlDbType.Varchar);
    //                await writer.WriteAsync(f.Actualamt, NpgsqlTypes.NpgsqlDbType.Numeric);
    //                await writer.WriteAsync(f.Actualhours, NpgsqlTypes.NpgsqlDbType.Numeric);
    //                await writer.WriteAsync(f.ForecastedCost.HasValue ? f.ForecastedCost.Value : (decimal?)null, NpgsqlTypes.NpgsqlDbType.Numeric);

    //            }

    //            await writer.CompleteAsync();
    //            Console.WriteLine($"✅ Chunk {index + 1}/{chunks.Count} inserted ({chunk.Count()} rows)");
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine($"❌ Error in chunk {index + 1}: {ex.Message}");
    //        }
    //        finally
    //        {
    //            throttler.Release();
    //        }
    //    });

    //    await Task.WhenAll(tasks);
    //}

    //Working version
    public static async Task InsertPlForecastsParallelAsync(
    List<PlForecast> forecasts,
    int chunkSize,
    int maxParallelTasks,
    string connectionString)
    {
        if (forecasts == null || forecasts.Count == 0)
            return;

        // Split forecasts into chunks
        var chunks = forecasts
            .Chunk(chunkSize)
            .ToList();

        using var throttler = new SemaphoreSlim(maxParallelTasks);

        var tasks = chunks.Select(async (chunk, index) =>
        {
            await throttler.WaitAsync();
            try
            {
                var options = new DbContextOptionsBuilder<MydatabaseContext>()
                    .UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        // Increase timeout to handle large inserts
                        npgsqlOptions.CommandTimeout(180);
                        npgsqlOptions.EnableRetryOnFailure(5);
                    })
                    // ✅ Disable change tracking for speed
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .Options;

                using var parallelContext = new MydatabaseContext(options);
                parallelContext.ChangeTracker.AutoDetectChangesEnabled = false;

                // Normalize timestamps
                foreach (var f in chunk)
                {
                    f.Empl = null;
                    f.Emple = null;
                    f.Pl = null;
                    f.Proj = null;
                    f.DirectCost = null;

                    f.Createdat = DateTime.SpecifyKind(f.Createdat ?? DateTime.UtcNow, DateTimeKind.Unspecified);
                    f.Updatedat = DateTime.SpecifyKind(f.Updatedat ?? DateTime.UtcNow, DateTimeKind.Unspecified);
                }


                await parallelContext.BulkInsertAsync(chunk, new BulkConfig { SetOutputIdentity = true });
                //await parallelContext.SaveChangesAsync();

                Console.WriteLine($"✅ Inserted chunk {index + 1}/{chunks.Count} ({chunk.Count()} rows)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inserting chunk {index + 1}: {ex.Message}");
                // Optionally: log ex.InnerException?.Message for transient issues
                throw;
            }
            finally
            {
                throttler.Release();
            }
        });

        await Task.WhenAll(tasks);
    }


    public static async Task InsertEntitiesCopyAsync<TContext, TEntity>(
        List<TEntity> entities,
        Func<TContext> contextFactory, // ✅ create DbContext via delegate
        string connectionString,
        int chunkSize = 500,
        int maxParallelTasks = 4)
        where TContext : DbContext
        where TEntity : class
    {
        if (entities == null || entities.Count == 0)
            return;

        var chunks = entities.Chunk(chunkSize).ToList();
        using var throttler = new SemaphoreSlim(maxParallelTasks);
        var errors = new ConcurrentBag<(int, string)>();

        // ✅ Create context once for metadata discovery
        using var context = contextFactory();
        var entityType = context.Model.FindEntityType(typeof(TEntity))
                         ?? throw new InvalidOperationException($"Entity {typeof(TEntity).Name} not found in DbContext model.");

        var tableName = entityType.GetTableName();
        var schema = entityType.GetSchema();
        var fullTableName = !string.IsNullOrEmpty(schema) ? $"{schema}.{tableName}" : tableName;

        var propertyColumnMap = entityType
            .GetProperties()
            .Where(p => !p.IsShadowProperty() && !p.IsConcurrencyToken && !p.IsPrimaryKey()) // ✅ skip PK/Concurrency
            .Select(p => new
            {
                Property = typeof(TEntity).GetProperty(p.Name),
                ColumnName = p.GetColumnName(StoreObjectIdentifier.Table(tableName, schema))
            })
            .Where(x => x.Property != null)
            .ToList();

        Console.WriteLine($"📋 EF Model: {typeof(TEntity).Name} → {fullTableName}");
        Console.WriteLine($"   Columns: {string.Join(", ", propertyColumnMap.Select(c => c.ColumnName))}");

        var copySql = $"COPY {fullTableName} ({string.Join(", ", propertyColumnMap.Select(c => c.ColumnName))}) FROM STDIN (FORMAT BINARY)";

        var tasks = chunks.Select(async (chunk, index) =>
        {
            await throttler.WaitAsync();
            try
            {
                await using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync();
                await using var writer = conn.BeginBinaryImport(copySql);

                foreach (var entity in chunk)
                {
                    await writer.StartRowAsync();

                    foreach (var map in propertyColumnMap)
                    {
                        var value = map.Property.GetValue(entity) ?? DBNull.Value;
                        await writer.WriteAsync(value);
                    }
                }

                await writer.CompleteAsync();
                Console.WriteLine($"✅ Inserted chunk {index + 1}/{chunks.Count} ({chunk.Count()} rows)");
            }
            catch (Exception ex)
            {
                errors.Add((index, ex.Message));
                Console.WriteLine($"❌ Chunk {index + 1} failed: {ex.Message}");
            }
            finally
            {
                throttler.Release();
            }
        });

        await Task.WhenAll(tasks);

        if (!errors.IsEmpty)
        {
            Console.WriteLine("\n⚠️ Some chunks failed:");
            foreach (var e in errors)
                Console.WriteLine($"  Chunk {e.Item1 + 1}: {e.Item2}");
        }
    }


    private static void CleanForecastForInsert(PlForecast forecast, int? employeeId, int? dctId)
    {
        forecast.Forecastid = 0;
        forecast.Emple = null;
        forecast.Empl = null;
        forecast.DirectCost = null;
        forecast.Proj = null;
        forecast.Pl = null;
        forecast.DctId = dctId;
        forecast.empleId = employeeId;
        forecast.Createdat = DateTime.UtcNow;
        forecast.Updatedat = DateTime.UtcNow;
    }
}
