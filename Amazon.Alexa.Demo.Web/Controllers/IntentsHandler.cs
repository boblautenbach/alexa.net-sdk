using Amazon.Alexa.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Alexa.Demo.Web.Controllers
{
    public class IntentsHandler
    {

        static public Func<AlexaRequest, AlexaResponse, AlexaResponse> Process = (req, res) =>
        {

            var method = typeof(IntentsHandler).GetMethod(req.Request.Intent.Name);
            var response = method.Invoke(null,new object[] { req, res }) as AlexaResponse;
            return response;
        };

        private static AlexaResponse HelloIntent(AlexaRequest req, AlexaResponse res)
        {

            return res;
        }
    }
}