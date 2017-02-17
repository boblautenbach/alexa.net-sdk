using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace Amazon.Alexa.Models
{
    public class AlexaRequestType
    {
        /// <summary>
        /// Types include LaunchRequest, IntentRequest, SessionEndedRequest, 
        /// PlaybackController.NextCommandIssued, PlaybackController.NextCommandIssued,
        /// PlaybackController.PauseCommandIssued, PlaybackController.PlayCommandIssued, 
        /// PlaybackController.PreviousCommandIssued and AudioPlayer.PlaybackStarted.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("timestamp"), JsonConverter(typeof(JavaScriptDateTimeConverter))]
        public DateTime Timestamp { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("reason")]
        public string Reason { get; set; }

        [JsonProperty("offsetMilliseconds")]
        public int OffSetMilliseconds { get; set; }

        [JsonProperty("intent")]
        public AlexaIntent Intent { get; set; }

        [JsonProperty("error")]
        public AlexaError Error { get; set; }

        public AlexaRequestType()
        {
            Intent = new AlexaIntent();
            Error = new AlexaError();
        }
    }
}