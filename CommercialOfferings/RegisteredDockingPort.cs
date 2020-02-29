using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    public class RegisteredDockingPort
    {
        [Persistent]
        public uint flightId;
        [Persistent]
        public string Name;

        public RegisteredDockingPort Copy()
        {
            var copy = new RegisteredDockingPort();
            copy.flightId = flightId;
            copy.Name = Name;
            return copy;
        }
    }
}
