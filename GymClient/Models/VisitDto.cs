using Newtonsoft.Json;

namespace GymClient.Models
{
    public class VisitDto : ModelAbstract
    {
        [JsonProperty("visitId")]
        public override int Id { get; set; }
        public int VisitId { get; set; }
        public DateTime VisitDateTime { get; set; }
        public string? ChipNumber { get; set; }
        public int ChipId { get; set; }
        public int MemberId { get; set; }
        public string? MemberFullName { get; set; }

        [JsonIgnore]
        public override string Path { get; set; } = "api/visits/full";
    }
}
