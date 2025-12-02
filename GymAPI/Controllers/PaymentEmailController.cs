using GymAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class PaymentEmailController : ControllerBase
{
    private readonly EmailService _emailService;

    public PaymentEmailController(EmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> SendPaymentEmail([FromBody] PaymentEmailRequest request)
    {
        try
        {
            byte[] qrBytes = Convert.FromBase64String(request.QrPngBase64);

            await _emailService.SendPaymentEmailAsync(
                request.ToEmail,
                request.MemberName,
                request.Amount,
                request.PaymentUrl,
                qrBytes);

            return Ok(new { Message = "Email sent successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}