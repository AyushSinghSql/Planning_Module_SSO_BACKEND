using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using PlanningAPI.Repositories;
using QuestPDF.Infrastructure;
using Serilog;
using System;
using System.Text;
using System.Text.Json.Serialization;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Repositories;
using WebApi.Services;


var builder = WebApplication.CreateBuilder(args);

// Add this to your service configuration
builder.Services.AddControllers(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// --- 1. ADD SERVICES ---
builder.Services.AddControllers();
// 1. Add this to your services block

// 1. Add Authentication Services
// 1. Start the authentication builder
var authenticationBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

// 2. Add Microsoft Identity (Azure AD)
authenticationBuilder.AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// 3. Add your Custom JWT Authentication with a specific name/scheme
authenticationBuilder.AddJwtBearer("LocalAuth", options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    // 1. Define the policy that accepts either Microsoft or Local tokens
    var policy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme, "LocalAuth")
        .RequireAuthenticatedUser()
        .Build();

    // 2. Set as Default for [Authorize] attributes
    options.DefaultPolicy = policy;

    // 3. FallbackPolicy ensures that even if you forget [Authorize] 
    // on a controller, it remains protected.
    options.FallbackPolicy = policy;
});

// Define the CORS policy properly
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://planning-module-sso.vercel.app") // Your React URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Important for Auth
    });
});

// ADD THIS: You need authentication services for [Authorize] to work

// Configure Serilog for file logging
Serilog.Log.Logger = new LoggerConfiguration()
    .WriteTo.File("Logs/log-.txt",         // Output folder and rolling file name
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 7,    // Optional: keep last 7 logs
                  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();
QuestPDF.Settings.License = LicenseType.Community;
builder.Host.UseSerilog(); // Use Serilog instead of default logging
// add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    services.AddCors();
    services.AddControllers().AddJsonOptions(x =>
    {
        // serialize enums as strings in api responses (e.g. Role)
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        // ignore omitted parameters on models to enable optional params (e.g. User update)
        x.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddOpenApi();

    var endpoint = builder.Configuration["AI:Endpoint"];
    var apiKey = builder.Configuration["AI:ApiKey"];
    var model = builder.Configuration["AI:ModelName"];

    builder.Services.AddChatClient(services =>
      new ChatClientBuilder(
        (
          !string.IsNullOrEmpty(apiKey)
            ? new AzureOpenAIClient(new Uri(endpoint!), new AzureKeyCredential(apiKey))
            : new AzureOpenAIClient(new Uri(endpoint!), new DefaultAzureCredential())
        ).GetChatClient(model).AsIChatClient()
      )
      .UseFunctionInvocation()
      .Build());

    services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

    // configure strongly typed settings object
    // services.Configure<DbSettings>(builder.Configuration.GetSection("DbSettings"));

    // configure DI for application services
    //services.AddSingleton<DataContext>();
    builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
    builder.Services.AddHostedService<PlanningAPI.Services.QueuedHostedService>();
    builder.Services.Configure<PowerBISettings>(
    builder.Configuration.GetSection("PowerBI"));




    services.AddScoped<IOrgRepository, OrgRepository>();
    services.AddScoped<IOrgService, OrgService>();
    services.AddScoped<IPlForecastRepository, PlForecastRepository>();
    services.AddScoped<IPl_ForecastService, Pl_ForecastService>();

    services.AddScoped<IProjPlanRepository, ProjPlanRepository>();
    services.AddScoped<IProjPlanService, ProjPlanService>();

    services.AddScoped<IProjRepository, ProjRepository>();
    services.AddScoped<IProjService, ProjService>();
    services.AddScoped<IEmplRepository, EmplRepository>();
    services.AddScoped<IEmplService, EmplService>();

    services.AddScoped<IDirectCostRepository, DirectCostRepository>();
    services.AddScoped<IDirectCostService, DirectCostService>();

    services.AddScoped<IProjectFees_Repository, ProjectFees_Repository>();
    builder.Services.AddScoped<IProspectiveEntityRepository, ProspectiveEntityRepository>();

    builder.Services.AddScoped<IProjectPlcRateRepository, ProjectPlcRateRepository>();
    builder.Services.AddScoped<IProjBgtRevSetupRepository, ProjBgtRevSetupRepository>();
    builder.Services.AddScoped<IProjVendRtRepository, ProjVendRtRepository>();
    builder.Services.AddScoped<IProjEmplRtRepository, ProjEmplRtRepository>();
    services.AddScoped<IRevFormulaRepository, RevFormulaRepository>();
    services.AddScoped<IProjRevWrkPdRepository, ProjRevWrkPdRepository>();
    services.AddScoped<IVendorEmployeeRepository, VendorEmployeeRepository>();
    services.AddScoped<IVendorEmployeeRepository, VendorEmployeeRepository>();
    services.AddScoped<IHolidayCalendarRepository, HolidayCalendarRepository>();
    services.AddScoped<IHolidayCalendarRepository, HolidayCalendarRepository>();
    services.AddScoped<IUserRepository, UserRepository>();
    //services.AddScoped<IAiService, OpenAiService>();

    builder.Services.AddHttpClient<IAiService, OpenAiService>();
    //services.AddDbContext<MydatabaseContext>(options => options.UseNpgsql("Host=localhost;Database=planning;Username=myuser;Password=mypassword;Include Error Detail=true;"));
    //services.AddDbContext<MydatabaseContext>(options => options.UseNpgsql("Host=dpg-d0n1vd2li9vc7380m3o0-a.singapore-postgres.render.com;Database=planning;Username=myuser;Password=ODIfyKykuj6zdwchsnqAzccSMNeRgGQ7;Include Error Detail=true;"));

    services.AddDbContext<MydatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
    //services.AddDbContext<MydatabaseContext>(options => options.UseNpgsql("Host=dpg-d0n1vd2li9vc7380m3o0-a.singapore-postgres.render.com;Database=Test_Import;Username=myuser;Password=ODIfyKykuj6zdwchsnqAzccSMNeRgGQ7;Include Error Detail=true;"));
}

var app = builder.Build();
app.UseRouting();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
// ✅ Correct relative path for Docker (no hardcoded localhost)
c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");

// ✅ Ensures Swagger UI is at http://localhost:<port>/swagger
c.RoutePrefix = "swagger";
});

//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    //app.UseSwaggerUI(); // Optional: you can add options here
//    app.UseSwaggerUI(c =>
//    {
//        // ✅ Explicit relative path — DO NOT use localhost URLs
//        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
//        c.RoutePrefix = "swagger"; // so it's at /swagger
//    });
//}

//// ensure database and tables exist
//{
//    using var scope = app.Services.CreateScope();
//    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
//    await context.Init();
//}

// configure HTTP request pipeline
{
    // global cors policy
    //app.UseCors(x => x
    //    .AllowAnyOrigin()
    //    .AllowAnyMethod()
    //    .AllowAnyHeader());

    app.UseCors("AllowLocalhost");

    // global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
}

//app.Run("http://localhost:4000");
//app.Run("http://0.0.0.0:8080");
app.Run();