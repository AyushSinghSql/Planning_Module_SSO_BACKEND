using System.Text;
using System.Text.Json;
using WebApi.DTO;

namespace PlanningAPI.Models
{
    public interface IAiService
    {
        Task<string> GetForecastInsightAsync(PlanForecastSummary forecast);
        Task<string> GetVarianceSummaryAsync(List<ProjForecastSummary> summary);
        Task<string> GetVarianceSummaryAsync(ProjForecastSummary summary);
        Task<string> GetVarianceSummaryAsync(string plType, int versionA, int versionB, List<VarianceComparison> data);
    }

    public class VarianceComparison
    {
        public string ProjId { get; set; }
        public string PlType { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }

        // Raw values from A
        public decimal? ForecastedCostA { get; set; }
        public decimal? ActualCostA { get; set; }
        public decimal? ForecastedHoursA { get; set; }
        public decimal? ActualHoursA { get; set; }
        public decimal? RevenueA { get; set; }

        // Raw values from B
        public decimal? ForecastedCostB { get; set; }
        public decimal? ActualCostB { get; set; }
        public decimal? ForecastedHoursB { get; set; }
        public decimal? ActualHoursB { get; set; }
        public decimal? RevenueB { get; set; }

        // Auto-calculated differences (B - A)
        public decimal? ForecastedCostDiff => ForecastedCostB - ForecastedCostA;
        public decimal? ActualCostDiff => ActualCostB - ActualCostA;
        public decimal? ForecastedHoursDiff => ForecastedHoursB - ForecastedHoursA;
        public decimal? ActualHoursDiff => ActualHoursB - ActualHoursA;
        public decimal? RevenueDiff => RevenueB - RevenueA;
    }

    //public class VarianceComparison
    //{
    //    public string ProjId { get; set; }
    //    public string PlType { get; set; }
    //    public int Month { get; set; }
    //    public int Year { get; set; }

    //    public decimal? ForecastedCostDiff { get; set; }
    //    public decimal? ActualCostDiff { get; set; }
    //    public decimal? ForecastedHoursDiff { get; set; }
    //    public decimal? ActualHoursDiff { get; set; }
    //    public decimal? RevenueDiff { get; set; }
    //}
    public class OpenAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public OpenAiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            //_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["OpenAI:ApiKey"]}");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["OpenAI:ApiKey"]}");
        }

        public async Task<string> GetForecastInsightAsync(PlanForecastSummary forecast)
        {
            // Prepare context for AI
            var summaryData = new
            {
                Project = forecast.Proj_Id,
                TotalHours = forecast.EmployeeForecastSummary.Sum(e => e.TotalForecastedHours),
                TotalCost = forecast.EmployeeForecastSummary.Sum(e => e.TotalForecastedCost),
                Employees = forecast.EmployeeForecastSummary.Select(e => new
                {
                    e.EmplId,
                    e.TotalForecastedHours,
                    e.TotalForecastedCost
                })
            };

            var prompt = $@"
Analyze the following project forecast and provide a short summary (2–3 sentences) 
highlighting risks, anomalies, or cost drivers. Be clear and business-friendly.

Project: {summaryData.Project}
Total Hours: {summaryData.TotalHours}
Total Cost: {summaryData.TotalCost:C}

Employees:
{string.Join("\n", summaryData.Employees.Select(e => $"{e.EmplId}: {e.TotalForecastedHours} hrs, {e.TotalForecastedCost:C}"))}
";

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "system", content = "You are a project cost analyst." },
                new { role = "user", content = prompt }
            }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<dynamic>();
            //var insight = result?["choices"]?[0]?["message"]?["content"]?.ToString();

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = jsonDoc.RootElement;

            // Get the "choices" array
            if (root.TryGetProperty("choices", out JsonElement choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var firstChoice = choices[0];  // now this works
                if (firstChoice.TryGetProperty("message", out JsonElement message) &&
                    message.TryGetProperty("content", out JsonElement content))
                {
                    var insight = content.GetString();
                    return insight ?? "No AI insight generated.";
                }
            }

            return "No AI insight generated.";

            //return string.IsNullOrWhiteSpace(insight)
            //    ? "No AI insights were generated for this forecast."
            //    : insight.Trim();
        }

        public async Task<string> GetVarianceSummaryAsync(ProjForecastSummary summary)
        {
            var prompt = $@"
You are a financial analyst. Analyze the following project forecast data and generate a concise variance summary for management. Highlight major differences between forecasted and actual values, trends, and potential risks. Keep it under 3 sentences.

Project: {summary.ProjId}
Plan Type: {summary.PlType}
Version: {summary.Version}
Month: {summary.Month} / Year: {summary.Year}

Data:
- Monthly Forecasted Cost: {summary.MonthlyForecastedAmt:C}
- Monthly Actual Cost: {summary.MonthlyActualAmt:C}
- YTD Forecasted Cost: {summary.YtdForecastedAmt:C}
- YTD Actual Cost: {summary.YtdActualAmt:C}
- Monthly Forecasted Hours: {summary.MonthlyForecastedHours}
- Monthly Actual Hours: {summary.MonthlyActualHours}
- Monthly Target Revenue: {summary.MonthlyTargetRevenue:C}
- Monthly Revenue: {summary.MonthlyRevenue:C}
- Monthly Burden: {summary.MonthlyBurden:C}

Instructions:
1. Highlight if actuals exceeded forecast or targets.
2. Mention trends in cost, hours, revenue, or burden.
3. Suggest actionable insight if variance is significant.
";

            var requestBody = new
            {
                //model = "gpt-4o-mini",
                model = "gpt-4o",
                messages = new[]
                {
            new { role = "system", content = "You are a financial analyst specialized in variance reporting." },
            new { role = "user", content = prompt }
        }
            };

            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
            response.EnsureSuccessStatusCode();

            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = jsonDoc.RootElement;

            var choices = root.GetProperty("choices");
            if (choices.GetArrayLength() > 0)
            {
                var content = choices[0].GetProperty("message").GetProperty("content").GetString();
                return content ?? "No variance summary generated.";
            }

            return "No variance summary generated.";
        }


//        public async Task<string> GetVarianceSummaryAsync(List<ProjForecastSummary> summaries)
//        {
//            var promptBuilder = new StringBuilder();
//            promptBuilder.AppendLine("You are a financial analyst. Generate a concise variance summary for the following projects:");

//            foreach (var s in summaries)
//            {
//                promptBuilder.AppendLine($@"
//Project: {s.ProjId}, Plan Type: {s.PlType}, Version: {s.Version}, Month: {s.Month}/{s.Year}
//- Monthly Forecasted Cost: {s.MonthlyForecastedAmt:C}, Actual Cost: {s.MonthlyActualAmt:C}
//- YTD Forecasted Cost: {s.YtdForecastedAmt:C}, Actual Cost: {s.YtdActualAmt:C}
//- Monthly Forecasted Hours: {s.MonthlyForecastedHours}, Actual Hours: {s.MonthlyActualHours}
//- Monthly Revenue: {s.MonthlyRevenue:C}, Target Revenue: {s.MonthlyTargetRevenue:C}
//- Monthly Burden: {s.MonthlyBurden:C}
//");
//            }

//            promptBuilder.AppendLine(@"
//Instructions:
//1. Highlight major variances between forecasted and actual values.
//2. Mention trends in cost, hours, revenue, or burden.
//3. Keep the summary concise (under 5 sentences).
//");

//            var requestBody = new
//            {
//                model = "gpt-4o-mini",
//                messages = new[]
//                {
//            new { role = "system", content = "You are a financial analyst specialized in variance reporting." },
//            new { role = "user", content = promptBuilder.ToString() }
//        }
//            };

//            var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
//            response.EnsureSuccessStatusCode();

//            using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
//            var root = jsonDoc.RootElement;

//            var choices = root.GetProperty("choices");
//            if (choices.GetArrayLength() > 0)
//            {
//                var content = choices[0].GetProperty("message").GetProperty("content").GetString();
//                return content ?? "No variance summary generated.";
//            }

//            return "No variance summary generated.";
//        }


        public async Task<string> GetVarianceSummaryAsync(List<ProjForecastSummary> group)
        {
            // Build combined prompt for the group
            var promptBuilder = new StringBuilder();
            promptBuilder.AppendLine("You are a financial analyst. Analyze the following projects and generate a concise variance summary for management:");

            foreach (var f in group)
            {
                promptBuilder.AppendLine($@"
Project: {f.ProjId}, Month: {f.Month}/{f.Year}
- Forecasted Cost: {f.MonthlyForecastedAmt:C}, Actual Cost: {f.MonthlyActualAmt:C}
- Forecasted Hours: {f.MonthlyForecastedHours}, Actual Hours: {f.MonthlyActualHours}
- Monthly Revenue: {f.MonthlyRevenue:C}, Target Revenue: {f.MonthlyTargetRevenue:C}
- Monthly Burden: {f.MonthlyBurden:C}
");
            }

            promptBuilder.AppendLine(@"
Instructions:
1. Highlight major variances between forecasted and actual values.
2. Mention trends in cost, hours, revenue, or burden.
3. Keep the summary concise (under 5 sentences).
");

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "system", content = "You are a financial analyst specialized in variance reporting." },
                new { role = "user", content = promptBuilder.ToString() }
            }
            };

            return await PostWithRetryAsync(requestBody);
        }

        private async Task<string> PostWithRetryAsync(object requestBody, int maxRetries = 3)
        {
            int delay = 1000; // 1 second initial
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                var response = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", requestBody);
                if (response.IsSuccessStatusCode)
                {
                    using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    var root = jsonDoc.RootElement;
                    var choices = root.GetProperty("choices");
                    if (choices.GetArrayLength() > 0)
                    {
                        return choices[0].GetProperty("message").GetProperty("content").GetString()
                            ?? "No variance summary generated.";
                    }
                }
                else if ((int)response.StatusCode == 429) // rate limit
                {
                    await Task.Delay(delay);
                    delay *= 2; // exponential backoff
                    continue;
                }
                else
                {
                    response.EnsureSuccessStatusCode(); // throw for other errors
                }
            }

            return "No variance summary generated due to rate limits.";
        }

        public async Task<string> GetVarianceSummaryAsync(string plType, int versionA, int versionB, List<VarianceComparison> data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"You are a financial analyst. Compare Version {versionA} and Version {versionB} for Plan Type BUD.");
            sb.AppendLine("Use the provided project data. Focus on differences in forecasted costs, actual costs, hours, and revenue.\n\n");

            sb.AppendLine("Respond in exactly 3 labeled sections:\n\n");

            sb.AppendLine("- **Trends:** (1–2 sentences describing overall performance patterns)\n");
            sb.AppendLine("- **Risks:** (1–2 sentences highlighting key risks or concerns)\n");
            sb.AppendLine("- **Opportunities:** (1–2 sentences noting any positive shifts or improvements)\n\n");

            sb.AppendLine("Do not restate raw numbers.Keep the total response under 5 sentences.\n\n");

            sb.AppendLine("\nData:");

            foreach (var v in data)
            {
                //sb.AppendLine($@"
                //    Project: {v.ProjId}, Month/Year: {v.Month}/{v.Year}
                //    - Forecasted Cost Diff: {v.ForecastedCostDiff:C}
                //    - Actual Cost Diff: {v.ActualCostDiff:C}
                //    - Forecasted Hours Diff: {v.ForecastedHoursDiff}
                //    - Actual Hours Diff: {v.ActualHoursDiff}
                //    - Revenue Diff: {v.RevenueDiff:C}
                //    ");
                sb.AppendLine($@"
                    Project: {v.ProjId}, Month/Year: {v.Month}/{v.Year}
                    - Revenue Diff: {v.RevenueDiff:C}
                    ");
            }

            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                new { role = "system", content = "You are a financial analyst specialized in variance reporting for BUD and EAC." },
                new { role = "user", content = sb.ToString() }
            }
            };

            return await PostWithRetryAsync(requestBody);
        }


    }

}
