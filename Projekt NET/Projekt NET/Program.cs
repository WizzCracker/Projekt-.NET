using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projekt_NET.Models.System;
using Projekt_NET.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;
// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "API", Version = "v1" });

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var actionDescriptor = apiDesc.ActionDescriptor as ControllerActionDescriptor;

        return actionDescriptor?.ControllerTypeInfo
            .GetCustomAttributes(typeof(ApiControllerAttribute), inherit: true)
            .Any() == true;
    });
});


builder.Services.AddAntiforgery();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.ConfigureApplicationCookie(options =>
{

    options.AccessDeniedPath = "/Home/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Home/Login";
});
builder.Services.AddAuthorization();
builder.Services.AddTransient<AuthService>();
var connectionString = builder.Configuration.GetConnectionString("DRONES");
builder.Services.AddDbContext<DroneDbContext>(x => x.UseSqlServer(connectionString));
builder.Services.AddSingleton<WeatherService>();
builder.Services.AddScoped<DroneService>();
builder.Services.AddHttpClient<GoogleGeocodingService>();
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddCors();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<DroneDbContext>();
        var droneService = services.GetRequiredService<DroneService>();

        var flights = context.Flights
            .Where(f => f.ArrivDate == null && f.DroneId != null)
            .ToList();

        foreach (var flight in flights)
        {
            droneService.MoveDroneAsync(flight.DroneId.Value, flight.DeliveryCoordinates.Latitude, flight.DeliveryCoordinates.Longitude);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Startup drone move failed: " + ex.Message);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin()
          .AllowAnyHeader()
          .WithMethods("GET", "POST");
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
