using MimeKit;

public class EmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly bool _enableSsl;
    private readonly string _smtpUser;
    private readonly string _smtpPass;
    private readonly string _fromName;
    private readonly string _fromEmail;

    public EmailService(string smtpHost, int smtpPort, bool enableSsl, string smtpUser, string smtpPass, string fromName, string fromEmail)
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _enableSsl = enableSsl;
        _smtpUser = smtpUser;
        _smtpPass = smtpPass;
        _fromName = fromName;
        _fromEmail = fromEmail;
    }

    /// <summary>
    /// Send payment email with an inline QR image and optional header image (local file).
    /// headerImagePath: optional local file path (will be attached inline if exists).
    /// qrPngBytes: required PNG bytes for the generated QR.
    /// </summary>
    public async Task SendPaymentEmailAsync(string toEmail,
                                            string memberName,
                                            decimal amount,
                                            string paymentUrl,
                                            byte[] qrPngBytes,
                                            CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = $"Ссылка для оплаты абонемента — {memberName}";

        var builder = new BodyBuilder();

        string html = $@"
        <div style='font-family:Segoe UI, Roboto, Arial, sans-serif;color:#222;max-width:600px;margin:0 auto;padding:18px;'>
          <div style='background:#fff;border-radius:8px;padding:18px;border:1px solid #eee;'>
            <h2 style='margin:0 0 8px 0;font-weight:600;color:#111;'>Оплата абонемента</h2>
            <p style='margin:0 0 8px 0;color:#555;'>Клиент: <strong style='color:#111'>{System.Net.WebUtility.HtmlEncode(memberName)}</strong></p>
            <p style='margin:0 0 16px 0;color:#555;font-size:18px;'><strong style=""font-size:22px;color:#1976d2;"">{amount:N2} ₽</strong></p>
            <div style='text-align:center;margin:16px 0;'>
                <img src='cid:qrcode' alt='QR' style='width:260px;height:260px;border:1px solid #f0f0f0;padding:8px;border-radius:8px;background:#fff;'/>
            </div>
            <p style='margin:12px 0;color:#666;'>Отсканируйте QR-код или нажмите кнопку для перехода к оплате.</p>
            <div style='text-align:center;margin-top:14px;'>
              <a href='{System.Net.WebUtility.HtmlEncode(paymentUrl)}' style='background:#1976d2;color:#fff;padding:12px 22px;text-decoration:none;border-radius:6px;display:inline-block;font-weight:600;'>Оплатить</a>
            </div>
            <p style='margin-top:18px;color:#999;font-size:12px;'>Если кнопка не работает — скопируйте ссылку:<br/><a href='{System.Net.WebUtility.HtmlEncode(paymentUrl)}' style='color:#1976d2;'>{System.Net.WebUtility.HtmlEncode(paymentUrl)}</a></p>
          </div>
          <p style='margin-top:12px;color:#aaa;font-size:12px;text-align:center;'>Gym • Контакты: +7 (000) 000-00-00</p>
        </div>";

        builder.HtmlBody = html;

        // Attach QR as inline resource (cid:qrcode)
        var qrResource = builder.LinkedResources.Add("qrcode.png", qrPngBytes);
        qrResource.ContentId = "qrcode";
        qrResource.ContentType.MediaType = "image";
        qrResource.ContentType.MediaSubtype = "png";

        message.Body = builder.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(_smtpHost, _smtpPort, _enableSsl ? MailKit.Security.SecureSocketOptions.SslOnConnect : MailKit.Security.SecureSocketOptions.StartTls, ct);
        if (!string.IsNullOrEmpty(_smtpUser))
            await client.AuthenticateAsync(_smtpUser, _smtpPass, ct);

        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }
}
