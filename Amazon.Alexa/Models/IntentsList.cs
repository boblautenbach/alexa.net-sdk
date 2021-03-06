﻿using System;
using System.Collections.Generic;

namespace Amazon.Alexa.Models
{
    public class IntentsList : List<KeyValuePair<string, Func<AlexaRequest, dynamic>>>
    {
        public void Add(string intentName, Func<AlexaRequest, dynamic> function)
        {
            var intent = new KeyValuePair<string, Func<AlexaRequest, dynamic>>(intentName, function);
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