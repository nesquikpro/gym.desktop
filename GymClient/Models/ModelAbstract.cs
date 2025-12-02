using Newtonsoft.Json;

namespace GymClient.Models
{
    public abstract class ModelAbstract
    {
        public abstract int Id { get; set; }

        public abstract string Path { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {            
                NullValueHandling = NullValueHandling.Ignore
            });
        }
    }
}
