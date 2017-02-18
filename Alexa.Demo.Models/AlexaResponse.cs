using System.Collections.Generic;
using Amazon.Alexa.Speechlet;
using Newtonsoft.Json;

namespace Alexa.Demo.Models
{

    //public class AlexaDemoResponse : Amazon.Alexa.Speechlet.AlexaResponse
    //{
    //    public new AlexaDemoSkillAttributes SessionAttributes { get; set; }
    //}
    //public class AlexaDemoSkillAttributes : Amazon.Alexa.Speechlet.Attributes
    //{
    //    public string Test { get; set; }

    //}

    [JsonObject("response")]
    public class AlexaDemoResponse : Amazon.Alexa.Speechlet.AlexaResponse
    {
        [JsonProperty("sessionAttributes")]
        public new AlexaDemoAttributes SessionAttributes { get; set; }
    }

    //public class AlfredAlexaResponse
    //{
    //    [JsonProperty("version")]
    //    public string Version { get; set; }

    //    [JsonProperty("sessionAttributes")]
    //    public AlfredAttributes SessionAttributes { get; set; }

    //    [JsonProperty("response")]
    //    public Response Response { get; set; }

    //    public AlfredAlexaResponse()
    //    {
    //        Version = "1.0";
    //        SessionAttributes = new AlfredAttributes();
    //        Response = new Response();
    //    }


}