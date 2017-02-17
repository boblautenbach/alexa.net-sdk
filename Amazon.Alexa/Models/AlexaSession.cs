namespace Amazon.Alexa.Models
{
    public class AlexaSession
    {
        public bool New { get; set; }
        public string SessionId { get; set; }
        public AlexaApplication Application { get; set; }
        public AlexaAttributes Attributes { get; set; }
        public AlexaUser User { get; set; }

        public AlexaSession()
        {
            Application = new AlexaApplication();
            Attributes = new AlexaAttributes();
            User = new AlexaUser();
        }
    }
}