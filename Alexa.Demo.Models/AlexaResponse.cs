using System.Collections.Generic;
using Amazon.Alexa.Speechlet;
using Newtonsoft.Json;

namespace Alexa.Demo.Models
{

    public class AlexaDemoResponse : Amazon.Alexa.Speechlet.AlexaResponse
    {
        public AlexaDemoResponse()
        {
            SessionAttributes = new AlexaDemoSkillAttributes();
        }
    }
    public class AlexaDemoSkillAttributes : Amazon.Alexa.Speechlet.SkillAttributes
    {
        public string Test { get; set; }

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