namespace GymAPI.Models
{
    public class VisitFullDto
    {
        public int VisitId { get; set; }
        public DateTime VisitDateTime { get; set; }
        public string ChipNumber { get; set; }
        public int ChipId { get; set; }
        public int MemberId { get; set; }
        public string MemberFullName { get; set; }
    }
}
