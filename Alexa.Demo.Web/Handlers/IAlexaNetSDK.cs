using Alexa.Demo.Web.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alfred.Api.Handlers
{
    public interface IAlexaNetSDK
    {
        dynamic HandleIntent(dynamic request, dynamic response);

        void RegisterHandlers(List<IIntentHandler> handlers);
        
    }
}