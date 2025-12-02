using Newtonsoft.Json;

namespace GymClient.Models
{
    public class MemberDto : ModelAbstract
    {
        [JsonProperty("memberId")]
        public override int Id { get; set; }
        public int MemberId { get; set; }
        public string? FullName { get; set; }

        [JsonIgnore]
        public override string Path { get; set; } = "api/members/available_for_chip";
    }
}
