using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionLanding
    {
        [XmlElement]
        public double Time = 0;
        [XmlElement]
        public string Body = "";
        [XmlElement]
        public MissionCoordinates Coordinates = null;
        [XmlElement]
        public double Funds = 0.0;
        [XmlElement]
        public int Crew = 0;
        [XmlElement]
        public int CrewCapacity = 0;
        [XmlArray("Parts")]
        [XmlArrayItem(typeof(MissionPart), ElementName = "Part")]
        public List<MissionPart> Parts = null;
        [XmlArray("Resources")]
        [XmlArrayItem(typeof(MissionResource), ElementName = "Resource")]
        public List<MissionResource> Resources = null;
    }
}
