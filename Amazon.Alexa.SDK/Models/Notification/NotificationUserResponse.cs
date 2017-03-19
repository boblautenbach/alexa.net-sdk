﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Alexa.SDK.Models.Notification
{
    public class NotificationUserResponse
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ReasonPhrase { get; set; }
    }
}
