using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionArrival
    {
        [XmlElement]
        public double Time = 0;
        [XmlElement]
        public string Body = "";
        [XmlElement]
        public MissionOrbit Orbit = null;
        [XmlElement]
        public uint flightIDDockPart = 0;
        [XmlElement]
        public int Crew = 0;
        [XmlElement]
        public int CrewCapacity = 0;
        [XmlArray("Parts")]
        [XmlArrayItem(typeof(MissionPart), ElementName = "Part")]
        public List<MissionPart> Parts = null;
    }
}
