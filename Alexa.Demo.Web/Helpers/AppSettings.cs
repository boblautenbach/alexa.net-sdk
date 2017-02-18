using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Alexa.Demo.Web.Helpers
{
    public static class AppSettings
    {
        public static string AmazonAppId = ConfigurationManager.AppSettings["AmazonAppId"];
    }
}