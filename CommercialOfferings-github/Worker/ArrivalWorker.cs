using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Worker
{
    public class ArrivalWorker
    {
        private double _nextLogicTime = 0;

        //arrival transaction 
        public bool CompleteArrival = false;
        private int _arrivalStage = 0;
        private Vessel transactionVessel = null;
        private string tempID = "";
        private uint missionFlightIDDockPart = 0;

        private RoutineArrivalMission _mission;
        private Vessel _targetVessel = null;
        private Part _targetPart = null;

        public void StartArrival(RoutineArrivalMission mission, Vessel targetVessel)
        {
            _mission = mission;
            _targetVessel = targetVessel;
            _targetPart = RmmUtil.GetVesselPart(targetVessel, mission.flightIdArrivalDockPart);
            CompleteArrival = true;
            _nextLogicTime = Planetarium.GetUniversalTime();
            _arrivalStage = 0;
        }

        public void HandleArrivalCompletion()
        {
            if (!CompleteArrival) { return; }
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }

            if (_arrivalStage == 0)
            {
                if (_targetVessel != null && _targetPart != null)
                {
                    _arrivalStage = 1;
                    CompleteArrival = true;
                    _nextLogicTime = Planetarium.GetUniversalTime();
                }
                else
                {
                    abortArrival();
                }
            }

            if (_targetVessel.packed || !_targetVessel.loaded)
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                return;
            }

            if (CompleteArrival)
            {
                switch (_arrivalStage)
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
                        if (transactionVessel == null || _targetVessel.packed || !_targetVessel.loaded || transactionVessel.packed || !transactionVessel.loaded)
                        {
                            _nextLogicTime = Planetarium.GetUniversalTime();
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
            //load Offering of current mission

            if (_mission.Allowed(_targetVessel).CheckSucces && CrewAvailable())
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                _arrivalStage = 2;
            }
            else
            {
                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    Funding.Instance.AddFunds(_mission.Price, TransactionReasons.VesselRecovery);
                }
                CompleteArrival = false;

                _nextLogicTime = 0;
                _arrivalStage = -1;
            }
        }

        private void dockStage2()
        {
            RmmUtil.ToMapView();
            LoggerRmm.Debug("st2.1");
            ProtoVessel ProtoFlightVessel = loadVessel(_mission.FolderPath);
            LoggerRmm.Debug("st2.2");
            if (ProtoFlightVessel == null) { abortArrival(); return; }
            LoggerRmm.Debug("st2.3");
            if (loadVesselForRendezvous(ProtoFlightVessel, _targetVessel))
            {
                LoggerRmm.Debug("st2.4");
                _nextLogicTime = Planetarium.GetUniversalTime();
                _arrivalStage = 3;
            }
        }

        private void dockStage3()
        {
            //search for the vessel for five seconds, else abort
            if (_nextLogicTime < (Planetarium.GetUniversalTime() - 5)) { logreport(); abortArrival(); return; }

            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == tempID)
                {
                    transactionVessel = ve;
                    transactionVessel.vesselName = _mission.VesselName;
                    placeVesselForRendezvous(transactionVessel, _targetVessel);
                    _nextLogicTime = Planetarium.GetUniversalTime();
                    _arrivalStage = 4;
                    return;
                }
            }
        }

        private void logreport()
        {
            LoggerRmm.Error("Rmm at stage " + _arrivalStage + " and can not find vessel " + tempID + ". the vessels i can find are:");
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                LoggerRmm.Error(ve.vesselName);
            }
            LoggerRmm.Error("--test run");
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == tempID)
                {
                    LoggerRmm.Error("test 1");
                    transactionVessel = ve;
                    LoggerRmm.Error("test 2");
                    transactionVessel.vesselName = _mission.VesselName;
                    LoggerRmm.Error("test 3");
                    placeVesselForRendezvous(transactionVessel, _targetVessel);
                    LoggerRmm.Error("test 4");
                    _nextLogicTime = Planetarium.GetUniversalTime();
                    _arrivalStage = 4;
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
                        var scienceDatas = moduleScienceContainer.GetData();
                        for (int i = 0; i < scienceDatas.Count(); i++)
                        {
                            moduleScienceContainer.RemoveData(scienceDatas[i]);
                        }
                    }
                }
            }
            transactionVessel.targetObject = null;
            handleLoadCrew(transactionVessel, _mission.CrewCount, _mission.MinimumCrew, _mission.CrewSelection);
            RmmContract.HandleContracts(transactionVessel, true, false);
            LoggerRmm.Debug("st4.5" + RmmUtil.IsDocked(_targetVessel, _targetPart) + checkDockingPortCompatibility(placePort, _targetPart));
            if (!RmmUtil.IsDocked(_targetVessel, _targetPart) && checkDockingPortCompatibility(placePort, _targetPart))
            {
                LoggerRmm.Debug("st4.6");
                placeVesselForDock(transactionVessel, placePort, _targetVessel, _targetPart, RmmUtil.GetDockingDistance(placePort));
                LoggerRmm.Debug("st4.7");
                _nextLogicTime = Planetarium.GetUniversalTime();
                _arrivalStage = 5;
            }
            else
            {
                ScreenMessages.PostScreenMessage(_mission.VesselName + " rendezvoused", 4, ScreenMessageStyle.UPPER_CENTER);
                finishArrival();
            }
        }

        private void dockStage5()
        {
            RmmUtil.ToMapView();
            if (RmmUtil.IsDocked(_targetVessel, _targetPart)
                || (_nextLogicTime < (Planetarium.GetUniversalTime() - 8)))
            {
                ScreenMessages.PostScreenMessage(_mission.VesselName + " docked", 4, ScreenMessageStyle.UPPER_CENTER);

                finishArrival();
            }
        }

        private ProtoVessel loadVessel(string folderName)
        {
            ConfigNode loadnode = null;
            string path = RmmUtil.GamePath + Path.DirectorySeparatorChar + _mission.FolderPath + Path.DirectorySeparatorChar + "arrivalvesselfile";
            if (!File.Exists(path)) { abortArrival(); return null; }
            loadnode = ConfigNode.Load(path);
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
                if (p.flightID == _mission.flightIDDockPart)
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
                CompleteArrival = false;
                _nextLogicTime = 0;
                _arrivalStage = -1;
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
                    if (!ves.packed && ves.loaded && ves.id != placeVessel.id)
                    {

                        var dist = Vector3.Distance(ves.orbit.pos, targetVessel.orbit.pos + offset);
                        if (dist < rendezvousDistance)
                        {
                            good = false;
                        }
                    }
                }

            } while (!good && attempts < 100);

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
            CompleteArrival = false;

            _nextLogicTime = 0;
            _arrivalStage = -1;
        }

        private void abortArrival()
        {
            CompleteArrival = false;

            _nextLogicTime = 0;
            _arrivalStage = -1;
        }

        //Thanks to sarbian's Kerbal Crew Manifest for showing all this crew handling stuff
        private void handleLoadCrew(Vessel ves, int crewCount, int minCrew, string crewSelection)
        {
            if (ves.GetCrewCapacity() < crewCount)
                crewCount = ves.GetCrewCapacity();
            LoggerRmm.Debug("st4.32");

            string[] prefCrewNames = GetPreferredCrewNames(crewSelection);
            LoggerRmm.Debug("st4.33");
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
            LoggerRmm.Debug("st4.35");
        }

        private bool CrewAvailable()
        {
            int availableCrew = 0;
            foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                if (cr.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                {
                    availableCrew++;
                }
            }

            if (availableCrew < _mission.MinimumCrew)
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

        private string[] GetPreferredCrewNames(string missionPreferedCrew)
        {
            if (String.IsNullOrEmpty(missionPreferedCrew))
            {
                return new string[0];
            }
            return missionPreferedCrew.Split(',');
        }

        public RoutineArrivalMission Mission
        {
            get { return _mission; }
        }
    }
}
