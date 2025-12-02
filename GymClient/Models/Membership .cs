using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace GymClient.Models
{
    public class Membership : ModelAbstract, ICloneable
    {
        [JsonProperty("membershipId")]
        public override int Id { get; set; }
        public int MemberId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsPaidByCard { get; set; }
        public string? PaymentQRCode { get; set; }
        public bool IsFrozen { get; set; }
        public DateOnly? FreezeStartDate { get; set; }
        public DateOnly? FreezeEndDate { get; set; }
        public int TotalFrozenDays { get; set; }

        [JsonIgnore]
        public DateTime? FreezeStartDateTime
        {
            get => FreezeStartDate.HasValue ? FreezeStartDate.Value.ToDateTime(new TimeOnly(0, 0)) : (DateTime?)null;
            set => FreezeStartDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }

        [JsonIgnore]
        public DateTime? FreezeEndDateTime
        {
            get => FreezeEndDate.HasValue ? FreezeEndDate.Value.ToDateTime(new TimeOnly(0, 0)) : (DateTime?)null;
            set => FreezeEndDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : null;
        }


        [NotMapped]
        public string FreezePeriod => IsFrozen && FreezeStartDate.HasValue && FreezeEndDate.HasValue
        ? $"{FreezeStartDate:dd.MM.yyyy} – {FreezeEndDate:dd.MM.yyyy}"
        : "";

        [JsonIgnore]
        public override string Path { get; set; } = "/api/memberships";

        [JsonIgnore]
        public string MemberFullName { get; set; } = string.Empty;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
