using Amazon.Alexa.Speechlet;
using Newtonsoft.Json;

namespace Alexa.Demo.Models
{
    //public class AlfredAlexaRequest
    //{
    //    public string Version { get; set; }
    //    public AlfredSession Session { get; set; }
    //    public Context Context { get; set; }
    //    public Request Request { get; set; }

    //    public AlfredAlexaRequest()
    //    {
    //        Version = "1.0";
    //        Session = new AlfredSession();
    //        Request = new Request();
    //        Context = new Context();
    //    }
    //}

    //public class AlfredSession
    //{
    //    public bool New { get; set; }
    //    public string SessionId { get; set; }
    //    public Application Application { get; set; }
    //    public AlfredAttributes Attributes { get; set; }
    //    public User User { get; set; }

    //    public AlfredSession()
    //    {
    //        Application = new Application();
    //        Attributes = new AlfredAttributes();
    //        User = new User();
    //    }
    //}

    //public class AlfredAlexaRequest: Amazon.Alexa.Speechlet.AlexaRequest
    //{
    //}

    //public class AlfredSession : Amazon.Alexa.Speechlet.Session
    //{
    //    public new AlfredAttributes Attributes { get; set; }
    //}

    public class AlfredAttributes : Amazon.Alexa.Speechlet.Attributes
    {
        public string EventFor { get; set; }

        public bool IsCreatingEvent { get; set; }
        public string EventType { get; set; }
        public string ReminderDate { get; set; }
        public bool IsRecurring { get; set; }

        public string LastSpokenDocId { get; set; }

        public string ExpectedIntents { get; set; }

        public string YesNoAction { get; set; }
        public string RequestedListMonth { get; set; }

        public bool IsSimpleDateEvent { get; set; }
        public bool HasReachedDocListEnd { get; set; }
        public int NextDocIndex { get; set; }

    }

}


