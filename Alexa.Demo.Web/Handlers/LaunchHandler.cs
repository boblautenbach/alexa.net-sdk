using Alexa.Demo.Models;
using Alexa.Demo.Web.Handlers;
using Alfred.Api.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alfred.Api.Handlers
{
    public class LaunchHandler: IIntentHandler
    {
        public string State { get; set; }

        public LaunchHandler(string state)
        {
            State = state;
        }

        public AlexaDemoResponse LaunchRequest(AlexaDemoRequest request, AlexaDemoResponse response)
        {

            response.Response.OutputSpeech.Text = "We Launched!";
            return response;
        }

        public AlexaDemoResponse SimpleTestIntent(AlexaDemoRequest request, AlexaDemoResponse response)
        {

            response.Response.OutputSpeech.Text = "We Are in an Intent!";
            return response;
        }
    }
}