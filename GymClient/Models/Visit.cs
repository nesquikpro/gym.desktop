using Newtonsoft.Json;

namespace GymClient.Models
{
    public class Visit : ModelAbstract
    {
        [JsonProperty("visitId")]
        public override int Id { get; set; }
        public int ChipId { get; set; }
        public DateTime VisitDateTime { get; set; }

        [JsonIgnore]
        public override string Path { get; set; } = "/api/visits";
    }
}
