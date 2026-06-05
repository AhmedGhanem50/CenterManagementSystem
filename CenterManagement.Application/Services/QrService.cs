using System.Text;
using CenterManagement.Application.Interfaces;
using QRCoder;

namespace CenterManagement.Application.Services
{
    public class QrService : IQrService
    {
        public string GenerateStudentQrCode(string userId)
            => Convert.ToBase64String(Encoding.UTF8.GetBytes(userId));

        public string? DecodeQrCode(string qrCode)
        {
            try { return Encoding.UTF8.GetString(Convert.FromBase64String(qrCode)); }
            catch { return null; }
        }

        public byte[] GenerateQrCodeImage(string content)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(10);
        }
    }
}
