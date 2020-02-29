using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace CommercialOfferings.MissionData
{
    public class MissionOrbit
    {
        [XmlElement]
        public double semiMajorAxis = 0.0;
        [XmlElement]
        public double eccentricity = 0.0;
        [XmlElement]
        public double inclination = 0.0;
        [XmlElement]
        public double argumentOfPeriapsis = 0.0;
        [XmlElement]
        public double LAN = 0.0;
        [XmlElement]
        public double meanAnomalyAtEpoch = 0.0;

        public static MissionOrbit GetMissionOrbit(Orbit orbit)
        {
            MissionOrbit missionOrbit = new MissionOrbit
            {

                semiMajorAxis = orbit.semiMajorAxis,
                eccentricity = orbit.eccentricity,
                inclination = orbit.inclination,
                argumentOfPeriapsis = orbit.argumentOfPeriapsis,
                LAN = orbit.LAN,
                meanAnomalyAtEpoch = orbit.meanAnomalyAtEpoch,
            };
            return missionOrbit;
        }
    }
}
