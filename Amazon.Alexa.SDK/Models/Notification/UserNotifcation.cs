using Newtonsoft.Json;
using System;

namespace Amazon.Alexa.SDK.Models.Notification
{
    public class UserNotifcation
    {
        [JsonProperty("displayInfo")]
        public Displayinfo DisplayInfo { get; set; }

        [JsonProperty("referenceId")]
        public string ReferenceId { get; set; }

        [JsonProperty("expiryTime")]
        public string ExpiryTime { get; set; }

        [JsonProperty("spokenInfo")]
        public SpokenInfo SpokenInfo { get; set; }

        public UserNotifcation()
        {
            ReferenceId = Guid.NewGuid().ToString().Replace("-","");
        }
    }

    public class Displayinfo
    {
        [JsonProperty("content")]
        public DisplayContent[] Content { get; set; }
    }

    public class DisplayContent
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("toast")]
        public MainText Toast { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("bodyItems")]
        public MainText[] BodyItems { get; set; }
    }

    public class MainText
    {
        [JsonProperty("primaryText")]
        public string PrimaryText { get; set; }
    }

    public class SpokenInfo
    {
        [JsonProperty("content")]
        public Content[] Content { get; set; }
    }

    public class Content
    {
        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("ssml")]
        public string SSML { get; set; }
    }
}
