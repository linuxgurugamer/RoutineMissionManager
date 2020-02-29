using CommercialOfferings.MissionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    class RoutineDepartureMission : RoutineMission
    {
        public RoutineDepartureMission()
        {
        }

        public string Name
        {
            get { return _mission.Info.Name; }
        }

        public string VesselName
        {
            get { return _mission.Departure.VesselName; }
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
                double landingTime = 0;
                foreach (MissionLanding missionLanding in _mission.Landings)
                {
                    if (missionLanding.Time > landingTime)
                    {
                        landingTime = missionLanding.Time;
                    }
                }

                return landingTime - _mission.Departure.Time;
            }
        }


        public double Price
        {
            get
            {
                double price = 0.0;
                foreach (MissionLanding missionLanding in _mission.Landings)
                {
                    price += missionLanding.Funds;

                    foreach (MissionResource missionResource in missionLanding.Resources)
                    {
                        if (_mission.Departure.Propellants.Contains(missionResource.Name)) { continue; }

                        price -= RmmUtil.Cost(missionResource.Name, missionResource.Amount);
                    }
                }
                return price;
            }
        }

        public List<string> Proppelants
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

        public double CargoMass
        {
            get
            {
                double cargoMass = 0.0;
                foreach (MissionLanding missionLanding in _mission.Landings)
                {
                    
                    foreach (MissionResource missionResource in missionLanding.Resources)
                    {
                        if (_mission.Departure.Propellants.Contains(missionResource.Name)) { continue; }

                        cargoMass += RmmUtil.Mass(missionResource.Name, missionResource.Amount);
                    }
                }
                return cargoMass;
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
                int crewCapacity = 0;
                foreach (MissionLanding missionLanding in _mission.Landings)
                {
                    crewCapacity += missionLanding.CrewCapacity;
                }
                return crewCapacity;
            }
        }

        #region Order

        public double DepartureTime
        {
            get { return GetOrderValue<double>("DepartureTime"); }
            set { SetOrderValue("DepartureTime", value); }
        }

        public uint flightIdDepartureDockPart
        {
            get { return GetOrderValue<uint>("flightIdDepartureDockPart"); }
            set { SetOrderValue("flightIdDepartureDockPart", value); }
        }

        #endregion Order

        public override CheckList Valid()
        {
            var checkList = new CheckList();

            checkList.Check(_mission.Departure != null, "no departure tracked");
            checkList.Check(_mission.Landings != null && _mission.Landings.Count > 0, "no landing tracked");
            if (!checkList.CheckSucces) { return checkList; }

            int crewBalance = _mission.Departure.Crew;
            foreach (MissionLanding landing in _mission.Landings)
            {
                crewBalance -= landing.Crew;
            }
            checkList.Check(crewBalance <= 0, "not all crew returned");

            return checkList;
        }

        public CheckList Allowed(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid mission departure mission");
            if (!checkList.CheckSucces) { return checkList; }

            checkList.Check(AllowedVessel(vessel), "vessel not allowed");
            if (!checkList.CheckSucces) { return checkList; }

            List<Part> departureParts = RmmUtil.GetDockedParts(vessel, RmmUtil.GetDockedPart(vessel, RmmUtil.GetVesselPart(vessel, flightIdDepartureDockPart)));

            checkList.Check(departureParts != null, "no docked vessel on docking port");
            if (!checkList.CheckSucces) { return checkList; }

            checkList.Check(RmmUtil.AstronautCrewCount(departureParts) >= _mission.Departure.Crew, "not enough crew");
            int crewCapacity = 0;
            foreach (MissionLanding missionLanding in _mission.Landings)
            {
                crewCapacity += missionLanding.CrewCapacity;
            }
            checkList.Check(RmmUtil.AstronautCrewCount(departureParts) <= crewCapacity, "too many crew");

            foreach (MissionResource missionResource in _mission.Departure.Resources)
            {
                if (!_mission.Departure.Propellants.Contains(missionResource.Name)) { continue; }

                checkList.Check(RmmUtil.ReadResource(departureParts, missionResource.Name) >= (missionResource.Amount * 0.99), "insufficient " + missionResource.Name);
            }

            double vesselCargoMass = 0;
            List<MissionResource> vesselResources = MissionResource.GetMissionResourceList(departureParts);
            foreach (MissionResource vesselResource in vesselResources)
            {
                if (Proppelants.Contains(vesselResource.Name)) { continue; }

                vesselCargoMass += RmmUtil.Mass(vesselResource.Name, vesselResource.Amount);
            }
            LoggerRmm.Debug(vesselCargoMass + " " + CargoMass);
            checkList.Check((vesselCargoMass * 0.99) <= CargoMass, "too much cargomass");

            return checkList;
        }

        public CheckList AllowedVessel(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid mission departure mission");
            if (!checkList.CheckSucces) { return checkList; }

            checkList.Check(AllowedLocation(vessel), "location not allowed");

            Structure structure = Structure.GetDockedStructure(vessel, RmmUtil.GetDockedPart(vessel, RmmUtil.GetVesselPart(vessel, flightIdDepartureDockPart)));

            checkList.Check(structure != null, "no docked structure detected on docking port");
            if (!checkList.CheckSucces) { return checkList; }

            checkList.Check(_mission.Departure.Structure.Equal(structure), "docked structure unequal to tracked structure");

            return checkList;
        }


        public CheckList AllowedLocation(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid mission departure mission");
            if (!checkList.CheckSucces) { return checkList; }

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
