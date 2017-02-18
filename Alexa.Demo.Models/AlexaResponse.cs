using System.Collections.Generic;
using Amazon.Alexa.Speechlet;
using Newtonsoft.Json;

namespace Alexa.Demo.Models
{

    [JsonObject("response")]
    public class AlexaDemoResponse : Amazon.Alexa.Speechlet.AlexaResponse
    {
        [JsonProperty("sessionAttributes")]
        public new AlexaDemoAttributes SessionAttributes { get; set; }
    }
}