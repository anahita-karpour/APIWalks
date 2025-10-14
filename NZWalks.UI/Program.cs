using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.Cookies;
using NZWalks.UI.Mappings;
using NZWalks.UI.Services.Auths;
using NZWalks.UI.Services.Common;
using NZWalks.UI.Services.Members;
using NZWalks.UI.Services.Regions;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache(); // Or another distributed cache
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

//Add versioning
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


// inject the HttpClient service into the DI container
builder.Services.AddHttpClient("NZWalksAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:LocalUrl"]); //ApiExplorerSettingsAttribute:ApiBaseURl// Replace with your API's base URL
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

//builder.Services.AddHttpClient<RegionApiService>(client =>
//{
//    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]);
//    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//});


builder.Services.AddAutoMapper(typeof(AutoMapperProfileUI)); // Register AutoMapper with the assembly
builder.Services.AddScoped<IHttpClientAuthService, HttpClientAuthService>();
builder.Services.AddScoped<ICookieService, CookieService>(); // or AddTransient or AddSingleton
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IJwtTokenServiceUI, JwtTokenServiceUI>();
builder.Services.AddScoped<IMemberService, MemberService>(); // Register the MemberService
builder.Services.AddScoped<IRegionService, RegionService>();

// Add Authentication Configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Adjust as needed
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Auths/AccessDenied"; // Ensure this matches your controller route
    options.LoginPath = "/Auths/Login";           // Ensure this matches your controller route
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Recommended for security
    options.Cookie.SameSite = SameSiteMode.Strict;       // Recommended for security
});

// Clear default claim mapping and set custom role claim type
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // necessary for roles to correctly map

//Policy base authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnlyInAdminArea", policy =>
        policy.RequireRole("Admin")
              .RequireClaim("area", "Admin"));
});


builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add Session Middleware
app.UseSession();

app.UseAuthentication(); // Ensure this comes before UseAuthorization
app.UseAuthorization();

app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}"
    );

app.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");




app.Run();
