namespace CenterManagement.Application.Interfaces
{
    public interface IQrService
    {
        string GenerateStudentQrCode(string userId);
        string? DecodeQrCode(string qrCode);
        byte[] GenerateQrCodeImage(string content);
    }
}
