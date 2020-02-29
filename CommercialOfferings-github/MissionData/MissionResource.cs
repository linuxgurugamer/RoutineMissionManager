using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionResource
    {
        [XmlElement]
        public string Name = "";
        [XmlElement]
        public double Amount = 0.0;

        public static List<MissionResource> GetMissionResourceList(Vessel vessel)
        {
            return GetMissionResourceList(vessel.parts);
        }

        public static List<MissionResource> GetMissionResourceList(List<Part> parts)
        {
            List<MissionResource> missionResources = new List<MissionResource>();

            List<string> resources = RmmUtil.DetermineResourceArray(parts);


            foreach (String resource in resources)
            {
                var amount = RmmUtil.ReadResource(parts, resource);
                if (amount != 0)
                {
                    MissionResource missionResource = new MissionResource
                    {
                        Name = resource,
                        Amount = amount,
                    };
                    missionResources.Add(missionResource);
                }
            }

            return missionResources;
        }
    }
}
