/*Copyright (c) 2014, Flip van Toly
 All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are 
permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of 
conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of 
conditions and the following disclaimer in the documentation and/or other materials provided with 
the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to 
endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
OF SUCH DAMAGE.*/

//Namespace Declaration 
using System;
using System.Collections.Generic;
using KSP.Localization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using CommercialOfferings.MissionData;
using SpaceTuxUtility;
using ClickThroughFix;

namespace CommercialOfferings
{
    public class Tracking
    {
        private RMMModule _partModule;
        private Mission _mission;

        private double landedTime = 0.0;
        private double landedLatitude = 0.0;
        private double landedLongitude = 0.0;
        private int landedTimeMessage = 0;

        //GUI Tracking
        private bool renderGUITracking = false;
        private static Rect windowPosGUITracking = new Rect(200, 200, 200, 30);
        //GUI variables
        private string trackStrName = "";
        private string trackStrVehicleName = "";

        public Tracking(RMMModule partModule)
        {
            _partModule = partModule;
            if (_mission == null && !String.IsNullOrEmpty(trackMissionId))
            {
                _mission = Mission.GetMissionById(trackMissionId);
            }

        }

        #region Saved Properties

        private string trackMissionId
        {
            get { return _partModule.trackMissionId; }
            set { _partModule.trackMissionId = value; }
        }

        private bool trackingActive
        {
            get { return _partModule.trackingActive; }
            set { _partModule.trackingActive = value; }
        }

        private bool trackingPrimary
        {
            get { return _partModule.trackingPrimary; }
            set { _partModule.trackingPrimary = value; }
        }

        private string trackingStatus
        {
            get { return _partModule.trackingStatus; }
            set { _partModule.trackingStatus = value; }
        }



        private float trackPartCount
        {
            get { return _partModule.trackPartCount; }
            set { _partModule.trackPartCount = value; }
        }

        private bool returnMission
        {
            get { return _partModule.returnMission; }
            set { _partModule.returnMission = value; }
        }

        private bool returnMissionDeparted
        {
            get { return _partModule.returnMissionDeparted; }
            set { _partModule.returnMissionDeparted = value; }
        }

        private string trackFolderName
        {
            get { return _partModule.trackFolderName; }
            set { _partModule.trackFolderName = value; }
        }

        private float trackMissionTime
        {
            get { return _partModule.trackMissionTime; }
            set { _partModule.trackMissionTime = value; }
        }

        private string trackBody
        {
            get { return _partModule.trackBody; }
            set { _partModule.trackBody = value; }
        }

        private float trackMaxOrbitAltitude
        {
            get { return _partModule.trackMaxOrbitAltitude; }
            set { _partModule.trackMaxOrbitAltitude = value; }
        }

        private string trackDescription
        {
            get { return _partModule.trackDescription; }
            set { _partModule.trackDescription = value; }
        }

        private float trackCrew
        {
            get { return _partModule.trackCrew; }
            set { _partModule.trackCrew = value; }
        }

        private float trackCrewCapacity
        {
            get { return _partModule.trackCrewCapacity; }
            set { _partModule.trackCrewCapacity = value; }
        }

        private bool trackReturnEnabled
        {
            get { return _partModule.trackReturnEnabled; }
            set { _partModule.trackReturnEnabled = value; }
        }

        private bool trackSafeReturn
        {
            get { return _partModule.trackSafeReturn; }
            set { _partModule.trackSafeReturn = value; }
        }

        private string trackReturnResources
        {
            get { return _partModule.trackReturnResources; }
            set { _partModule.trackReturnResources = value; }
        }

        private double trackReturnCargoMass
        {
            get { return _partModule.trackReturnCargoMass; }
            set { _partModule.trackReturnCargoMass = value; }
        }

        private uint trackPort
        {
            get { return _partModule.trackPort; }
            set { _partModule.trackPort = value; }
        }

        private float trackDockingDistance
        {
            get { return _partModule.trackDockingDistance; }
            set { _partModule.trackDockingDistance = value; }
        }

        public float trackMinimumReturnCrew
        {
            get { return _partModule.trackMinimumReturnCrew; }
            set { _partModule.trackMinimumReturnCrew = value; }
        }

        //public float trackCrewBalance
        //{
        //    get { return _partModule.trackCrewBalance; }
        //    set { _partModule.trackCrewBalance = value; }
        //}
        //
        //public float trackReturnCrewBalance
        //{
        //    get { return _partModule.trackReturnCrewBalance; }
        //    set { _partModule.trackReturnCrewBalance = value; }
        //}

        #endregion Saved Properties

        #region Derived Properties

        private Vessel vessel
        {
            get { return _partModule.vessel; }
        }

        private Part part
        {
            get { return _partModule.part; }
        }

        #endregion Derived Properties

        #region Public Properties

        public string MissionName
        {
            get { return _mission?.Info?.Name ?? ""; }
        }

        public string VesselName
        {
            get { return Localizer.Format(vessel?.vesselName ?? ""); }
        }

        public string Status
        {
            get { return trackingStatus; }
        }

        #endregion Public Properties

        public void DrawGUI()
        {
            //Tracking GUI rendering
            if (renderGUITracking)
            {
                drawGUITracking();
            }
        }

        public void handleTracking()
        {
            if (_mission == null || 
                _mission.Info == null)
            {
                return;
            }

            if (trackingPrimary)
            {
                handleTrackingPrimary();
            }
            else if (trackingActive)
            {
                ensurePrimaryExists();
            }
        }

        public void handleTrackingPrimary()
        {
            switch (trackingStatus)
            {
                case "Launch":
                    if (vessel.situation == Vessel.Situations.FLYING || 
                        vessel.situation == Vessel.Situations.SUB_ORBITAL ||
                        vessel.situation == Vessel.Situations.ORBITING)
                    {
                        trackLaunch();
                    }
                    break;
                case "Departure":
                    foreach (Part p in vessel.parts)
                    {
                        if (p.flightID == trackPort && !RmmUtil.IsDocked(vessel, p))
                        {
                            trackDeparture();
                        }
                    }
                    break;
                case "Underway":
                    updatePartCount();
                    updateCrewCount();
                    updateMaxCrewCount();

                    // check for arrival
                    if (_mission.Info.Type == 10)
                    {
                        if (vessel.situation == Vessel.Situations.ORBITING && RmmUtil.AllowedBody(vessel.mainBody.name))
                        {
                            foreach (Part p in vessel.parts)
                            {
                                if (Mission.PartIsMissionPart(p, _mission.Launch.Parts))
                                {
                                    ModuleDockingNode dockingModule = p.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                                    if (dockingModule != null)
                                    {
                                        Part dockedPart = RmmUtil.GetDockedPart(vessel, p);
                                        if (dockedPart != null && !Mission.PartIsMissionPart(dockedPart, _mission.Launch.Parts))
                                        {
                                            trackArrival(p);
                                            return;
                                        }
                                    }
                                }

                            }
                        }
                    }

                    // make snapshot of vessel
                    if (_mission.Info.Type == 10)
                    {
                        if (vessel.situation == Vessel.Situations.ORBITING && RmmUtil.AllowedBody(vessel.mainBody.name))
                        {
                            // don't make snapshot when the vessel is in the process of docking. It leaves the docking port in the wrong state.
                            bool isAnyPartDocking = false;
                            foreach (Part p in vessel.parts)
                            {
                                ModuleDockingNode dockingModule = p.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                                if (dockingModule != null && RmmUtil.IsDocking(p))
                                {
                                    isAnyPartDocking = true;
                                }
                            }

                            if (!isAnyPartDocking)
                            {
                                TakeVesselSnapShot();
                            }
                        }
                    }

                    // check for landing
                    if (_mission.Info.Type == 10 || _mission.Info.Type == 20)
                    {
                        if ((vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.SPLASHED) && RmmUtil.HomeBody(vessel.mainBody.name))
                        {
                            if (landedTime == 0.0)
                            {
                                landedTime = Planetarium.GetUniversalTime();
                                landedLatitude = vessel.latitude;
                                landedLongitude = vessel.longitude;
                                landedTimeMessage = 0;
                            }
                        }

                        if (landedTime > 0.0)
                        {
                            // Check still landed
                            if (!(vessel.situation == Vessel.Situations.LANDED || vessel.situation == Vessel.Situations.SPLASHED) || !RmmUtil.HomeBody(vessel.mainBody.name))
                            {
                                landedTime = 0.0;
                                landedLatitude = 0.0;
                                landedLongitude = 0.0;
                                landedTimeMessage = 0;
                            }
                            // Check not moved
                            else if (RmmUtil.GetDistanceBetweenPoints(landedLatitude, landedLongitude, vessel.latitude, vessel.longitude, vessel.mainBody.Radius) > 2)
                            {
                                landedTime = 0.0;
                                landedLatitude = 0.0;
                                landedLongitude = 0.0;
                                landedTimeMessage = 0;
                            }
                            // Countdown
                            else if (Planetarium.GetUniversalTime() - landedTime > 0.0 && Planetarium.GetUniversalTime() - landedTime < 10)
                            {
                                if (Planetarium.GetUniversalTime() - landedTime > landedTimeMessage + 1.0)
                                {
                                    landedTimeMessage = landedTimeMessage + 1;
                                    ScreenMessages.PostScreenMessage("landing confirmation " + (10 - landedTimeMessage).ToString() + " seconds", 1, ScreenMessageStyle.UPPER_CENTER);
                                }
                            }
                            // Landed
                            else if (Planetarium.GetUniversalTime() - landedTime > 10)
                            {
                                TrackLanding();
                                return;
                            }
                        }
                    }

                    if (RmmUtil.CountVesselParts(vessel) > trackPartCount)
                    {
                        trackAbort("vessel has joined with non mission vessel");
                    }

                    if (RmmUtil.AstronautCrewCount(vessel) > trackCrew)
                    {
                        trackAbort("crew has been added to vessel");
                    }

                    updateTrackingVars(vessel);

                    break;
            }
        }


        private void updatePartCount()
        {
            int currentCount = RmmUtil.CountVesselParts(vessel);

            if (currentCount < trackPartCount)
            {
                trackPartCount = currentCount;
            }
        }

        private void updateCrewCount()
        {
            int currentCount = RmmUtil.AstronautCrewCount(vessel);

            if (currentCount < trackCrew)
            {
                trackCrew = currentCount;
            }
        }

        private void updateMaxCrewCount()
        {
            int currentCount = RmmUtil.CrewCapacityCount(vessel);

            if (currentCount < trackCrewCapacity)
            {
                trackCrewCapacity = currentCount;
            }
        }

        //private void updateReturnCrewBalanceCount()
        //{
        //    int currentCount = RmmUtil.AstronautCrewCount(vessel);
        //
        //    if (currentCount < trackReturnCrewBalance)
        //    {
        //        trackReturnCrewBalance = currentCount;
        //    }
        //}

        private void determineReturnResources()
        {
            string content = "";

            string[] arrResource = new string[0];
            //RmmUtil.DetermineProppellantArray(vessel, ref arrResource);

            foreach (String res in arrResource)
            {
                double amount = 0;

                foreach (Part p in vessel.parts)
                {
                    foreach (PartResource r in p.Resources)
                    {
                        if (r.info.name == res)
                        {
                            amount = amount + r.amount;
                        }
                    }
                }

                if (content == "")
                {
                    content = res + ":" + amount.ToString();
                }
                else
                {
                    content = content + "," + res + ":" + amount.ToString();
                }
            }
            trackReturnResources = content;
        }

        private void determineReturnCargoMass()
        {
            trackReturnCargoMass = getTrackingCargoMass();
        }

        private double getTrackingCargoMass()
        {
            double cargoMass = 0.0;

            string[] cargoArray = new string[0];
            //RmmUtil.GetCargoArray(vessel, trackReturnResources, ref cargoArray);

            foreach (Part p in vessel.parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (cargoArray.Contains(r.info.name))
                    {
                        cargoMass = cargoMass + RmmUtil.Mass(r.info.name, r.amount);
                    }
                }
            }
            return (cargoMass);
        }

        private void TakeVesselSnapShot()
        {
            ConfigNode savenode = new ConfigNode();
            vessel.BackupVessel();
            vessel.protoVessel.Save(savenode);
            savenode.Save(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + _mission.MissionId + Path.DirectorySeparatorChar + "vesselfiletrack" + part.flightID);
            vessel.BackupVessel();
        }

        private void StoreVesselSnapShot(string prefix)
        {
            string[] line = System.IO.File.ReadAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + _mission.MissionId + Path.DirectorySeparatorChar + "vesselfiletrack" + part.flightID);

            System.IO.File.WriteAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + _mission.MissionId + Path.DirectorySeparatorChar + prefix + "vesselfile", line);
        }

        private void updateTrackingVars(Vessel ves)
        {
            foreach (Part p in ves.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "RMMModule" && part.flightID != p.flightID)
                    {
                        RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        if (ComOffMod.trackMissionId == trackMissionId)
                        {
                            ComOffMod.trackingActive = trackingActive;
                            ComOffMod.trackingStatus = trackingStatus;
                            ComOffMod.trackPartCount = trackPartCount;
                            ComOffMod.returnMission = returnMission;
                            ComOffMod.returnMissionDeparted = returnMissionDeparted;
                            ComOffMod.trackFolderName = trackFolderName;
                            ComOffMod.trackMissionTime = trackMissionTime;
                            ComOffMod.trackBody = trackBody;
                            ComOffMod.trackMaxOrbitAltitude = trackMaxOrbitAltitude;
                            ComOffMod.trackDescription = trackDescription;
                            ComOffMod.trackCrew = trackCrew;
                            ComOffMod.trackCrewCapacity = trackCrewCapacity;
                            ComOffMod.trackReturnEnabled = trackReturnEnabled;
                            ComOffMod.trackSafeReturn = trackSafeReturn;
                            ComOffMod.trackReturnResources = trackReturnResources;
                            ComOffMod.trackReturnCargoMass = trackReturnCargoMass;
                            ComOffMod.trackDockingDistance = trackDockingDistance;
                            ComOffMod.trackMinimumReturnCrew = trackMinimumReturnCrew;
                            //ComOffMod.trackCrewBalance = trackCrewBalance;
                            //ComOffMod.trackReturnCrewBalance = trackReturnCrewBalance;
                        }
                    }
                }
            }
        }

        private void updateNextLogicTime(Vessel ves)
        {
            foreach (Part p in ves.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "RMMModule" && part.flightID != p.flightID)
                    {
                        RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        if (ComOffMod.trackMissionId == trackMissionId)
                        {
                            ComOffMod.nextLogicTime = _partModule.nextLogicTime;
                        }
                    }
                }
            }
        }

        private void ensurePrimaryExists()
        {
            foreach (Part p in vessel.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "RMMModule")
                    {
                        RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        if (ComOffMod.trackMissionId == trackMissionId && ComOffMod.trackingPrimary)
                        {
                            return;
                        }
                    }
                }
            }

            foreach (Part p in vessel.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "RMMModule")
                    {
                        RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        if (ComOffMod.trackMissionId == trackMissionId)
                        {
                            if (p.flightID > part.flightID) { return; }
                        }
                    }
                }
            }
            trackingPrimary = true;
        }

        public void StartLaunchMission(string name)
        {
            trackStrName = name;
            _mission = Mission.NewMission(RmmUtil.Rand.Next(1000000, 9999999).ToString(), 10, trackStrName);
            trackMissionId = _mission.MissionId;

            trackingActive = false;
            SetThisPortPrimary();
            trackingStatus = "Launch";

            _partModule.nextLogicTime = Planetarium.GetUniversalTime();
        }

        public void StartDepartMission(string name, Part dockingPort)
        {
            trackStrName = name;
            _mission = Mission.NewMission(RmmUtil.Rand.Next(1000000, 9999999).ToString(), 20, trackStrName);
            trackMissionId = _mission.MissionId;

            trackPort = dockingPort.flightID;

            trackingActive = false;
            SetThisPortPrimary();
            trackingStatus = "Departure";

            _partModule.nextLogicTime = Planetarium.GetUniversalTime();
        }

        private void trackLaunch()
        {
            MissionLaunch launch = new MissionLaunch
            {
                Time = (float)Planetarium.GetUniversalTime(),
                Body = vessel.mainBody.name,
                VesselName = vessel.name,
                Funds = RmmUtil.CalculateVesselPrice(vessel),
                Crew = RmmUtil.AstronautCrewCount(vessel),
                Parts = MissionPart.GetMissionPartList(vessel),
            };
            _mission.TrackLaunch(launch);
            trackingActive = true;
            trackingPrimary = true;
            trackingStatus = "Underway";

            //-----------------------
            trackPartCount = RmmUtil.CountVesselParts(vessel);
            trackCrew = RmmUtil.AstronautCrewCount(vessel);
            trackCrewCapacity = RmmUtil.CrewCapacityCount(vessel);

            ScreenMessages.PostScreenMessage("mission tracking-LAUNCH", 4, ScreenMessageStyle.UPPER_CENTER);
            setOtherModules();
            updateTrackingVars(vessel);
            updateNextLogicTime(vessel);
        }

        private void trackDeparture()
        {
            Part dockedPart = null;
            foreach (Part p in vessel.parts)
            {
                if (p.flightID == trackPort)
                {
                    dockedPart = p;
                    break;
                }
            }

            MissionDeparture departure = new MissionDeparture
            {
                Time = (float)Planetarium.GetUniversalTime(),
                Body = vessel.mainBody.name,
                Orbit = MissionOrbit.GetMissionOrbit(vessel.orbit),
                flightIDDockPart = trackPort,
                VesselName = vessel.name,
                Crew = RmmUtil.AstronautCrewCount(vessel),
                Parts = MissionPart.GetMissionPartList(vessel),
                Resources = MissionResource.GetMissionResourceList(vessel),
                Propellants = RmmUtil.DetermineProppellantArray(vessel),
                Structure = Structure.GetDockedStructure(vessel, dockedPart)
            };

            _mission.TrackDeparture(departure);
            trackingActive = true;
            trackingPrimary = true;
            trackingStatus = "Underway";

            //-----------------------
            trackPartCount = RmmUtil.CountVesselParts(vessel);
            trackCrew = RmmUtil.AstronautCrewCount(vessel);
            trackCrewCapacity = RmmUtil.CrewCapacityCount(vessel);

            ScreenMessages.PostScreenMessage("mission tracking-DEPARTURE", 4, ScreenMessageStyle.UPPER_CENTER);
            setOtherModules();
            updateTrackingVars(vessel);
            updateNextLogicTime(vessel);
        }

        private void trackArrival(Part dockedPort)
        {
            MissionArrival arrival = new MissionArrival
            {
                Time = Planetarium.GetUniversalTime(),
                Body = vessel.mainBody.name,
                Orbit = MissionOrbit.GetMissionOrbit(vessel.orbit),
                flightIDDockPart = dockedPort.flightID,
                Crew = (int)trackCrew,
                CrewCapacity = (int)trackCrewCapacity,
                Parts = MissionPart.GetMissionPartList(RmmUtil.GetDockedParts(vessel, dockedPort))
            };

            _mission.TrackArrival(arrival);

            trackingActive = false;
            trackingPrimary = false;
            trackingStatus = "Arrived";

            //-----------------------
            StoreVesselSnapShot("arrival");

            _partModule.PortCode = "";


            ScreenMessages.PostScreenMessage("mission tracking-ARRIVAL", 4, ScreenMessageStyle.UPPER_CENTER);
            _partModule.nextLogicTime = 0;
            updateTrackingVars(vessel);
            updateNextLogicTime(vessel);
        }

        private void TrackLanding()
        {
            MissionLanding landing = new MissionLanding
            {
                Time = Planetarium.GetUniversalTime(),
                Body = vessel.mainBody.name,
                Coordinates = MissionCoordinates.GetMissionCoordinates(vessel.latitude, vessel.longitude),
                Funds = RmmUtil.CalculateVesselPrice(vessel),
                Crew = (int)trackCrew,
                CrewCapacity = (int)trackCrewCapacity,
                Parts = MissionPart.GetMissionPartList(vessel),
                Resources = MissionResource.GetMissionResourceList(vessel),
            };

            _mission.TrackLanding(landing);

            trackingPrimary = false;
            trackingActive = false;
            trackingStatus = "Landed";

            ScreenMessages.PostScreenMessage("mission tracking-LANDING", 4, ScreenMessageStyle.UPPER_CENTER);
            _partModule.nextLogicTime = 0;
            updateTrackingVars(vessel);
            updateNextLogicTime(vessel);
        }

        private void TrackReturn()
        {
            trackReturnEnabled = true;

            //createMissionFile();
            ScreenMessages.PostScreenMessage("mission tracking-REENTRY", 4, ScreenMessageStyle.UPPER_CENTER);
            updateTrackingVars(vessel);
        }

        private void TrackSafeReturn()
        {
            determineReturnCargoMass();
            trackSafeReturn = true;

            //createMissionFile();
            trackingActive = false;
            ScreenMessages.PostScreenMessage("mission tracking-RETURN", 4, ScreenMessageStyle.UPPER_CENTER);
            updateTrackingVars(vessel);
        }

        private void trackAbort(string message = null)
        {
            trackingActive = false;
            trackingPrimary = false;
            returnMission = false;
            ScreenMessages.PostScreenMessage("mission tracking ended", 4, ScreenMessageStyle.UPPER_CENTER);
            if (!String.IsNullOrEmpty(message))
            {
                ScreenMessages.PostScreenMessage(message, 4, ScreenMessageStyle.UPPER_CENTER);
            }
            _partModule.nextLogicTime = 0;
            updateTrackingVars(vessel);
            updateNextLogicTime(vessel);
        }

        //private void createMissionFile()
        //{
        //    string[] data = new string[16];
        //
        //    data[0] = "Name=" + trackName;
        //    data[1] = "CompanyName=" + trackCompanyName;
        //    data[2] = "VehicleName=" + trackVehicleName;
        //    data[3] = "LaunchSystemName=" + trackLaunchSystemName;
        //    data[4] = "Price=" + trackPrice.ToString();
        //    data[6] = "Time=" + trackMissionTime.ToString();
        //    data[7] = "Body=" + trackBody;
        //    data[8] = "MaxOrbitAltitude=" + trackMaxOrbitAltitude.ToString();
        //    data[9] = "MinimumCrew=" + trackMinimumCrew.ToString();
        //    data[10] = "MaximumCrew=" + trackMaximumCrew.ToString();
        //    data[11] = "ReturnEnabled=" + trackReturnEnabled.ToString();
        //    data[12] = "SafeReturn=" + trackSafeReturn.ToString();
        //    data[13] = "ReturnResources=" + trackReturnResources;
        //    data[14] = "ReturnCargoMass=" + trackReturnCargoMass.ToString();
        //    data[15] = "DockingDistance=0.15";
        //    data[16] = "MinimumReturnCrew=" + trackMinimumReturnCrew.ToString();
        //    data[17] = "CrewBalance=" + trackCrewBalance.ToString();
        //
        //    System.IO.File.WriteAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + trackFolderName + Path.DirectorySeparatorChar + "mission.txt", data);
        //
        //}

        private void createLandingFile()
        {
            string[] data = new string[6];

            data[0] = "ReturnLanding=" + returnMission.ToString();
            data[1] = "FlightID=" + part.flightID;
            data[2] = "LandingPrice=" + CalculateVesselPrice(false);
            data[3] = "LandingCrew=" + RmmUtil.AstronautCrewCount(vessel);
            data[4] = "MaximumLandingCrew=" + RmmUtil.AstronautCrewCount(vessel);
            data[5] = "ReturnCargoMass=" + trackReturnCargoMass.ToString();

            System.IO.File.WriteAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + trackFolderName + Path.DirectorySeparatorChar + "landing.txt", data);
        }


        private void debugFile()
        {
            string[] data = new string[17];

            data[6] = "Time=" + trackMissionTime.ToString();
            data[7] = "Body=" + trackBody;
            data[8] = "MaxOrbitAltitude=" + trackMaxOrbitAltitude.ToString();
            data[9] = "Partcount" + trackPartCount;
            data[10] = "MinimumCrew=" + trackCrew.ToString();
            data[11] = "MaximumCrew=" + trackCrewCapacity.ToString();
            data[12] = "ReturnEnabled=" + trackReturnEnabled.ToString();
            data[13] = "SafeReturn=" + trackSafeReturn.ToString();
            data[14] = "ReturnResources=" + trackReturnResources;
            data[15] = "Port=" + trackPort.ToString();
            data[16] = "DockingDistance=0.15";
        }

        int trackWinMode = -1;
        /// <summary>
        /// GUI Tracking
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUITracking(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 200, 30));
            GUILayout.BeginVertical();

            if (!trackingActive && RmmUtil.IsPreLaunch(vessel))
            {
                if (trackWinMode != 0)
                {
                    windowPosGUITracking = new Rect(200, 200, 300, 100);
                    trackWinMode = 0;
                }
                createMissionForm();
            }
            else if (trackingActive)
            {
                if (trackWinMode != 1)
                {
                    windowPosGUITracking = new Rect(0, 400, 200, 50);
                    trackWinMode = 1;
                }
                GUILayout.BeginHorizontal();
                GUILayout.Label("tracking", RmmStyle.Instance.LabelStyle, GUILayout.Width(60));
                if (GUILayout.Button("stop tracking", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
                {
                    trackAbort();
                }
                GUILayout.EndHorizontal();
            }
            else if (returnMission && !trackingActive && RmmUtil.IsDocked(vessel, part))
            {
                if (trackWinMode != 2)
                {
                    windowPosGUITracking = new Rect(200, 200, 300, 50);
                    trackWinMode = 2;
                }
                returnMissionForm();
            }
            else
            {
                CloseGUITracking();
            }

            GUILayout.EndVertical();
        }

        private void createMissionForm()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mission Name", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            trackStrName = GUILayout.TextField(trackStrName, 20, RmmStyle.Instance.TextFieldStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Vehicle:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            trackStrVehicleName = GUILayout.TextField(trackStrVehicleName, 50, RmmStyle.Instance.TextFieldStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Mission", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
            {
                //startMission();
            }
            if (GUILayout.Button("Cancel", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
            {
                CloseGUITracking();
            }
            GUILayout.EndHorizontal();
        }


        private void returnMissionForm()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Return Mission", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
            {
                startReturnMission();
            }
            if (GUILayout.Button("Cancel", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
            {
                CloseGUITracking();
            }
            GUILayout.EndHorizontal();
        }



        private void startReturnMission()
        {
            trackingActive = true;
            trackPort = 2;
            _partModule.nextLogicTime = Planetarium.GetUniversalTime();
            updateTrackingVars(vessel);
            updateNextLogicTime(vessel);
        }


        private void setOtherModules()
        {
            foreach (Part p in vessel.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "RMMModule")
                    {
                        RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        ComOffMod.trackMissionId = trackMissionId;
                    }
                }
            }
        }

        private void SetThisPortPrimary()
        {
            foreach (Part p in vessel.parts)
            {
                RMMModule rmmModule = p.Modules.OfType<RMMModule>().FirstOrDefault();
                if (rmmModule != null && rmmModule.trackMissionId == trackMissionId)
                {
                    rmmModule.trackingPrimary = false;
                    rmmModule.PortCode = "";
                }
            }
            trackingPrimary = true;
            _partModule.PortCode = "Mission Part";
        }




        private float CalculateVesselPrice(bool withCargo)
        {
            double price = 0.0f;

            string[] propellantResources = new string[0];

            if (trackReturnResources == "")
            {
                //RmmUtil.DetermineProppellantArray(vessel, ref propellantResources);
            }
            else
            {
                RmmUtil.GetProppellantArray(trackReturnResources, ref propellantResources);
            }
            //cost parts
            foreach (Part p in vessel.parts)
            {
                double missingResCost = 0;
                foreach (PartResource r in p.Resources)
                {
                    if (withCargo || propellantResources.Contains(r.info.name))
                    {
                        missingResCost = missingResCost + RmmUtil.Cost(r.info.name, (r.maxAmount - r.amount));
                    }
                    else
                    {
                        missingResCost = missingResCost + RmmUtil.Cost(r.info.name, r.maxAmount);
                    }
                }

                price = price + (p.partInfo.cost - missingResCost + part.GetModuleCosts(0));
            }
            return ((float)price);
        }

        private static int windowPosGUITrackingID = WindowHelper.NextWindowId("windowPosGUITracking");

        public void drawGUITracking()
        {
            windowPosGUITracking = ClickThruBlocker.GUILayoutWindow(windowPosGUITrackingID, windowPosGUITracking, WindowGUITracking, "Mission Tracking", RmmStyle.Instance.WindowStyle);
        }

        public void trackingEvent()
        {
            if (HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                if (trackingPrimary)
                {
                    trackingPrimary = false;
                    _partModule.PortCode = "";
                }
                else
                {
                    trackingPrimary = true;
                    _partModule.PortCode = "Mission Part";
                }

            }

            if (HighLogic.LoadedScene == GameScenes.FLIGHT) { OpenGUITracking(); }
        }

        public void OpenGUITracking()
        {
            if (RmmUtil.IsPreLaunch(vessel) && !trackingActive)
            {
                trackStrName = vessel.vesselName;
                trackStrVehicleName = vessel.vesselName;
                SetThisPortPrimary();
            }

            if (returnMission && !trackingActive)
            {
                SetThisPortPrimary();
            }

            renderGUITracking = true;
        }

        public void CloseGUITracking()
        {
            renderGUITracking = false;
        }

        public void AbortTracking()
        {
            trackAbort();
        }

        public Mission Mission
        {
            get { return _mission; }
        }
    }
}
