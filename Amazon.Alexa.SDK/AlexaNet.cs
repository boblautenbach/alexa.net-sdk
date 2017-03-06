using Amazon.Alexa.SDK.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amazon.Alexa.SDK
{
    public sealed class AlexaNet: IAlexaNetSDK
    {
        private static readonly ConcurrentDictionary<string, string> HandlerDict = new ConcurrentDictionary<string, string>();

        private static readonly Lazy<AlexaNet> sdk =
            new Lazy<AlexaNet>(() => new AlexaNet());

        public static AlexaNet Instance { get { return sdk.Value; } }

        private AlexaNet()
        {
        }

        public static void RegisterHandlers()
        {
            if (!HandlerDict.Any())
            {
                LoadSDKAttributedClasses();
            }
        }

        private static void LoadSDKAttributedClasses()
        {
            var classes = GetTypesWith<AlexaNetSDK>(true);
            foreach (var c in classes)
            {
                MethodInfo[] methodInfos = Type.GetType(c.AssemblyQualifiedName)
                           .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                foreach(var m in methodInfos)
                {
                    HandlerDict.TryAdd(m.Name, c.AssemblyQualifiedName);
                }
            }
        }

        private static dynamic ProcessRequest(dynamic request, dynamic response)
        {
            object typeInstance = null;
            dynamic intentResponse = null;
            try
            {
                var func = (string)(string.IsNullOrEmpty(request.Request.Intent.Name) ? request.Request.Type : request.Request.Intent.Name);
                func = func.Replace("Amazon.", "").Replace("AMAZON.", "");

                var target = HandlerDict.FirstOrDefault(x => x.Key == func);

                var intentMgr = Type.GetType(target.Value);

                var method = intentMgr.GetMethod(func);

                if (method == null)
                {
                    throw new Exception("Not found");
                }

                //Handles static classes or methods as well as instance classes
                if (intentMgr.IsAbstract || method.IsAbstract)
                {
                    //don't instantiate, its a static class and/or method
                    intentResponse = method.Invoke(intentMgr, new object[] { request, response });
                }else
                {
                    //Need to conside that this is really creating a new object for every call vs
                    //getting a refernce to an object and calling methods.  This also requires
                    //a parameterless class and the main issue is related to using you API controller as 
                    //your intent handler ("this").  In that instance, you would be creating an 
                    //instance of your API controller for all intent requests (even though the API object
                    //already exists
                    ConstructorInfo typeContructor = intentMgr.GetConstructor(Type.EmptyTypes);
                    typeInstance = typeContructor.Invoke(new object[] { });
                    intentResponse = method.Invoke(typeInstance, new object[] { request, response });
                }

                //set type to null to prompt garbage collection
                //perhaps require intent handlers to implemention IDisposable??
                typeInstance = null;

                return intentResponse;

            }catch
            {
                throw new Exception("Oops");
            }
        }

        public static bool ShouldIgnoreClass(Type t)
        {
            // Get instance of the attribute.
            AlexaNetSDK attrib =
                (AlexaNetSDK)Attribute.GetCustomAttribute(t, typeof(AlexaNetSDK));

            System.Diagnostics.Trace.TraceInformation(t.AssemblyQualifiedName +  " " + attrib.Ignore.ToString());
            return attrib.Ignore;
        }

        static IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                  where TAttribute : System.Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit) 
                   && ShouldIgnoreClass(t) == false
                   select t;
        }

        public static dynamic HandleIntent(dynamic request, dynamic response)
        {
            return ProcessRequest(request, response);
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
