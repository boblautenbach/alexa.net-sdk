using Amazon.Alexa.Models;
using System;
using System.Web.Http;

namespace Amazon.Alexa.Demo.Web.Controllers
{
    public class ApiAlexaController : ApiController
    {
        /// <summary>
        /// This is a sample Alexa endpoint that implements the SDK.
        /// </summary>
        /// <param name="request">The incoming AlexaRequest object.</param>
        /// <returns>Outgoing AlexaResponse object.</returns>
        [HttpPost, Route("api/alexa")]
        // POST api/alexa
        public dynamic Post(AlexaRequest request)
        {
            dynamic response;
            
            //option 1 - uncomment the following to test option 1, comment out to test option 2
            var interactionHandler1 = new Interactions(request);
            interactionHandler1.Messages.StopMessage = "This is where you can override the default stop message.";
            response = interactionHandler1.Process(this);

            //option 2 - uncomment the following to test option 2, comment out to test option 1
            //var interactionHandler2 = new Interactions(request);
            //interactionHandler2.IntentsList.Add("LaunchRequest", (req) => LaunchRequest(req));
            //interactionHandler2.IntentsList.Add("WelcomeIntent", (req) => WelcomeIntent(req));
            //interactionHandler2.IntentsList.Add("AMAZON.HelpIntent", (req) => HelpIntent());
            //interactionHandler2.IntentsList.Add("AMAZON.StopIntent", (req) => new { message = "This is the overriden built-in stop message." });
            //response = interactionHandler2.Process();

            //This is created as Option #1 in the Alexa.Demo.Web Project it V2 AlexaController..instead
            //of creating a intenthandler and calling them dirrectly as below, just pass a list of your
            //intent handlerst to the SDK.
            response =  BuidOutWithCard(IntentsHandler.Process(request, response)) as AlexaResponse;

            
            return response;
        }

        private AlexaResponse BuidOutWithCard(AlexaResponse response)
        {
            return response;
        }

        public dynamic LaunchRequest(AlexaRequest request)
        {
            return new { message = "This is the launch request message!" };
        }

        /// <summary>
        /// This is an example of a custom intent handler, these should match the intent names in the Amazon Developer Portal.
        /// </summary>
        /// <returns>The AlexaResponse object.</returns>
        public dynamic WelcomeIntent(AlexaRequest request)
        {
            return new { message = "It works! " + request.Request.Intent.Name };
        }

        /// <summary>
        /// This is an example of overriding a built-in intent handler, these should match the intent names in the Amazon Developer Portal.
        /// Built-in intents are handled automatically in the SDK in which case you can set the response text in 
        /// <see cref="Amazon.Alexa.Messages"/>.
        /// </summary>
        /// <example> 
        /// To override built-in intents, simply omit the "AMAZON." portion, 
        /// for example, instead of AMAAZON.HelpIntent, use simply HelpIntent as follows:
        /// <code>
        /// public dynamic HelpIntent 
        /// {
        ///     return null;
        /// }
        /// </code>
        /// </example>
        /// <returns>The AlexaResponse object.</returns>
        public dynamic HelpIntent()
        {
            return new { message = "This is the overriden built-in help message." };
        }
    }
}