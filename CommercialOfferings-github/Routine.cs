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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using Contracts;
using CommercialOfferings.MissionData;
using ClickThroughFix;

namespace CommercialOfferings
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public partial class RMMModule : PartModule
    {
        List<Mission> OfferingsList = new List<Mission>();

        //current mission
        [KSPField(isPersistant = true, guiActive = false)]
        public bool missionUnderway = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public string missionFolderName = "";
        [KSPField(isPersistant = true, guiActive = false)]
        public float missionArrivalTime = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        public int missionCrewCount = 0;
        private Mission missionOffering = new Mission();
        [KSPField(isPersistant = true, guiActive = false)]
        public bool missionRepeat = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public int missionRepeatDelay = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        public string missionPreferedCrew = "";

        //values to save orbit
        [KSPField(isPersistant = true, guiActive = false)]
        public float SMAsave = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        public float ECCsave = 0;
        [KSPField(isPersistant = true, guiActive = false)]
        public float INCsave = 0;


        //arrival transaction 
        public double nextLogicTime = 0;
        public bool completeArrival = false;
        private int ArrivalStage = 0;
        Vessel transactionVessel = null;
        private string tempID = "";
        private uint missionFlightIDDockPart = 0;

        //Port Code
        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true, guiName = "Port Code", guiUnits = "")]
        public string PortCode = "";

        [KSPField(isPersistant = true, guiActive = false)]
        public bool OrderingEnabled = true;

        //GUI Main
        private bool renderGUIMain = false;
        private static Rect windowPosGUIMain = new Rect(200, 200, 200, 450);
        private Vector2 scrollPositionAvailableCommercialOfferings;
        public float windowGUIMainX = 1;
        public float windowGUIMainY = 1;
        public float windowGUIMainWidth = 10;

        Mission selectedOffering = new Mission();

        //GUI Offering
        private bool renderGUIOffering = false;
        private static Rect windowPosGUIOffering = new Rect(300, 100, 100, 100);
        Mission GUIOffering = new Mission();

        //GUI Order
        private bool renderGUIMission = false;
        private static Rect windowPosGUIMission = new Rect(500, 400, 300, 75);
        Mission GUIMission = new Mission();
        private int intCrewCount = 0;
        private string strCrewCount = "";
        //private string strGUIerrmess = "";

        //GUI Pref Crew
        private bool renderGUIPrefCrew = false;
        private static Rect windowPosGUIPrefCrew = new Rect(700, 200, 200, 600);
        List<ProtoCrewMember> preferredCrewList = new List<ProtoCrewMember>();
        private Vector2 scrollPositionPreferredCrew;
        private Vector2 scrollPositionAvailableCrew;

        //GUI Register Port
        private bool renderGUIRegister = false;
        private static Rect windowPosGUIRegister = new Rect(300, 300, 240, 100);
        public string StrPortCode = "";

        //commercialvehiclemode
        [KSPField(isPersistant = true, guiActive = false)]
        public bool commercialvehiclemode = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public bool vehicleAutoDepart = false;
        [KSPField(isPersistant = true, guiActive = false)]
        public string commercialvehicleFolderName = "";
        [KSPField(isPersistant = true, guiActive = false)]
        public float commercialvehiclePartCount = 0.0f;
        private Mission commercialvehicleOffering = new Mission();
        private bool commercialvehicleOfferingLoaded = false;





        private bool handleCommercialVehicleMode()
        {
            if (commercialvehiclemode && !commercialvehicleOfferingLoaded && commercialvehicleFolderName != "")
            {
                loadOfferings();

                foreach (Mission Off in OfferingsList)
                {
                    if (commercialvehicleFolderName == Off.FolderPath)
                    {
                        commercialvehicleOffering = Off;
                        commercialvehicleOfferingLoaded = true;
                        return false;
                    }
                }

                return (true);
            }

            if (commercialvehiclemode)
            {
                if (vehicleAutoDepart && !RmmUtil.IsDocked(vessel, part))
                {
                    if (!vessel.isActiveVessel)
                    {
                        handleAutoDepart(); return false;
                    }
                    else
                    {
                        foreach (Vessel ves in FlightGlobals.Vessels)
                        {
                            if (!ves.packed && ves.loaded && ves.id.ToString() != vessel.id.ToString())
                            {
                                FlightGlobals.SetActiveVessel(ves);
                                nextLogicTime = Planetarium.GetUniversalTime() + 1;
                                return false;
                            }
                        }
                        vehicleAutoDepart = false;
                        nextLogicTime = 0;
                        ScreenMessages.PostScreenMessage("no other asset in vicinity", 4, ScreenMessageStyle.UPPER_CENTER);
                    }
                }
            }
            return true;
        }


        private void handleArrivalCompletion()
        {
            if (ArrivalStage == 0)
            {
                if (missionUnderway && missionArrivalTime < Planetarium.GetUniversalTime())
                {
                    if (!otherModulesCompletingArrival())
                    {
                        ArrivalStage = 1;
                        completeArrival = true;
                        nextLogicTime = Planetarium.GetUniversalTime();
                    }
                    else
                    {
                        ArrivalStage = 0;
                        nextLogicTime = Planetarium.GetUniversalTime() + 1.25;
                    }
                }
                else
                {
                    ArrivalStage = -1;
                    nextLogicTime = 0;
                }
            }
            if (completeArrival)
            {
                switch (ArrivalStage)
                {
                    case 1:
                        dockStage1();
                        break;
                    case 2:
                        dockStage2();
                        break;
                    case 3:
                        dockStage3();
                        break;
                    case 4:
                        if (transactionVessel == null || vessel.packed || !vessel.loaded || transactionVessel.packed || !transactionVessel.loaded)
                        {
                            nextLogicTime = Planetarium.GetUniversalTime();
                            return;
                        }
                        dockStage4();
                        break;
                    case 5:
                        dockStage5();
                        break;
                }
            }
        }

        private void dockStage1()
        {
            loadOfferings();

            foreach (Mission Off in OfferingsList)
            {
                if (missionFolderName == Off.FolderPath)
                {
                    missionOffering = Off;
                }
            }
            if (missionOffering == null) { abortArrival(); return; }

            //load Offering of current mission

            if (offeringAllowed(missionOffering) && crewAvailable(missionOffering))
            {
                nextLogicTime = Planetarium.GetUniversalTime();
                ArrivalStage = 2;
            }
            else
            {
                Funding.Instance.AddFunds(missionOffering.Price, TransactionReasons.VesselRecovery);
                missionUnderway = false;
                completeArrival = false;

                nextLogicTime = 0;
                ArrivalStage = -1;
            }
        }

        private void dockStage2()
        {
            RmmUtil.ToMapView();
            ProtoVessel ProtoFlightVessel = loadVessel(missionFolderName);
            if (ProtoFlightVessel == null) { abortArrival(); return; }
            if (loadVesselForRendezvous(ProtoFlightVessel, vessel))
            {
                nextLogicTime = Planetarium.GetUniversalTime();
                ArrivalStage = 3;
            }
        }

        private void dockStage3()
        {
            //search for the vessel for five seconds, else abort
            if (nextLogicTime < (Planetarium.GetUniversalTime() - 5)) { logreport(); abortArrival(); return; }

            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == tempID)
                {
                    transactionVessel = ve;
                    transactionVessel.vesselName = missionOffering.VehicleName;
                    placeVesselForRendezvous(transactionVessel, vessel);
                    nextLogicTime = Planetarium.GetUniversalTime();
                    ArrivalStage = 4;
                    return;
                }
            }
        }

        private void logreport()
        {
            print("i'm at stage " + ArrivalStage + " and can not find vessel " + tempID + ". the vessels i can find are:");
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                print(ve.vesselName);
            }
            print("--test run");
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == tempID)
                {
                    print("test 1");
                    transactionVessel = ve;
                    print("test 2");
                    transactionVessel.vesselName = missionOffering.VehicleName;
                    print("test 3");
                    placeVesselForRendezvous(transactionVessel, vessel);
                    print("test 4");
                    nextLogicTime = Planetarium.GetUniversalTime();
                    ArrivalStage = 4;
                    return;
                }
            }
        }


        private void dockStage4()
        {
            RmmUtil.ToMapView();
            Part placePort = new Part();

            // int portNumber = 0;
            foreach (Part p in transactionVessel.parts)
            {
                if (p.flightID == missionFlightIDDockPart)
                {
                    placePort = p;
                }
                foreach (PartModule pm in p.Modules)
                {

                    //if (pm.GetType() == typeof(ModuleDockingNode))
                    //{
                    //    RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                    //    if (ComOffMod.trackingPrimary == true)
                    //    {
                    //        placePort = p;
                    //        if (missionOffering.ReturnEnabled)
                    //        {
                    //            ComOffMod.commercialvehiclemode = true;
                    //            ComOffMod.commercialvehicleFolderName = missionOffering.FolderPath;
                    //            ComOffMod.commercialvehiclePartCount = (float)RmmUtil.CountVesselParts(transactionVessel);
                    //            ComOffMod.trackingPrimary = false;
                    //        }
                    //    }
                    //    portNumber = portNumber + 1;
                    //
                    //    ComOffMod.trackingActive = false;
                    //    ComOffMod.returnMission = false;
                    //    ComOffMod.trackMissionId = "";
                    //    ComOffMod.PortCode = "";
                    //}

                    // empty all science
                    if (pm.GetType() == typeof(ModuleScienceContainer))
                    {
                        ModuleScienceContainer moduleScienceContainer = (ModuleScienceContainer)pm;
                        var scienceDatas  = moduleScienceContainer.GetData();
                        for (int i = 0; i < scienceDatas.Count(); i++)
                        {
                            moduleScienceContainer.RemoveData(scienceDatas[i]);
                        } 
                    }
                }
            }

            transactionVessel.targetObject = null;


            handleLoadCrew(transactionVessel, missionCrewCount, missionOffering.MinimumCrew);

            RmmContract.HandleContracts(transactionVessel, true, false);
            
            if (!RmmUtil.IsDocked(vessel, part) && checkDockingPortCompatibility(placePort, part))
            {
                placeVesselForDock(transactionVessel, placePort, vessel, part, RmmUtil.GetDockingDistance(placePort));

                nextLogicTime = Planetarium.GetUniversalTime();
                ArrivalStage = 5;
            }
            else
            {
                 ScreenMessages.PostScreenMessage(missionOffering.VehicleName + " rendezvoused", 4, ScreenMessageStyle.UPPER_CENTER);
                 finishArrival();
            }
        }

        private void dockStage5()
        {
            RmmUtil.ToMapView();
            if (RmmUtil.IsDocked(vessel, part)
                || (nextLogicTime < (Planetarium.GetUniversalTime() - 8)))
            {
                ScreenMessages.PostScreenMessage(missionOffering.VehicleName + " docked", 4, ScreenMessageStyle.UPPER_CENTER);

                finishArrival();
            }
        }

        private ProtoVessel loadVessel(string folderName)
        {
            ConfigNode loadnode = null;
            if (!File.Exists(RmmUtil.GamePath + folderName + "/vesselfile")) { abortArrival(); return null; }
            loadnode = ConfigNode.Load(RmmUtil.GamePath + folderName + "/vesselfile");
            if (loadnode == null) { abortArrival(); return null; }
            ProtoVessel loadprotovessel = new ProtoVessel(loadnode, HighLogic.CurrentGame);
            return loadprotovessel;
        }

        private bool loadVesselForRendezvous(ProtoVessel placeVessel, Vessel targetVessel)
        {
            targetVessel.BackupVessel();

            placeVessel.orbitSnapShot = targetVessel.protoVessel.orbitSnapShot;

            placeVessel.orbitSnapShot.epoch = 0.0;

            tempID = RmmUtil.Rand.Next(1000000000).ToString();
            //rename any vessels present with "AdministativeDockingName"
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == tempID)
                {
                    Vessel NameVessel = null;
                    NameVessel = ve;
                    NameVessel.vesselName = "1";
                }
            }

            placeVessel.vesselID = Guid.NewGuid(); 

            placeVessel.vesselName = tempID;

            foreach (ProtoPartSnapshot p in placeVessel.protoPartSnapshots)
            {
                uint newFlightID = (UInt32)RmmUtil.Rand.Next(1000000000, 2147483647);

                // save flight ID of part which should dock
                if (p.flightID == missionOffering.flightIDDockPart)
                {
                    missionFlightIDDockPart = newFlightID;
                }

                // update flight ID of each part and vessel reference transform id.
                if (placeVessel.refTransform == p.flightID)
                {
                    p.flightID = newFlightID;
                    placeVessel.refTransform = p.flightID;
                }
                else
                {
                    p.flightID = newFlightID;
                }

                // clear out all crew 
                if (p.protoModuleCrew != null && p.protoModuleCrew.Count() != 0)
                {
                    List<ProtoCrewMember> cl = p.protoModuleCrew;
                    List<ProtoCrewMember> clc = new List<ProtoCrewMember>(cl);

                    foreach (ProtoCrewMember c in clc)
                    {
                        p.RemoveCrew(c);
                    }
                }

                foreach (ProtoPartModuleSnapshot pm in p.modules)
                {
                    if (pm.moduleName == "RMMModule")
                    {
                        pm.moduleValues.SetValue("trackingPrimary", false);
                        pm.moduleValues.SetValue("trackingActive", false);
                    }
                }
            }

            try
            {
                placeVessel.Load(HighLogic.CurrentGame.flightState);
                return true;
            }
            catch
            {
                return true;
            }
        }

        private void placeVesselForRendezvous(Vessel placeVessel, Vessel targetVessel)
        {
            Vector3d offset = new Vector3d();

            if (!determineRendezvousOffset(placeVessel, targetVessel, ref offset)) 
            {
                completeArrival = false;
                nextLogicTime = 0;
                ArrivalStage = -1;
                return;
            }

            placeVessel.orbit.UpdateFromStateVectors(targetVessel.orbit.pos + offset, targetVessel.orbit.vel, targetVessel.orbit.referenceBody, Planetarium.GetUniversalTime());
        }

        private bool determineRendezvousOffset(Vessel placeVessel, Vessel targetVessel, ref Vector3d offset)
        {
            int rendezvousDistance = vesselScale(placeVessel);

            //determine max scale of al vessels involved
            foreach (Vessel ves in FlightGlobals.Vessels)
            {
                if (!ves.packed && ves.loaded)
                {
                    int scale = vesselScale(ves);
                    if (scale > rendezvousDistance)
                    {
                        rendezvousDistance = scale;
                    }
                }
            }

            bool good;
            int attempts = 0;

            do
            {
                good = true;
                attempts = attempts + 1;
                if (rendezvousDistance > 115) { rendezvousDistance = 115; }
                if (rendezvousDistance <= 0) { rendezvousDistance = 100; }

            
                ///make a random offset
                double x = 0;
                double y = 0;
                double z = 0;
                int ra = RmmUtil.Rand.Next(3);
                switch (ra)
                {
                    case 0:
                        x = (double)rendezvousDistance;
                        y = (double)RmmUtil.Rand.Next(rendezvousDistance);
                        z = (double)RmmUtil.Rand.Next(rendezvousDistance);
                        break;
                    case 1:
                        x = (double)RmmUtil.Rand.Next(rendezvousDistance);
                        y = (double)rendezvousDistance;
                        z = (double)RmmUtil.Rand.Next(rendezvousDistance);
                        break;
                    case 2:
                        x = (double)RmmUtil.Rand.Next(rendezvousDistance);
                        y = (double)RmmUtil.Rand.Next(rendezvousDistance);
                        z = (double)rendezvousDistance;
                        break;
                }
                if (RmmUtil.Rand.Next(0, 2) == 1)
                {
                    x = x * -1;
                }
                if (RmmUtil.Rand.Next(0, 2) == 1)
                {
                    y = y * -1;
                }
                if (RmmUtil.Rand.Next(0, 2) == 1)
                {
                    z = z * -1;
                }

                offset.x = x;
                offset.y = y;
                offset.z = z;


                // check if offset is far enough from all vessels in area
                foreach (Vessel ves in FlightGlobals.Vessels)
                {
                    if (!ves.packed && ves.loaded && ves.id != placeVessel.id )
                    {
                        
                        var dist = Vector3.Distance(ves.orbit.pos, targetVessel.orbit.pos + offset);
                        if (dist < rendezvousDistance)
                        {
                            good = false;
                        }
                    }
                }

            } while(!good && attempts < 100);

            return (good);
        }

        private bool checkDockingPortCompatibility(Part placePort, Part targetPort)
        {
            ModuleDockingNode placeDockingNode = placePort.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
            ModuleDockingNode targetDockingNode = targetPort.Modules.OfType<ModuleDockingNode>().FirstOrDefault();

            return (placeDockingNode.nodeType == targetDockingNode.nodeType);
        }

        private int vesselScale(Vessel ves)
        {
            float longestdistance = 0f;
            foreach (Part p in ves.parts)
            {
                if (Math.Abs(p.orgPos[0] - ves.rootPart.orgPos[0]) > longestdistance) { longestdistance = Math.Abs(p.orgPos[0] - ves.rootPart.orgPos[0]); }
                if (Math.Abs(p.orgPos[1] - ves.rootPart.orgPos[1]) > longestdistance) { longestdistance = Math.Abs(p.orgPos[1] - ves.rootPart.orgPos[1]); }
                if (Math.Abs(p.orgPos[2] - ves.rootPart.orgPos[2]) > longestdistance) { longestdistance = Math.Abs(p.orgPos[2] - ves.rootPart.orgPos[2]); }
            }
            return (((int)(longestdistance * 4 + 10)));
        }

        private void placeVesselForDock(Vessel placeVessel, Part placePort, Vessel targetVessel, Part targetPort, float distanceFactor)
        {

            ModuleDockingNode placeDockingNode = placePort.Modules.OfType<ModuleDockingNode>().FirstOrDefault();

            ModuleDockingNode targetDockingNode = targetPort.Modules.OfType<ModuleDockingNode>().FirstOrDefault();



            QuaternionD placeVesselRotation = targetDockingNode.controlTransform.rotation * Quaternion.Euler(180f, (float)RmmUtil.Rand.Next(0, 360), 0);
            placeVessel.SetRotation(placeVesselRotation);

            placeVessel.SetRotation(placeDockingNode.controlTransform.rotation * Quaternion.Euler(0, (float)RmmUtil.Rand.Next(0, 360), 0));


            float anglePlaceDock = angleNormal(placeDockingNode.nodeTransform.forward.normalized, placeVessel.vesselTransform.forward.normalized, placeVessel.vesselTransform.up.normalized);
            float angleTargetDock = angleNormal(-targetDockingNode.nodeTransform.forward.normalized, placeVessel.vesselTransform.forward.normalized, placeVessel.vesselTransform.up.normalized);


            placeVessel.SetRotation(placeVessel.vesselTransform.rotation * Quaternion.Euler(0, (anglePlaceDock - angleTargetDock), 0));

            //position vessel
            Vector3d placePortLocation = targetDockingNode.nodeTransform.position + (targetDockingNode.nodeTransform.forward.normalized * distanceFactor);

            Vector3d placeVesselPosition = placePortLocation + (placeVessel.vesselTransform.position - placeDockingNode.nodeTransform.position);

            placeVessel.SetPosition(placeVesselPosition);

        }


        //Thanks to NavyFish's Docking Port Alignment Indicator for showing how to calculate an angle 
        private float angleNormal(Vector3 measure, Vector3 reference, Vector3 axis)
        {
            //in contrast to NavyFish's Docking Port Alignment Indicator we need a 0-360 value
            return (angleSigned(Vector3.Cross(measure, axis), Vector3.Cross(reference, axis), axis) + 180);
        }

        //-180 to 180 angle
        private float angleSigned(Vector3 measureAngle, Vector3 referenceAngle, Vector3 axis)
        {
            if (Vector3.Dot(Vector3.Cross(measureAngle, referenceAngle), axis) < 0) //greater than 90 i.e v1 left of v2
                return -Vector3.Angle(measureAngle, referenceAngle);
            return Vector3.Angle(measureAngle, referenceAngle);
        }

        private void finishArrival()
        {
            missionUnderway = false;
            completeArrival = false;

            nextLogicTime = 0;
            ArrivalStage = -1;

            if (missionRepeat)
            {
                procureOffering(missionOffering, true);
            }
        }

        private void abortArrival()
        {
            missionUnderway = false;
            completeArrival = false;
            missionRepeat = false;

            nextLogicTime = 0;
            ArrivalStage = -1;
        }

        //Thanks to sarbian's Kerbal Crew Manifest for showing all this crew handling stuff
        private void handleLoadCrew(Vessel ves, int crewCount, int minCrew)
        {
            if (ves.GetCrewCapacity() < crewCount)
                crewCount = ves.GetCrewCapacity();

            string[] prefCrewNames = new string[0];
            getPreferredCrewNames (ref prefCrewNames);

            foreach (Part p in ves.parts)
            {
                if (p.CrewCapacity > p.protoModuleCrew.Count)
                {
                    for (int i = 0; i < p.CrewCapacity && crewCount > 0; i++)
                    {
                        bool added = false;

                        //tourist
                        if (minCrew <= 0)
                        {
                            foreach (String name in prefCrewNames)
                            {
                                if (!added)
                                {
                                    foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Tourist)
                                    {
                                        if (name == cr.name && cr.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                                        {
                                            if (AddCrew(p, cr))
                                            {
                                                crewCount = crewCount - 1;
                                                added = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //preferred crew
                        foreach (String name in prefCrewNames)
                        {
                            if (!added)
                            {
                                foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
                                {
                                    if (name == cr.name && cr.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                                    {
                                        if (AddCrew(p, cr))
                                        {
                                            crewCount = crewCount - 1;
                                            minCrew = minCrew - 1;
                                            added = true;

                                        }
                                    }
                                }
                            }
                        }
                        //next crew or new crew
                        //print("one crew start");
                        //print("crew" + kerbal.name);
                        if (!added)
                        {
                            ProtoCrewMember crew = null;
                            crew = HighLogic.CurrentGame.CrewRoster.GetNextAvailableKerbal();
                            if (crew != null)
                            {
                                if (AddCrew(p, crew))
                                {
                                    crewCount = crewCount - 1;
                                    minCrew = minCrew - 1;
                                    added = true;
                                }
                            }
                        }
                    }
                }
            }
            ves.SpawnCrew();
        }

        private bool crewAvailable(Mission Off)
        {
            int availableCrew = 0;
            foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (cr.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                {
                    availableCrew++;
                }
            }

            if (availableCrew < Off.MinimumCrew)
            {
                ScreenMessages.PostScreenMessage("not enough crew was available for mission", 4, ScreenMessageStyle.UPPER_CENTER);
                return false;
            }

            return true;
        }

        private bool AddCrew(Part p, ProtoCrewMember kerbal)
        {
            p.AddCrewmember(kerbal);

            kerbal.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;

            if (kerbal.seat != null)
                kerbal.seat.SpawnCrew();

            return (true);
        }

        private void handleUnloadCrew(Vessel ves, bool savereturn)
        {
            foreach (Part p in ves.parts)
            {
                if (p.CrewCapacity > 0 && p.protoModuleCrew.Count > 0)
                {
                    for (int i = p.protoModuleCrew.Count - 1; i >= 0; i--)
                    {
                        unloadCrew(p.protoModuleCrew[i], p, savereturn);
                    }
                }
            }
            ves.DespawnCrew();
        }

        private void unloadCrew(ProtoCrewMember crew, Part p, bool savereturn)
        {
            p.RemoveCrewmember(crew);

            if (savereturn)
            {
                crew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
            }
            else
            {
                if (HighLogic.CurrentGame.Parameters.Difficulty.MissingCrewsRespawn)
                {
                    crew.rosterStatus = ProtoCrewMember.RosterStatus.Missing;
                }
                else
                {
                    crew.rosterStatus = ProtoCrewMember.RosterStatus.Dead;
                }
            }
        }

        private bool staticorbit()
        {
            //Determine deviation between saved values and current values if they don't changed to much update the staticTime and return staticTime if not return zero.
            //the parameter AnomalyAtEpoch changes over time and must be excluded from analysis

            float MaxDeviationValue = 10;

            bool[] parameters = new bool[5];
            parameters[0] = false;
            parameters[1] = false;
            parameters[2] = false;

            //lenght
            if (MaxDeviationValue / 100 > Math.Abs(((vessel.orbit.semiMajorAxis - SMAsave) / SMAsave))) { parameters[0] = true; }
            //ratio
            if (MaxDeviationValue / 100 > Math.Abs(vessel.orbit.eccentricity - ECCsave)) { parameters[1] = true; }

            float angleD = MaxDeviationValue;
            //angle
            if (angleD > Math.Abs(vessel.orbit.inclination - INCsave) || angleD > Math.Abs(Math.Abs(vessel.orbit.inclination - INCsave) - 360)) { parameters[2] = true; }

            if (parameters[0] == false || parameters[1] == false || parameters[2] == false)
            {
                return (false);
            }
            else
            {
                return (true);
            }
        }






        private void handleAutoDepart()
        {
            if (autoDepartAllowed(commercialvehicleOffering)
                && vesselClean(vessel)
                && returnResourcesAvailable(commercialvehicleOffering)
                && minimalCrewPresent(commercialvehicleOffering))
            {
                RmmUtil.ToMapView();

                RmmContract.HandleContracts(vessel, false, true);

                if (commercialvehicleOffering.SafeReturn && HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    Funding.Instance.AddFunds(commercialvehicleOffering.VehicleReturnFee + cargoFee(), TransactionReasons.VesselRecovery);
                }

                handleUnloadCrew(vessel, commercialvehicleOffering.SafeReturn);
                vessel.Unload();
                vessel.Die();

                ScreenMessages.PostScreenMessage(commercialvehicleOffering.VehicleName + " returned to " + RmmUtil.HomeBodyName(), 4, ScreenMessageStyle.UPPER_CENTER);
            }
            else
            {
                vehicleAutoDepart = false;
            }
        }

        private double cargoFee()
        {
            double fee = 0.0;
            
            if (commercialvehicleOffering.ReturnCargoMass == 0) { return 0; }

            double cargoMass = commercialvehicleOffering.ReturnCargoMass;
            string[] cargoArray = new string[0];
            //RmmUtil.GetCargoArray(vessel, commercialvehicleOffering.ReturnResources, ref cargoArray);

            orderCargoArray(ref cargoArray);

            foreach (String s in cargoArray)
            {
                foreach (Part p in vessel.parts)
                {
                    foreach (PartResource r in p.Resources)
                    {
                        if (r.info.name == s)
                        {
                            if (r.amount != 0)
                            {
                                if (RmmUtil.Mass(r.info.name, r.amount) <= cargoMass)
                                {
                                    fee = fee + RmmUtil.Cost(r.info.name, r.amount);
                                    cargoMass = cargoMass - RmmUtil.Mass(r.info.name, r.amount);
                                }
                                else
                                {
                                    fee = fee + ((cargoMass / RmmUtil.Mass(r.info.name, r.amount)) * RmmUtil.Cost(r.info.name, r.amount));
                                    return fee;
                                }
                            }
                        }
                    }
                }
            }
            return fee;
        }


        private void orderCargoArray(ref string[] cargoArray)
        {
            string[] unorderCargoArray = new string[cargoArray.Length];
            double[] costPerMass = new double[cargoArray.Length];

            for (int i = 0; i < cargoArray.Length; i++)
            {
                unorderCargoArray[i] = cargoArray[i];
                PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(cargoArray[i]);
                costPerMass[i] = prd.unitCost / prd.density;
            }

            for (int u = 0; u < cargoArray.Length; u++)
            {
                int highestCargoResource = -1;
           
                for (int i = 0; i < cargoArray.Length; i++)
                {
                    if (unorderCargoArray[i] != "")
                    {
                        if (highestCargoResource != -1)
                        {
                            if (costPerMass[i] > costPerMass[highestCargoResource])
                                highestCargoResource = i;
                        }
                        else
                        {
                            highestCargoResource = i;
                        }
                    }
                }

                if (highestCargoResource != -1)
                {
                    cargoArray[u] = unorderCargoArray[highestCargoResource];
                    unorderCargoArray[highestCargoResource] = "";
                }
            }
        }


        private bool otherModulesCompletingArrival()
        {
            foreach (Part p in vessel.parts)
            {
                if (p.flightID != part.flightID)
                {
                    foreach (PartModule pm in p.Modules)
                    {
                        if (pm.ClassName == "RMMModule")
                        {
                            RMMModule otherComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                            if (otherComOffMod.completeArrival) { return (true); }
                        }
                    }
                }
            }
            return (false);
        }

        private bool autoDepartAllowed(Mission Off)
        {
            if (offeringAllowed(Off))
            {
                return (true);
            }
            else
            {
                ScreenMessages.PostScreenMessage("not rated for this orbit", 4, ScreenMessageStyle.UPPER_CENTER);
                return (false);
            }
        }

        private bool vesselClean(Vessel ves)
        {
            if (commercialvehiclePartCount >= RmmUtil.CountVesselParts(ves))
            {
                return (true);
            }
            else
            {
                ScreenMessages.PostScreenMessage("vessel in unknown configuration for return", 4, ScreenMessageStyle.UPPER_CENTER);
                return (false);
            }

        }

        private bool minimalCrewPresent(Mission Off)
        {
            if (Off.MinimumCrew == 0) { return (true); }
            if (Off.MinimumCrew > RmmUtil.AstronautCrewCount(vessel)) { ScreenMessages.PostScreenMessage("not enough crew for return", 4, ScreenMessageStyle.UPPER_CENTER); return (false); }
            if (!Off.SafeReturn && RmmUtil.CrewCount(vessel) > 0) { ScreenMessages.PostScreenMessage("not rated for safe crew return", 4, ScreenMessageStyle.UPPER_CENTER); return (false); }
            return (true);
        }

        private bool returnResourcesAvailable(Mission Off)
        {
            if (Off.ReturnResources == "") { return (true); }

            string[] SplitArray = Off.ReturnResources.Split(',');

            string[] arrResource = new string[0];
            //RmmUtil.DetermineProppellantArray(vessel, ref arrResource);

            foreach (String st in SplitArray)
            {
                string[] SplatArray = st.Split(':');
                string resourceName = SplatArray[0].Trim();
                double amount = Convert.ToDouble(SplatArray[1].Trim());

                if (!arrResource.Contains(resourceName)) { ScreenMessages.PostScreenMessage("vessel in unknown configuration for return", 4, ScreenMessageStyle.UPPER_CENTER); return (false); }

                if (amount * 0.99 > RmmUtil.ReadResource(vessel, resourceName)) { ScreenMessages.PostScreenMessage("not enough resources for return", 4, ScreenMessageStyle.UPPER_CENTER); return (false); }
            }

            return (true);
        }

        private bool returnCargoMassNotExceeded(Mission Off)
        {
            //if (Off.ReturnCargoMass == 0.0 || RmmUtil.GetCargoMass(vessel, Off.ReturnResources) <= Off.ReturnCargoMass * 1.1)
            //{
            //    return true;
            //}
            //else
            //{
            //    ScreenMessages.PostScreenMessage("cargo mass exceeds rated amount", 4, ScreenMessageStyle.UPPER_CENTER); 
            //    return false;
            //}
            return true;
        }





        /// <summary>
        /// GUI Main
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUIMain(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 200, 30));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Current:", RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            if (GUILayout.Button(selectedOffering.Name, RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
            {
                GUIOffering = selectedOffering;
                intCrewCount = missionCrewCount;
                openGUIOffering();
            }
            if (missionUnderway)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Underway", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                if (GUILayout.Button("Cancel", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
                {
                    if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                    {
                        if (Planetarium.GetUniversalTime() < missionArrivalTime - selectedOffering.Time + 3600)
                        {
                            if (Funding.Instance.Funds > GUIOffering.Price)
                            {
                                Funding.Instance.AddFunds(GUIOffering.Price, TransactionReasons.VesselRecovery);
                            }
                        }
                    }
                    missionUnderway = false;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ETA:", RmmStyle.Instance.LabelStyle, GUILayout.Width(35));
                GUILayout.Label(timeETAString(missionArrivalTime - Planetarium.GetUniversalTime()), RmmStyle.Instance.LabelStyle, GUILayout.Width(165));
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            }
            GUILayout.EndVertical();

            GUILayout.Label("  ", RmmStyle.Instance.LabelStyle, GUILayout.Width(10));

            GUILayout.BeginVertical();
            missionRepeat = GUILayout.Toggle(missionRepeat, "Repeat");
            GUILayout.Label("Repeat Delay:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            GUILayout.BeginHorizontal();
                        
            if (GUILayout.Button("<", RmmStyle.Instance.ButtonStyle, GUILayout.Width(15), GUILayout.Height(15))) 
            {
                if (missionRepeatDelay >= 1) { missionRepeatDelay -= (missionRepeatDelay / 10 > 1 ? missionRepeatDelay/10 : 1); }
            }
            GUILayout.Label( missionRepeatDelay.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(25));
            if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(15), GUILayout.Height(15)))
            {
                if (missionRepeatDelay <= 999) { missionRepeatDelay += (missionRepeatDelay / 10 > 1 ? missionRepeatDelay / 10 : 1); }
                if (missionRepeatDelay > 999) { missionRepeatDelay = 999; }
            }
            GUILayout.Label("days", RmmStyle.Instance.LabelStyle, GUILayout.Width(30));
                        
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            scrollPositionAvailableCommercialOfferings = GUILayout.BeginScrollView(scrollPositionAvailableCommercialOfferings, false, true, GUILayout.Width(200), GUILayout.Height(300));
            foreach (Mission Off in OfferingsList)
            {
                if (GUILayout.Button(Off.Name, RmmStyle.Instance.ButtonStyle, GUILayout.Width(165), GUILayout.Height(22)))
                {
                    GUIOffering = Off;
                    if (GUIOffering.MinimumCrew > 0)
                    {
                        if (GUIOffering.MinimumCrew < GUIOffering.MaximumCrew)
                        {
                            intCrewCount = GUIOffering.MaximumCrew;
                        }
                        else
                        {
                            intCrewCount = GUIOffering.MinimumCrew;
                        }
                    }
                    else if (GUIOffering.MinimumCrew < GUIOffering.MaximumCrew)
                    {
                        intCrewCount = GUIOffering.MaximumCrew;
                    }
                    else if (GUIOffering.MinimumCrew == 0 && GUIOffering.MaximumCrew == 0)
                    {
                        intCrewCount = 0;
                    }

                    openGUIOffering();
                }
            }
            GUILayout.EndScrollView();

            //development mode buttons
            if (DevMode)
            {
                if (GUILayout.Button("Docking Stage 2"))
                {
                    dockStage2();
                }

                if (GUILayout.Button("Docking Stage 3"))
                {
                    dockStage3();
                }
                if (GUILayout.Button("Save vessel"))
                {
                    ConfigNode savenode = new ConfigNode();
                    vessel.BackupVessel();
                    vessel.protoVessel.Save(savenode);
                    savenode.Save(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + Path.DirectorySeparatorChar + "Missions" + Path.DirectorySeparatorChar + "DevMode" + Path.DirectorySeparatorChar + "vesselfile");
                }
            }
            //

            if (GUILayout.Button("Close", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
            {
                closeGUIMain();
            }
            GUILayout.EndVertical();
        }

//      [KSPEvent(name = "ordering", isDefault = false, guiActive = true, guiName = "Available Missions")]
        public void ordering()
        {
            openGUIMain();
        }

        private void drawGUIMain()
        {
            windowPosGUIMain.xMin = windowGUIMainX;
            windowPosGUIMain.yMin = windowGUIMainY;
            windowPosGUIMain.width = windowGUIMainWidth;
            windowPosGUIMain.height = 450;
            windowPosGUIMain = ClickThruBlocker.GUILayoutWindow(3404, windowPosGUIMain, WindowGUIMain, "Docking Port " + PortCode, RmmStyle.Instance.WindowStyle);
        }

        public void openGUIMain()
        {
            loadOfferings();

            renderGUIMain = true;
        }

        public void closeGUIMain()
        {
            renderGUIMain = false;
        }

        public void loadOfferings()
        {
            OfferingsList.Clear();

            loadOfferingsDirectory(RmmUtil.GamePath + Path.DirectorySeparatorChar + "GameData");

            if (missionFolderName != "")
            {
                foreach (Mission Off in OfferingsList)
                {
                    if (Off.FolderPath == missionFolderName)
                    {
                        selectedOffering = Off;
                    }
                }
            }
        }

        private void loadOfferingsDirectory(string searchDirectory)
        {

            if (File.Exists(searchDirectory + Path.DirectorySeparatorChar + "CommercialOfferingsPackMarkerFile"))
            {
                string[] directoryOfferings = Directory.GetDirectories(searchDirectory);

                foreach (String dir in directoryOfferings)
                {
                    if (File.Exists(dir + Path.DirectorySeparatorChar + Mission.MISSION_FILE))
                    {
                        string folderPath = dir.Substring(RmmUtil.GamePath.ToString().Length, dir.Length - RmmUtil.GamePath.ToString().Length);
                        Mission Off = Mission.GetMissionByPath(folderPath);

                        if (offeringAllowed(Off))
                        {
                            OfferingsList.Add(Off);
                        }
                    }
                }
            }
            else
            {
                string[] searchDirectories = Directory.GetDirectories(searchDirectory);

                foreach (String dir in searchDirectories)
                {
                    loadOfferingsDirectory(dir);
                }
            }
        }

        private bool isCurrentCampaign(Mission off)
        {
            if (isACampaign(off))
            {
                if (off.CompanyName.Length >= 14 && off.CompanyName.Substring(14) == HighLogic.SaveFolder)
                {
                    return (true);
                }
            }
            else
            {
                return (true);
            }
            return (false);
        }


        private bool isACampaign(Mission off)
        {
            if (off.CompanyName.Length >= 14 && off.CompanyName.Substring(0, 14) == "KSPCAMPAIGN:::")
                return (true);
            return (false);
        }

        /// <summary>
        /// GUI Offering
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUIOffering(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 500, 30));
            GUILayout.BeginVertical();

            if (!isACampaign(GUIOffering))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Company:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(GUIOffering.CompanyName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Vehicle:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            GUILayout.Label(GUIOffering.VehicleName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Launch System:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            GUILayout.Label(GUIOffering.LaunchSystemName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Price:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            GUILayout.Label(GUIOffering.Price.ToString() + ((GUIOffering.VehicleReturnFee > 0) ? " (" + (GUIOffering.Price - GUIOffering.VehicleReturnFee).ToString() + " with vehicle return)" : ""), RmmStyle.Instance.LabelStyle, GUILayout.Width(250));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Arrival in:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            GUILayout.Label(timeString(GUIOffering.Time), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();
            if (GUIOffering.MinimumCrew > 0)
            {
                if (GUIOffering.MinimumCrew < GUIOffering.MaximumCrew)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Minimal crew required:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label(GUIOffering.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Maximum crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label(GUIOffering.MaximumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(GUIOffering.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }
            }
            else if (GUIOffering.MinimumCrew < GUIOffering.MaximumCrew)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(GUIOffering.MaximumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }
            else if (GUIOffering.MinimumCrew == 0 && GUIOffering.MaximumCrew == 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Unmanned", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.EndHorizontal();
            }
            if (!GUIOffering.ReturnEnabled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("No return mission", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
            else if (!GUIOffering.SafeReturn)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("No safe return mission", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
            if (GUIOffering.ReturnResources != "")
            {
                GUILayout.Label("Required return resources: ", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                string[] SplitArray = GUIOffering.ReturnResources.Split(',');
                foreach (String st in SplitArray)
                {
                    string[] SplatArray = st.Split(':');
                    string resourceName = SplatArray[0].Trim();
                    double amount = Convert.ToDouble(SplatArray[1].Trim());
                    GUILayout.Label("   " + resourceName + ": " + RoundToSignificantDigits(amount,4).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                }
            }

            if (GUIOffering.ReturnCargoMass != 0.0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Return cargo mass: " + RoundToSignificantDigits(GUIOffering.ReturnCargoMass,3).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("", RmmStyle.Instance.RedLabelStyle, GUILayout.Width(430));
            if (GUILayout.Button("Delete", RmmStyle.Instance.ButtonStyle, GUILayout.Width(70), GUILayout.Height(22)))
            {
                if (Directory.Exists(RmmUtil.GamePath + GUIOffering.FolderPath))
                {
                    DeleteDirectory(RmmUtil.GamePath + GUIOffering.FolderPath);
                    closeGUIOffering();
                    loadOfferings();
                }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (!missionUnderway)
            {
                if (GUILayout.Button("Procure", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
                {
                    if (!missionUnderway)
                    {
                        GUIMission = GUIOffering;
                        openGUIMission();
                    }
                }
            }
            if (GUILayout.Button("Close", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
            {
                closeGUIOffering();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void drawGUIOffering()
        {
            windowPosGUIOffering = ClickThruBlocker.GUILayoutWindow(3414, windowPosGUIOffering, WindowGUIOffering, GUIOffering.Name, RmmStyle.Instance.WindowStyle);
        }

        public void openGUIOffering()
        {
            print(GUIOffering.Name + " in folder: " + GUIOffering.FolderPath);
            renderGUIOffering = true;
        }

        public void closeGUIOffering()
        {
            renderGUIOffering = false;
            renderGUIMission = false;
            renderGUIPrefCrew = false;
        }

        /// <summary>
        /// GUI Order
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUIMission(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 300, 30));
            GUILayout.BeginVertical();

            if (!isACampaign(GUIMission))
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(GUIMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }

            if (GUIOffering.MinimumCrew < GUIOffering.MaximumCrew)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Planned crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(intCrewCount.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(50));
                strCrewCount = GUILayout.TextField(strCrewCount, 3, GUILayout.Width(50));

                if (GUILayout.Button("set", RmmStyle.Instance.ButtonStyle, GUILayout.Width(50), GUILayout.Height(22)))
                {
                    int.TryParse(strCrewCount, out intCrewCount);
                    if (intCrewCount < GUIOffering.MinimumCrew) { intCrewCount = GUIOffering.MinimumCrew; }
                    if (intCrewCount > GUIOffering.MaximumCrew) { intCrewCount = GUIOffering.MaximumCrew; }
                }
                GUILayout.EndHorizontal();
            }

            if (GUIOffering.MaximumCrew > 0)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("set preferred crew", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(20)))
                {
                    openGUIPrefCrew();
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("   ", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            }

            if (GUILayout.Button("Confirm", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
            {
                procureOffering(GUIOffering,false);
            }
            if (GUILayout.Button("Close", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
            {
                closeGUIMission();
            }

            GUILayout.EndVertical();
        }

        private void drawGUIMission()
        {
            windowPosGUIMission = ClickThruBlocker.GUILayoutWindow(3444, windowPosGUIMission, WindowGUIMission, "Mission", RmmStyle.Instance.WindowStyle);
        }

        public void openGUIMission()
        {
            renderGUIMission = true;
        }

        public void closeGUIMission()
        {
            renderGUIMission = false;
            renderGUIPrefCrew = false;
        }

        public void procureOffering(Mission Off,bool repeat)
        {
            if (missionUnderway == true) { ScreenMessages.PostScreenMessage("already a mission underway", 4, ScreenMessageStyle.UPPER_CENTER); return; }
            if (!offeringAllowed(Off)) { ScreenMessages.PostScreenMessage("not rated for this orbit", 4, ScreenMessageStyle.UPPER_CENTER); return; }

            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                if (Funding.Instance.Funds > Off.Price)
                {
                    Funding.Instance.AddFunds(-Off.Price, TransactionReasons.VesselRollout);
                }
                else
                {
                    ScreenMessages.PostScreenMessage("insufficient funds", 4, ScreenMessageStyle.UPPER_CENTER);
                    return;
                }
            }


            missionUnderway = true;
            missionFolderName = Off.FolderPath;
            if (!repeat)
            {
                missionArrivalTime = (float)(Planetarium.GetUniversalTime() + Off.Time);
                missionCrewCount = intCrewCount;
            }
            else
            {
                missionArrivalTime = (float)(Planetarium.GetUniversalTime() + (Off.Time > (missionRepeatDelay * 21600) ? Off.Time : (missionRepeatDelay * 21600)));
            }

            SMAsave = (float)vessel.orbit.semiMajorAxis;
            ECCsave = (float)vessel.orbit.eccentricity;
            INCsave = (float)vessel.orbit.inclination;

            selectedOffering = Off;

            closeGUIOffering();
        }


        /// <summary>
        /// GUI PrefCrew
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUIPrefCrew(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 200, 30));
            GUILayout.BeginVertical();


            GUILayout.Label("Preferred:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));

            scrollPositionPreferredCrew = GUILayout.BeginScrollView(scrollPositionPreferredCrew, false, true, GUILayout.Width(200), GUILayout.Height(200));
            foreach (ProtoCrewMember cr in preferredCrewList)
            {
                if (GUILayout.Button(cr.type == ProtoCrewMember.KerbalType.Tourist ? cr.name + " T" : cr.name, RmmStyle.Instance.ButtonStyle, GUILayout.Width(165), GUILayout.Height(22)))
                {
                    preferredCrewList.Remove(cr);
                }
            }
            GUILayout.EndScrollView();
                
            GUILayout.Label("Roster:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));


            scrollPositionAvailableCrew = GUILayout.BeginScrollView(scrollPositionAvailableCrew, false, true, GUILayout.Width(200), GUILayout.Height(300));
            foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (cr.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
                {
                    if (GUILayout.Button(cr.name, RmmStyle.Instance.ButtonStyle, GUILayout.Width(165), GUILayout.Height(22)))
                    {
                        bool alreadyAdded = false;
                        foreach (ProtoCrewMember cre in preferredCrewList)
                        {
                            if (cre.name == cr.name)
                            {
                                alreadyAdded = true;
                            }
                        }
                        if (!alreadyAdded) { preferredCrewList.Add(cr); }
                    }
                }
            }
            foreach (ProtoCrewMember to in HighLogic.CurrentGame.CrewRoster.Tourist)
            {
                if (GUILayout.Button(to.name + " T", RmmStyle.Instance.ButtonStyle, GUILayout.Width(165), GUILayout.Height(22)))
                {
                    bool alreadyAdded = false;
                    foreach (ProtoCrewMember cre in preferredCrewList)
                    {
                        if (cre.name == to.name)
                        {
                            alreadyAdded = true;
                        }
                    }
                    if (!alreadyAdded) { preferredCrewList.Add(to); }
                }
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
            {
                missionPreferedCrew = "";
                foreach (ProtoCrewMember cr in preferredCrewList)
                {
                    missionPreferedCrew = missionPreferedCrew + cr.name + ",";
                }
                closeGUIPrefCrew();
            }
            if (GUILayout.Button("Close", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
            {
                closeGUIPrefCrew();
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void drawGUIPrefCrew()
        {
            windowPosGUIPrefCrew = ClickThruBlocker.GUILayoutWindow(3447, windowPosGUIPrefCrew, WindowGUIPrefCrew, "Crew", RmmStyle.Instance.WindowStyle);
        }

        public void openGUIPrefCrew()
        {
            preferredCrewList.Clear();
            string[] prefCrewNames = new string[0];
            getPreferredCrewNames(ref prefCrewNames);
            foreach (String name in prefCrewNames)
            {
                foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
                {
                    if (name == cr.name) { preferredCrewList.Add(cr); }
                }
                foreach (ProtoCrewMember to in HighLogic.CurrentGame.CrewRoster.Tourist)
                {
                    if (name == to.name) { preferredCrewList.Add(to); }
                }
            }
            renderGUIPrefCrew = true;
        }

        public void closeGUIPrefCrew()
        {
            renderGUIPrefCrew = false;
        }


        private void getPreferredCrewNames(ref string[] names)
        {
            string[] SplitArray = missionPreferedCrew.Split(',');

            Array.Resize(ref names, SplitArray.Length);
            Array.Copy(SplitArray, names, SplitArray.Length);
        }

        private double RoundToSignificantDigits(double d, int digits)
        {
            if (d == 0)
                return 0;

            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(d))) + 1);
            return scale * Math.Round(d / scale, digits);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        /// <summary>
        /// depart
        /// </summary>
        [KSPEvent(name = "setAutoDepart", isDefault = false, guiActive = false, guiName = "Commence Return")]
        public void setAutoDepart()
        {
            if (RmmUtil.IsDocked(vessel, part))
            {
                ModuleDockingNode DockNode = part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                DockNode.Undock();
            }
            vehicleAutoDepart = true;
            nextLogicTime = Planetarium.GetUniversalTime() + 2;
        }

        /// <summary>
        /// general functions
        /// </summary>
        /// <returns></returns>
        /// 

        private bool offeringAllowed(Mission Off)
        {
            if (vessel.mainBody.name == Off.Body && (RmmUtil.OrbitAltitude(vessel) < 4 || Off.MaxOrbitAltitude == 0) && isCurrentCampaign(Off) && vessel.situation == Vessel.Situations.ORBITING)
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        //return a day-hour-minute-seconds-time format for the time
        public string timeString(double time)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string strT = "";

            bool positive;

            if (time >= 0)
            {
                positive = true;
            }
            else
            {
                positive = false;
            }

            days = (int)Math.Floor(time / 21600);
            time = time - (days * 21600);

            hours = (int)Math.Floor(time / 3600);
            time = time - (hours * 3600);

            minutes = (int)Math.Floor(time / 60);
            time = time - (minutes * 60);

            seconds = (int)Math.Floor(time);

            if (days > 0)
            {
                strT = days.ToString() + "d";
                strT = (hours != 0) ? strT + hours.ToString() + "h" : strT;
                strT = (minutes != 0) ? strT + minutes.ToString() + "m" : strT;
                strT = (seconds != 0) ? strT + seconds.ToString() + "s" : strT;
            }
            else if (hours > 0)
            {
                strT = hours.ToString() + "h";
                strT = (minutes != 0) ? strT + minutes.ToString() + "m" : strT;
                strT = (seconds != 0) ? strT + seconds.ToString() + "s" : strT;
            }
            else if (minutes > 0)
            {
                strT = minutes.ToString() + "m";
                strT = (seconds != 0) ? strT + seconds.ToString() + "s" : strT;
            }
            else if (seconds > 0)
            {
                strT = seconds.ToString() + "s";
            }

            //strT = days.ToString() + "d" + hours.ToString() + "h" + minutes.ToString() + "m" + seconds + "s";

            if (positive)
                return (strT);
            else
                return ("-" + strT);
        }

        public string timeETAString(double time)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string strT = "";

            if (time >= 0)
            {
                days = (int)Math.Floor(time / 21600);
                time = time - (days * 21600);

                hours = (int)Math.Floor(time / 3600);
                time = time - (hours * 3600);

                minutes = (int)Math.Floor(time / 60);
                time = time - (minutes * 60);

                seconds = (int)Math.Floor(time);

                if (days > 1)
                {
                    strT = days.ToString() + "d";
                }
                else if (days > 0)
                {

                    strT = days.ToString() + "d" + hours.ToString() + "h";
                }
                else if (hours > 0)
                {
                    strT = hours.ToString() + "h";
                }
                else
                {
                    strT = "soon(TM)";
                }
            }
            else
            {
                if (time > -3600)
                {
                    strT = "come back later";
                }
                else
                {
                    strT = "maybe later";
                }
            }

            return (strT);
        }





        /// <summary>
        /// Register Port Code logic
        /// </summary>
        [KSPEvent(name = "register", isDefault = false, guiActive = true, guiName = "Register Docking Port")]
        public void register()
        {
            openGUIRegister();
        }

        private void WindowGUIRegister(int windowID)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Docking Port Code");
            StrPortCode = GUILayout.TextField(StrPortCode, 15, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Register", GUILayout.Width(60)))
            {
                if (StrPortCode != "")
                {
                    PortCode = StrPortCode;
                    closeGUIRegister();
                }
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(60)))
            {
                closeGUIRegister();
            }
            GUILayout.EndHorizontal();
        }

        private void drawGUIRegister()
        {
            windowPosGUIRegister = GUI.Window(157, windowPosGUIRegister, WindowGUIRegister, "Register", RmmStyle.Instance.WindowStyle);
        }

        public void openGUIRegister()
        {
            renderGUIRegister = true;
        }

        public void closeGUIRegister()
        {
            renderGUIRegister = false;
        }
    }
}
