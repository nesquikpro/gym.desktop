using System.ComponentModel.DataAnnotations.Schema;

namespace GymAPI.Models;

public partial class Chip
{
    public int ChipId { get; set; }
    public int MemberId { get; set; }
    public string? ChipNumber { get; set; }
    public bool IsActive { get; set; }
    [NotMapped]
    public string? MemberFullName { get; set; }
}
