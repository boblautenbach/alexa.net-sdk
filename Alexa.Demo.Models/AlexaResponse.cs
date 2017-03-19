using Newtonsoft.Json;
using Amazon.Alexa.SDK.Models.AlexaSpeechlets;

namespace Alexa.Demo.Models
{

    [JsonObject("response")]
    public class AlexaDemoResponse : AlexaResponse
    {
        [JsonProperty("sessionAttributes")]
        public new AlexaDemoAttributes SessionAttributes { get; set; }
    }
}