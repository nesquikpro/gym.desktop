using Newtonsoft.Json;

namespace GymClient.Models
{
    public class Member : ModelAbstract, ICloneable
    {
        [JsonProperty("memberId")]
        public override int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? RegistrationDate { get; set; }

        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}";

        [JsonIgnore]
        public string FullNameWithPhone => $"{FirstName} {LastName}, {PhoneNumber}"; 

        [JsonIgnore]
        public override string Path { get; set; } = "api/members";

        public static Member Create(string firstName, string lastName, DateOnly dateOfBirth, 
            string phoneNumber, string email, int id = 0)
        {
            return new Member
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                PhoneNumber = phoneNumber,
                Email = email,
                RegistrationDate = DateTime.Now
            };
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
