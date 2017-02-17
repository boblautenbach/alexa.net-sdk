namespace Amazon.Alexa.Models
{
    public class AlexaSystem
    {
        public AlexaApplication Application { get; set; }

        public AlexaUser User { get; set; }

        public AlexaDevice Device { get; set; }

        public AlexaSystem()
        {
            Application = new AlexaApplication();
            User = new AlexaUser();
            Device = new AlexaDevice();
        }
    }
}