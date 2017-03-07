//using Alfred.Api.Handlers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Alexa.Demo.Web.Handlers;
//using System.Reflection;

//namespace Alfred.Api.BaseClasses
//{
//    public class AlexaNetSDK : IAlexaNetSDK
//    {
//        private List<IIntentHandler> _handlers = new List<IIntentHandler>();

//        public dynamic BuildOutput()
//        {
//            throw new NotImplementedException();
//        }

//        public dynamic BuildOutputWithCard()
//        {
//            throw new NotImplementedException();
//        }

//        public T BuildOutputWithCard<T>(T response)
//        {
//            throw new NotImplementedException();
//        }

//        public dynamic BuildOutputWithCardImages()
//        {
//            throw new NotImplementedException();
//        }

//        public dynamic HandleIntent(dynamic request, dynamic response)
//        {
//            try
//            {
//                return ProcessRequest(request, response);

//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//            return response;
//        }

//        public void RegisterHandlers(List<IIntentHandler> handlers)
//        {
//            //TODO
//            //check at least one handler provided
//            //check that each unique State values
//            //check anything else?
//            _handlers = handlers;
//        }

//        private dynamic ProcessRequest(dynamic request, dynamic response)
//        {
//            foreach (object handler in _handlers)
//            {
//                var intentMgr = handler.GetType();

//                PropertyInfo prop = intentMgr.GetProperty("State", BindingFlags.Instance | BindingFlags.Public);

//                var propValue = (string)prop.GetValue(handler);

//                propValue = (string.IsNullOrEmpty(propValue) ? "" : propValue);
//                string requestObjectState = (string.IsNullOrEmpty(request.Session.Attributes.State) ? "" : request.Session.Attributes.State);

//                if (propValue == requestObjectState)
//                {
//                    var func =(string)(string.IsNullOrEmpty(request.Request.Intent.Name) ? request.Request.Type : request.Request.Intent.Name);
//                    func = func.Replace("Amazon.", "").Replace("AMAZON.","");

//                    var method = intentMgr.GetMethod(func);

//                    if (method == null)
//                    {
//                        method = intentMgr.GetMethod("UnhandledIntent");
//                    }
//                    return method.Invoke(handler, new object[] { request, response });
//                }
//            }

//            return response;
//        }
//    }
//}

