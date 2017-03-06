using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Amazon.Alexa.SDK
{
    public interface IAlexaNetSDK
    {
       // dynamic HandleIntent(dynamic request, dynamic response);

        T BuildOutputWithCard<T>(T response);

        dynamic BuildOutputWithCardImages();


        dynamic BuildOutput();


    }
}