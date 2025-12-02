using GymClient.Interfaces;
using QRCoder;

namespace GymClient.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GeneratePng(string payload, int pixelsPerModule = 20)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var data = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var png = new PngByteQRCode(data);
            return png.GetGraphic(pixelsPerModule);
        }
    }
}
