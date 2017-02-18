using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Alfred.Api.Filters;
using System.Runtime.CompilerServices;
using Alfred.Api.Helpers;
using System.Collections.Generic;
using AlfredAlexaModels;
using Alexa.Notification.Manager;
using Alexa.Notification.Manager.Models;
using Amazon.Alexa.Speechlet;
using Microsoft.Azure.Documents.Client;
using WhatNow.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Documents;
using System.Linq;
using System.Globalization;

namespace WhatNow.Api.Controllers
{
    [UnhandledExceptionFilter]
    [RoutePrefix("api/alexa")]
    public class AlexaController : ApiController
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
        private static DocumentClient client;
        const string MY_NOTIFCATIONS = "MY_NOTIFCATIONS";
        const string MY_NOTIFCATIONS_MONTH = "MY_NOTIFCATIONS_MONTH";

        string INVALID_RESPONSE_CONTENT = "This is embarassing, but for some reason we weren't able understand what you said.  Could you repeat that  again?";
        string INVALID_RESPONSE_REPROMT = "Give us another chance, please. Could you repeat that again?";
        bool isValid = false;

        public enum YesNoAction
        {
            ShouldDeleteReminder,
            IsForAPerson,
            ShouldMakeAnnual
        }
        private async Task CreateNewReminder(NotificationRequestItem reminder)
        {
            try
            {
       
                await client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(docdb, docdbcollection), reminder);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not write to the Document DB");
            }
        }

        List<NotificationRequestItem> GetMyNotificationsByMonth(string month, string userId)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<NotificationRequestItem> docs = new List<NotificationRequestItem>();

            docs = client.CreateDocumentQuery<NotificationRequestItem>(
                UriFactory.CreateDocumentCollectionUri(docdb, docdbcollection), queryOptions)
                .AsEnumerable()
                .Where(f => DateTime.Parse(f.EventDate).Month.ToString() == month)
                .OrderBy(d => DateTime.Parse(DateTime.Today.Year + "-" + DateTime.Parse(d.EventDate).Month + "-" + DateTime.Parse(d.EventDate).Day))
                .ToList();

            MemCacher.MemoryCacher.Delete(MY_NOTIFCATIONS);
            MemCacher.MemoryCacher.Add(MY_NOTIFCATIONS, docs, DateTimeOffset.UtcNow.AddMinutes(10));

            return docs;
        }

        List<NotificationRequestItem> GetMyNotifications(string userId)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };
            List<NotificationRequestItem> docs = new List<NotificationRequestItem>();

                docs = client.CreateDocumentQuery<NotificationRequestItem>(
                  UriFactory.CreateDocumentCollectionUri(docdb, docdbcollection), queryOptions)
                  .AsEnumerable()
                  .Where(f => f.UserId == userId && DateTime.Parse(f.EventDate).Month >= DateTime.Today.Date.Month && DateTime.Parse(f.EventDate).Day >= DateTime.Today.Date.Day)
                  .OrderBy(d => DateTime.Parse(DateTime.Today.Year + "-" + DateTime.Parse(d.EventDate).Month + "-" +  DateTime.Parse(d.EventDate).Day))
                  .Take(10)
                  .ToList();

            MemCacher.MemoryCacher.Delete(MY_NOTIFCATIONS);
            MemCacher.MemoryCacher.Add(MY_NOTIFCATIONS, docs, DateTimeOffset.UtcNow.AddMinutes(10));

            return docs;
        }

        NotificationRequestItem FilterMyNotifications(int nextDocIndex)
        {
            var docs = (List<NotificationRequestItem>)MemCacher.MemoryCacher.GetValue(MY_NOTIFCATIONS);
            if(docs == null)
            {
                //we had an issue
            }

            if (nextDocIndex > docs.Count() - 1)
            {
                return null;
            }
            else
            {
                return docs.ElementAt(nextDocIndex);
            }
        }

        void DeleteReminder(string id)
        {
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            Document doc = client.CreateDocumentQuery(UriFactory.CreateDocumentCollectionUri(docdb, docdbcollection), queryOptions)
               .Where(x => x.Id == id).AsEnumerable().FirstOrDefault();
            client.DeleteDocumentAsync(doc.SelfLink);
        }

        private NotificationRequestItem CreateNewDocument(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            var doc = new NotificationRequestItem();

            if(string.IsNullOrEmpty(response.SessionAttributes.ReminderDate))
            {
                return null;
            }

            doc.TimeZone = "";
            doc.RequestId = request.Request.RequestId;
            doc.UserId = request.Session.User.UserId;
            doc.Status = "pending";
            doc.EventType = (response.SessionAttributes.EventType == null ? "" : response.SessionAttributes.EventType);
            doc.EventFor = (response.SessionAttributes.EventFor == null ? "" : response.SessionAttributes.EventFor);
            doc.IsRecurring = response.SessionAttributes.IsRecurring;
            doc.EventDate = (response.SessionAttributes.ReminderDate == null ? "" : response.SessionAttributes.ReminderDate);

            return doc;
        }

        private async Task<AlfredAlexaResponse> ThrowSafeException(Exception exception, AlfredAlexaResponse response, [CallerMemberName] string methodName = "")
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
            client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
        }

        #region :   Main-End-Points   :
        [HttpPost, Route("main")]
        public async Task<AlfredAlexaResponse> Main(AlfredAlexaRequest alexaRequest)
        {
            AlfredAlexaResponse response = new AlfredAlexaResponse();
            response.SessionAttributes = new AlfredAttributes();

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

                switch (alexaRequest.Request.Type)
                {
                    case "LaunchRequest":
                        response = LaunchRequest(alexaRequest, response);
                        break;
                    case "IntentRequest":
                        response = await IntentRequest(alexaRequest, response);
                        break;
                    case "SessionEndedRequest":
                        response = SessionEndedRequest(alexaRequest, response);
                        break;
                    case "Messaging.MessageReceived":
                        response = await MessageReceivedRequest(alexaRequest, response);
                        break;
                }

                //set value for repeat intent
                response.SessionAttributes.OutputSpeech = response.Response.OutputSpeech;
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> MessageReceivedRequest(AlfredAlexaRequest alexaRequest, AlfredAlexaResponse response)
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

        private bool ValidatePermission(AlfredAlexaRequest request)
        {
            return (!string.IsNullOrEmpty(request.Context.System.User.Permissions.ConsentToken));
        }

        private AlfredAlexaResponse ProcessUnknownIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            try
            {
                return ProcessHelpIntent(request, response);

            }
            catch (Exception ex)
            {
                return ProcessHelpIntent(request, response);
            }

        }

        private AlfredAlexaResponse ProcessHelpIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
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

        private AlfredAlexaResponse ProcessCancelIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            response.Response.OutputSpeech.Text = "Thank you for using our Skill. Make it a great day!";
            response.Response.ShouldEndSession = true;
            response.SessionAttributes.IsCreatingEvent = false;
            return response;
        }

        private AlfredAlexaResponse ProcessRepeatIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            if (string.IsNullOrEmpty(request.Session.Attributes.LastRequestIntent))
            {
                return ProcessHelpIntent(request, response);
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

        private AlfredAlexaResponse LaunchRequest(AlfredAlexaRequest request, AlfredAlexaResponse response)
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

        private async Task<AlfredAlexaResponse> CreateEventOnlyIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;

            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (!SlotIsEmptyOrNull(slot, "event"))
                    {

                        response.SessionAttributes.EventType = (String)slot["event"].value;

                        content = "You can relate events to people you know, like a birthday for Joe or a wedding for Sally. Is this event for a person?"; ;
                        reprompt = "Would you like to associate this event to a person?";

                        isValid = true;

                        response = BuildResponseOutput(response, content, reprompt, "Amazon.YesIntent, Amazon.NoIntent", YesNoAction.IsForAPerson.ToString());
                        response.Response.ShouldEndSession = false;
                    }
                }

               if(!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> CreateNeedDateEventIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;

            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (!SlotIsEmptyOrNull(slot, "forWho") || !SlotIsEmptyOrNull(slot, "event"))
                    {
                        response.SessionAttributes.EventFor = (String)slot["forWho"].value;
                        response.SessionAttributes.EventType = (String)slot["event"].value;

                        content = "OK, What is the date of this event?";
                        reprompt = "What is the date of this event?";

                        response = BuildResponseOutput(response, content, reprompt, "DateIntent, DateOnlyIntent");
                        response.Response.ShouldEndSession = false;

                        isValid = true;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> CreateReminderOnlyEventIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;

            try
            {
                content = "We support a large number of event types, like birthday, wedding, holiday, and deadline, etc..  What type of event is this?";

                reprompt = content;

                response = BuildResponseOutput(response, content, reprompt, "EventOnlyIntent, EventIntent");
                response.Response.ShouldEndSession = false;


           }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> ListRemindersByMonthIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;

            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (!SlotIsEmptyOrNull(slot, "monthDate"))
                    {
                        response.SessionAttributes.RequestedListMonth = (String)slot["monthDate"].value;
                        response.SessionAttributes.RequestedListMonth = DateTime.Parse(response.SessionAttributes.RequestedListMonth).Month.ToString();

                        var docs = GetMyNotificationsByMonth(response.SessionAttributes.RequestedListMonth, request.Session.User.UserId);
                        response = ProcessDocListLoop(docs, request, response, 0);
                        content = response.Response.OutputSpeech.Text;
                        reprompt = response.Response.Reprompt.OutputSpeech.Text;

                        isValid = true;
                        response = BuildResponseOutput(response, content, reprompt, "SSML");
                        response.Response.ShouldEndSession = false;
                    }
                }

            if (!isValid)
            {
                response = BuildResponseOutput(response, content, reprompt);
                response.Response.ShouldEndSession = false;
            }

        }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> ListRemindersByMonthNameIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;

            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (!SlotIsEmptyOrNull(slot, "monthName"))
                    {
                        response.SessionAttributes.RequestedListMonth = (String)slot["monthName"].value;
                        response.SessionAttributes.RequestedListMonth = DateTime.ParseExact(response.SessionAttributes.RequestedListMonth, "MMMM", CultureInfo.CurrentCulture).Month.ToString();

                        var docs = GetMyNotificationsByMonth(response.SessionAttributes.RequestedListMonth, request.Session.User.UserId);
                        response = ProcessDocListLoop(docs, request, response, 0);
                        content = response.Response.OutputSpeech.Text;
                        reprompt = response.Response.Reprompt.OutputSpeech.Text;

                        isValid = true;
                        response = BuildResponseOutput(response, content, reprompt, "SSML");
                        response.Response.ShouldEndSession = false;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private AlfredAlexaResponse ProcessDocListLoop(List<NotificationRequestItem> docs, AlfredAlexaRequest request, AlfredAlexaResponse response, int element= 0)
        {
            var content = "";
            var reprompt = "";

            var prefix = @"This will list up to 10 of your upcoming notification reminders. ";

            if (docs != null && docs.Count > 0)
            {

                if (request.Session.Attributes.NextDocIndex <= docs.Count() - 1)
                {
                    var doc = docs.ElementAt(element);
                    response.SessionAttributes.NextDocIndex = element + 1;
                    response.SessionAttributes.LastSpokenDocId = doc.Id;

                    if (string.IsNullOrEmpty(doc.EventFor))
                    {
                        content = ((element == 0) ? prefix : @"") + "You have" + ((element == 0) ? " a " : " another ") + (doc.IsRecurring ? "recurring" : "") + "notification event called " + doc.EventType + @", on <say-as interpret-as=""date"">????" + DateTime.Parse(doc.EventDate).Month.ToString("d2") + DateTime.Parse(doc.EventDate).Day.ToString("d2") + "</say-as>. Would you like to delete this notification?";
                        reprompt = content;
                    }
                    else
                    {
                        content = ((element == 0) ? prefix : @"") + "You have" + ((element == 0) ? " a " : " another ") + (doc.IsRecurring ? "recurring" : "") + "notification event called " + doc.EventType + ", for " + doc.EventFor + @", on <say-as interpret-as=""date"">????" + DateTime.Parse(doc.EventDate).Month.ToString("d2") + DateTime.Parse(doc.EventDate).Day.ToString("d2") + "</say-as>. Would you like to delete this notification?";
                        reprompt = content;
                    }
                }else
                {
                    response.SessionAttributes.NextDocIndex = 0;
                    response.SessionAttributes.LastSpokenDocId = "";
                    response.SessionAttributes.HasReachedDocListEnd = true;

                    content = "We've reached the end of the notification list.  Why not add a new notification now.  Just say, Alexa, ask my event notifications to add a reminder.";
                    reprompt = "Why not add a new notification now. Just say, Alexa, ask my event notifications to add a reminder.";
                }
            }
            else
            {
                content = "I couldn't find any notifications for that request.  Why not add some now.  Just say, Alexa, ask my event notifications to add a reminder.";
                reprompt = "Why not add notificatons now?  Just say, Alexa, ask my event notifications to add a reminder.";
            }



            response.Response.OutputSpeech.Text = content;
            response.Response.Reprompt.OutputSpeech.Text = reprompt;

            return response;
        }

        private async Task<AlfredAlexaResponse> ListRemindersIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;

            try
            {
                var docs = GetMyNotifications(request.Session.User.UserId);
                response = ProcessDocListLoop(docs, request, response, 0);
                content = response.Response.OutputSpeech.Text;
                reprompt = response.Response.Reprompt.OutputSpeech.Text;

                if (docs != null) {
                    response.Response.Card = new Card();
                    response.Response.Card.Title = "My Event Notifications Reminders";
                    response.Response.Card.Content = (docs.Count == 0 ? "You have no upcoming notifications.  Why not add one today.  Just say, Alexa, ask My Event Notifications to set a reminder." : "We found "+ docs.Count.ToString() + " upcoming notifications.");
                    response.Response.Card.Type = "Standard";
                    response.Response.Card.Image = new Image();
                    response.Response.Card.Image.SmallImageUrl = "https://whatnowapi.azurewebsites.net/images/skillSmall.png";
                    response.Response.Card.Image.LargeImageUrl = "https://whatnowapi.azurewebsites.net/images/skillLarge.png";
                    response.Response.ShouldEndSession = false;
                }

                response = BuildResponseOutput(response, content, reprompt,"amazon.yesintent, amazon.nointent",YesNoAction.ShouldDeleteReminder.ToString(), "SSML");
                response.Response.ShouldEndSession = false;
                
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> ForWhoIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;

            string expectedIntents = string.Empty;
            string yesNoAciton = string.Empty;

            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;
                    if (!SlotIsEmptyOrNull(slot, "forWho"))
                    {

                        response.SessionAttributes.EventFor = (String)slot["forWho"].value;

                        if (string.IsNullOrEmpty(request.Session.Attributes.ReminderDate))
                        {
                            content = "OK, What is the date of the event?";
                            reprompt = "OK, What is the date of the event?";
                            expectedIntents = "DateOnlyIntent, DateIntent";
                        }
                        else
                        {
                            content = "OK, I will save this event. Do you want to make this an annual notification?";
                            reprompt = "Would you like to make this an annual notification?";
                            expectedIntents = "AMAZON.YesIntent, AMAZON.NoIntent";
                            yesNoAciton = YesNoAction.ShouldMakeAnnual.ToString();

                        }

                        isValid = true;
                        response = BuildResponseOutput(response, content, reprompt, expectedIntents, yesNoAciton);
                        response.Response.ShouldEndSession = false;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> DateIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;

            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;
                    if (!SlotIsEmptyOrNull(slot, "date"))
                    {

                        response.SessionAttributes.ReminderDate = (String)slot["date"].value;

                        content = "OK, I will save this event. Do you want to make this an annual notification?";
                        reprompt = "Would you like to make this an annual notification?";
                        isValid = true;

                        response = BuildResponseOutput(response, content, reprompt, "AMAZON.YesIntent, AMAZON.NoIntent", YesNoAction.ShouldMakeAnnual.ToString());
                        response.Response.ShouldEndSession = false;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }


            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> EventIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;
            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;
                    if (!SlotIsEmptyOrNull(slot, "event"))
                    {

                        response.SessionAttributes.EventFor = (String)slot["event"].value;

                        content = "You can relate events to people you know, like a birthday for Joe or a wedding for Sally. Is this event for a person?";
                        reprompt = "Would you like to associate this event to a person?";

                        isValid = true;
                        response = BuildResponseOutput(response, content, reprompt, "Amazon.YesIntent, Amazon.NoIntent", YesNoAction.IsForAPerson.ToString());
                        response.Response.ShouldEndSession = false;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private string ExpectedResponseValidation(AlfredAlexaRequest request, AlfredAlexaResponse response)
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

        private async Task<AlfredAlexaResponse> CreateSimpleDateEventIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = INVALID_RESPONSE_REPROMT;
            isValid = false;
            try
            {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (!SlotIsEmptyOrNull(slot, "date") || !SlotIsEmptyOrNull(slot, "event"))
                    {

                        response.SessionAttributes.ReminderDate = (String)slot["date"].value;
                        response.SessionAttributes.EventType = (String)slot["event"].value;

                        isValid = true;
                        content = "You can relate events to people you know, like a birthday for Joe or a wedding for Sally. Is this event for a person?";
                        reprompt = "Is this event for a person?";


                        response = BuildResponseOutput(response, content, reprompt, "Amazon.YesIntent, Amazon.NoIntent", YesNoAction.IsForAPerson.ToString());
                        response.Response.ShouldEndSession = false;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private async Task<AlfredAlexaResponse> CreateFullEventIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = INVALID_RESPONSE_CONTENT;
            string reprompt = content;
            isValid = false;

            try {
                if (request.Request.Intent.Slots != null)
                {
                    var slot = request.Request.Intent.Slots;

                    if (!SlotIsEmptyOrNull(slot, "date") || !SlotIsEmptyOrNull(slot, "forWho") || !SlotIsEmptyOrNull(slot, "event"))
                    {

                        response.SessionAttributes.ReminderDate = (String)slot["date"].value;
                        response.SessionAttributes.EventFor = (String)slot["forWho"].value;
                        response.SessionAttributes.EventType = (String)slot["event"].value;


                        if (string.IsNullOrEmpty(request.Session.Attributes.EventFor))
                        {
                            content = "You can relate events to people you know, like a birthday for Joe or a wedding for Sally. Is this event for a person?";
                            reprompt = "Is this event for a person?";
                            response = BuildResponseOutput(response, content, reprompt, "Amazon.yesintent, amazon.nointent", YesNoAction.IsForAPerson.ToString());
                        }
                        else
                        {
                            content = "OK, I will save this event. Do you want to make this an annual notification?";
                            reprompt = "Would you like to make this an annual notification?";

                            response = BuildResponseOutput(response, content, reprompt, "Amazon.yesintent, amazon.nointent", YesNoAction.ShouldMakeAnnual.ToString());
                            response.Response.ShouldEndSession = false;
                        }

                        isValid = true;
                    }
                }

                if (!isValid)
                {
                    response = BuildResponseOutput(response, content, reprompt);
                    response.Response.ShouldEndSession = false;
                }

            }
            catch(Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        private bool ValidateEventCompleted(AlfredAlexaResponse response)
        {
            return (!string.IsNullOrEmpty(response.SessionAttributes.EventType) &&
                !string.IsNullOrEmpty(response.SessionAttributes.EventFor) &&
                !response.SessionAttributes.IsRecurring &&
                !string.IsNullOrEmpty(response.SessionAttributes.ReminderDate));
        }

        private AlfredAlexaResponse SessionEndedRequest(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            response.Response.OutputSpeech.Text = "Make it a great day!";
            response.Response.ShouldEndSession = true;
            response.SessionAttributes.IsCreatingEvent = false;


            return response;
        }

        private async Task<AlfredAlexaResponse> ProcessYesIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = string.Empty;
            string reprompt = string.Empty;
            response.Response.ShouldEndSession = false;
            try
            {
                //said yes to delete a reminder
                if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldDeleteReminder.ToString())
                {

                    DeleteReminder(response.SessionAttributes.LastSpokenDocId);

                    //get next notification it list and read it aloud
                    var docs = (List<NotificationRequestItem>)MemCacher.MemoryCacher.GetValue(MY_NOTIFCATIONS);

                    response = ProcessDocListLoop(docs, request, response, request.Session.Attributes.NextDocIndex);


                    content = "Ok, it's been deleted. " + response.Response.OutputSpeech.Text;
                    reprompt = response.Response.Reprompt.OutputSpeech.Text;

                    response.Response.Card = new Card();
                    response.Response.Card.Title = "My Event Notifications - Reminder Removed";
                    response.Response.Card.Content = "We deleted your saved notification.";
                    response.Response.Card.Type = "Standard";
                    response.Response.Card.Image = new Image();
                    response.Response.Card.Image.SmallImageUrl = "https://whatnowapi.azurewebsites.net/images/skillSmall.png";
                    response.Response.Card.Image.LargeImageUrl = "https://whatnowapi.azurewebsites.net/images/skillLarge.png";

                    response.Response.ShouldEndSession = false;
                    response = BuildResponseOutput(response, content, reprompt, "amazon.yesintent, amazon.nointent", YesNoAction.ShouldDeleteReminder.ToString(), "SSML");
                    response.Response.ShouldEndSession = false;


                }
                //said yes to its recurring
                else if (request.Session.Attributes.YesNoAction == YesNoAction.IsForAPerson.ToString())
                    {

                    content = "What is the person's name?";
                    reprompt = content;
                    response = BuildResponseOutput(response, content, reprompt, "ForWhoIntent, ForWhoOnlyIntent");

                }
                //said yes to its recurring
                else if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldMakeAnnual.ToString())
                {

                    response.SessionAttributes.IsRecurring = true;

                    var doc = CreateNewDocument(request, response);

                    if (doc == null)
                    {
                        content = "It looks like we're missing one piece for this reminder.  What is the date for this event?";
                        reprompt = "What is the date for this event?";
                        response = BuildResponseOutput(response, content, reprompt, "DateIntent, DateOnlyIntent");
                    }else
                    {
                        await CreateNewReminder(doc);

                        content = "You're all set.  I'll send you a notification 7 days before your event, just to let you know it's coming. You will also receive a notification on the day of your event. Make it  a great day!";
                        response.Response.ShouldEndSession = true;


                        response.Response.Card = new Card();
                        response.Response.Card.Title = "My Event Notifications - Reminder Added";
                        response.Response.Card.Content = "We created a new " + doc.EventType.ToString() + " notification reminder for " + DateTime.Parse(doc.EventDate).ToLongDateString() + ".";
                        response.Response.Card.Type = "Standard";
                        response.Response.Card.Image = new Image();
                        response.Response.Card.Image.SmallImageUrl = "https://whatnowapi.azurewebsites.net/images/skillSmall.png";
                        response.Response.Card.Image.LargeImageUrl = "https://whatnowapi.azurewebsites.net/images/skillLarge.png";
                        response = BuildResponseOutput(response, content, reprompt, "SSML");
                    }
                }
                else
                {

                    return ProcessRepeatIntent(request, response);

                }
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }

            return response;
        }

        public AlfredAlexaResponse BuildResponseOutput(AlfredAlexaResponse response, string outPut, string reprompt, string expectedIntents = "", string yesNoAction = "", string textType = "PlainText", Dictionary<string, string> outputSubstitution = null, Dictionary<string, string> repromptSubstitution = null)
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
        private async Task<AlfredAlexaResponse> ProcessNoIntent(AlfredAlexaRequest request, AlfredAlexaResponse response)
        {
            string content = string.Empty;
            string reprompt = string.Empty;

            try
            {

                if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldDeleteReminder.ToString())
                //said No to delete a reminder
                {
                    //get next notification it list and read it aloud
                    var docs = (List<NotificationRequestItem>)MemCacher.MemoryCacher.GetValue(MY_NOTIFCATIONS);

                    response = ProcessDocListLoop(docs, request, response, request.Session.Attributes.NextDocIndex);

                    content =  response.Response.OutputSpeech.Text;
                    reprompt = response.Response.Reprompt.OutputSpeech.Text;

                    response = BuildResponseOutput(response, content, reprompt, "amazon.yesintent, amazon.nointent", YesNoAction.ShouldDeleteReminder.ToString(), "SSML");
                    response.Response.ShouldEndSession = false;

                }
                //no on the recruing questions
                else if (request.Session.Attributes.YesNoAction == YesNoAction.ShouldMakeAnnual.ToString())
                {
                    var doc = CreateNewDocument(request, response);

                    if (doc == null)
                    {
                        content = "It looks like we're missing one piece for this reminder.  What is the date for this event?";
                        reprompt = "What is the date for this event?";
                        response = BuildResponseOutput(response, content, reprompt, "DateIntent, DateOnlyIntent");
                    }
                    else
                    {
                        response.SessionAttributes.IsRecurring = false;
                        await CreateNewReminder(CreateNewDocument(request, response));
                        content = "You're all set.  I'll send you a notification 7 days before your event, just to let you know it's coming. You will also receive a notification on the day of your event. Make it a great day!";

                        response.Response.Card = new Card();
                        response.Response.Card.Title = "New My Event Notification - Reminder Added!";
                        response.Response.Card.Content = "We created a new " + doc.EventType.ToString() + " notification reminder for " + DateTime.Parse(doc.EventDate).ToLongDateString() + ".";
                        response.Response.Card.Type = "Standard";
                        response.Response.Card.Image = new Image();
                        response.Response.Card.Image.SmallImageUrl = "https://whatnowapi.azurewebsites.net/images/skillSmall.png";
                        response.Response.Card.Image.LargeImageUrl = "https://whatnowapi.azurewebsites.net/images/skillLarge.png";
                        response.Response.ShouldEndSession = true;

                        response = BuildResponseOutput(response, content, reprompt, "SSML");
                    }

                }
            
                //no on is for a peson
                else if (request.Session.Attributes.YesNoAction ==  YesNoAction.IsForAPerson.ToString())
                {

                    if (string.IsNullOrEmpty(request.Session.Attributes.ReminderDate))
                    {
                        content = "OK. What is the date of this event?";
                        reprompt = content;
                        response = BuildResponseOutput(response, content, reprompt, "DateOnlyIntent, DateIntent");
                    }
                    else
                    {
                        content = "Would you like to make this an annual notification reminder?";
                        reprompt = content;
                        response = BuildResponseOutput(response, content, reprompt, "Amazon.YesIntent, Amazon.NoIntent", YesNoAction.ShouldMakeAnnual.ToString());
                    }
                }
                else
                {

                    return ProcessRepeatIntent(request, response);

                }
            }
            catch (Exception ex)
            {
                return await ThrowSafeException(ex, response);
            }
            return response;
        }


        private async Task<AlfredAlexaResponse> IntentRequest(AlfredAlexaRequest request, AlfredAlexaResponse response)
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
                    response = ProcessRepeatIntent(request, response);
                    shouldSetLastIntent = false;
                    break;
                case "UnknownIntent":
                    response = ProcessUnknownIntent(request, response);
                    break;
                case "CreateSimpleDateEventIntent":
                    response.SessionAttributes.IsCreatingEvent = true;
                    response = await CreateSimpleDateEventIntent(request, response);
                    break;
                case "CreateEventOnlyIntent":
                    response.SessionAttributes.IsCreatingEvent = true;
                    response = await CreateEventOnlyIntent(request, response);
                    break;
                case "CreateNeedDateEventIntent":
                    response.SessionAttributes.IsCreatingEvent = true;
                    response = await CreateNeedDateEventIntent(request, response);
                    break;
                case "CreateReminderOnlyEventIntent":
                    response.SessionAttributes.IsCreatingEvent = true;
                    response = await CreateReminderOnlyEventIntent(request, response);
                    break;
                case "ListRemindersByMonthIntent":
                    response = await ListRemindersByMonthIntent(request, response);
                    break;
                case "ListRemindersByMonthNameIntent":
                    response = await ListRemindersByMonthNameIntent(request, response);
                    break;
                case "ListRemindersIntent":
                    response = await ListRemindersIntent(request, response);
                    break;
                case "ForWhoIntent":
                    response = await ForWhoIntent(request, response);
                    break;
                case "ForWhoOnlyIntent":
                    response = await ForWhoIntent(request, response);
                    break;
                case "DateIntent":
                    response = await DateIntent(request, response);
                    break;
                case "DateOnlyIntent":
                    response = await DateIntent(request, response);
                    break;
                case "EventIntent":
                    response = await EventIntent(request, response);
                    break;
                case "EventOnlyIntent":
                    response = await EventIntent(request, response);
                    break;
                case "CreateFullEventIntent":
                    response.SessionAttributes.IsCreatingEvent = true;
                    response = await CreateFullEventIntent(request, response);
                    break;
                case "CreateSimpleEventIntent":
                    response.SessionAttributes.IsCreatingEvent = true;
                    response = await CreateFullEventIntent(request, response);
                    break;
                case "AMAZON.CancelIntent":
                    response = ProcessCancelIntent(request, response);
                    break;
                case "AMAZON.StopIntent":
                    response = ProcessCancelIntent(request, response);
                    break;
                case "AMAZON.HelpIntent":
                    response = ProcessHelpIntent(request, response);
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

            if (shouldSetLastIntent)
            {
                response.SessionAttributes.LastRequestIntent = request.Request.Intent.Name;
            }

            return response;
        }

     
        #endregion :   Alexa Type Handlers   :

    }
}