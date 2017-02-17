using Newtonsoft.Json;

namespace Amazon.Alexa.Models
{
    public class AlexaContext
    {
        [JsonProperty("System")]
        public AlexaSystem System { get; set; }

        [JsonProperty("AudioPlayer")]
        public string AudioPlayer { get; set; }

        public AlexaContext()
        {
            System = new AlexaSystem();
        }
    }
}