using Amazon.Alexa.Models;

namespace Amazon.Alexa
{
    public class Interactions
    {
        /*
         * TODO:
         *  - Finish adding built-in intents and default messages.
         *  - Create AlexaRequest and AlexaResponse objects that developers can override and extend.
         *  - Handle LaunchRequest and SessionEndedRequest
         *  - Add CertificateHandler
         *  - Add caching to IntentsList
         *  - Add Exceptions
         */

        private AlexaRequest _request;

        public Messages Messages = new Messages();
        public IntentsList IntentsList = new IntentsList();

        /// <summary>
        /// Constructor requires AlexaRequest object.
        /// </summary>
        /// <param name="request">AlexaRequest object</param>
        public Interactions(AlexaRequest request)
        {
            _request = request;

            //handle LaunchRequest and SessionEndedRequest just like any other intents
            if (_request.Request.Type != "IntentRequest" && string.IsNullOrWhiteSpace(_request.Request.Intent.Name))
                _request.Request.Intent.Name = "LaunchRequest";

            //automatically includes built-in intent handlers
            IntentsList.Add("LaunchRequest", (req) => new { message = Messages.LaunchMessage });
            IntentsList.Add("AMAZON.CancelIntent", (req) => new { });
            IntentsList.Add("AMAZON.LoopOffIntent", (req) => new { });
            IntentsList.Add("AMAZON.LoopOnIntent", (req) => new { });
            IntentsList.Add("AMAZON.NextIntent", (req) => new { });
            IntentsList.Add("AMAZON.NoIntent", (req) => new { });
            IntentsList.Add("AMAZON.HelpIntent", (req) => new { message = Messages.HelpMessage });
            IntentsList.Add("AMAZON.StopIntent", (req) => new { message = Messages.StopMessage });
            //public string PauseMessage = "This is the pause message";
            //public string PreviousMessage = "This is the previous message";
            //public string RepeatMessage = "This is the repeat message";
            //public string ResumeMessage = "This is the resume message";
            //public string ShuffleOffMessage = "This is the shuffle off message";
            //public string ShuffleOnMessage = "This is the shuffle on message";
            //public string StartOverMessage = "This is the start over message";
            //public string StopMessage = "This is the stop message";
            //public string YesMessage = "This is the yes message";
        }

        /// <summary>
        /// Use this method to process intent handler methods found in the sender class.
        /// </summary>
        /// <typeparam name="T">The sender class type.</typeparam>
        /// <param name="sender">The sender class.</param>
        /// <returns>AlexaResponse object.</returns>
        public dynamic r<T>(T sender)
        {
            var intentName = (_request.Request.Intent.Name).Replace("AMAZON.", string.Empty);
            var method = typeof(T).GetMethod(intentName);

            //if method exist, invoke it with optional request param, 
            //otherwise check to see if it's available in the IntentsList
            return (method == null)
                ? Process()
                : method.Invoke(sender, (method.GetParameters().Length == 0) ? null : new object[] { _request });
        }

        /// <summary>
        /// Use this method to process a list of all custom intent handler. This will remove all previous built-in and custom intents.
        /// </summary>
        /// <param name="intentHandlers">List of new intent handlers.</param>
        /// <returns>Alexa.Models.Response object.</returns>
        public dynamic Process(IntentsList intentsList)
        {
            IntentsList = intentsList;
            return Process();
        }

        /// <summary>
        /// Use to this method to process the IntentsList including automatically generated built-in intents as well as any custom intents added.
        /// </summary>
        /// <returns>Alexa.Models.Response object.</returns>
        public dynamic Process()
        {
            var index = IntentsList.FindIndex(i => i.Key.Equals(_request.Request.Intent.Name));

            //TODO: does not exit, return exception instead
            return (index > -1) ? IntentsList[index].Value(_request) : null;
        }
    }
}