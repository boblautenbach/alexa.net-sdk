using Alexa.Demo.Models;
using Alexa.Demo.Web.Handlers;
using Alfred.Api.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alfred.Api.Handlers
{
    public class ListReminderHandler: IIntentHandler
    {
        public string State { get; set; }

        public ListReminderHandler(string state)
        {
            State = state;
        }

        public AlexaDemoResponse LaunchRequest(AlexaDemoRequest request, AlexaDemoResponse response)
        {

            response.Response.OutputSpeech.Text = "it worked!";
            return response;
        }

        public AlexaDemoResponse SimpleTestIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {

            response.Response.OutputSpeech.Text = "it worked!";
            return response;
        }
    }
}