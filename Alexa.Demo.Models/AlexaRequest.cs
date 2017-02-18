using Amazon.Alexa.Speechlet;
using Newtonsoft.Json;

namespace Alexa.Demo.Models
{

    public class AlexaDemoRequest : Amazon.Alexa.Speechlet.AlexaRequest
    {
        public new AlexaDemoSession Session { get; set; }
    }

    public class AlexaDemoSession : Amazon.Alexa.Speechlet.Session
    {
        public new AlexaDemoAttributes Attributes { get; set; }
    }

    public class AlexaDemoAttributes : Amazon.Alexa.Speechlet.Attributes
    {
        public string ExpectedIntents { get; set; }
        public string YesNoAction { get; set; }
    }
}


