using Npgsql;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Globalization;
using System.Text;

namespace PlanningAPI.Helpers
{
    public class ImportByColumnSequence
    {
        public void Import(
        string excelPath,
        string sheetName,
        string connString,
        string targetTable,
        string uniqueKey)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            // 1. Read target table columns
            List<string> tableColumns = new();
            string columnsSql = @"
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = @table
            ORDER BY ordinal_position;";

            using (var cmd = new NpgsqlCommand(columnsSql, conn))
            {
                cmd.Parameters.AddWithValue("table", targetTable);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                    tableColumns.Add(reader.GetString(0));
            }


            Console.WriteLine($"Detected {tableColumns.Count} columns in '{targetTable}'.");

            // 2. Create temp table
            using (var cmd = new NpgsqlCommand(
                $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
            {
                cmd.ExecuteNonQuery();
            }

            // 3. Read Excel
            using var fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            IWorkbook workbook = new XSSFWorkbook(fs);
            ISheet sheet = workbook.GetSheet(sheetName)
                ?? throw new Exception($"Sheet '{sheetName}' not found.");

            // 4. Map Excel headers
            var excelColumnMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            IRow headerRow = sheet.GetRow(sheet.FirstRowNum);

            for (int c = headerRow.FirstCellNum; c < headerRow.LastCellNum; c++)
            {
                var cell = headerRow.GetCell(c);
                if (cell != null && !string.IsNullOrWhiteSpace(cell.StringCellValue))
                    excelColumnMap[cell.StringCellValue.Trim()] = c;
            }

            string[] pkColumns = {
                "gd_srce_key",
                "gd_lvl1_key",
                "gd_lvl2_key",
                "gd_lvl3_key"
            };

            using (var writer = conn.BeginTextImport(
                $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
            {
                for (int r = sheet.FirstRowNum + 1; r <= sheet.LastRowNum; r++)
                {
                    IRow row = sheet.GetRow(r);
                    if (row == null) continue;

                    // ---- SKIP ROW IF ANY PK COLUMN IS NULL / EMPTY ----
                    //bool skipRow = false;
                    //foreach (var col in pkColumns)
                    //{
                    //    if (!excelColumnMap.TryGetValue(col, out int idx) ||
                    //        string.IsNullOrWhiteSpace(GetCellString(row.GetCell(idx))))
                    //    {
                    //        skipRow = true;
                    //        break;
                    //    }
                    //}
                    //if (skipRow) continue;
                    // --------------------------------------------------

                    var line = new StringBuilder();

                    for (int i = 0; i < tableColumns.Count; i++)
                    {
                        if (i > 0) line.Append(',');

                        // 🔥 KEY CHANGE: use column position
                        line.Append(GetCellString(row.GetCell(i)));
                    }

                    writer.WriteLine(line.ToString());
                }
            }

            // 6. TRUNCATE target table (full refresh)
            using (var cmd = new NpgsqlCommand(
                $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
            {
                cmd.ExecuteNonQuery();
            }


            // 7. INSERT fresh data into main table
            string insertSql = $@"
                INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
                SELECT {string.Join(",", tableColumns)}
                FROM {targetTable}_temp;";

            using (var cmd = new NpgsqlCommand(insertSql, conn))
            {
                int inserted = cmd.ExecuteNonQuery();
                Console.WriteLine($"Insert completed: {inserted} rows inserted.");
            }

            using (var cmd = new NpgsqlCommand(
                $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
            {
                cmd.ExecuteNonQuery();
            }

        }


        static string GetCellString(ICell cell)
        {
            if (cell == null || cell.CellType == CellType.Blank) return "";

            return cell.CellType switch
            {
                CellType.Numeric => DateUtil.IsCellDateFormatted(cell)
                     ? cell.DateCellValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? ""
                    : cell.NumericCellValue.ToString("0.########", CultureInfo.InvariantCulture),
                CellType.String => EscapeCsv(cell.StringCellValue),
                CellType.Formula => cell.NumericCellValue.ToString("0.########", CultureInfo.InvariantCulture),
                CellType.Boolean => cell.BooleanCellValue ? "TRUE" : "FALSE",
                _ => EscapeCsv(cell.ToString())
            };
        }

        static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }


        static string GetTableNameFromSheetName(string value)
        {
            switch (value.ToLower())
            {
                case "lab_hours":
                    return "lab_hours";
                case "psr_final_data":
                    return "psr_final_data";
                case "psr_header":
                    return "psr_header";
                case "project_modifications":
                    return "project_modifications";
                case "plc_codes":
                    return "plc_codes";
                case "gl_post_details":
                    return "gl_post_details";
                //case "lab_hours":
                //    return "lab_hours";
                //case "psr_final_data":
                //    return "psr_final_data";
                default:
                    throw new Exception($"No target table mapping for sheet '{value}'");
            }
        }


        public void ImportAllSheets(
        string excelPath,
        string connString)
        {
            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            string targetTable = string.Empty;
            List<string> tableColumns = new();


            // 3. Open workbook ONCE
            using var fs = new FileStream(excelPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            IWorkbook workbook = new XSSFWorkbook(fs);

            // 4. Loop through all sheets
            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                ISheet sheet = workbook.GetSheetAt(s);
                Console.WriteLine($"Importing sheet: {sheet.SheetName}");

                if (sheet.SheetName.ToLower() != "gl_post_details")
                {
                    continue;
                }

                targetTable = GetTableNameFromSheetName(sheet.SheetName);
                // 1. Read target table columns
                tableColumns = new();
                string columnsSql = @"
                        SELECT column_name
                        FROM information_schema.columns
                        WHERE table_schema = 'public'
                          AND table_name = @table
                        ORDER BY ordinal_position;
                    ";

                using (var cmd = new NpgsqlCommand(columnsSql, conn))
                {
                    cmd.Parameters.AddWithValue("table", targetTable);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        tableColumns.Add(reader.GetString(0));
                }

                Console.WriteLine($"Detected {tableColumns.Count} columns in '{targetTable}'.");

                // 2. Create temp table ONCE
                using (var cmd = new NpgsqlCommand(
                    $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
                {
                    cmd.ExecuteNonQuery();
                }



                IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                if (headerRow == null) continue;

                using (var writer = conn.BeginTextImport(
                    $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
                {
                    for (int r = sheet.FirstRowNum + 1; r <= sheet.LastRowNum; r++)
                    {
                        IRow row = sheet.GetRow(r);
                        if (row == null) continue;

                        var line = new StringBuilder();

                        for (int i = 0; i < tableColumns.Count; i++)
                        {
                            if (i > 0) line.Append(',');
                            line.Append(GetCellString(row.GetCell(i)));
                        }

                        writer.WriteLine(line.ToString());
                    }
                }

                // 5. TRUNCATE target table ONCE
                using (var cmd = new NpgsqlCommand(
                    $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 6. INSERT all sheets data
                string insertSql = $@"
                INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
                SELECT {string.Join(",", tableColumns)}
                FROM {targetTable}_temp;
    ";

                using (var cmd = new NpgsqlCommand(insertSql, conn))
                {
                    int inserted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Insert completed: {inserted} rows inserted.");
                }

                // 7. Cleanup
                using (var cmd = new NpgsqlCommand(
                    $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void ImportAllSheets(
        Stream excelFile)
        {


            if (excelFile == null || excelFile.Length == 0)
                throw new ArgumentException("Excel file is empty or missing.");


            string connString =
                "Host=dpg-d0n1vd2li9vc7380m3o0-a.singapore-postgres.render.com;Database=planning_demo;Username=myuser;Password=ODIfyKykuj6zdwchsnqAzccSMNeRgGQ7;Include Error Detail=true;";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();

            string targetTable = string.Empty;
            List<string> tableColumns = new();

            // 1. Open workbook from uploaded file
            //using var stream = excelFile.OpenReadStream();

            try
            {
                IWorkbook workbook = new XSSFWorkbook(excelFile);


            // 2. Loop through all sheets
            for (int s = 0; s < workbook.NumberOfSheets; s++)
            {
                ISheet sheet = workbook.GetSheetAt(s);
                Console.WriteLine($"Importing sheet: {sheet.SheetName}");

                if (!sheet.SheetName.Equals("gl_post_details", StringComparison.OrdinalIgnoreCase))
                    continue;

                targetTable = GetTableNameFromSheetName(sheet.SheetName);

                // 3. Read target table columns
                tableColumns = new();
                string columnsSql = @"
                    SELECT column_name
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = @table
                    ORDER BY ordinal_position;";

                using (var cmd = new NpgsqlCommand(columnsSql, conn))
                {
                    cmd.Parameters.AddWithValue("table", targetTable);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                        tableColumns.Add(reader.GetString(0));
                }

                Console.WriteLine($"Detected {tableColumns.Count} columns in '{targetTable}'.");

                // 4. Create temp table
                using (var cmd = new NpgsqlCommand(
                    $"CREATE TEMP TABLE \"{targetTable}_temp\" (LIKE \"{targetTable}\")", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
                if (headerRow == null) continue;

                // 5. COPY data
                using (var writer = conn.BeginTextImport(
                    $"COPY \"{targetTable}_temp\" ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
                {
                    for (int r = sheet.FirstRowNum + 1; r <= sheet.LastRowNum; r++)
                    {
                        IRow row = sheet.GetRow(r);
                        if (row == null) continue;

                        var line = new StringBuilder();

                        for (int i = 0; i < tableColumns.Count; i++)
                        {
                            if (i > 0) line.Append(',');
                            line.Append(GetCellString(row.GetCell(i)));
                        }

                        writer.WriteLine(line.ToString());
                    }
                }

                // 6. TRUNCATE target table
                using (var cmd = new NpgsqlCommand(
                    $"TRUNCATE TABLE \"{targetTable}\" RESTART IDENTITY;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 7. INSERT fresh data
                string insertSql = $@"
            INSERT INTO ""{targetTable}"" ({string.Join(",", tableColumns)})
            SELECT {string.Join(",", tableColumns)}
            FROM ""{targetTable}_temp"";
        ";

                using (var cmd = new NpgsqlCommand(insertSql, conn))
                {
                    int inserted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Insert completed: {inserted} rows inserted.");
                }

                // 8. Cleanup
                using (var cmd = new NpgsqlCommand(
                    $"DROP TABLE IF EXISTS \"{targetTable}_temp\";", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            }

            catch (Exception ex)
            {
            }
        }


    }
}
