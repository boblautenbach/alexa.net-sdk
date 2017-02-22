using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Alfred.Api.Filters;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Alexa.Notification.Manager;
using Alexa.Notification.Manager.Models;
using Amazon.Alexa.Speechlet;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Globalization;
using Alexa.Demo.Models;
using Alexa.Demo.Web.Helpers;
using Alfred.Api.BaseClasses;
using Alfred.Api.Handlers;
using Alexa.Demo.Web.Handlers;

namespace Alexa.Demo.Web.Api.Controllers
{
    [UnhandledExceptionFilter]
    [RoutePrefix("api/alexa")]
    public class AlexaController : ApiController, IIntentHandler
    {
        #region :   Fields   :
        private const double TimeStampTolerance = 150;
        private const int DATE_MAX_IN_DAYS = 60; //example of timeframe you might to check for
        private const int CacheExpireMinutes = 5;

        //TODO
        string _launchMessagerePrompt = @"You can create notification reminders for weddings, birthdays, anniversaries, deadlines, and many, many more.  Just say, Alexa, tell my event notifications to add a birthday reminder on july 3rd, or, to get a list of your upcoming notifications, you can say, Alexa, what are my upcoming reminders.  It's quick and easy, so why not add a notification now?";
        string _launchMessage = @"It’s a wonderful thing when you can schedule an event reminder, knowing Alexa will notify you when it arrives. Now with My Event Notifications, Alexa will keep you on task with a notification on the day of your event, as well as a 7 day pre-event notification.  You can create notification reminders for weddings, birthdays, anniversaries, deadlines, and many, many more.  Just say, Alexa, tell my event notifications to add a birthday reminder on july 3rd, or, to get a list of your upcoming notifications, you can say, Alexa, what are my upcoming reminders.  It's quick and easy, so why not add a notification now?";

        string _helpMessagerePrompt = @"You can create notification reminders for weddings, birthdays, anniversaries, deadlines, and many, many more.  Just say, Alexa, tell my event notifications to add a birthday reminder on july 3rd, or, to get a list of your upcoming notifications, justsay, Alexa, what are my upcoming reminders.  It's quick and easy, so why not add a notification now?";

        string _helpMessage = @"You can create notification reminders for weddings, birthdays, anniversaries, deadlines, and many, many more.  Just say, Alexa, tell my event notifications to add a birthday reminder on july 3rd, or, to get a list of your upcoming notifications, justsay, Alexa, what are my upcoming reminders.  It's quick and easy, so why not add a notification now?";

        #endregion

        #region : Helpers :
        private const string EndpointUri = "https://whatnow.documents.azure.com:443/";
        private const string docdb = "whatnow";
        private const string docdbcollection = "whatnowcollection";
        private const string PrimaryKey = "MmUGsTNK8LWFc2DeDUYaNO9HRzyn0IYCPPHFmOiPMcSvE6yOw7x9NE5a73y8IsCeYV3ADpvzJ3S7g7bPDOnzuA==";
        const string MY_NOTIFCATIONS = "MY_NOTIFCATIONS";
        const string MY_NOTIFCATIONS_MONTH = "MY_NOTIFCATIONS_MONTH";

        string INVALID_RESPONSE_CONTENT = "This is embarassing, but for some reason we weren't able understand what you said.  Could you repeat that  again?";
        string INVALID_RESPONSE_REPROMT = "Give us another chance, please. Could you repeat that again?";
        bool isValid = false;

        public string State { get; set; }

        public enum YesNoAction
        {
            ShouldDeleteReminder,
            IsForAPerson,
            ShouldMakeAnnual
        }

        private async Task<AlexaDemoResponse> ThrowSafeException(Exception exception, AlexaDemoResponse response, [CallerMemberName] string methodName = "")
        {
            var content = @"We encountered some trouble, but don't worry, we have our team looking into it now.  We apologize for the inconvenience, we should have this fixed shortly. Please try again later.";

            System.Diagnostics.Trace.TraceInformation(exception.Message);
            response.Response.OutputSpeech.Text = content;
            response.Response.ShouldEndSession = true;

            try
            {

            }
            catch (Exception ex)
            {
                //TODO
            }

            return response;
        }

        #endregion : Helpers :

        //TODO
        /// <summary>
        /// Add random prefix string to responses
        /// 
        /// </summary>
        public AlexaController()
        {
        }

        #region :   Main-End-Points   :
        [HttpPost, Route("main")]
        public async Task<AlexaDemoResponse> Main(AlexaDemoRequest alexaRequest)
        {
            AlexaDemoResponse response = new AlexaDemoResponse();

            try
            {
                //check timestamp
                var totalSeconds = (DateTime.UtcNow - alexaRequest.Request.Timestamp).TotalSeconds;
                if (totalSeconds >= TimeStampTolerance)
                    throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                if (alexaRequest.Session.Application.ApplicationId != null)
                {
                    if (alexaRequest.Session.Application.ApplicationId != AppSettings.AmazonAppId)
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                }else
                {
                    if (alexaRequest.Context.System.Application.ApplicationId != AppSettings.AmazonAppId)
                        throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.BadRequest));

                }
                response.SessionAttributes = alexaRequest.Session.Attributes;

                var handler1 = new ListReminderHandler("ListReminder");
                var handler2 = new CreateReminderHandler("CreateReminder");
                this.State = "Main";

                var sdk = new AlexaNetSDK();
                sdk.RegisterHandlers(new List<IIntentHandler>() {this, handler1 , handler2});

                alexaRequest =  SetRequestTypeState(alexaRequest);

                response = sdk.HandleIntent(alexaRequest, response);

                response.SessionAttributes.OutputSpeech = response.Response.OutputSpeech;

                //BuildOutTextOutPut();
                //BuildOutTextOutPutWithCard()
                //BuildOutTextOutPutWithCardImages();

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private AlexaDemoRequest SetRequestTypeState(AlexaDemoRequest request)
        {

            switch (request.Request.Type)
            {
                case "LaunchRequest":
                    request.Session.Attributes.State = "Main"; ;
                    break;
                case "SessionEndedRequest":
                    request.Session.Attributes.State = "Main";
                    break;
                case "Messaging.MessageReceived":
                    request.Session.Attributes.State = "Main";
                    break;
                case "IntentRequest":
                    switch (request.Request.Intent.Name)
                    {
                        case "ListReminders":
                            request.Session.Attributes.State = "ListReminder";
                            break;
                        default:
                            request.Session.Attributes.State = "CreateReminder";
                            break;
                    }
                    break;
            }

            return request;
        }


        public async Task<AlexaDemoResponse> MessageReceivedRequest(AlexaDemoRequest alexaRequest, AlexaDemoResponse response)
        {
            try
            {
                if (ValidatePermission(alexaRequest)){

                    AlexaNotificationManager manager = new AlexaNotificationManager();

                    var notif = new UserNotifcation();
                    notif.DisplayInfo = new Displayinfo();
                    notif.SpokenInfo = new SpokenInfo();

                    List<DisplayContent> displayList = new List<DisplayContent>();
                    List<Content> contentList = new List<Content>();

                    var data = alexaRequest.Request.Message;
                    var content = (JArray)data["content"];

                    foreach (JObject locale in content.Children())
                    {
                        DisplayContent display = new DisplayContent();
                        Content spoke = new Content();
             
                        display.Title = (String)locale["notificationTitle"].ToString();
                        display.Body = (String)locale["notificationBody"].ToString();
                        display.Locale = (String)locale["locale"].ToString();

                        spoke.Locale = (String)locale["locale"].ToString();
                        spoke.Text = (String)locale["spokenOutput"].ToString();
                        // spoke.type = (String)locale["spokenOutputType"].ToString().ToUpper();

                        contentList.Add(spoke);
                        displayList.Add(display);

                    }

                    notif.DisplayInfo.Content = displayList.ToArray();
                    notif.SpokenInfo.Content = contentList.ToArray();

                    var result = manager.SendToUser(alexaRequest.Context.System.User.Permissions.ConsentToken, notif);

                }else
                {
                    response.Response.Card = new Amazon.Alexa.Speechlet.Card();
                    response.Response.Card.Type = "AskForPermissionsConsent";
                    response.Response.Card.Permissions = Alexa.Notification.Manager.Models.Permissions.Permission;
                }

            }
            catch(Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }
            return response;
        }

        private bool ValidatePermission(AlexaDemoRequest request)
        {
            return (!string.IsNullOrEmpty(request.Context.System.User.Permissions.ConsentToken));
        }

        public AlexaDemoResponse UnHandledIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            try
            {
                return HelpIntent(request, response);

            }
            catch (Exception ex)
            {
                return HelpIntent(request, response);
            }

        }

        public AlexaDemoResponse HelpIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            response.Response.OutputSpeech.Text = _helpMessage;
            response.Response.Reprompt.OutputSpeech.Text = _helpMessagerePrompt;

            response.Response.Card = new Card();
            response.Response.Card.Title = "My Event Notifications Help";
            response.Response.Card.Content = _helpMessage;
            response.Response.Card.Type = "Standard";
            response.Response.Card.Image = new Image();
            response.Response.Card.Image.SmallImageUrl = "https://whatnowapi.azurewebsites.net/images/skillSmall.png";
            response.Response.Card.Image.LargeImageUrl = "https://whatnowapi.azurewebsites.net/images/skillLarge.png";

            response.Response.ShouldEndSession = false;

            return response;
        }

        public AlexaDemoResponse CancelIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            response.Response.OutputSpeech.Text = "Thank you for using our Skill. Make it a great day!";
            response.Response.ShouldEndSession = true;
            return response;
        }

        public AlexaDemoResponse RepeatIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            if (string.IsNullOrEmpty(request.Session.Attributes.LastRequestIntent))
            {
                return HelpIntent(request, response);
            }
            else
            {
                response.Response.OutputSpeech = request.Session.Attributes.OutputSpeech;
                response.Response.Reprompt = request.Session.Attributes.Reprompt;
                return response;
            }
        }
        #endregion :   Main-End-Points   :

        #region :   Alexa Type Handlers   :

        public AlexaDemoResponse LaunchRequest(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            response.Response.OutputSpeech.Text = _launchMessage;
            response.Response.Reprompt.OutputSpeech.Text = _launchMessagerePrompt;

            response.Response.Card = new Card();
            response.Response.Card.Title = "My Event Notifications - Welcome!";
            response.Response.Card.Content = _launchMessage;
            response.Response.Card.Type = "Standard";
            response.Response.Card.Image = new Image();
            response.Response.Card.Image.SmallImageUrl = "https://whatnowapi.azurewebsites.net/images/skillSmall.png";
            response.Response.Card.Image.LargeImageUrl = "https://whatnowapi.azurewebsites.net/images/skillLarge.png";
            response.Response.ShouldEndSession = false;

            return response;
        }
        
        private string ExpectedResponseValidation(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            string message = string.Empty;

            if (string.IsNullOrEmpty(request.Session.Attributes.ExpectedIntents))
            {
                return string.Empty;
            }

            if (request.Request.Intent.Name.ToLower().Contains("amazon.")){
                if(!request.Request.Intent.Name.ToLower().Contains("amazon.yesintent, amazon.nointent"))
                {
                    return string.Empty;
                }

            }


            if (!request.Session.Attributes.ExpectedIntents.ToLower().Contains(request.Request.Intent.Name.ToLower()))
            {
                message = "I'm sorry, I didn't understand your response.  Can your repeat that. ";

                //if (request.Session.Attributes.OutputSpeech.Type == "SSML")
                //{
                //    var speakTagEnd = request.Session.Attributes.OutputSpeech.Ssml.IndexOf(">");
                //    var newResponse = request.Session.Attributes.OutputSpeech.Ssml.Insert(speakTagEnd, messagePrefix);
                //}
                //else
                //{
                //    var speakTagEnd = request.Session.Attributes.OutputSpeech.Text.IndexOf(">");
                //    var newResponse = request.Session.Attributes.OutputSpeech.Text.Insert(speakTagEnd, messagePrefix);
                //}
            }

            return message;
            
        }

        private bool SlotIsEmptyOrNull(dynamic slot, string name)
        {
            return (slot[name] == null);
        }

        private AlexaDemoResponse SessionEndedRequest(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            response.Response.OutputSpeech.Text = "Make it a great day!";
            response.Response.ShouldEndSession = true;


            return response;
        }

        private async Task<AlexaDemoResponse> ProcessYesIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            string content = string.Empty;
            string reprompt = string.Empty;
            response.Response.ShouldEndSession = false;
            try
            {
                //said yes to delete a reminder
                if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldDeleteReminder.ToString())
                {


                }
                //said yes to its recurring
                else if (request.Session.Attributes.YesNoAction == YesNoAction.IsForAPerson.ToString())
                    {


                }
                //said yes to its recurring
                else if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldMakeAnnual.ToString())
                {

                }
                else
                {

                    return RepeatIntent(request, response);

                }
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        public AlexaDemoResponse BuildResponseOutput(AlexaDemoResponse response, string outPut, string reprompt, string expectedIntents = "", string yesNoAction = "", string textType = "PlainText", Dictionary<string, string> outputSubstitution = null, Dictionary<string, string> repromptSubstitution = null)
        {
            //use SSML to most flexibility
            //get response for given language

            response.Response.OutputSpeech.Type = textType;
            response.Response.Reprompt.OutputSpeech.Type = textType;
            response.Response.OutputSpeech.Text = "";
            response.Response.Reprompt.OutputSpeech.Text = "";

            response.SessionAttributes.ExpectedIntents = expectedIntents;
            response.SessionAttributes.YesNoAction = yesNoAction;

            //response.Response.output.Type = lang.speechtype
            if (textType == "SSML")
            {
                response.Response.OutputSpeech.Type = textType;
                response.Response.Reprompt.OutputSpeech.Type = textType;

                response.Response.OutputSpeech.Ssml = @"<speak>" + outPut + "</speak>";
                response.Response.Reprompt.OutputSpeech.Ssml = @"<speak>" + reprompt + "</speak>";

                if (outputSubstitution != null)
                {
                    foreach (KeyValuePair<string, string> entry in outputSubstitution)
                    {
                        response.Response.OutputSpeech.Ssml = response.Response.OutputSpeech.Ssml.Replace(entry.Key, entry.Value);
                    }
                }

                if (repromptSubstitution != null)
                {
                    foreach (KeyValuePair<string, string> entry in repromptSubstitution)
                    {
                        response.Response.Reprompt.OutputSpeech.Ssml = response.Response.Reprompt.OutputSpeech.Ssml.Replace(entry.Key, entry.Value);
                    }
                }
            }
            else
            {
                response.Response.OutputSpeech.Text = outPut;
                response.Response.Reprompt.OutputSpeech.Text = reprompt;

                if (outputSubstitution != null)
                {
                    foreach (KeyValuePair<string, string> entry in outputSubstitution)
                    {
                        response.Response.OutputSpeech.Text = response.Response.OutputSpeech.Text.Replace(entry.Key, entry.Value);
                    }
                }

                if (repromptSubstitution != null)
                {
                    foreach (KeyValuePair<string, string> entry in repromptSubstitution)
                    {
                        response.Response.Reprompt.OutputSpeech.Text = response.Response.Reprompt.OutputSpeech.Text.Replace(entry.Key, entry.Value);
                    }
                }
            }

            return response;
        }

        private async Task<AlexaDemoResponse> ProcessNoIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            string content = string.Empty;
            string reprompt = string.Empty;

            try
            {

                if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldDeleteReminder.ToString())
                //said No to delete a reminder
                {
                    //get next notification it list and read it aloud

                }
                //no on the recruing questions
                else if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldMakeAnnual.ToString())
                {
                }
            
                //no on is for a peson
                else if (request.Session.Attributes.YesNoAction ==  YesNoAction.IsForAPerson.ToString())
                {

                }
                else
                {

                    return RepeatIntent(request, response);

                }
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }
            return response;
        }
    
        private async Task<AlexaDemoResponse> IntentRequest(AlexaDemoRequest request, AlexaDemoResponse response)
        {
            bool shouldSetLastIntent = true;

            System.Diagnostics.Trace.TraceInformation(request.Request.Intent.Name);

          //  var result = GetType().GetMethod(request.Request.Intent.Name).Invoke(this, new Object[] { request, response });

            var validationMessage = ExpectedResponseValidation(request, response);

            if (!string.IsNullOrEmpty(validationMessage))
            {
                response.Response.ShouldEndSession = false;
                response = BuildResponseOutput(response, validationMessage, validationMessage);
                return response;
            }

            switch (request.Request.Intent.Name)
            {
                case "AMAZON.RepeatIntent":
                    response = RepeatIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "UnknownIntent":
                    response = UnHandledIntent(request, response);
                    break;
                case "SimpleTestIntent":
                    response = SimpleTestIntent(request, response);
                    break;
                case "AMAZON.CancelIntent":
                    response = CancelIntent(request, response);
                    break;
                case "AMAZON.StopIntent":
                    response = CancelIntent(request, response);
                    break;
                case "AMAZON.HelpIntent":
                    response = HelpIntent(request, response);
                    break;
                case "AMAZON.NoIntent":
                    response = await ProcessNoIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "AMAZON.YesIntent":
                    response = await ProcessYesIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
            }

            response.SessionAttributes.ExpectedIntents = "Hello";
            response.SessionAttributes.YesNoAction = "World";
            response.SessionAttributes.LastRequestIntent = "LAst";

            if (shouldSetLastIntent)
            {
                response.SessionAttributes.LastRequestIntent = request.Request.Intent.Name;
            }

            return response;
        }

        private AlexaDemoResponse SimpleTestIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {

            return BuildResponseOutput(response, "Hello Alexa.Net SDK Team", "");
        }

        public dynamic UnHandledIntent(dynamic request, dynamic response)
        {
            throw new NotImplementedException();
        }


        #endregion :   Alexa Type Handlers   :

    }
}