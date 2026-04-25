using GestionCom.Components;
using GestionCom.Data;
using GestionCom.Services;
using GestionCom.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<VenteService>();
builder.Services.AddScoped<VenteQueryService>();
builder.Services.AddScoped<AchatService>();
builder.Services.AddScoped<AchatQueryService>();
builder.Services.AddScoped<CalendrierService>();
builder.Services.AddScoped<DepenseService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<UserManagementService>();
builder.Services.AddScoped<AppSettingsService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<ClientQueryService>();
builder.Services.AddScoped<ProduitService>();
builder.Services.AddScoped<ProduitQueryService>();
builder.Services.AddScoped<StockMovementService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PasswordService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddControllers();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
DbInitializer.Seed(db);



app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
