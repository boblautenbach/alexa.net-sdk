using Amazon.Alexa.SDK.Models.AlexaSpeechlets;

namespace Alexa.Demo.Models
{

    public class AlexaDemoRequest : AlexaRequest
    {
        public new AlexaDemoSession Session { get; set; }
    }

    public class AlexaDemoSession : Session
    {
        public new AlexaDemoAttributes Attributes { get; set; }
    }

    public class AlexaDemoAttributes : Attributes
    {
        public string ExpectedIntents { get; set; }
        public string YesNoAction { get; set; }
    }
}


