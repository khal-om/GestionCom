using GestionCom.Components;
using GestionCom.Data;
using GestionCom.Services;
using GestionCom.Services.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    // Rethrow unexpected exceptions so the app doesn't start in a bad state.
    app.Logger.LogError(ex, "Unexpected error while migrating the database.");
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

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
