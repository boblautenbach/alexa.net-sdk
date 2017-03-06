using Alexa.Demo.Models;
using Amazon.Alexa.SDK.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alexa.Demo.Web.Handlers
{   [AlexaNetSDK]
    public  class TestIntentHandler
    {
        public static AlexaDemoResponse SimpleTestIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {

            response.Response.OutputSpeech.Text = "We Are in an Intent..YAH!";
            return response;
        }
    }
}