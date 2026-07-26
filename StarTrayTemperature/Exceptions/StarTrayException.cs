using System;

namespace StarTrayTemperature
{
    public class StarTrayException : Exception
    {
        public StarTrayException(string message) : base(message) { }

        public StarTrayException(string message, Exception inner) : base(message, inner) { }
    }
}
