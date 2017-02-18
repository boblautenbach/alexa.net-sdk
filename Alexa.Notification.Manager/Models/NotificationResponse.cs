using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Alexa.Notification.Manager.Models
{
    public class NotificationResponse
    {
        public string UserId { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string ReasonPhrase { get; set; }

        public string XAmznRequestId { get; set; }
    }
}
