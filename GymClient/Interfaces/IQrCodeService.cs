namespace GymClient.Interfaces
{
    public interface IQrCodeService
    {
        byte[] GeneratePng(string payload, int pixelsPerModule = 20);
    }
}
