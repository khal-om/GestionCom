using System.Text.Json;
using Npgsql;

// Simple helper to insert migration ids into __EFMigrationsHistory without modifying schema.
// Usage: dotnet run --project Tools/ApplyMigrationsHistory

var repoRoot = Directory.GetCurrentDirectory();
// If running from Tools/ApplyMigrationsHistory, repo root is two levels up
var possible = Path.GetFullPath(Path.Combine(repoRoot, "..", ".."));
if (Directory.Exists(Path.Combine(possible, "GestionCom")))
{
    repoRoot = possible;
}

var appsettingsPath = Path.Combine(repoRoot, "GestionCom", "appsettings.json");
if (!File.Exists(appsettingsPath))
{
    Console.Error.WriteLine("Could not find appsettings.json at: " + appsettingsPath);
    return 1;
}

var json = await File.ReadAllTextAsync(appsettingsPath);
using var doc = JsonDocument.Parse(json);
var root = doc.RootElement;
var conn = root.GetProperty("ConnectionStrings").GetProperty("DefaultConnection").GetString();
if (string.IsNullOrWhiteSpace(conn))
{
    Console.Error.WriteLine("Connection string not found in appsettings.json");
    return 1;
}

var migrationsDir = Path.Combine(repoRoot, "GestionCom", "Migrations");
if (!Directory.Exists(migrationsDir))
{
    Console.Error.WriteLine("Migrations folder not found: " + migrationsDir);
    return 1;
}

// Collect migration ids and product versions from Designer.cs files
var migrations = new List<(string Id, string ProductVersion)>();
foreach (var file in Directory.GetFiles(migrationsDir, "*.Designer.cs"))
{
    var content = await File.ReadAllTextAsync(file);
    // extract Migration attribute and ProductVersion
    string id = null;
    string version = "10.0.6"; // fallback

    // find [Migration("...Id...")]
    var migTag = "[Migration(\"";
    var idx = content.IndexOf("[Migration(\"");
    if (idx >= 0)
    {
        var start = idx + "[Migration(\"".Length;
        var end = content.IndexOf('"', start);
        if (end > start)
        {
            id = content.Substring(start, end - start);
        }
    }

    // find ProductVersion
    var pvTag = ".HasAnnotation(\"ProductVersion\", \"";
    idx = content.IndexOf("ProductVersion", StringComparison.OrdinalIgnoreCase);
    if (idx >= 0)
    {
        // crude: find last occurrence of HasAnnotation("ProductVersion", "..."
        var pvIdx = content.IndexOf("HasAnnotation(\"ProductVersion\", \"", StringComparison.OrdinalIgnoreCase);
        if (pvIdx >= 0)
        {
            var pvStart = pvIdx + "HasAnnotation(\"ProductVersion\", \"".Length;
            var pvEnd = content.IndexOf('\"', pvStart);
            if (pvEnd > pvStart)
            {
                version = content.Substring(pvStart, pvEnd - pvStart);
            }
        }
        else
        {
            // fallback: try to find "ProductVersion", "x"
            var near = content.Substring(Math.Max(0, idx - 80), Math.Min(200, content.Length - Math.Max(0, idx - 80)));
            var quote1 = near.IndexOf('"');
            var quote2 = near.IndexOf('"', quote1 + 1);
            if (quote1 >= 0 && quote2 > quote1)
            {
                version = near.Substring(quote1 + 1, quote2 - quote1 -1);
            }
        }
    }

    if (id == null)
    {
        // derive from filename
        id = Path.GetFileName(file).Replace(".Designer.cs", "");
    }

    migrations.Add((id, version));
}

// Also include migration .cs files that may not have Designer.cs (e.g., SQL-only migrations)
foreach (var file in Directory.GetFiles(migrationsDir, "*.cs"))
{
    if (file.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)) continue;
    if (file.EndsWith("AppDbContextModelSnapshot.cs", StringComparison.OrdinalIgnoreCase)) continue;

    var content = await File.ReadAllTextAsync(file);
    string id = null;
    // find [Migration("...")]
    var idx = content.IndexOf("[Migration(\"");
    if (idx >= 0)
    {
        var start = idx + "[Migration(\"".Length;
        var end = content.IndexOf('"', start);
        if (end > start)
            id = content.Substring(start, end - start);
    }

    if (id == null)
    {
        id = Path.GetFileName(file).Replace(".cs", "");
    }

    // use default product version for these
    var version = "10.0.6";
    // avoid duplicates
    if (!migrations.Any(m => m.Id == id)) migrations.Add((id, version));
}

// Order migrations by id (filename prefix)
migrations = migrations.OrderBy(m => m.Id).ToList();

Console.WriteLine($"Found {migrations.Count} migrations to insert into __EFMigrationsHistory.");

// Build SQL statements
var sqlBuilder = new System.Text.StringBuilder();
sqlBuilder.AppendLine("BEGIN;");
foreach (var m in migrations)
{
    var safeId = m.Id.Replace("'", "''");
    var safeVer = m.ProductVersion.Replace("'", "''");
    sqlBuilder.AppendLine($"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ('{safeId}', '{safeVer}') ON CONFLICT (\"MigrationId\") DO NOTHING;");
}
sqlBuilder.AppendLine("COMMIT;");

var sql = sqlBuilder.ToString();

Console.WriteLine("Will execute SQL to insert migration history. Connection: " + conn);
Console.WriteLine("Proceed? (y/n)");
var ans = Console.ReadLine();
if (!ans.Equals("y", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Aborted by user.");
    return 0;
}

try
{
    await using var connPg = new NpgsqlConnection(conn);
    await connPg.OpenAsync();
    await using var cmd = connPg.CreateCommand();
    cmd.CommandText = sql;
    var rows = await cmd.ExecuteNonQueryAsync();
    Console.WriteLine("Executed migration history inserts.");
}
catch (Exception ex)
{
    Console.Error.WriteLine("Error executing SQL: " + ex.Message);
    return 1;
}

return 0;
