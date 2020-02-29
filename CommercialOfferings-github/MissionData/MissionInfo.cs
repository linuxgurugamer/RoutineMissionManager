using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionInfo
    {
        [XmlElement]
        public int Type = 0;
        [XmlElement]
        public string Name = "";
        [XmlElement]
        public string Campaign = "";
    }
}
