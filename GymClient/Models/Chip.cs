using Newtonsoft.Json;

namespace GymClient.Models
{
    public class Chip : ModelAbstract, ICloneable
    {
        [JsonProperty("chipId")]
        public override int Id { get; set; }
        public int MemberId { get; set; }
        public string? ChipNumber { get; set; }
        public bool IsActive { get; set; }
        public string? MemberFullName { get; set; }

        [JsonIgnore]
        public override string Path{ get; set; } = "/api/chips";

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
