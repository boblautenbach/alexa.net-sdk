using System;
using System.Collections.Generic;

namespace Amazon.Alexa.SDK.Models
{
    public class IntentsList : List<KeyValuePair<string, Func<dynamic, dynamic>>>
    {
        public void Add(string intentName, Func<dynamic, dynamic> function)
        {
            var intent = new KeyValuePair<string, Func<dynamic, dynamic>>(intentName, function);
            var index = FindIndex(i => i.Key.Equals(intentName));
            if (index != -1)
            {
                this[index] = intent;
            }
            else
            {
                Add(intent);
            }
        }
    }
}