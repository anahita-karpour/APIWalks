//--we are creating a builder and then we are injecting services to our builder object or inside our application
//--these services are then later on used by our application

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using NZWalks.API;
using NZWalks.API.Data;
using NZWalks.API.Mappings;
using NZWalks.API.Middlewares;
using NZWalks.API.Repositories;
using NZWalks.API.Services.Auth;
using NZWalksAPI.Controllers;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json"); // Make sure this line is present

// Add services to the container.
// 1. create and configure your Serilog Logger instance using new LoggerConfiguration() create and configure your Serilog Logger instance using new LoggerConfiguration()
//var _logger 

Log.Logger = new LoggerConfiguration()  // Create a new Serilog logger
.WriteTo.Console()                  // Write log messages to the console
.WriteTo.File("Logs/NzWalks_Log.txt", rollingInterval: RollingInterval.Day) // Write log messages to a file
.MinimumLevel.Warning()         // Set the minimum log level to Information - ONce we change the log level to Information, it will only log the information level and above
                                // for example if we change this to Warning, it will not show  the information level logs or debug level logs
.CreateLogger();                    // Create the logger

// 2. configure the logging system to use Serilog
builder.Logging.ClearProviders();       // Remove the default logging provider

// 3. assign this configured logger to Log.Logger (Serilog's static entry point)
//builder.Logging.AddSerilog(_logger);     // Add Serilog as the logging provider
builder.Host.UseSerilog();               // Use Serilog as the logging provider

builder.Services.AddControllers();

// Add versioning
builder.Services.AddApiVersioning(option =>
    {
        //options.AssumeDefaultVersionWhenUnspecified = true;
        option.DefaultApiVersion = new ApiVersion(1, 0); // Default version is 1.0
        option.AssumeDefaultVersionWhenUnspecified = true;
        option.ReportApiVersions = true; // Report the available versions
        //opt.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader(),
        //                                               new HeaderApiVersionReader("X-Api-Version"),
        //                                             new MediaTypeApiVersionReader("x-api-version"));
    }).AddApiExplorer(
    options =>
    {
        options.GroupNameFormat = "'v'VVV"; // Format of the version
        options.SubstituteApiVersionInUrl = true; // Substitute the version in the URL
    }
);

builder.Services.Configure<RouteOptions>(options =>
{
    options.ConstraintMap.Add("apiVersion", typeof(Asp.Versioning.Routing.ApiVersionRouteConstraint));
});

builder.Services.AddHttpContextAccessor();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Inject the DbContext to the services
builder.Services.AddDbContext<NZWalksDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NZWalksConnectionString"))
   );

builder.Services.AddDbContext<NZWalksAuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("NZWalksAuthConnectionString"))
);

// injects the region repository with the implementation SQL region repository.
// AddScoped: a new instance per request; it is for the lifetime of the request; 
//            A database context (like Entity Framework's DbContext).
//            All database operations during a single HTTP request should use 
//            the same connection to ensure consistency.
builder.Services.AddScoped<IRegionRepository, SQLRegionRepository>();
builder.Services.AddScoped<IWalkRepository, SQLWalkRepository>();
builder.Services.AddScoped<IImageRepository, SQLImageRepository>();
builder.Services.AddScoped<IMemberRepository, SQLMemberRepository>();
builder.Services.AddScoped<IRefreshTokenService, SQLRefreshTokenRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();


// for in memory implementation
// builder.Services.AddScoped<IRegionRepository, InMemoryRegionRepository>();

// inject automapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfiles));

// inject identity - before AddAuthentication
builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddTokenProvider<DataProtectorTokenProvider<IdentityUser>>("NZWalks")
    .AddEntityFrameworkStores<NZWalksAuthDbContext>()
    .AddSignInManager<SignInManager<IdentityUser>>() // Add SignInManager (important for sign-in related actions)
    .AddDefaultTokenProviders();
//By using AddIdentityCore in your API, you're keeping the authentication logic separate from the UI.

//builder.Services.AddIdentity<IdentityUser, IdentityRole>()
//    .AddEntityFrameworkStores<NZWalksUIAuthDbContext>() // The .AddDefaultUI() and .AddEntityFrameworkStores() calls will add the
//    .AddDefaultUI();                                    //  signin manager to the dependency injection container.


// Identity Options: Password Configuration
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;
});

//var keyVaultEndpoint = builder.Configuration["KeyVault:Endpoint"];
//if (!string.IsNullOrEmpty(keyVaultEndpoint))
//{
//    builder.Configuration.AddAzureKeyVault(
//        new Uri(keyVaultEndpoint),
//        new DefaultAzureCredential());
//}

// 2. Configure JWT authentication:
// Authentication: Verifies the identity of the user (e.g., "Are you who you claim to be?"). JWT Bearer authentication handles this part.
// JWT Bearer authentication is used to extract and validate // the JWT from the incoming request. This establishes the user's identity.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            AuthenticationType = "Jwt",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudiences = new[] { builder.Configuration["Jwt:Audience"] },

            //IssuerSigningKey = new SymmetricSecurityKey(
            //    Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."))),
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is missing from configuration."))),
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            // API expects roles to be found under ClaimTypes.Role, but our JWT token has them under a different claim key.
            // By default, ASP.NET Core's [Authorize(Roles = "Writer")] looks for the role claim (just "role", or ClaimTypes.Role).
            // But in our token, it's using the long-form URI.
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"API Authentication Failed: {context.Exception?.Message}");
                return Task.CompletedTask;
            }
        };
    });

// Authorization: Determines what a user is allowed to do (e.g., "Are you allowed to access this resource?").
// AddAuthorization() sets up the framework for this.
// this sets up the middleware that checks the user's roles, claims, or other authorization policies to determine
// if they have the necessary permissions to access the requested resource.
//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("WriterOnly", policy => policy.RequireRole("writer"));
//});
builder.Services.AddAuthorization();

//Register the servce for the JWT Token Service
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RegionsController>();

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>(); // Add the ConfigureSwaggerOptions to the services

//--finally we build the builder object
var app = builder.Build();


//--Configuring the HTTP request pipeline
//--we are adding middleware to handle request and responses
var versionDescriptionProvider =
app.Services.GetRequiredService<IApiVersionDescriptionProvider>(); // Get the version description provider

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        //registering all the api descriptions available in the provider in the form swagger endpoint
        foreach (var description in versionDescriptionProvider.ApiVersionDescriptions) // Loop through the API versions descriptions
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant()); // Add the Swagger endpoint description
        }
    });
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();

// Add Authentication
app.UseAuthentication();
app.UseAuthorization();

// static file middleware
app.UseStaticFiles(new StaticFileOptions
{
    //FileProvider
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Images")),
    RequestPath = "/Images"
    //https://Localhost:1234/Images
});

app.MapControllers();

//--Once middleware is configured, we run our application
app.Run();
