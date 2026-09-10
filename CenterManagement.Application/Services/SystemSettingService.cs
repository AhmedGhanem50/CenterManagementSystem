using CenterManagement.Application.Interfaces;
using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CenterManagement.Application.Services
{
    public class SystemSettingService : ISystemSettingService
    {
        private readonly CenterManagementDbContext _db;
        private readonly IAuditLogService _auditLogService;

        public SystemSettingService(CenterManagementDbContext db, IAuditLogService auditLogService)
        {
            _db = db;
            _auditLogService = auditLogService;
        }

        public async Task<string?> GetSettingAsync(string key)
        {
            var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            return setting?.Value;
        }

        public async Task SetSettingAsync(string key, string value, string adminId)
        {
            var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
            if (setting == null)
            {
                setting = new SystemSetting
                {
                    Key = key,
                    Value = value,
                    CreatedAt = DateTime.UtcNow
                };
                _db.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            await _auditLogService.LogAsync(
                adminId,
                "SystemSettingChanged",
                "SystemSetting",
                setting.Id,
                null,
                JsonSerializer.Serialize(new { Key = key, Value = value }));
        }

        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            return await _db.SystemSettings
                .ToDictionaryAsync(s => s.Key, s => s.Value);
        }
    }
}
