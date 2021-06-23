using CommercialOfferings.MissionData;
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    public class RoutineArrivalMission : RoutineMission
    {
        public RoutineArrivalMission()
        {
        }

        public string Name
        {
            get { return _mission.Info.Name; }
        }

        public string VesselName
        {
            get { return Localizer.Format(_mission.Launch.VesselName); }
        }

        public string Body
        {
            get { return _mission.Arrival.Body; }
        }

        public double MaxOrbitAltitude
        {
            get
            {
                if (RmmUtil.HomeBody(_mission.Arrival.Body))
                {
                    return RmmUtil.OrbitAltitude(_mission.Arrival.Orbit.semiMajorAxis, _mission.Arrival.Body) * 1.3;
                }
                else
                {
                    return 0.0;
                }
            }
        }

        public uint flightIDDockPart
        {
            get { return _mission.Arrival.flightIDDockPart; }
        }

        public double Duration
        {
            get
            {
                return _mission.Arrival.Time - _mission.Launch.Time;
            }
        }

        public double Price
        {
            get
            {
                double price = 0.0;
                price += _mission.Launch.Funds;
                foreach (MissionLanding Landing in _mission.Landings)
                {
                    price -= Landing.Funds;
                }
                return price;
            }
        }

        public int MinimumCrew
        {
            get
            {
                return _mission.Arrival.Crew;
            }
        }

        public int MinimumCrewOutset
        {
            get
            {
                return _mission.Launch.Crew;
            }
        }

        public int CrewCapacity
        {
            get
            {
                return _mission.Arrival.CrewCapacity;
            }
        }

        #region Order

        public double ArrivalTime
        {
            get { return GetOrderValue<double>("ArrivalTime"); }
            set { SetOrderValue("ArrivalTime", value); }
        }

        public int CrewCount
        {
            get { return GetOrderValue<int>("CrewCount"); }
            set { SetOrderValue("CrewCount", value);}
        }

        public string CrewSelection
        {
            get { return GetOrderValue<string>("CrewSelection"); }
            set { SetOrderValue("CrewSelection", value); }
        }

        public uint flightIdArrivalDockPart
        {
            get { return GetOrderValue<uint>("flightIdArrivalDockPart"); }
            set { SetOrderValue("flightIdArrivalDockPart", value); }
        }
        #endregion Order

        public override CheckList Valid()
        {
            var checkList = new CheckList();

            checkList.Check(_mission.Launch != null, "no launch tracked");
            checkList.Check(_mission.Landings != null, "no potential landings data");
            checkList.Check(_mission.Arrival != null, "no arrival tracked");
            if (!checkList.CheckSuccess) { return checkList; }

            int crewBalance = _mission.Launch.Crew;
            foreach (MissionLanding landing in _mission.Landings)
            {
                crewBalance -= landing.Crew;
            }
            crewBalance -= _mission.Arrival.Crew;
            checkList.Check(crewBalance <= 0, "not all crew returned");

            return checkList;
        }

        public CheckList Allowed(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid arrival mission");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(AllowedLocation(vessel), "location not allowed");
            checkList.Check(CrewCount >= _mission.Arrival.Crew, "not enough crew");
            checkList.Check(CrewCount <= _mission.Arrival.CrewCapacity, "too many crew");
            checkList.Check(flightIdArrivalDockPart > 0, "no docking port selected");
            return checkList;
        }

        public CheckList AllowedLocation(Vessel vessel)
        {
            var checkList = new CheckList();

            checkList.Check(Valid(), "invalid arrival mission");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(vessel.situation == Vessel.Situations.ORBITING, "vessel not in orbit");
            checkList.Check(vessel.mainBody.name == _mission.Arrival.Body, "vessel not at " + _mission.Arrival.Body);
            checkList.Check(RmmUtil.HomeBody(_mission.Arrival.Body) || vessel.orbit.semiMajorAxis < _mission.Arrival.Orbit.semiMajorAxis * 1.3, "vessel orbit too high");

            return checkList;
        }

        public bool OrderValid()
        {
            return true;
        }
    }
}
