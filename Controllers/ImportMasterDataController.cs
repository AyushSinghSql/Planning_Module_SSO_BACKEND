using Amazon.Runtime;
using Amazon.S3;
using ExcelDataReader;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using System.Data;
using System.Globalization;
using System.Text;
using WebApi.Controllers;
using WebApi.Services;

namespace PlanningAPI.Controllers
{
    public class ImportMasterDataController : ControllerBase
    {

        private readonly MydatabaseContext _context;
        private readonly ILogger<ProjectController> _logger;
        NewBusinessService _service;

        private string BucketName = "";
        private string Region = "";            // Change to your region
        private string AWS_ACCESS_KEY_ID = "c";
        private string AWS_SECRET_ACCESS_KEY = "";
        private int EXPIRES_IN_MINUTES = 0;

        //private static readonly RegionEndpoint bucketRegion = RegionEndpoint.APSouth2;
        private static IAmazonS3 s3Client;
        private readonly IConfiguration _config;
        public ImportMasterDataController(ILogger<ProjectController> logger, MydatabaseContext context, IConfiguration config)
        {
            _context = context;
            _logger = logger;
            _service = new NewBusinessService(_context);
            _config = config;
            AWS_ACCESS_KEY_ID = _config["AwsS3:AWS_ACCESS_KEY_ID"];
            AWS_SECRET_ACCESS_KEY = _config["AwsS3:AWS_SECRET_ACCESS_KEY"];
            Region = _config["AwsS3:REGION"];
            BucketName = _config["AwsS3:BUCKETNAME"];
            EXPIRES_IN_MINUTES = Convert.ToInt16(_config["AwsS3:EXPIRES_IN_MINUTES"]);
        }

        [HttpPost("ImportAccounts")]
        public async Task<IActionResult> ImportAccounts(IFormFile file)
        {
            _logger.LogInformation("ImportAccounts called");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var AccoungtsData = new List<Account>();
            int plID = 0;

            try
            {
                using var stream = file.OpenReadStream();
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);


                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    Account account = new Account();
                    //var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());
                    try
                    {
                        account = new Account
                        {
                            AcctId = row.GetCell(0)?.ToString() ?? string.Empty,
                            ActiveFlag = row.GetCell(1)?.ToString() ?? string.Empty,
                            //ActiveFlag = row.GetCell(1)?.ToString()?.Trim().Substring(0, 1) ?? string.Empty,
                            L1AcctName = row.GetCell(14)?.ToString() ?? string.Empty,
                            L2AcctName = row.GetCell(15)?.ToString() ?? string.Empty,
                            L3AcctName = row.GetCell(16)?.ToString() ?? string.Empty,
                            L4AcctName = row.GetCell(17)?.ToString() ?? string.Empty,
                            L5AcctName = row.GetCell(18)?.ToString() ?? string.Empty,
                            L6AcctName = row.GetCell(19)?.ToString() ?? string.Empty,
                            L7AcctName = row.GetCell(20)?.ToString() ?? string.Empty,
                            LvlNo = (int)(row.GetCell(21)?.NumericCellValue ?? 0),
                            //int.TryParse(row.GetCell(21)?.ToString(), out lvlNo),
                            AcctName = row.GetCell(10)?.ToString() ?? string.Empty,
                            SAcctTypeCd = row.GetCell(6)?.ToString() ?? string.Empty,
                            //SAcctTypeCd = row.GetCell(6)?.ToString()?.Trim().Substring(0, 1) ?? string.Empty,

                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    AccoungtsData.Add(account);
                }
                _context.Accounts.AddRange(AccoungtsData);
                _context.SaveChanges();
                return Ok("Excel file processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
        }

        [HttpPost("ImportAccountsGroupSetup")]
        public async Task<IActionResult> ImportAccountsGroupSetup(IFormFile file)
        {
            _logger.LogInformation("ImportAccounts called");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var AccoungtsGroupData = new List<AccountGroupSetup>();
            int plID = 0;

            try
            {
                using var stream = file.OpenReadStream();
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);


                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    AccountGroupSetup account = new AccountGroupSetup();
                    //var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());
                    try
                    {
                        account = new AccountGroupSetup
                        {
                            AccountFunctionDescription = row.GetCell(2)?.ToString() ?? string.Empty,
                            ActiveFlag = (row.GetCell(7)?.ToString()?.Trim().ToUpper() == "Y"),
                            AccountId = row.GetCell(0)?.ToString() ?? string.Empty,
                            CompanyId = row.GetCell(5)?.ToString() ?? string.Empty,
                            AcctGroupCode = row.GetCell(1)?.ToString() ?? string.Empty,
                            ProjectAccountAbbreviation = row.GetCell(6)?.ToString() ?? string.Empty,
                            RevenueMappedAccount = row.GetCell(8)?.ToString() ?? string.Empty,
                            SalaryCapMappedAccount = row.GetCell(10)?.ToString() ?? string.Empty,

                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    AccoungtsGroupData.Add(account);
                }
                _context.AccountGroupSetup.AddRange(AccoungtsGroupData);
                _context.SaveChanges();
                return Ok("Excel file processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
        }


        [HttpPost("ImportOrgMaster")]
        public async Task<IActionResult> ImportOrgMaster(IFormFile file)
        {
            _logger.LogInformation("ImportAccounts called");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var AccoungtsGroupData = new List<Organization>();
            int plID = 0;

            try
            {
                using var stream = file.OpenReadStream();
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);


                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    Organization account = new Organization();
                    //var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());
                    try
                    {
                        account = new Organization
                        {
                            L1OrgName = row.GetCell(16)?.ToString() ?? string.Empty,
                            L2OrgName = row.GetCell(17)?.ToString() ?? string.Empty,
                            L3OrgName = row.GetCell(18)?.ToString() ?? string.Empty,
                            L4OrgName = row.GetCell(19)?.ToString() ?? string.Empty,
                            L5OrgName = row.GetCell(20)?.ToString() ?? string.Empty,
                            L6OrgName = row.GetCell(21)?.ToString() ?? string.Empty,
                            L7OrgName = row.GetCell(22)?.ToString() ?? string.Empty,
                            L8OrgName = row.GetCell(23)?.ToString() ?? string.Empty,
                            L9OrgName = row.GetCell(24)?.ToString() ?? string.Empty,
                            LvlNo = (int)(row.GetCell(3)?.NumericCellValue ?? 0),
                            OrgId = row.GetCell(0)?.ToString() ?? string.Empty,
                            OrgName = row.GetCell(1)?.ToString() ?? string.Empty,
                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    AccoungtsGroupData.Add(account);
                }
                _context.Organizations.AddRange(AccoungtsGroupData);
                _context.SaveChanges();
                return Ok("Excel file processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
        }


        [HttpPost("ImportEmployeeMaster")]
        public async Task<IActionResult> ImportEmployeeMaster(IFormFile file)
        {
            _logger.LogInformation("ImportAccounts called");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var AccoungtsGroupData = new List<PlEmployee>();
            int plID = 0;

            try
            {
                using var stream = file.OpenReadStream();
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);


                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    PlEmployee account = new PlEmployee();
                    //var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());
                    try
                    {
                        account = new PlEmployee
                        {
                            EmplId = row.GetCell(0)?.ToString() ?? string.Empty,
                            CountyName = row.GetCell(41)?.ToString() ?? string.Empty,
                            Email = row.GetCell(37)?.ToString() ?? string.Empty,
                            FirstName = row.GetCell(11)?.ToString() ?? string.Empty,
                            Gender = row.GetCell(34)?.ToString() ?? string.Empty,
                            //HireDate = (row.GetCell(21)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(21))) ? row.GetCell(21).DateCellValue : DateTime.TryParse(row.GetCell(21)?.ToString(), out var d) ? d : (DateTime?)null,
                            HireDate = (row.GetCell(21)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(21)))
                                    ? DateOnly.FromDateTime(row.GetCell(21).DateCellValue)
                                    : DateOnly.TryParse(row.GetCell(21)?.ToString(), out var d) ? d : default,

                            LastName = row.GetCell(22)?.ToString() ?? string.Empty,
                            Ln1Adr = row.GetCell(23)?.ToString() ?? string.Empty,
                            Ln2Adr = row.GetCell(24)?.ToString() ?? string.Empty,
                            PerHourRate = (decimal)(row.GetCell(3)?.NumericCellValue ?? 0),
                            OrgId = row.GetCell(0)?.ToString() ?? string.Empty,
                            Ln3Adr = row.GetCell(1)?.ToString() ?? string.Empty,
                            MaritalCd = row.GetCell(1)?.ToString() ?? string.Empty,
                            MidName = row.GetCell(1)?.ToString() ?? string.Empty,
                            PhoneNumber = row.GetCell(1)?.ToString() ?? string.Empty,
                            PostalCd = row.GetCell(1)?.ToString() ?? string.Empty,
                            Salary = (decimal)(row.GetCell(3)?.NumericCellValue ?? 0),
                            Role = row.GetCell(1)?.ToString() ?? string.Empty,
                            IsBrd = true,
                            IsRev = true,
                            //MailStateDc = row.GetCell(1)?.ToString() ?? string.Empty,
                            //PlcGlcCode = row.GetCell(1)?.ToString() ?? string.Empty,

                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    AccoungtsGroupData.Add(account);
                }
                _context.PlEmployees.AddRange(AccoungtsGroupData);
                _context.SaveChanges();
                return Ok("Excel file processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
        }

        [HttpPost("UpdateProjDates")]
        public async Task<IActionResult> UpdateProjDates(IFormFile file)
        {
            _logger.LogInformation("ImportAccounts called");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var AccoungtsGroupData = new List<PlEmployee>();
            int plID = 0;

            try
            {
                using var stream = file.OpenReadStream();
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);


                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    PlEmployee account = new PlEmployee();
                    //var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());
                    try
                    {
                        account = new PlEmployee
                        {
                            EmplId = row.GetCell(0)?.ToString() ?? string.Empty,
                            CountyName = row.GetCell(41)?.ToString() ?? string.Empty,
                            Email = row.GetCell(37)?.ToString() ?? string.Empty,
                            FirstName = row.GetCell(11)?.ToString() ?? string.Empty,
                            Gender = row.GetCell(34)?.ToString() ?? string.Empty,
                            //HireDate = (row.GetCell(21)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(21))) ? row.GetCell(21).DateCellValue : DateTime.TryParse(row.GetCell(21)?.ToString(), out var d) ? d : (DateTime?)null,
                            HireDate = (row.GetCell(21)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(21)))
                                    ? DateOnly.FromDateTime(row.GetCell(21).DateCellValue)
                                    : DateOnly.TryParse(row.GetCell(21)?.ToString(), out var d) ? d : default,

                            LastName = row.GetCell(22)?.ToString() ?? string.Empty,
                            Ln1Adr = row.GetCell(23)?.ToString() ?? string.Empty,
                            Ln2Adr = row.GetCell(24)?.ToString() ?? string.Empty,
                            PerHourRate = (decimal)(row.GetCell(3)?.NumericCellValue ?? 0),
                            OrgId = row.GetCell(0)?.ToString() ?? string.Empty,
                            Ln3Adr = row.GetCell(1)?.ToString() ?? string.Empty,
                            MaritalCd = row.GetCell(1)?.ToString() ?? string.Empty,
                            MidName = row.GetCell(1)?.ToString() ?? string.Empty,
                            PhoneNumber = row.GetCell(1)?.ToString() ?? string.Empty,
                            PostalCd = row.GetCell(1)?.ToString() ?? string.Empty,
                            Salary = (decimal)(row.GetCell(3)?.NumericCellValue ?? 0),
                            Role = row.GetCell(1)?.ToString() ?? string.Empty,
                            IsBrd = true,
                            IsRev = true,
                            //MailStateDc = row.GetCell(1)?.ToString() ?? string.Empty,
                            //PlcGlcCode = row.GetCell(1)?.ToString() ?? string.Empty,

                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    AccoungtsGroupData.Add(account);
                }
                _context.PlEmployees.AddRange(AccoungtsGroupData);
                _context.SaveChanges();
                return Ok("Excel file processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
        }




        [HttpPost("ImportEmployeeMaster1")]
        public async Task<IActionResult> ImportEmployeeMaster1(IFormFile file)
        {
            _logger.LogInformation("ImportAccounts called");

            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var AccoungtsGroupData = new List<PlEmployee>();
            int plID = 0;

            try
            {
                using var stream = file.OpenReadStream();
                var workbook = new XSSFWorkbook(stream);
                var sheet = workbook.GetSheetAt(0);


                for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
                {
                    var row = sheet.GetRow(rowIndex);
                    if (row == null) continue;
                    PlEmployee account = new PlEmployee();
                    //var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());
                    try
                    {
                        account = new PlEmployee
                        {
                            EmplId = row.GetCell(0)?.ToString() ?? string.Empty,
                            CountyName = row.GetCell(41)?.ToString() ?? string.Empty,
                            Email = row.GetCell(37)?.ToString() ?? string.Empty,
                            FirstName = row.GetCell(11)?.ToString() ?? string.Empty,
                            Gender = row.GetCell(34)?.ToString() ?? string.Empty,
                            //HireDate = (row.GetCell(21)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(21))) ? row.GetCell(21).DateCellValue : DateTime.TryParse(row.GetCell(21)?.ToString(), out var d) ? d : (DateTime?)null,
                            HireDate = (row.GetCell(21)?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(row.GetCell(21)))
                                    ? DateOnly.FromDateTime(row.GetCell(21).DateCellValue)
                                    : DateOnly.TryParse(row.GetCell(21)?.ToString(), out var d) ? d : default,

                            LastName = row.GetCell(22)?.ToString() ?? string.Empty,
                            Ln1Adr = row.GetCell(23)?.ToString() ?? string.Empty,
                            Ln2Adr = row.GetCell(24)?.ToString() ?? string.Empty,
                            PerHourRate = (decimal)(row.GetCell(3)?.NumericCellValue ?? 0),
                            OrgId = row.GetCell(0)?.ToString() ?? string.Empty,
                            Ln3Adr = row.GetCell(1)?.ToString() ?? string.Empty,
                            MaritalCd = row.GetCell(1)?.ToString() ?? string.Empty,
                            MidName = row.GetCell(1)?.ToString() ?? string.Empty,
                            PhoneNumber = row.GetCell(1)?.ToString() ?? string.Empty,
                            PostalCd = row.GetCell(1)?.ToString() ?? string.Empty,
                            Salary = (decimal)(row.GetCell(3)?.NumericCellValue ?? 0),
                            Role = row.GetCell(1)?.ToString() ?? string.Empty,
                            IsBrd = true,
                            IsRev = true,
                            //MailStateDc = row.GetCell(1)?.ToString() ?? string.Empty,
                            //PlcGlcCode = row.GetCell(1)?.ToString() ?? string.Empty,

                        };
                    }
                    catch (Exception ex)
                    {

                    }
                    AccoungtsGroupData.Add(account);
                }
                _context.PlEmployees.AddRange(AccoungtsGroupData);
                _context.SaveChanges();
                return Ok("Excel file processed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
        }


        [HttpPost("UpdateProjectDates")]
        public async Task<IActionResult> UpdateProjectDates()
        {
            try
            {
                var projects = _context.PlProjects.Where(p => p.ProjStartDt.HasValue || p.ProjEndDt.HasValue)
                     .Select(p => p)
                     .Distinct()
                     .ToList();
                var projectsWithoutStartEndDate = _context.PlProjects.Where(p => !p.ProjStartDt.HasValue || !p.ProjEndDt.HasValue)
                     .Select(p => p.ProjId)
                     .Distinct()
                     .ToList();
                var definitations = _context.ProjRevDefinitions.Where(p => p.ProjectId != null).ToList();


                foreach (var proj in projectsWithoutStartEndDate)
                {

                    var parts = proj.Split('.', StringSplitOptions.RemoveEmptyEntries);
                    var prefixes = Enumerable
                        .Range(1, parts.Length - 1)
                        .Select(i => string.Join('.', parts.Take(i)))
                        .ToList();

                    var projectsWithLevel = definitations
                        .Where(p => !string.IsNullOrWhiteSpace(p.ProjectId)
                                    && p.ProjectId.StartsWith(prefixes[0]))
                        .Select(p => new
                        {
                            ProjectId = p.ProjectId,
                            RevenueLevel =
                                p.ProjectId.Length
                                - p.ProjectId.Replace(".", "").Length
                                + 1
                        })
                        .ToList();

                    if (projectsWithLevel.Any())
                    {
                        var projId = projectsWithLevel
                            .Where(p => p.ProjectId == proj)
                            .Select(p => p.ProjectId)
                            .FirstOrDefault();

                        var pr = projects.FirstOrDefault(p => p.ProjId == projId);

                    }


                    var revenuelevel = definitations
                        .Where(p => p.ProjectId != null && p.ProjectId.StartsWith(prefixes[0]))
                        .Select(z =>
                            z.ProjectId.Length
                            - z.ProjectId.Replace(".", "").Length
                            + 1
                        )
                        .Distinct()
                        .ToList();

                    if (revenuelevel.Count() > 0)
                    {
                        int projLevel = proj.Length - proj.Replace(".", "").Length + 1;
                        if (projLevel > revenuelevel.Min())
                        {

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import plan Ver1");
                return StatusCode(500, "An error occurred while importing the plan.");
            }
            return Ok("Excel file processed successfully.");

        }


        [HttpPost("TestImport")]
        public async Task<IActionResult> TestImport(string filename, string Username)
        {
            string connString =
        "Host=dpg-d0n1vd2li9vc7380m3o0-a.singapore-postgres.render.com;Database=planning_demo;Username=myuser;Password=ODIfyKykuj6zdwchsnqAzccSMNeRgGQ7;Include Error Detail=true;";

            using var conn = new NpgsqlConnection(connString);
            conn.Open();
            string targetTable = string.Empty;
            List<string> tableColumns = new();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var credentials = new BasicAWSCredentials(
    AWS_ACCESS_KEY_ID,
    AWS_SECRET_ACCESS_KEY);

            var s3Client = new AmazonS3Client(
                credentials,
                Amazon.RegionEndpoint.GetBySystemName(Region));

            using var response = await s3Client.GetObjectAsync(BucketName, filename);
            using var excelStream = response.ResponseStream;
            using var reader = ExcelReaderFactory.CreateReader(excelStream);

            do
            {
                // Process ONLY required sheet
                if (!reader.Name.Equals("gl_post_details", StringComparison.OrdinalIgnoreCase))
                    continue;

                Console.WriteLine($"Importing sheet: {reader.Name}");

                targetTable = GetTableNameFromSheetName(reader.Name);

                // 1️⃣ Read table columns
                tableColumns = new List<string>();
                const string columnsSql = @"
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name = @table
            ORDER BY ordinal_position;";

                using (var cmd = new NpgsqlCommand(columnsSql, conn))
                {
                    cmd.Parameters.AddWithValue("table", targetTable);
                    using var colReader = cmd.ExecuteReader();
                    while (colReader.Read())
                        tableColumns.Add(colReader.GetString(0));
                }

                Console.WriteLine($"Detected {tableColumns.Count} columns.");

                // 2️⃣ Create temp table
                using (var cmd = new NpgsqlCommand(
                    $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                // 3️⃣ Read header row
                reader.Read(); // header row (row 0)

                // 4️⃣ COPY STREAMING
                using (var writer = conn.BeginTextImport(
                    $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
                {
                    while (reader.Read()) // ROW BY ROW
                    {
                        var line = new StringBuilder();

                        for (int i = 0; i < tableColumns.Count; i++)
                        {
                            if (i > 0) line.Append(',');

                            var value = reader.GetValue(i)?.ToString() ?? "";
                            line.Append(EscapeCsv(value));
                        }

                        writer.WriteLine(line.ToString());
                    }
                }

                // 5️⃣ Replace target table
                using (var cmd = new NpgsqlCommand(
                    $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand($@"
            INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
            SELECT {string.Join(",", tableColumns)}
            FROM {targetTable}_temp;", conn))
                {
                    var inserted = cmd.ExecuteNonQuery();
                    Console.WriteLine($"Inserted {inserted} rows.");
                }

                using (var cmd = new NpgsqlCommand(
                    $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
                {
                    cmd.ExecuteNonQuery();
                }

                break; // done with target sheet

            } while (reader.NextResult());

            //using var stream = file.OpenReadStream();
            //using var reader = new StreamReader(stream);
            // Create S3 client with credentials and region
            //var credentials = new BasicAWSCredentials(AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY);
            //var s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.GetBySystemName(Region));
            //string csvText;
            //using (var response = await s3Client.GetObjectAsync(BucketName, filename))
            //using (var memoryStream = new MemoryStream())
            //{
            //    await response.ResponseStream.CopyToAsync(memoryStream);
            //    memoryStream.Position = 0;

            //    //var stream = file.OpenReadStream();
            //    IWorkbook workbook = new XSSFWorkbook(memoryStream);  // XLSX
            //                                                          // 4. Loop through all sheets
            //    for (int s = 0; s < workbook.NumberOfSheets; s++)
            //    {
            //        ISheet sheet = workbook.GetSheetAt(s);
            //        Console.WriteLine($"Importing sheet: {sheet.SheetName}");

            //        if (sheet.SheetName.ToLower() != "gl_post_details")
            //        {
            //            continue;
            //        }

            //        targetTable = GetTableNameFromSheetName(sheet.SheetName);
            //        // 1. Read target table columns
            //        tableColumns = new();
            //        string columnsSql = @"
            //            SELECT column_name
            //            FROM information_schema.columns
            //            WHERE table_schema = 'public'
            //              AND table_name = @table
            //            ORDER BY ordinal_position;
            //        ";

            //        using (var cmd = new NpgsqlCommand(columnsSql, conn))
            //        {
            //            cmd.Parameters.AddWithValue("table", targetTable);
            //            using var reader = cmd.ExecuteReader();
            //            while (reader.Read())
            //                tableColumns.Add(reader.GetString(0));
            //        }

            //        Console.WriteLine($"Detected {tableColumns.Count} columns in '{targetTable}'.");

            //        // 2. Create temp table ONCE
            //        using (var cmd = new NpgsqlCommand(
            //            $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
            //        {
            //            cmd.ExecuteNonQuery();
            //        }



            //        IRow headerRow = sheet.GetRow(sheet.FirstRowNum);
            //        if (headerRow == null) continue;

            //        using (var writer = conn.BeginTextImport(
            //            $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
            //        {
            //            for (int r = sheet.FirstRowNum + 1; r <= sheet.LastRowNum; r++)
            //            {
            //                IRow row = sheet.GetRow(r);
            //                if (row == null) continue;

            //                var line = new StringBuilder();

            //                for (int i = 0; i < tableColumns.Count; i++)
            //                {
            //                    if (i > 0) line.Append(',');
            //                    line.Append(GetCellString(row.GetCell(i)));
            //                }

            //                writer.WriteLine(line.ToString());
            //            }
            //        }

            //        // 5. TRUNCATE target table ONCE
            //        using (var cmd = new NpgsqlCommand(
            //            $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
            //        {
            //            cmd.ExecuteNonQuery();
            //        }

            //        // 6. INSERT all sheets data
            //        string insertSql = $@"
            //    INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
            //    SELECT {string.Join(",", tableColumns)}
            //    FROM {targetTable}_temp;";

            //        using (var cmd = new NpgsqlCommand(insertSql, conn))
            //        {
            //            int inserted = cmd.ExecuteNonQuery();
            //            Console.WriteLine($"Insert completed: {inserted} rows inserted.");
            //        }

            //        // 7. Cleanup
            //        using (var cmd = new NpgsqlCommand(
            //            $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
            //        {
            //            cmd.ExecuteNonQuery();
            //        }
            //    }

            //}
            return Ok();
        }

        [NonAction]
        public string GetCellString(ICell cell)
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

        [NonAction]
        static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
        [NonAction]
        public string GetTableNameFromSheetName(string value)
        {
            switch (value.ToLower())
            {
                case "lab_hours_1":
                case "lab_hours":
                    return "lab_hours";
                case "psr_final_data":
                case "psr_final_data_1":
                case "psr_final_data_2":
                case "psr_final_data_3":
                case "psr_final_data_4":
                case "psr_final_data_5":
                case "psr_final_data_6":
                case "psr_final_data_7":
                    return "psr_final_data";
                case "psr_header":
                case "psr_header_1":
                    return "psr_header";
                case "project_modifications":
                    return "project_modifications";
                case "plc_codes":
                    return "plc_codes";
                case "gl_post_details_6":
                case "gl_post_details_5":
                case "gl_post_details_4":
                case "gl_post_details_3":
                case "gl_post_details_2":
                case "gl_post_details_1":
                case "gl_post_details":
                    return "gl_post_details";
                case "proj_rev_definition":
                    return "proj_rev_definition";
                case "rev_adj_hist":
                    return "project_revenue_adjustments";
                case "account_group_setup":
                    return "account_group_setup";
                case "poolrates_costpoint":
                    return "poolrates_costpoint";
                case "pool_cost_account":
                    return "pool_cost_account";
                case "pool_base_account":
                    return "pool_base_account";

                default:
                    return value;
            }
        }

        [NonAction]
        public string GetTableNameFromSheetNamesSlave(string value)
        {
            switch (value.ToLower())
            {
                case "lab_hours_1":
                case "lab_hours":
                    return "lab_hours";
                case "psr_final_data":
                case "psr_final_data_1":
                case "psr_final_data_2":
                case "psr_final_data_3":
                case "psr_final_data_4":
                case "psr_final_data_5":
                case "psr_final_data_6":
                case "psr_final_data_7":
                    return "psr_final_data";
                case "psr_header":
                case "psr_header_1":
                    return "psr_header";
                case "project_modifications":
                    return "project_modifications";
                case "plc_codes":
                    return "plc_codes";
                case "gl_post_details_6":
                case "gl_post_details_5":
                case "gl_post_details_4":
                case "gl_post_details_3":
                case "gl_post_details_2":
                case "gl_post_details_1":
                case "gl_post_details":
                    return "gl_post_details";
                case "proj_rev_definition":
                    return "proj_rev_definition";
                case "rev_adj_hist":
                    return "project_revenue_adjustments";
                case "account_group_setup":
                    return "account_group_setup";
                case "poolrates_costpoint":
                    return "poolrates_costpoint";
                case "pool_cost_account":
                    return "pool_cost_account";
                case "pool_base_account":
                    return "pool_base_account";

                default:
                    throw new Exception($"No target table mapping for sheet '{value}'");
            }
        }

        [NonAction]
        public string NormalizeValue(object cell)
        {
            if (cell == null)
                return "";

            // Excel dates often come as DateTime already
            if (cell is DateTime dt)
                return dt.ToString("yyyy-MM-dd HH:mm:ss");

            var text = cell.ToString()?.Trim();
            if (string.IsNullOrEmpty(text))
                return "";

            // Handle dd-MM-yyyy or dd-MM-yyyy HH:mm:ss
            if (DateTime.TryParseExact(
                    text,
                    new[] {
                "dd-MM-yyyy",
                "dd-MM-yyyy HH:mm:ss",
                "dd/MM/yyyy",
                "dd/MM/yyyy HH:mm:ss"
                    },
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsed))
            {
                return parsed.ToString("yyyy-MM-dd HH:mm:ss");
            }

            // Pass through everything else
            return text;
        }


        [HttpPost("TestImportV1")]
        public async Task<IActionResult> TestImportV1(string filename, string Username)
        {
            string connString =
                "Host=dpg-d0n1vd2li9vc7380m3o0-a.singapore-postgres.render.com;Database=planning_demo;Username=myuser;Password=ODIfyKykuj6zdwchsnqAzccSMNeRgGQ7;Include Error Detail=true;";

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 1️⃣ Download S3 file to temp disk
            var tempFile = Path.Combine(
                Path.GetTempPath(),   // /tmp in Docker
                $"{Guid.NewGuid()}.xlsx");

            try
            {
                var credentials = new BasicAWSCredentials(
                    AWS_ACCESS_KEY_ID,
                    AWS_SECRET_ACCESS_KEY);

                var s3Client = new AmazonS3Client(
                    credentials,
                    Amazon.RegionEndpoint.GetBySystemName(Region));

                using (var response = await s3Client.GetObjectAsync(BucketName, filename))
                using (var fs = new FileStream(
                    tempFile,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 64 * 1024,
                    useAsync: true))
                {
                    await response.ResponseStream.CopyToAsync(fs);
                }

                using var conn = new NpgsqlConnection(connString);
                await conn.OpenAsync();

                // 2️⃣ Open seekable stream
                using var excelStream = new FileStream(
                    tempFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    useAsync: false);

                using var reader = ExcelReaderFactory.CreateReader(excelStream);

                do
                {
                    if (!reader.Name.Equals("proj_rev_definition", StringComparison.OrdinalIgnoreCase)
                        || reader.Name.ToLower().Equals("rev_adj_hist", StringComparison.OrdinalIgnoreCase)
                        || reader.Name.ToLower().Equals("account_group_setup", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string targetTable = GetTableNameFromSheetName(reader.Name);

                    // Read table columns
                    var tableColumns = new List<string>();
                    const string columnsSql = @"
                SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = @table
                ORDER BY ordinal_position;";

                    using (var cmd = new NpgsqlCommand(columnsSql, conn))
                    {
                        cmd.Parameters.AddWithValue("table", targetTable);
                        using var colReader = cmd.ExecuteReader();
                        while (colReader.Read())
                            tableColumns.Add(colReader.GetString(0));
                    }

                    // Create temp table
                    using (var cmd = new NpgsqlCommand(
                        $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    reader.Read(); // header row

                    // COPY streaming
                    using (var writer = conn.BeginTextImport(
                        $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
                    {
                        while (reader.Read())
                        {
                            var line = new StringBuilder();

                            for (int i = 0; i < tableColumns.Count; i++)
                            {
                                if (i > 0) line.Append(',');

                                var cell = reader.GetValue(i);
                                line.Append(EscapeCsv(NormalizeValue(cell)));
                                //var value = reader.GetValue(i)?.ToString() ?? "";
                                //line.Append(EscapeCsv(value));
                            }

                            writer.WriteLine(line.ToString());
                        }
                    }

                    using (var cmd = new NpgsqlCommand(
                        $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand($@"
                INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
                SELECT {string.Join(",", tableColumns)}
                FROM {targetTable}_temp;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new NpgsqlCommand(
                        $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    break;

                } while (reader.NextResult());

                return Ok("Import completed successfully");
            }
            finally
            {
                // 3️⃣ Always cleanup temp file
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }
        }


        [HttpPost("TestImportV2")]
        [RequestSizeLimit(200_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        public async Task<IActionResult> TestImportV2(IFormFile file, string Username)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File missing");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //string connString =
            //    "Host=dpg-d0n1vd2li9vc7380m3o0-a.singapore-postgres.render.com;Database=planning_demo;Username=myuser;Password=ODIfyKykuj6zdwchsnqAzccSMNeRgGQ7;Include Error Detail=true;";

            // 1️⃣ Save uploaded file to temp disk (/tmp in Docker)
            var tempFile = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}_{file.FileName}");

            try
            {
                // Copy upload → temp file (STREAMING)
                using (var fs = new FileStream(
                    tempFile,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 64 * 1024,
                    useAsync: true))
                {
                    await file.CopyToAsync(fs);
                }

                //using var conn = new NpgsqlConnection(connString);
                //await conn.OpenAsync();

                // 2️⃣ Get connection from DbContext
                var conn = (NpgsqlConnection)_context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                // 2️⃣ Open seekable stream
                using var excelStream = new FileStream(
                    tempFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    useAsync: false);

                using var reader = ExcelReaderFactory.CreateReader(excelStream);

                do
                {
                    try
                    {
                        //if (!reader.Name.Equals("pool_cost_account", StringComparison.OrdinalIgnoreCase))
                        //    continue;

                        string targetTable = GetTableNameFromSheetName(reader.Name);

                        // Read table columns
                        var tableColumns = new List<string>();
                        const string columnsSql = @"
                SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = @table
                ORDER BY ordinal_position;";

                        using (var cmd = new NpgsqlCommand(columnsSql, conn))
                        {
                            cmd.Parameters.AddWithValue("table", targetTable);
                            using var colReader = cmd.ExecuteReader();
                            while (colReader.Read())
                                tableColumns.Add(colReader.GetString(0));
                        }

                        // Create temp table
                        using (var cmd = new NpgsqlCommand(
                            $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        reader.Read(); // header row

                        // COPY streaming
                        using (var writer = conn.BeginTextImport(
                            $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
                        {
                            while (reader.Read())
                            {
                                var line = new StringBuilder();

                                for (int i = 0; i < tableColumns.Count; i++)
                                {
                                    try
                                    {
                                        if (i > 0) line.Append(',');

                                        var cell = reader.GetValue(i);
                                        line.Append(EscapeCsv(NormalizeValue(cell)));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error reading cell at column index {i}: {ex.Message}");
                                        line.Append(""); // Append empty value for error cells
                                        continue; // Skip to next cell
                                    }


                                }

                                writer.WriteLine(line.ToString());
                            }
                        }

                        using (var cmd = new NpgsqlCommand(
                            $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
                        {
                            //cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand($@"
                INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
                SELECT {string.Join(",", tableColumns)}
                FROM {targetTable}_temp;", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(
                            $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        //break;

                    }
                    catch (Exception ex)
                    {

                    }

                } while (reader.NextResult());

                return Ok("Import completed successfully");
            }
            finally
            {
                // 3️⃣ Cleanup temp file
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }
        }



        [HttpPost("TestImportV3")]
        [RequestSizeLimit(200_000_000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 200_000_000)]
        public async Task<IActionResult> TestImportV3(IFormFile file, string Username, string Type)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File missing");

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // 1️⃣ Save uploaded file to temp disk (/tmp in Docker)
            var tempFile = Path.Combine(
                Path.GetTempPath(),
                $"{Guid.NewGuid()}_{file.FileName}");

            try
            {
                // Copy upload → temp file (STREAMING)
                using (var fs = new FileStream(
                    tempFile,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 64 * 1024,
                    useAsync: true))
                {
                    await file.CopyToAsync(fs);
                }

                //using var conn = new NpgsqlConnection(connString);
                //await conn.OpenAsync();

                // 2️⃣ Get connection from DbContext
                var conn = (NpgsqlConnection)_context.Database.GetDbConnection();

                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                // 2️⃣ Open seekable stream
                using var excelStream = new FileStream(
                    tempFile,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: 64 * 1024,
                    useAsync: false);

                using var reader = ExcelReaderFactory.CreateReader(excelStream);

                do
                {
                    try
                    {
                        //if (!reader.Name.Equals("pool_cost_account", StringComparison.OrdinalIgnoreCase))
                        //    continue;
                        string targetTable = string.Empty;

                        targetTable = GetTableNameFromSheetName(reader.Name);

                        // Read table columns
                        var tableColumns = new List<string>();
                        const string columnsSql = @"
                SELECT column_name
                FROM information_schema.columns
                WHERE table_schema = 'public'
                  AND table_name = @table
                ORDER BY ordinal_position;";

                        using (var cmd = new NpgsqlCommand(columnsSql, conn))
                        {
                            cmd.Parameters.AddWithValue("table", targetTable);
                            using var colReader = cmd.ExecuteReader();
                            while (colReader.Read())
                                tableColumns.Add(colReader.GetString(0));
                        }

                        // Create temp table
                        using (var cmd = new NpgsqlCommand(
                            $"CREATE TEMP TABLE {targetTable}_temp (LIKE {targetTable})", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        reader.Read(); // header row

                        // COPY streaming
                        using (var writer = conn.BeginTextImport(
                            $"COPY {targetTable}_temp ({string.Join(",", tableColumns)}) FROM STDIN WITH (FORMAT csv)"))
                        {
                            while (reader.Read())
                            {
                                var line = new StringBuilder();

                                for (int i = 0; i < tableColumns.Count; i++)
                                {
                                    try
                                    {
                                        if (i > 0) line.Append(',');

                                        var cell = reader.GetValue(i);
                                        line.Append(EscapeCsv(NormalizeValue(cell)));
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error reading cell at column index {i}: {ex.Message}");
                                        line.Append(""); // Append empty value for error cells
                                        continue; // Skip to next cell
                                    }


                                }

                                writer.WriteLine(line.ToString());
                            }
                        }

                        //using (var cmd = new NpgsqlCommand(
                        //    $"TRUNCATE TABLE {targetTable} RESTART IDENTITY;", conn))
                        //{
                        //    cmd.ExecuteNonQuery();
                        //}

                        using (var cmd = new NpgsqlCommand($@"
                INSERT INTO {targetTable} ({string.Join(",", tableColumns)})
                SELECT {string.Join(",", tableColumns)}
                FROM {targetTable}_temp;", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        using (var cmd = new NpgsqlCommand(
                            $"DROP TABLE IF EXISTS {targetTable}_temp;", conn))
                        {
                            cmd.ExecuteNonQuery();
                        }

                        //break;

                    }
                    catch (Exception ex)
                    {

                    }

                } while (reader.NextResult());

                return Ok("Import completed successfully");
            }
            finally
            {
                // 3️⃣ Cleanup temp file
                if (System.IO.File.Exists(tempFile))
                    System.IO.File.Delete(tempFile);
            }
        }



    }
}