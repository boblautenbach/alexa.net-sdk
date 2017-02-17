namespace Amazon.Alexa.Models
{
    public class AlexaError
    {
        /// <summary>
        /// This is the type of error.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// This is the error message.
        /// </summary>
        public string Message { get; set; }
    }
}