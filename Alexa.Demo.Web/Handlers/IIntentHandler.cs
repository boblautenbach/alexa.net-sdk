using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alexa.Demo.Web.Handlers
{
    public interface IIntentHandler
    {
       string State { get; set; }

        dynamic UnHandledIntent(dynamic request, dynamic response);

    }
}