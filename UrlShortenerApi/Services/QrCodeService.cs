using System.Drawing.Imaging;
using System.IO;
using QRCoder;

namespace UrlShortenerApi.Services
{
    public class QrCodeService
    {
        public string GenerateQRCore(string path, string httpApiUrl, string hash)
        {
            using var stream = new MemoryStream();

            var qrGenerator = new QRCodeGenerator();

            var qrCodeData = qrGenerator.CreateQrCode(httpApiUrl + hash, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(10);

            qrCodeImage.Save(stream, ImageFormat.Jpeg);

            var imagePath = Path.Join(path, hash + ".jpg");

            File.WriteAllBytes(imagePath, stream.ToArray());

            return imagePath;
        }
    }
}