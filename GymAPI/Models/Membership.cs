namespace GymAPI.Models;

public partial class Membership
{
    public int MembershipId { get; set; }
    public int MemberId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsPaidByCard { get; set; }
    public string? PaymentQRCode { get; set; }
    public bool IsFrozen { get; set; }
    public DateOnly? FreezeStartDate { get; set; }
    public DateOnly? FreezeEndDate { get; set; }
    public int TotalFrozenDays { get; set; }
}
