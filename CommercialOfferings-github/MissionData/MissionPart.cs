using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionPart
    {
        [XmlElement]
        public uint flightID = 0;

        public static List<MissionPart> GetMissionPartList(Vessel vessel)
        {
            return GetMissionPartList(vessel.parts);
        }

        public static List<MissionPart> GetMissionPartList(List<Part> parts)
        {
            List<MissionPart> missionParts = new List<MissionPart>();

            foreach (Part part in parts)
            {
                MissionPart missionPart = new MissionPart
                {
                    flightID = part.flightID,
                };
                missionParts.Add(missionPart);
            }

            return missionParts;
        }
    }
}
