using CommercialOfferings.MissionData;
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    class RoutineRetrievalMission : RoutineMission
    {
        public RoutineRetrievalMission()
        {
        }

        public string Name
        {
            get { return _mission.Info.Name; }
        }

        public string VesselName
        {
            get { return Localizer.Format(_mission.Departure.VesselName); }
        }

        public string Body
        {
            get { return _mission.Departure.Body; }
        }

        public double MaxOrbitAltitude
        {
            get
            {
                if (RmmUtil.HomeBody(_mission.Departure.Body))
                {
                    return RmmUtil.OrbitAltitude(_mission.Departure.Orbit.semiMajorAxis, _mission.Departure.Body) * 1.3;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        public double Duration
        {
            get
            {
                return _mission.Arrival.Time - _mission.Departure.Time;
            }
        }

        public double Price
        {
            get
            {
                return 0.0;
            }
        }

        public int MinimumCrew
        {
            get
            {
                return _mission.Departure.Crew;
            }
        }

        public int CrewCapacity
        {
            get
            {
                return _mission.Arrival.CrewCapacity;
            }
        }

        public List<string> Propellants
        {
            get
            {
                return _mission.Departure.Propellants;
            }
        }

        public List<MissionResource> Resources
        {
            get
            {
                return _mission.Departure.Resources;
            }
        }

        public List<MissionResource> ArrivalResources
        {
            get
            {
                return _mission.Arrival?.Resources;
            }
        }

        public uint flightIDDockPart
        {
            get { return _mission.Arrival.flightIDDockPart; }
        }

        #region Order

        public double DepartureTime
        {
            get { return GetOrderValue<double>("DepartureTime"); }
            set { SetOrderValue("DepartureTime", value); }
        }

        public double ArrivalTime
        {
            get { return GetOrderValue<double>("ArrivalTime"); }
            set { SetOrderValue("ArrivalTime", value); }
        }

        public uint flightIdDepartureDockPart
        {
            get { return GetOrderValue<uint>("flightIdDepartureDockPart"); }
            set { SetOrderValue("flightIdDepartureDockPart", value); }
        }

        public int CrewCount
        {
            get { return GetOrderValue<int>("CrewCount"); }
            set { SetOrderValue("CrewCount", value); }
        }

        public string CrewSelection
        {
            get { return GetOrderValue<string>("CrewSelection"); }
            set { SetOrderValue("CrewSelection", value); }
        }

        #endregion Order

        public override CheckList Valid()
        {
            var checkList = new CheckList();

            checkList.Check(_mission.Departure != null, "no departure tracked");
            checkList.Check(_mission.Arrival != null, "no arrival tracked");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_mission.Departure.Crew <= _mission.Arrival.Crew, "not all crew returned");

            return checkList;
        }

        /// <summary>
        /// Gets the amount of a propellant consumed during the tracked mission (departure amount - arrival amount).
        /// Falls back to departure amount if arrival resources are not available.
        /// </summary>
        public double GetPropellantConsumedAmount(string propellantName)
        {
            double departureAmount = 0;
            foreach (MissionResource res in _mission.Departure.Resources)
            {
                if (res.Name == propellantName) { departureAmount = res.Amount; break; }
            }

            if (_mission.Arrival.Resources != null)
            {
                double arrivalAmount = 0;
                foreach (MissionResource res in _mission.Arrival.Resources)
                {
                    if (res.Name == propellantName) { arrivalAmount = res.Amount; break; }
                }
                double consumed = departureAmount - arrivalAmount;
                return consumed > 0 ? consumed : 0;
            }

            // Fallback: require full departure amount if no arrival resources recorded
            return departureAmount;
        }

        public CheckList Allowed(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid retrieval mission");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(AllowedVessel(vessel), "vessel not allowed");
            if (!checkList.CheckSuccess) { return checkList; }

            List<Part> departureParts = RmmUtil.GetDockedParts(vessel, RmmUtil.GetDockedPart(vessel, RmmUtil.GetVesselPart(vessel, flightIdDepartureDockPart)));

            checkList.Check(departureParts != null, "no docked vessel on docking port");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(RmmUtil.AstronautCrewCount(departureParts) >= _mission.Departure.Crew, "not enough crew");
            checkList.Check(RmmUtil.AstronautCrewCount(departureParts) <= _mission.Arrival.CrewCapacity, "too many crew");

            foreach (MissionResource missionResource in _mission.Departure.Resources)
            {
                if (!_mission.Departure.Propellants.Contains(missionResource.Name)) { continue; }

                double consumed = GetPropellantConsumedAmount(missionResource.Name);
                checkList.Check(RmmUtil.ReadResource(departureParts, missionResource.Name) >= (consumed * 0.99), "insufficient " + missionResource.Name);
            }

            return checkList;
        }

        public CheckList AllowedVessel(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid retrieval mission");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(AllowedLocation(vessel), "location not allowed");

            Structure structure = Structure.GetDockedStructure(vessel, RmmUtil.GetDockedPart(vessel, RmmUtil.GetVesselPart(vessel, flightIdDepartureDockPart)));

            checkList.Check(structure != null, "no docked structure detected on docking port");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_mission.Departure.Structure.Equal(structure), "docked structure unequal to tracked structure");

            return checkList;
        }

        public CheckList AllowedLocation(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid retrieval mission");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(vessel.situation == Vessel.Situations.ORBITING, "vessel not in orbit");
            checkList.Check(vessel.mainBody.name == _mission.Departure.Body, "vessel not at " + _mission.Departure.Body);
            checkList.Check(!RmmUtil.HomeBody(_mission.Departure.Body) || vessel.orbit.semiMajorAxis < _mission.Departure.Orbit.semiMajorAxis * 1.3, "vessel orbit too high");

            return checkList;
        }

        public bool OrderValid()
        {
            return true;
        }
    }
}
