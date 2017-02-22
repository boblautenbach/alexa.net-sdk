using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Alexa.Demo.Web.Helpers
{
    public static class Helper
    {
        public static T EnumParse<T>(string input)
        {
            return (T)Enum.Parse(typeof(T), input, true);
        }

        public static TEnum ToEnum<TEnum>(this string strEnumValue, TEnum defaultValue)
        {
            if (!Enum.IsDefined(typeof(TEnum), strEnumValue))
                return defaultValue;

            return (TEnum)Enum.Parse(typeof(TEnum), strEnumValue);
        }

        public static string EnumName<T>(T input)
        {
            return Enum.GetName(typeof(T), input);
        }
    }
}