using System;
using Newtonsoft.Json;

namespace Amazon.Alexa.Speechlet
{
    public class AlexaRequest
    {
        public string Version { get; set; }
        public Session Session { get; set; }
        public Context Context { get; set; }
        public Request Request { get; set; }

        public AlexaRequest()
        {
            Version = "1.0";
            Session = new Session();
            Request = new Request();
            Context = new Context();
        }
    }


    public class Context
    {
        public System System { get; set; }
        public Audioplayer AudioPlayer { get; set; }

        public Context()
        {
            System = new System();
            AudioPlayer = new Audioplayer();
        }
    }

    public class Device
    {
        public Supportedinterfaces SupportedInterfaces { get; set; }

        public Device()
        {
            SupportedInterfaces = new Supportedinterfaces();
        }
    }

    public class Supportedinterfaces
    {
        public Audioplayer AudioPlayer { get; set; }

        public Supportedinterfaces()
        {
            AudioPlayer = new Audioplayer();
        }
    }

    public class Audioplayer
    {
        public string Token { get; set; }
        public int OffsetInMilliseconds { get; set; }
        public string PlayerActivity { get; set; }
    }
    public class System
    {
        public Application Application { get; set; }
        public User User { get; set; }
        public Device Device { get; set; }

        public System()
        {
            Application = new Application();
            User = new User();
            Device = new Device();
        }
    }

    public class Attributes
    {
        public string LastRequestIntent { get; set; }

        public Outputspeech OutputSpeech { get; set; }

        public Reprompt Reprompt { get; set; }

    }
    public class Session
    {
        public bool New { get; set; }
        public string SessionId { get; set; }
        public Application Application { get; set; }
        public virtual Attributes Attributes { get; set; }
        public User User { get; set; }

        public Session()
        {
            Application = new Application();
            Attributes = new Attributes();
            User = new User();
        }
    }

    public class Application
    {
        public string ApplicationId { get; set; }
    }


    public abstract class SkillAttributes
    {

        public string LastRequestIntent { get; set; }

        public Outputspeech OutputSpeech { get; set; }

        public Reprompt Reprompt { get; set; }

        public SkillAttributes()
        {
            LastRequestIntent = "";
            OutputSpeech = new Outputspeech();
            Reprompt = new Reprompt();
        }
    }


    public class User
    {
        public string UserId { get; set; }
        public Permissions Permissions { get; set; }

        public User()
        {
            Permissions = new Permissions();
        }
    }


    public class Permissions
    {
        public string ConsentToken { get; set; }
    }


    public class Request
    {

        public string Type { get; set; }
        public string RequestId { get; set; }
        public DateTime Timestamp { get; set; }
        public Intent Intent { get; set; }
        public dynamic Message { get; set; }
        public string Locale { get; set; }

        public Request()
        {
            Intent = new Intent();
        }
    }

    public class Intent
    {
        public string Name { get; set; }
        public dynamic Slots { get; set; }
    }
}



