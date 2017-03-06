using System;

namespace Amazon.Alexa.SDK.Attributes
{
    // The AuthorAttribute class is a user-defined attribute class.
    // It can be applied to classes only.
    // It has one property Ignore, which tells the SDK not
    // not to include the decorated class in the list of handlers to search
    [AttributeUsage(AttributeTargets.Class)]
    public class AlexaNetSDK : Attribute
    {
        // This constructor specifies the unnamed arguments to the attribute class.
        public AlexaNetSDK()
        {
            this.Ignore = false;
        }

        // This property is read-write (it has a set accessor)
        // so it can be used as a named argument when using this
        // class as an attribute class.
        public bool Ignore
        {
            get
            {
                return ignore;
            }
            set
            {
                ignore = value;
            }
        }

        public override string ToString()
        {
            return " Ignore : " + Ignore.ToString();
        }

        private bool ignore;
    }

}
