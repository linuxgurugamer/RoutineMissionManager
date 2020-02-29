using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionCoordinates
    {
        [XmlElement]
        public double Latitude = 0.0;
        [XmlElement]
        public double Longitude = 0.0;

        public static MissionCoordinates GetMissionCoordinates(double latitude, double longitude)
        {
            MissionCoordinates missionCoordinates = new MissionCoordinates
            {
                Latitude = latitude,
                Longitude = longitude,
            };
            return missionCoordinates;
        }
    }
}
