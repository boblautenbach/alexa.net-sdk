using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Amazon.Alexa.SDK
{

    //TOOD:
    // handle expections
    // document each method for clarification

    public sealed class AlexaNet: IAlexaNetSDK
    {
        private static readonly ConcurrentDictionary<string, string> _handlerDict = new ConcurrentDictionary<string, string>();
        private static ConcurrentBag<object> _handlerList = new ConcurrentBag<object>();

        private static readonly Lazy<AlexaNet> sdk =
            new Lazy<AlexaNet>(() => new AlexaNet());

        public static AlexaNet Instance { get { return sdk.Value; } }

        private AlexaNet()
        {
        }

        /// <summary>
        /// Registers your intent handlers.
        /// </summary>
        /// <param name="handlers">List of instantiated objects, classes with static methods or singleton objects with public methods to handle intents </param>
        /// <returns>void</returns>
        public static void RegisterHandlers(List<object> handlers)
        {
            try
            {
                if (!_handlerDict.Any())
                {
                    _handlerList = new ConcurrentBag<object>(handlers);
                    LoadClasses(handlers);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("RegisterHandler Exception.", ex);
            }
        }

        private static void LoadClasses(List<object> handlers)
        {
            foreach (var c in handlers)
            {
                var intentMgr = c.GetType();
                LoadInternalHandlerMethodMapper(intentMgr);
            }
        }
        private static void LoadInternalHandlerMethodMapper(Type c)
        {
            MethodInfo[] methodInfos = Type.GetType(c.AssemblyQualifiedName)
                       .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            foreach (var m in methodInfos)
            {
                string valueCheck = string.Empty;
               _handlerDict.TryGetValue(m.Name, out valueCheck);
                if (string.IsNullOrEmpty(valueCheck))
                {
                    throw new Exception("Duplicate handler method found.  When using more than one handler object, request type and intent handler methods must not be duplicated accross objects.");
                }
                _handlerDict.TryAdd(m.Name, c.AssemblyQualifiedName);
            }
        }
        private static dynamic ProcessRequest(dynamic request, dynamic response)
        {
            object typeInstance = null;
            dynamic intentResponse = null;

            var func = (string)(string.IsNullOrEmpty(request.Request.Intent.Name) ? request.Request.Type : request.Request.Intent.Name);
            func = func.Replace("Amazon.", "").Replace("AMAZON.", "");

            var target = _handlerDict.FirstOrDefault(x => x.Key == func);

            var intentMgr = Type.GetType(target.Value);

            var method = intentMgr.GetMethod(func);

            if (method == null)
            {
                throw new Exception("Intent method handler not found.");
            }

            //this is for handling passing a list of objects (vs using attributed classes
            typeInstance = _handlerList.FirstOrDefault(x => x.GetType().AssemblyQualifiedName == target.Value);
            intentResponse = method.Invoke(typeInstance, new object[] { request, response });

            //set type to null to prompt garbage collection
            //perhaps require intent handlers to implemention IDisposable??
            typeInstance = null;

            return intentResponse;
        }

        public static dynamic HandleIntent(dynamic request, dynamic response)
        {
            try
            { 
                 return ProcessRequest(request, response);
            }catch(Exception ex)
            {
                throw new Exception("Error processing request", ex);
            }
        }

        public T BuildOutputWithCard<T>(T response)
        {
            throw new NotImplementedException();
        }

        public dynamic BuildOutputWithCardImages()
        {
            throw new NotImplementedException();
        }

        public dynamic BuildOutput()
        {
            throw new NotImplementedException();
        }
    }
}
