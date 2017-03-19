using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Amazon.Alexa.SDK.Models.Notification;

namespace Amazon.Alexa.SDK.Notification
{
    public class NotificationManager
    {
        #region "Fields"
        const string TOKEN_BASE_URL = "https://api.amazon.com";
        const string NOTIFICATION_BASE_URL = "https://api.amazonalexa.com";

        string[] PERMISSIONS = new string[] { "write::alexa:devices:all:notifications:standard" };

        const string TOKEN_ENDPOINT = "/auth/O2/token";
        const string SKILL_NOTIFICATION_ENDPOINT = "/v1/skillmessages/users/";
        const string NOTIFICATION_PATH = "/v2/notifications";
        const string GET_PENDING_PATH = "/v2/notifications/pending";
        const string GET_ARCHIVED_PATH = "/v2/notifications/archived";

        #endregion

        #region "Message to Skill Messaging API"

        public async Task<string> GetAccessToken(string clientId, string secret)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(TOKEN_BASE_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                    var body = "grant_type=client_credentials&client_secret=" + secret + "&client_id=" + clientId + "&scope=alexa:skill_messaging";

                    var response = await client.PostAsync(TOKEN_ENDPOINT, new StringContent(body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded"));
                    var result = response.Content.ReadAsStringAsync();
                    return JObject.Parse(result.Result)["access_token"].ToString();
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public async Task<NotificationResponse> SendToSkill(string userId, string token, JObject data)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(NOTIFICATION_BASE_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
              
                    var response = await client.PostAsync(SKILL_NOTIFICATION_ENDPOINT + Uri.EscapeDataString(userId), new StringContent(JsonConvert.SerializeObject(data), System.Text.Encoding.UTF8, "application/json"));

                    IEnumerable<string> values;
                    string amazonRequestId = string.Empty;
                    if (response.Headers.TryGetValues("X-Amzn-RequestId", out values))
                    {
                        amazonRequestId = values.First();
                    }

                   var result =  new NotificationResponse() { ReasonPhrase = SendToSkillReasonCodeDecoder(response.StatusCode), StatusCode = response.StatusCode, XAmznRequestId = amazonRequestId, UserId = userId };
                   return result;

                }
            }
            catch (Exception ex)
            {
                return new NotificationResponse() { ReasonPhrase = SendToSkillReasonCodeDecoder(HttpStatusCode.InternalServerError), StatusCode = HttpStatusCode.InternalServerError, XAmznRequestId = "", UserId = userId };
            }
        }

        private string SendToSkillReasonCodeDecoder(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Accepted:
                    {
                        return "Message has been successfully accepted, and will be sent to the skill.";
                    }
                case HttpStatusCode.BadRequest:
                    {
                        return "Data is missing or not valid.";
                    }
                case HttpStatusCode.Forbidden:
                    {
                        return "The skillmessagingToken is expired or not valid.";
                    }
                case HttpStatusCode.NotFound:
                    {
                        return "The userId does not exist.";
                    }
                case HttpStatusCode.InternalServerError:
                    {
                        return "Internal service exception";
                    }
                default:
                    {
                        return "Error 429: The requester has exceeded their maximum allowable rate of messages.";
                    }
            }
        }
        #endregion

        #region "Message from Skill to Device Notification API Messaging"
        
        public async Task<NotificationUserResponse> SendToDevices(string constentToken, UserNotifcation body)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(NOTIFICATION_BASE_URL);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", constentToken);
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-length", JsonConvert.SerializeObject(body).Length.ToString());
                    var response = await client.PostAsync(NOTIFICATION_PATH, new StringContent(JsonConvert.SerializeObject(body,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            }), System.Text.Encoding.UTF8, "application/json"));
              
                    var result = new NotificationUserResponse() { ReasonPhrase = SendToUserReasonCodeDecoder(response.StatusCode), StatusCode = response.StatusCode };
                    return result;

                }
            }
            catch (Exception ex)
            {
                return new NotificationUserResponse() { ReasonPhrase = SendToUserReasonCodeDecoder(HttpStatusCode.InternalServerError), StatusCode = HttpStatusCode.InternalServerError};
            }

        }

        private string SendToUserReasonCodeDecoder(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.Created:
                    {
                        return "Message Created successfully.";
                    }
                case HttpStatusCode.BadRequest:
                    {
                        return "One or more input validations failed. For example, if displayInfo or spokenInfo does not contain data for the en-US local";
                    }
                case HttpStatusCode.Unauthorized:
                    {
                        return "Unauthorized request";
                    }
                case HttpStatusCode.Conflict:
                    {
                        return "Reference ID already used to create notification by the skill for this user";
                    }
                case HttpStatusCode.RequestEntityTooLarge:
                    {
                        return "Size limit exceeded.";
                    }
                case HttpStatusCode.InternalServerError:
                    {
                        return "Internal service exception";
                    }
                default:
                    {
                        return "Error 429: The requester has exceeded their maximum allowable rate of messages.";
                    }
            }
        }
        #endregion
    }
}
