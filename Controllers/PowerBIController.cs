using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlanningAPI.Models;
using System.Net.Http.Headers;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class PowerBIController : ControllerBase
{
    private readonly PowerBISettings _settings;

    public PowerBIController(IOptions<PowerBISettings> settings)
    {
        _settings = settings.Value;
    }

    [HttpGet("embed-config")]
    public async Task<IActionResult> GetEmbedConfig(string datasetName)
    {
        var accessToken = await GetAccessToken();

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        string workspaceId = null;
        string datasetId = null;

        var workspaceResponse = await client.GetAsync(
            "https://api.powerbi.com/v1.0/myorg/groups");

        workspaceResponse.EnsureSuccessStatusCode();

        var workspaceJson = JsonConvert.DeserializeObject<dynamic>(
            await workspaceResponse.Content.ReadAsStringAsync());

        foreach (var ws in workspaceJson.value)
        {
            var wsId = ws.id;

            var datasetResponse = await client.GetAsync(
                $"https://api.powerbi.com/v1.0/myorg/groups/{wsId}/datasets");

            if (!datasetResponse.IsSuccessStatusCode)
                continue;

            var datasetJson = JsonConvert.DeserializeObject<dynamic>(
                await datasetResponse.Content.ReadAsStringAsync());

            foreach (var ds in datasetJson.value)
            {
                if (ds.name == datasetName)
                {
                    workspaceId = wsId;
                    datasetId = ds.id;
                    break;
                }
            }

            if (datasetId != null)
                break;
        }

        if (datasetId == null)
            return BadRequest("Dataset not found");

        var reportResponse = await client.GetAsync(
            $"https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/reports");

        reportResponse.EnsureSuccessStatusCode();

        var reportJson = JsonConvert.DeserializeObject<dynamic>(
            await reportResponse.Content.ReadAsStringAsync());

        string reportId = null;
        string embedUrl = null;

        foreach (var r in reportJson.value)
        {
            if (r.datasetId == datasetId)
            {
                reportId = r.id;
                embedUrl = r.embedUrl;
                break;
            }
        }

        if (reportId == null)
            return BadRequest("Report not found for dataset");

        var tokenRequest = new
        {
            accessLevel = "View"
        };

        var content = new StringContent(
            JsonConvert.SerializeObject(tokenRequest),
            Encoding.UTF8,
            "application/json");

        var tokenResponse = await client.PostAsync(
            $"https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/reports/{reportId}/GenerateToken",
            content);

        tokenResponse.EnsureSuccessStatusCode();

        var tokenJson = JsonConvert.DeserializeObject<dynamic>(
            await tokenResponse.Content.ReadAsStringAsync());

        return Ok(new EmbedConfig
        {
            ReportId = reportId,
            EmbedUrl = embedUrl,
            EmbedToken = tokenJson.token
        });
    }

    private async Task<string> GetAccessToken()
    {
        var authority =
            $"https://login.microsoftonline.com/{_settings.TenantId}";

        var app = ConfidentialClientApplicationBuilder
            .Create(_settings.ClientId)
            .WithClientSecret(_settings.ClientSecret)
            .WithAuthority(authority)
            .Build();

        var result = await app.AcquireTokenForClient(
            new[] { "https://analysis.windows.net/powerbi/api/.default" })
            .ExecuteAsync();

        return result.AccessToken;
    }

    
    [HttpGet("datasets")]
    public async Task<IActionResult> GetAllDatasets()
    {
        var accessToken = await GetAccessToken();

        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // 1. Get workspaces
        var workspaceResponse = await client.GetAsync(
            "https://api.powerbi.com/v1.0/myorg/groups");

        workspaceResponse.EnsureSuccessStatusCode();

        var workspaceJson = JsonConvert.DeserializeObject<dynamic>(
            await workspaceResponse.Content.ReadAsStringAsync());

        var result = new List<object>();

        Console.WriteLine(result);

        // 2. Loop workspaces → get datasets
        foreach (var ws in workspaceJson["value"])
        {
            string workspaceId = ws["id"]?.ToString();
            string workspaceName = ws["name"]?.ToString();

            var datasetResponse = await client.GetAsync(
                $"https://api.powerbi.com/v1.0/myorg/groups/{workspaceId}/datasets");

            if (!datasetResponse.IsSuccessStatusCode)
                continue;

            var datasetJson = JsonConvert.DeserializeObject<JObject>(
    await datasetResponse.Content.ReadAsStringAsync());

            foreach (var ds in datasetJson["value"])
            {
                result.Add(new
                {
                    workspaceId,
                    workspaceName,
                    datasetId = ds["id"]?.ToString(),
                    datasetName = ds["name"]?.ToString()
                });
            }
        }

        return Ok(result);
    }


}