using System;
using System.Collections.Generic;
using KSP.Localization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Worker
{
    class RetrievalWorker
    {
        private double _nextLogicTime = 0;

        // Retrieval state
        public bool CompleteRetrieval = false;
        public bool ArrivalCompleted = false;
        private RoutineRetrievalMission _mission;
        private Vessel _vessel = null;           // station vessel
        private Part _part = null;               // station docking port
        private Part _departurePart = null;      // docked vessel's port (the part on the other side of the dock)

        // Phase tracking: 0=departure, 1=waiting, 2=arrival
        private int _phase = 0;
        private int _departureStage = 0;
        private int _arrivalStage = 0;

        private double _arrivalTime = 0;         // when to start arrival phase

        // Departure-specific state
        private Vessel _departureVessel = null;

        // Arrival-specific state
        private Vessel _transactionVessel = null;
        private string _tempID = "";
        private uint _missionFlightIDDockPart = 0;

        public void StartRetrieval(RoutineRetrievalMission mission, Vessel vessel)
        {
            _mission = mission;
            _vessel = vessel;
            _part = RmmUtil.GetVesselPart(vessel, mission.flightIdDepartureDockPart);
            _departurePart = RmmUtil.GetDockedPart(FlightGlobals.ActiveVessel, _part);
            CompleteRetrieval = true;
            _nextLogicTime = Planetarium.GetUniversalTime();
            _phase = 0;
            _departureStage = 0;
            _arrivalStage = 0;

            RmmDockingPortModule.AddToPart(_part);
            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(OnFixedUpdateRetrievalCompletion);
        }

        public void StartRetrievalArrival(RoutineRetrievalMission mission, Vessel vessel)
        {
            _mission = mission;
            _vessel = vessel;
            _part = RmmUtil.GetVesselPart(vessel, mission.flightIdDepartureDockPart);
            CompleteRetrieval = true;
            _nextLogicTime = Planetarium.GetUniversalTime();
            _phase = 2;
            _arrivalStage = 1;

            RmmDockingPortModule.AddToPart(_part);
            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(OnFixedUpdateRetrievalCompletion);
        }

        public void OnFixedUpdateRetrievalCompletion()
        {
            if (!CompleteRetrieval) { return; }
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }

            switch (_phase)
            {
                case 0: // Departure phase
                    OnFixedUpdateDeparture();
                    break;
                case 1: // Waiting phase
                    OnFixedUpdateWaiting();
                    break;
                case 2: // Arrival phase
                    OnFixedUpdateArrival();
                    break;
            }
        }

        #region Departure Phase

        private void OnFixedUpdateDeparture()
        {
            if (_departureStage == 0)
            {
                if (_vessel != null && _part != null && _departurePart != null)
                {
                    _departureStage = 1;
                    _nextLogicTime = Planetarium.GetUniversalTime();
                }
                else
                {
                    abortRetrieval();
                    return;
                }
            }

            if (_vessel.packed || !_vessel.loaded)
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                return;
            }

            if (CompleteRetrieval)
            {
                switch (_departureStage)
                {
                    case 1:
                        departureStage1();
                        break;
                    case 2:
                        departureStage2();
                        break;
                    case 3:
                        departureStage3();
                        break;
                }
            }
        }

        private void departureStage1()
        {
            if (RmmUtil.IsDocked(_vessel, _part))
            {
                ModuleDockingNode DockNode = _part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                DockNode.Undock();
            }
            _departureStage = 2;
            _nextLogicTime = Planetarium.GetUniversalTime() + 2;
        }

        private void departureStage2()
        {
            if (RmmUtil.IsDocked(_vessel, _departurePart))
            {
                abortRetrieval();
            }
            else
            {
                _departureVessel = _departurePart.vessel;
            }

            if (_departureVessel.isActiveVessel)
            {
                foreach (Vessel ves in FlightGlobals.Vessels)
                {
                    if (!ves.packed && ves.loaded && ves.id != _departureVessel.id)
                    {
                        FlightGlobals.SetActiveVessel(ves);
                        _departureStage = 3;
                        _nextLogicTime = Planetarium.GetUniversalTime() + 1;
                        return;
                    }
                }
            }
            else
            {
                _departureStage = 3;
                _nextLogicTime = Planetarium.GetUniversalTime() + 1;
            }
        }

        private void departureStage3()
        {
            DepartureCompletion();
        }

        private void DepartureCompletion()
        {
            RmmUtil.ToMapView();
            try { RmmContract.HandleContracts(_departureVessel, false, true); }
            catch (Exception ex) { LoggerRmm.Warning("HandleContracts departure exception (non-fatal): " + ex.Message); }

            // Capture crew names before unloading so the same kerbals return on arrival
            string crewNames = "";
            foreach (Part p in _departureVessel.parts)
            {
                foreach (ProtoCrewMember crew in p.protoModuleCrew)
                {
                    if (!String.IsNullOrEmpty(crewNames)) { crewNames += ","; }
                    crewNames += crew.name;
                }
            }
            _mission.CrewSelection = crewNames;

            handleUnloadCrew(_departureVessel, true);
            _departureVessel.Unload();
            _departureVessel.Die();

            ScreenMessages.PostScreenMessage(Localizer.Format(_mission.VesselName) + " departed for retrieval", 4, ScreenMessageStyle.UPPER_CENTER);

            // schedule arrival and persist to ordered mission
            _arrivalTime = Planetarium.GetUniversalTime() + _mission.Duration;
            _mission.ArrivalTime = _arrivalTime;
            _mission.OrderRoutineMission();

            finishRetrieval();
        }

        #endregion Departure Phase

        #region Waiting Phase

        private void OnFixedUpdateWaiting()
        {
            if (_arrivalTime < Planetarium.GetUniversalTime())
            {
                // Transition to arrival phase
                _phase = 2;
                _arrivalStage = 1;
                _nextLogicTime = Planetarium.GetUniversalTime();
            }
            else
            {
                _nextLogicTime = Planetarium.GetUniversalTime() + 1;
            }
        }

        #endregion Waiting Phase

        #region Arrival Phase

        private void OnFixedUpdateArrival()
        {
            if (_vessel.packed || !_vessel.loaded)
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                return;
            }

            if (CompleteRetrieval)
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
                        if (_transactionVessel == null || _vessel.packed || !_vessel.loaded || _transactionVessel.packed || !_transactionVessel.loaded)
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
            if (_mission.AllowedLocation(_vessel).CheckSuccess && CrewAvailable())
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                _arrivalStage = 2;
            }
            else
            {
                abortRetrieval();
            }
        }

        private void dockStage2()
        {
            RmmUtil.ToMapView();
            ProtoVessel ProtoFlightVessel = loadVessel();
            if (ProtoFlightVessel == null) { abortRetrieval(); return; }
            if (loadVesselForRendezvous(ProtoFlightVessel, _vessel))
            {
                _nextLogicTime = Planetarium.GetUniversalTime();
                _arrivalStage = 3;
            }
        }

        private void dockStage3()
        {
            //search for the vessel for five seconds, else abort
            if (_nextLogicTime < (Planetarium.GetUniversalTime() - 5)) { logreport(); abortRetrieval(); return; }

            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == _tempID)
                {
                    _transactionVessel = ve;
                    _transactionVessel.vesselName = _mission.VesselName;
                    placeVesselForRendezvous(_transactionVessel, _vessel);
                    _nextLogicTime = Planetarium.GetUniversalTime();
                    _arrivalStage = 4;
                    return;
                }
            }
        }

        private void logreport()
        {
            LoggerRmm.Error("Rmm at stage " + _arrivalStage + " and can not find vessel " + _tempID + ". the vessels i can find are:");
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                LoggerRmm.Error(ve.vesselName);
            }
            LoggerRmm.Error("--test run");
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == _tempID)
                {
                    LoggerRmm.Error("test 1");
                    _transactionVessel = ve;
                    LoggerRmm.Error("test 2");
                    _transactionVessel.vesselName = _mission.VesselName;
                    LoggerRmm.Error("test 3");
                    placeVesselForRendezvous(_transactionVessel, _vessel);
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
            foreach (Part p in _transactionVessel.parts)
            {
                if (p.flightID == _missionFlightIDDockPart)
                {
                    placePort = p;
                }
                foreach (PartModule pm in p.Modules)
                {
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
            _transactionVessel.targetObject = null;
            handleLoadCrew(_transactionVessel, _mission.CrewCount, _mission.MinimumCrew, _mission.CrewSelection);
            try { RmmContract.HandleContracts(_transactionVessel, true, false); }
            catch (Exception ex) { LoggerRmm.Warning("HandleContracts arrival exception (non-fatal): " + ex.Message); }
            if (placePort != null && !RmmUtil.IsDocked(_vessel, _part) && checkDockingPortCompatibility(placePort, _part))
            {
                placeVesselForDock(_transactionVessel, placePort, _vessel, _part, RmmUtil.GetDockingDistance(placePort));
                _nextLogicTime = Planetarium.GetUniversalTime();
                _arrivalStage = 5;
            }
            else
            {
                ScreenMessages.PostScreenMessage(_mission.VesselName + " rendezvoused", 4, ScreenMessageStyle.UPPER_CENTER);
                ArrivalCompleted = true;
                finishRetrieval();
            }
        }

        private void dockStage5()
        {
            RmmUtil.ToMapView();
            if (RmmUtil.IsDocked(_vessel, _part)
                || (_nextLogicTime < (Planetarium.GetUniversalTime() - 8)))
            {
                ScreenMessages.PostScreenMessage(_mission.VesselName + " docked", 4, ScreenMessageStyle.UPPER_CENTER);

                ArrivalCompleted = true;
                finishRetrieval();
            }
        }

        #endregion Arrival Phase

        #region Vessel Loading and Placement

        private ProtoVessel loadVessel()
        {
            ConfigNode loadnode = null;
            string path = RmmUtil.GamePath + Path.DirectorySeparatorChar + _mission.FolderPath + Path.DirectorySeparatorChar + "arrivalvesselfile";
            if (!File.Exists(path)) { abortRetrieval(); return null; }
            loadnode = ConfigNode.Load(path);
            if (loadnode == null) { abortRetrieval(); return null; }
            ProtoVessel loadprotovessel = new ProtoVessel(loadnode, HighLogic.CurrentGame);
            return loadprotovessel;
        }

        private bool loadVesselForRendezvous(ProtoVessel placeVessel, Vessel targetVessel)
        {
            targetVessel.BackupVessel();

            placeVessel.orbitSnapShot = targetVessel.protoVessel.orbitSnapShot;

            placeVessel.orbitSnapShot.epoch = 0.0;

            _tempID = RmmUtil.Rand.Next(1000000000).ToString();
            //rename any vessels present with the temp name
            foreach (Vessel ve in FlightGlobals.Vessels)
            {
                if (ve.vesselName == _tempID)
                {
                    Vessel NameVessel = null;
                    NameVessel = ve;
                    NameVessel.vesselName = "1";
                }
            }

            placeVessel.vesselID = Guid.NewGuid();

            placeVessel.vesselName = _tempID;

            foreach (ProtoPartSnapshot p in placeVessel.protoPartSnapshots)
            {
                uint newFlightID = (UInt32)RmmUtil.Rand.Next(1000000000, 2147483647);

                // save flight ID of part which should dock
                if (p.flightID == _mission.flightIDDockPart)
                {
                    _missionFlightIDDockPart = newFlightID;
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
                abortRetrieval();
                return;
            }

            placeVessel.orbit.UpdateFromStateVectors(targetVessel.orbit.pos + offset, targetVessel.orbit.vel, targetVessel.orbit.referenceBody, Planetarium.GetUniversalTime());
        }

        private bool determineRendezvousOffset(Vessel placeVessel, Vessel targetVessel, ref Vector3d offset)
        {
            int rendezvousDistance = vesselScale(placeVessel);

            //determine max scale of all vessels involved
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

        #endregion Vessel Loading and Placement

        #region Crew Handling

        //Thanks to sarbian's Kerbal Crew Manifest for showing all this crew handling stuff
        private void handleLoadCrew(Vessel ves, int crewCount, int minCrew, string crewSelection)
        {
            if (crewCount <= 0) { return; }

            if (ves.GetCrewCapacity() < crewCount)
            {
                crewCount = ves.GetCrewCapacity();
            }

            string[] prefCrewNames = GetPreferredCrewNames(crewSelection);
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

        #endregion Crew Handling

        #region Finish and Abort

        private void finishRetrieval()
        {
            CompleteRetrieval = false;

            _nextLogicTime = 0;
            _departureStage = -1;
            _arrivalStage = -1;

            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(null);
        }

        private void abortRetrieval()
        {
            CompleteRetrieval = false;

            _nextLogicTime = 0;
            _departureStage = -1;
            _arrivalStage = -1;

            var rmmDockingPortModule = _part.Modules.OfType<RmmDockingPortModule>().FirstOrDefault();
            rmmDockingPortModule.SetOnFixedUpdateCallback(null);
        }

        #endregion Finish and Abort

        public RoutineRetrievalMission Mission
        {
            get { return _mission; }
        }
    }
}
