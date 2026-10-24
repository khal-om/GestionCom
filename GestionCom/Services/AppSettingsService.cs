using GestionCom.Data;
using GestionCom.DTOs.Settings;
using GestionCom.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionCom.Services
{
    public class AppSettingsService
    {
        public const string CompanyNameKey = "CompanyName";
        public const string CompanyTaglineKey = "CompanyTagline";
        public const string CompanyAddressKey = "CompanyAddress";
        public const string CompanyPhoneKey = "CompanyPhone";
        public const string SupportEmailKey = "SupportEmail";
        public const string LowStockThresholdKey = "LowStockThreshold";

        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AppSettingsService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AppSettingsDto> GetAsync()
        {
            var settings = await _context.AppSettings
                .AsNoTracking()
                .ToDictionaryAsync(s => s.Key, s => s.Value);

            return new AppSettingsDto
            {
                CompanyName = GetValue(settings, CompanyNameKey, _configuration["Branding:CompanyName"] ?? "FedNad Holding"),
                CompanyTagline = GetNullableValue(settings, CompanyTaglineKey, "Gestion commerciale"),
                CompanyAddress = GetNullableValue(settings, CompanyAddressKey, null),
                CompanyPhone = GetNullableValue(settings, CompanyPhoneKey, null),
                SupportEmail = GetNullableValue(settings, SupportEmailKey, null),
                LowStockThreshold = int.TryParse(GetValue(settings, LowStockThresholdKey, "5"), out var threshold) ? threshold : 5
            };
        }

        public async Task SaveAsync(AppSettingsDto dto, int userId)
        {
            if (dto == null)
                throw new InvalidOperationException("Données invalides.");

            await UpsertAsync(CompanyNameKey, dto.CompanyName.Trim(), userId);
            await UpsertAsync(CompanyTaglineKey, dto.CompanyTagline?.Trim(), userId);
            await UpsertAsync(CompanyAddressKey, dto.CompanyAddress?.Trim(), userId);
            await UpsertAsync(CompanyPhoneKey, dto.CompanyPhone?.Trim(), userId);
            await UpsertAsync(SupportEmailKey, dto.SupportEmail?.Trim(), userId);
            await UpsertAsync(LowStockThresholdKey, dto.LowStockThreshold.ToString(), userId);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLowStockThresholdAsync()
        {
            var value = await _context.AppSettings
                .AsNoTracking()
                .Where(s => s.Key == LowStockThresholdKey)
                .Select(s => s.Value)
                .FirstOrDefaultAsync();

            return int.TryParse(value, out var threshold) ? threshold : 5;
        }

        private async Task UpsertAsync(string key, string? value, int userId)
        {
            var setting = await _context.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
            var normalizedValue = string.IsNullOrWhiteSpace(value) ? string.Empty : value;

            if (setting == null)
            {
                _context.AppSettings.Add(new AppSetting
                {
                    Key = key,
                    Value = normalizedValue,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
                return;
            }

            setting.Value = normalizedValue;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = userId;
        }

        private static string GetValue(IReadOnlyDictionary<string, string> settings, string key, string fallback)
            => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;

        private static string? GetNullableValue(IReadOnlyDictionary<string, string> settings, string key, string? fallback)
            => settings.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
    }
}
