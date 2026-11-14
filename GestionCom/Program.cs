using GestionCom.Components;
using GestionCom.Data;
using GestionCom.Services;
using GestionCom.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.Components.Server;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Enable detailed Blazor circuit errors in development so unhandled exceptions
// on the circuit surface as detailed messages instead of silently terminating
// the connection. Remove or gate this in production.
builder.Services.Configure<CircuitOptions>(options => options.DetailedErrors = true);

// Register an IDbContextFactory so interactive server components can create
// short-lived DbContext instances safely. Some pages inject IDbContextFactory<AppDbContext>.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
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

app.UseForwardedHeaders();

using var scope = app.Services.CreateScope();
var dbFactory = scope.ServiceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext>>();
await using var db = dbFactory.CreateDbContext();
try
{
    db.Database.Migrate();
}
catch (PostgresException ex) when (ex.SqlState == "42P07")
{
    // Relation already exists (table was already created). Log and continue with seeding.
    app.Logger.LogWarning(ex, "Database migration skipped: relation already exists.");
}
catch (InvalidOperationException ex) when (
    ex.Message != null && (ex.Message.Contains("PendingModelChangesWarning", StringComparison.OrdinalIgnoreCase)
    || ex.Message.Contains("pending changes", StringComparison.OrdinalIgnoreCase)))
{
    // EF Core reports pending model changes (migrations not in sync). Log and continue so app can run.
    app.Logger.LogWarning(ex, "Database migration skipped: pending model changes detected.");
}
catch (Exception ex)
{
    // Try to produce a DDL script for operators/DBA to apply manually, then rethrow.
    try
    {
        var script = db.Database.GenerateCreateScript();
        var path = Path.Combine(app.Environment.ContentRootPath, "ef_autogen_create_script.sql");
        await File.WriteAllTextAsync(path, script);
        app.Logger.LogError(ex, "Unexpected error while migrating the database. Generated DDL script at {Path}", path);
    }
    catch (Exception scriptEx)
    {
        app.Logger.LogError(scriptEx, "Failed to generate DDL script after migration error.");
    }

    app.Logger.LogError(ex, "Unexpected error while migrating the database. See generated script or exception details.");
    throw;
}

// Always attempt to seed the database (create roles/admin) even if migration reported existing relations
try
{
    DbInitializer.Seed(db);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Unexpected error while seeding the database.");
    throw;
}

// Diagnostic: log roles present in the database (ignoring soft-delete filter)
try
{
    var roles = db.Roles.IgnoreQueryFilters().AsNoTracking().ToList();
    app.Logger.LogInformation("Database roles found: {Count}", roles.Count);
    foreach (var r in roles)
    {
        app.Logger.LogInformation("Role: Id={Id}, Name={Name}, IsDeleted={IsDeleted}", r.Id, r.Name, r.IsDeleted);
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Unable to read roles for diagnostic logging.");
}



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

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
