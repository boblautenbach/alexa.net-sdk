using Newtonsoft.Json;

namespace Amazon.Alexa.Models
{
    public class AlexaRequest
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("session")]
        public AlexaSession Session { get; set; }

        [JsonProperty("context")]
        public AlexaContext Context { get; set; }

        [JsonProperty("request")]
        public AlexaRequestType Request { get; set; }

        public AlexaRequest()
        {
            Version = "1.0";
            Session = new AlexaSession();
            Context = new AlexaContext();
            Request = new AlexaRequestType();
        }
    }
}