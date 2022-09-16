using AlgorandAuthentication;
using Microsoft.OpenApi.Models;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Logging.ClearProviders();
builder.Host.UseNLog();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(
#if !DEBUG
    c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "KMD service API",
        Version = "v1",
        Description = File.ReadAllText("/kmd/doc/readme.md")
    });
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "ARC-0014 Algorand authentication transaction",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });

    var xmlFile = $"/kmd/doc/documentation.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
    c.IncludeXmlComments(xmlPath);

}
#endif
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

var app = builder.Build();


app.UseCors();
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();
