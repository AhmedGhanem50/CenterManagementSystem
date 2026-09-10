namespace CenterManagement.Application.Interfaces
{
    public interface ISystemSettingService
    {
        Task<string?> GetSettingAsync(string key);
        Task SetSettingAsync(string key, string value, string adminId);
        Task<Dictionary<string, string>> GetAllSettingsAsync();
    }
}
