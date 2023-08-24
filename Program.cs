using Algorand.Indexer.Model;
using AlgorandAuthentication;
using AlgorandKMDServer.Extension;
using AlgorandKMDServer.Model;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using NLog.Web;
using Prometheus;
using System.Reflection;

[assembly: AssemblyVersionAttribute("1.0.*")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(

    c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KMD service API",
        Version = "v1",
        Description = File.ReadAllText("doc/readme.md")
    });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "ARC-0014 Algorand authentication transaction",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });

    var xmlFile = $"doc/documentation.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.IncludeXmlComments(xmlPath);

}
    );

bool.TryParse(builder.Configuration["algod:checkExpiration"], out var checkExpiration);
if (string.IsNullOrEmpty(builder.Configuration["algod:checkExpiration"])) checkExpiration = true;
bool.TryParse(builder.Configuration["algod:debug"], out var debug);
Console.WriteLine($"checkExpiration: {builder.Configuration["algod:checkExpiration"]} {checkExpiration}");
Console.WriteLine($"debug: {builder.Configuration["algod:debug"]} {debug}");

builder.Services
   .AddAuthentication(AlgorandAuthenticationHandler.ID)
   .AddAlgorand(o =>
   {
       o.CheckExpiration = checkExpiration;
       o.AlgodServer = builder.Configuration["algod:server"];
       o.AlgodServerToken = builder.Configuration["algod:token"];
       o.AlgodServerHeader = builder.Configuration["algod:header"];
       o.Realm = builder.Configuration["algod:realm"];
       o.NetworkGenesisHash = builder.Configuration["algod:networkGenesisHash"];
       o.Debug = debug;
   });


var corsConfig = builder.Configuration.GetSection("Cors").AsEnumerable().Select(k => k.Value).Where(k => !string.IsNullOrEmpty(k)).ToArray();
Console.WriteLine($"CORS setup: {string.Join(", ", corsConfig)}");
if (corsConfig.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
        builder =>
        {
            builder.WithOrigins(corsConfig)
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}
else
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(
        builder =>
        {
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });
}

builder.Services.AddOpenTelemetryExtension(builder.Configuration, DiagnosticsConfig.ServiceName);
builder.Services.AddHealthChecks().AddCheck<AlgorandKMDServer.Extension.HealthCheck>("participation-server");

var app = builder.Build();

var version = Assembly.GetExecutingAssembly()?.GetName()?.Version;
if (version != null)
{
    Metrics.CreateGauge("BuildMajor", "version.Major").Set(Convert.ToDouble(version.Major));
    Metrics.CreateGauge("BuildMinor", "version.Minor").Set(Convert.ToDouble(version.Minor));
    Metrics.CreateGauge("BuildRevision", "version.Revision").Set(Convert.ToDouble(version.Revision));
    Metrics.CreateGauge("BuildBuild", "version.Build").Set(Convert.ToDouble(version.Build));
}

app.UseMetricServer();

app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    },
    ResponseWriter = HealthWriteResponse.WriteResponse
});

app.Run();
