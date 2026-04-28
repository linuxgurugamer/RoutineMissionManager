using CommercialOfferings.Gui;
using CommercialOfferings.MissionData;
using CommercialOfferings.Worker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    class RoutineControl
    {
        //private RoutineWindow _routineWindow = null;

        private double _nextLogicTime = 1;

        private List<Mission> _missions = null;
        private List<RoutineArrivalMission> _routineArrivalMissions = null;
        private List<RoutineDepartureMission> _routineDepartureMissions = null;
        private List<RoutineRetrievalMission> _routineRetrievalMissions = null;

        private ArrivalWorker _arrivalWorker = null;
        private DepartureWorker _departureWorker = null;
        private RetrievalWorker _retrievalWorker = null;

        public void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }

            HandleRoutine();
            HandleRoutineOverviewWindow();
            HandleRoutineDetailWindow();

            HandleRoutineDepartureOverviewWindow();
            HandleRoutineDepartureDetailWindow();

            HandleRoutineRetrievalOverviewWindow();
            HandleRoutineRetrievalDetailWindow();

            HandleCrewSelectionWindow();
            HandleDockingPortSelectionWindow();

            HandleOrderedMissionsWindow();

            _nextLogicTime = Planetarium.GetUniversalTime() + 1;
        }

        private void HandleRoutine()
        {
            if (_missions == null) { _missions = Mission.LoadMissions(); }
            if (_routineArrivalMissions == null) { LoadRoutineArrivalMissions(); }
            if (_routineDepartureMissions == null) { LoadRoutineDepartureMissions(); }
            if (_routineRetrievalMissions == null) { LoadRoutineRetrievalMissions(); }
            HandleRoutineArrivals();
            HandleRoutineDepartures();
            HandleRoutineRetrievals();
        }

        private void HandleRoutineArrivals()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (FlightGlobals.ActiveVessel.packed || !FlightGlobals.ActiveVessel.loaded) { return; }

            if (_arrivalWorker == null)
            {
                foreach (RoutineArrivalMission mission in _routineArrivalMissions)
                {
                    if (mission.ArrivalTime < Planetarium.GetUniversalTime() &&
                        RmmUtil.PartExistsInVessel(FlightGlobals.ActiveVessel, mission.flightIdArrivalDockPart) &&
                        mission.Allowed(FlightGlobals.ActiveVessel).CheckSuccess)
                    {
                        if (!SkyClear(FlightGlobals.ActiveVessel))

                        {
                            mission.ArrivalTime += 600;
                            ScreenMessages.PostScreenMessage("mission " + mission.Name + " delayed-LOCAL TRAFFIC", 4, ScreenMessageStyle.UPPER_CENTER);
                            continue;
                        }
                        _arrivalWorker = new ArrivalWorker();
                        _arrivalWorker.StartArrival(mission, FlightGlobals.ActiveVessel);
                        return;
                    }
                }
            }
            else
            {
                if (_arrivalWorker.CompleteArrival)
                {
                    return;
                }
                else
                {
                    RmmScenario.Instance.SetOrderedMission(_arrivalWorker.Mission.OrderId, null, 0, null);
                    _routineArrivalMissions = null;
                    _arrivalWorker = null;
                }
            }
        }

        private void HandleRoutineDepartures()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (FlightGlobals.ActiveVessel.packed || !FlightGlobals.ActiveVessel.loaded) { return; }


            if (_departureWorker == null)
            {
                foreach (RoutineDepartureMission mission in _routineDepartureMissions)
                {
                    // Departures are executed immdediatly so remove mission which are more then 30 seconds old.
                    if (mission.DepartureTime < Planetarium.GetUniversalTime() - 30)
                    {
                        RmmScenario.Instance.SetOrderedMission(mission.OrderId, null, 0, null);
                        _routineDepartureMissions = null;
                        _departureWorker = null;
                    }

                    if (mission.DepartureTime < Planetarium.GetUniversalTime() &&
                        RmmUtil.PartExistsInVessel(FlightGlobals.ActiveVessel, mission.flightIdDepartureDockPart) &&
                        mission.Allowed(FlightGlobals.ActiveVessel).CheckSuccess)
                    {
                        _departureWorker = new DepartureWorker();
                        _departureWorker.StartDeparture(mission, FlightGlobals.ActiveVessel);
                        return;
                    }
                }
            }
            else
            {
                if (_departureWorker.CompleteDeparture)
                {
                    return;
                }
                else
                {
                    RmmScenario.Instance.SetOrderedMission(_departureWorker.Mission.OrderId, null, 0, null);
                    _routineDepartureMissions = null;
                    _departureWorker = null;

                    RefreshRoutineDepartureOverview();
                }
            }
        }

        private void HandleRoutineRetrievals()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (FlightGlobals.ActiveVessel.packed || !FlightGlobals.ActiveVessel.loaded) { return; }

            if (_retrievalWorker == null)
            {
                foreach (RoutineRetrievalMission mission in _routineRetrievalMissions)
                {
                    LoggerRmm.Debug("HandleRoutineRetrievals: checking mission " + mission.Name + " DepartureTime=" + mission.DepartureTime + " ArrivalTime=" + mission.ArrivalTime + " now=" + Planetarium.GetUniversalTime());

                    // Phase 2: Arrival — departure already happened, waiting for vessel to return
                    if (mission.ArrivalTime > 0 && mission.ArrivalTime < Planetarium.GetUniversalTime() &&
                        RmmUtil.PartExistsInVessel(FlightGlobals.ActiveVessel, mission.flightIdDepartureDockPart))
                    {
                        LoggerRmm.Debug("  Arrival phase triggered!");
                        if (!SkyClear(FlightGlobals.ActiveVessel))
                        {
                            mission.ArrivalTime += 600;
                            mission.OrderRoutineMission();
                            ScreenMessages.PostScreenMessage("mission " + mission.Name + " delayed-LOCAL TRAFFIC", 4, ScreenMessageStyle.UPPER_CENTER);
                            continue;
                        }
                        _retrievalWorker = new RetrievalWorker();
                        _retrievalWorker.StartRetrievalArrival(mission, FlightGlobals.ActiveVessel);
                        return;
                    }

                    // Phase 1: Departure — vessel still docked, ready to depart
                    if (mission.DepartureTime < Planetarium.GetUniversalTime() && mission.ArrivalTime == 0 &&
                        RmmUtil.PartExistsInVessel(FlightGlobals.ActiveVessel, mission.flightIdDepartureDockPart) &&
                        mission.Allowed(FlightGlobals.ActiveVessel).CheckSuccess)
                    {
                        if (!SkyClear(FlightGlobals.ActiveVessel))
                        {
                            mission.DepartureTime += 600;
                            ScreenMessages.PostScreenMessage("mission " + mission.Name + " delayed-LOCAL TRAFFIC", 4, ScreenMessageStyle.UPPER_CENTER);
                            continue;
                        }
                        _retrievalWorker = new RetrievalWorker();
                        _retrievalWorker.StartRetrieval(mission, FlightGlobals.ActiveVessel);
                        return;
                    }
                }
            }
            else
            {
                if (_retrievalWorker.CompleteRetrieval)
                {
                    return;
                }
                else
                {
                    // If the worker completed the arrival phase, remove the ordered mission
                    // If it only completed departure (ArrivalTime > now), leave it for the arrival phase
                    if (_retrievalWorker.Mission.ArrivalTime <= 0 || _retrievalWorker.ArrivalCompleted)
                    {
                        RmmScenario.Instance.SetOrderedMission(_retrievalWorker.Mission.OrderId, null, 0, null);
                    }
                    _routineRetrievalMissions = null;
                    _retrievalWorker = null;
                }
            }
        }

        #region Routine Overview

        private static RoutineOverviewWindow _routineOverviewWindow = null;

        private void HandleRoutineOverviewWindow()
        {
            if (_routineOverviewWindow == null) { return; }
            if (!WindowManager.IsOpen(_routineOverviewWindow))
            {
                CancelRoutineOverview();
                return;
            }

        }

        public void CancelRoutineOverview()
        {
            if (_routineOverviewWindow != null)
            {
                WindowManager.Close(_routineOverviewWindow);
                _routineOverviewWindow = null;
            }
        }

        public void RoutineOverview(IWindow parent = null)
        {
            _missions = Mission.LoadMissions();
            CancelRoutineOverview();
            _routineOverviewWindow = new RoutineOverviewWindow(this);
            WindowManager.Open(_routineOverviewWindow, parent: parent);

            List<RoutineArrivalMission> routineArrivalMissions = new List<RoutineArrivalMission>();
            foreach (Mission mission in _missions)
            {
                if (mission.Info == null) { continue; }
                if (mission.Info.Type != 10) { continue; }
                var routineArrivalMission = RoutineMission.AssemblePotentialRoutineMission<RoutineArrivalMission>(mission);
                if (routineArrivalMission == null) { continue; }

                var check = routineArrivalMission.AllowedLocation(FlightGlobals.ActiveVessel);
                if (!routineArrivalMission.AllowedLocation(FlightGlobals.ActiveVessel).CheckSuccess) { continue; }
                routineArrivalMissions.Add(routineArrivalMission);
            }
            _routineOverviewWindow.RoutineArrivalMissions = routineArrivalMissions;
        }

        #endregion Routine Overview

        #region Routine Detail

        private static RoutineDetailWindow _routineDetailWindow = null;

        private void HandleRoutineDetailWindow()
        {
            if (_routineDetailWindow == null) { return; }
            if (!WindowManager.IsOpen(_routineDetailWindow))
            {
                CancelRoutineDetail();
                return;
            }
        }

        public void CancelRoutineDetail()
        {
            if (_routineDetailWindow != null)
            {
                WindowManager.Close(_routineDetailWindow);
                _routineDetailWindow = null;
            }
        }

        public void RoutineDetail(string missiondId, IWindow parent = null)
        {
            CancelRoutineDetail();
            _routineDetailWindow = new RoutineDetailWindow(this);
            WindowManager.Open(_routineDetailWindow, parent: parent);

            RoutineArrivalMission routineArrivalMission = null;
            foreach (Mission mission in _missions)
            {
                if (mission.Info == null) { continue; }
                if (mission.Info.Type != 10) { continue; }
                if (mission.MissionId == missiondId)
                {
                    routineArrivalMission = RoutineMission.AssemblePotentialRoutineMission<RoutineArrivalMission>(mission);
                    if (!routineArrivalMission.AllowedLocation(FlightGlobals.ActiveVessel).CheckSuccess) { continue; }
                }
            }
            _routineDetailWindow.RoutineArrivalMission = routineArrivalMission;
        }

        public void RoutineDetail(RoutineArrivalMission routineArrivalMission, IWindow parent = null)
        {
            CancelRoutineDetail();
            _routineDetailWindow = new RoutineDetailWindow(this);
            WindowManager.Open(_routineDetailWindow, parent: parent);

            _routineDetailWindow.RoutineArrivalMission = routineArrivalMission;
        }

        public CheckList OrderArrivalMissionAllowed()
        {
            var checkList = new CheckList();

            checkList.Check(_routineDetailWindow != null, "no window is open");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_routineDetailWindow.RoutineArrivalMission != null, "no mission selected");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_routineDetailWindow.RoutineArrivalMission.Allowed(FlightGlobals.ActiveVessel), "order not allowed");

            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                checkList.Check(Funding.Instance.Funds > _routineDetailWindow.RoutineArrivalMission.Price, "insufficient funds");
            }

            return checkList;
        }

        public void OrderArrivalMission()
        {
            if (_routineDetailWindow == null) { return; }

            if (_routineDetailWindow.RoutineArrivalMission != null)
            {
                var mission = _routineDetailWindow.RoutineArrivalMission;

                mission.ArrivalTime = Planetarium.GetUniversalTime() + mission.Duration;

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    Funding.Instance.AddFunds(-mission.Price, TransactionReasons.VesselRollout);
                }

                mission.OrderRoutineMission();

                _routineArrivalMissions = null;
            }
        }

        public void CancelArrivalMission()
        {
            if (_routineDetailWindow == null) { return; }

            if (_routineDetailWindow.RoutineArrivalMission != null)
            {
                var mission = _routineDetailWindow.RoutineArrivalMission;

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    Funding.Instance.AddFunds(mission.Price, TransactionReasons.VesselRollout);
                }

                mission.UnorderRoutineMission();

                _routineArrivalMissions = null;
            }
        }

        #endregion Routine Detail

        #region Routine Departure Overview

        private static RoutineDepartureOverviewWindow _routineDepartureOverviewWindow = null;

        private void HandleRoutineDepartureOverviewWindow()
        {
            if (_routineDepartureOverviewWindow == null) { return; }
            if (!WindowManager.IsOpen(_routineDepartureOverviewWindow))
            {
                CancelRoutineDepartureOverview();
                return;
            }

        }

        public void CancelRoutineDepartureOverview()
        {
            if (_routineDepartureOverviewWindow != null)
            {
                WindowManager.Close(_routineDepartureOverviewWindow);
                _routineDepartureOverviewWindow = null;
            }
        }

        public void RoutineDepartureOverview(IWindow parent = null)
        {
            _missions = Mission.LoadMissions();
            CancelRoutineDepartureOverview();
            _routineDepartureOverviewWindow = new RoutineDepartureOverviewWindow(this);
            WindowManager.Open(_routineDepartureOverviewWindow, parent: parent);

            RefreshRoutineDepartureOverview();
        }

        public void RefreshRoutineDepartureOverview()
        {
            if (_routineDepartureOverviewWindow == null) { return; }

            List<RoutineDepartureMission> routineDepartureMissions = new List<RoutineDepartureMission>();
            foreach (Mission mission in _missions)
            {
                if (mission.Info == null) { continue; }
                if (mission.Info.Type != 20) { continue; }
                var locationRoutineDepartureMission = RoutineMission.AssemblePotentialRoutineMission<RoutineDepartureMission>(mission);
                if (locationRoutineDepartureMission == null) { continue; }
                if (!locationRoutineDepartureMission.AllowedLocation(FlightGlobals.ActiveVessel).CheckSuccess) { continue; }
                foreach (Part part in FlightGlobals.ActiveVessel.parts)
                {
                    string name = RmmScenario.Instance.GetRegisteredDockingPort(part.flightID);
                    if (!String.IsNullOrEmpty(name))
                    {
                        Part dockedPart = RmmUtil.GetDockedPart(FlightGlobals.ActiveVessel, part);
                        if (dockedPart != null)
                        {
                            var routineDepartureMission = RoutineMission.AssemblePotentialRoutineMission<RoutineDepartureMission>(mission);
                            routineDepartureMission.flightIdDepartureDockPart = part.flightID;
                            var check = routineDepartureMission.AllowedVessel(FlightGlobals.ActiveVessel);
                            if (!routineDepartureMission.AllowedVessel(FlightGlobals.ActiveVessel).CheckSuccess) { continue; }
                            routineDepartureMissions.Add(routineDepartureMission);
                        }
                    }
                }
            }
            _routineDepartureOverviewWindow.RoutineDepartureMissions = routineDepartureMissions;
        }

        #endregion Routine Overview

        #region Routine Departure Detail

        private static RoutineDepartureDetailWindow _routineDepartureDetailWindow = null;

        private void HandleRoutineDepartureDetailWindow()
        {
            if (_routineDepartureDetailWindow == null) { return; }
            if (!WindowManager.IsOpen(_routineDepartureDetailWindow))
            {
                CancelRoutineDepartureDetail();
                return;
            }

            _routineDepartureDetailWindow.CrewCount = RmmUtil.CrewCount(_routineDepartureDetailWindow.DepartureParts);

            List<RoutineDepartureDetailWindow.ResourceItem> resources = new List<RoutineDepartureDetailWindow.ResourceItem>();
            foreach (String proppelant in _routineDepartureDetailWindow.RoutineDepartureMission.Proppelants)
            {
                foreach (MissionResource missionResource in _routineDepartureDetailWindow.RoutineDepartureMission.Resources)
                {
                    if (missionResource.Name == proppelant)
                    {
                        RoutineDepartureDetailWindow.ResourceItem resourceItem = new RoutineDepartureDetailWindow.ResourceItem
                        {
                            Name = missionResource.Name,
                            RequiredAmount = missionResource.Amount,
                            CurrentAmount = RmmUtil.ReadResource(_routineDepartureDetailWindow.DepartureParts, missionResource.Name),
                        };
                        resources.Add(resourceItem);
                    }
                }
            }
            _routineDepartureDetailWindow.Resources = resources;

            double currentVesselCargoMass = 0;
            double currentVesselCargoFunds = 0;
            List<MissionResource> vesselResources=  MissionResource.GetMissionResourceList(_routineDepartureDetailWindow.DepartureParts);
            foreach (MissionResource vesselResource in vesselResources)
            {
                if (_routineDepartureDetailWindow.RoutineDepartureMission.Proppelants.Contains(vesselResource.Name)) { continue; }

                currentVesselCargoMass += RmmUtil.Mass(vesselResource.Name, vesselResource.Amount);
                currentVesselCargoFunds += RmmUtil.Cost(vesselResource.Name, vesselResource.Amount);
            }
            _routineDepartureDetailWindow.CurrentCargoMass = currentVesselCargoMass;
            _routineDepartureDetailWindow.CurrentCargoFunds = currentVesselCargoFunds;
        }

        public void CancelRoutineDepartureDetail()
        {
            if (_routineDepartureDetailWindow != null)
            {
                WindowManager.Close(_routineDepartureDetailWindow);
                _routineDepartureDetailWindow = null;
            }
        }

        public void RoutineDepartureDetail(string missiondId, uint flightIdDepartureDockPart, IWindow parent = null)
        {
            CancelRoutineDepartureDetail();
            _routineDepartureDetailWindow = new RoutineDepartureDetailWindow(this);
            WindowManager.Open(_routineDepartureDetailWindow, parent: parent);

            RoutineDepartureMission routineDepartureMission = null;
            foreach (Mission mission in _missions)
            {
                if (mission.Info == null) { continue; }
                if (mission.Info.Type != 20) { continue; }
                if (mission.MissionId == missiondId)
                {
                    routineDepartureMission = RoutineMission.AssemblePotentialRoutineMission<RoutineDepartureMission>(mission);
                    routineDepartureMission.flightIdDepartureDockPart = flightIdDepartureDockPart;
                    if (!routineDepartureMission.AllowedVessel(FlightGlobals.ActiveVessel).CheckSuccess) { continue; }
                }
            }
            _routineDepartureDetailWindow.RoutineDepartureMission = routineDepartureMission;
            _routineDepartureDetailWindow.DepartureParts = RmmUtil.GetDockedParts(FlightGlobals.ActiveVessel, RmmUtil.GetDockedPart(FlightGlobals.ActiveVessel, RmmUtil.GetVesselPart(FlightGlobals.ActiveVessel, flightIdDepartureDockPart)));
        }

        public void RoutineDepartureDetail(RoutineDepartureMission routineDepartureMission, IWindow parent = null)
        {
            CancelRoutineDepartureDetail();
            _routineDepartureDetailWindow = new RoutineDepartureDetailWindow(this);
            WindowManager.Open(_routineDepartureDetailWindow, parent: parent);

            _routineDepartureDetailWindow.RoutineDepartureMission = routineDepartureMission;
        }

        public CheckList OrderDepartureMissionAllowed()
        {
            var checkList = new CheckList();

            checkList.Check(_routineDepartureDetailWindow != null, "no window is open");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_routineDepartureDetailWindow.RoutineDepartureMission != null, "no mission selected");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_routineDepartureDetailWindow.RoutineDepartureMission.Allowed(FlightGlobals.ActiveVessel), "order not allowed");

            return checkList;
        }

        public void OrderDepartureMission()
        {
            if (_routineDepartureDetailWindow == null) { return; }

            if (_routineDepartureDetailWindow.RoutineDepartureMission != null)
            {
                var mission = _routineDepartureDetailWindow.RoutineDepartureMission;

                mission.DepartureTime = Planetarium.GetUniversalTime();

                mission.OrderRoutineMission();

                _routineDepartureMissions = null;
            }

        }

        public void CancelDepartureMission()
        {
            if (_routineDepartureDetailWindow == null) { return; }

            if (_routineDepartureDetailWindow.RoutineDepartureMission != null)
            {
                var mission = _routineDepartureDetailWindow.RoutineDepartureMission;

                mission.UnorderRoutineMission();

                _routineDepartureMissions = null;
            }
        }

        #endregion Routine Detail


        #region Routine Retrieval Overview

        private static RoutineRetrievalOverviewWindow _routineRetrievalOverviewWindow = null;

        private void HandleRoutineRetrievalOverviewWindow()
        {
            if (_routineRetrievalOverviewWindow == null) { return; }
            if (!WindowManager.IsOpen(_routineRetrievalOverviewWindow))
            {
                CancelRoutineRetrievalOverview();
                return;
            }

        }

        public void CancelRoutineRetrievalOverview()
        {
            if (_routineRetrievalOverviewWindow != null)
            {
                WindowManager.Close(_routineRetrievalOverviewWindow);
                _routineRetrievalOverviewWindow = null;
            }
        }

        public void RoutineRetrievalOverview(IWindow parent = null)
        {
            _missions = Mission.LoadMissions();
            CancelRoutineRetrievalOverview();
            _routineRetrievalOverviewWindow = new RoutineRetrievalOverviewWindow(this);
            WindowManager.Open(_routineRetrievalOverviewWindow, parent: parent);

            RefreshRoutineRetrievalOverview();
        }

        public void RefreshRoutineRetrievalOverview()
        {
            if (_routineRetrievalOverviewWindow == null) { return; }

            List<RoutineRetrievalMission> routineRetrievalMissions = new List<RoutineRetrievalMission>();
            LoggerRmm.Debug("RefreshRoutineRetrievalOverview: checking " + _missions.Count + " missions");
            foreach (Mission mission in _missions)
            {
                if (mission.Info == null) { LoggerRmm.Debug("  mission has null Info"); continue; }
                if (mission.Info.Type != 30) { continue; }
                LoggerRmm.Debug("  found Type 30 mission: " + mission.Info.Name + " id=" + mission.MissionId);
                var locationRoutineRetrievalMission = RoutineMission.AssemblePotentialRoutineMission<RoutineRetrievalMission>(mission);
                if (locationRoutineRetrievalMission == null) { LoggerRmm.Debug("  AssemblePotentialRoutineMission returned null (Valid failed)"); continue; }
                LoggerRmm.Debug("  Valid passed");
                var locationCheck = locationRoutineRetrievalMission.AllowedLocation(FlightGlobals.ActiveVessel);
                if (!locationCheck.CheckSuccess) { LoggerRmm.Debug("  AllowedLocation failed: " + String.Join(", ", locationCheck.Messages.ToArray())); continue; }
                LoggerRmm.Debug("  AllowedLocation passed, checking docking ports");
                foreach (Part part in FlightGlobals.ActiveVessel.parts)
                {
                    string name = RmmScenario.Instance.GetRegisteredDockingPort(part.flightID);
                    if (!String.IsNullOrEmpty(name))
                    {
                        LoggerRmm.Debug("    registered port: " + name + " flightID=" + part.flightID);
                        Part dockedPart = RmmUtil.GetDockedPart(FlightGlobals.ActiveVessel, part);
                        if (dockedPart != null)
                        {
                            LoggerRmm.Debug("    has docked vessel");
                            var routineRetrievalMission = RoutineMission.AssemblePotentialRoutineMission<RoutineRetrievalMission>(mission);
                            routineRetrievalMission.flightIdDepartureDockPart = part.flightID;
                            var check = routineRetrievalMission.AllowedVessel(FlightGlobals.ActiveVessel);
                            if (!check.CheckSuccess) { LoggerRmm.Debug("    AllowedVessel failed: " + String.Join(", ", check.Messages.ToArray())); continue; }
                            LoggerRmm.Debug("    AllowedVessel passed! Adding mission.");
                            routineRetrievalMissions.Add(routineRetrievalMission);
                        }
                        else
                        {
                            LoggerRmm.Debug("    no docked vessel on this port");
                        }
                    }
                }
            }
            LoggerRmm.Debug("RefreshRoutineRetrievalOverview: found " + routineRetrievalMissions.Count + " valid missions");
            _routineRetrievalOverviewWindow.RoutineRetrievalMissions = routineRetrievalMissions;
        }

        #endregion Routine Retrieval Overview

        #region Routine Retrieval Detail

        private static RoutineRetrievalDetailWindow _routineRetrievalDetailWindow = null;

        private void HandleRoutineRetrievalDetailWindow()
        {
            if (_routineRetrievalDetailWindow == null) { return; }
            if (!WindowManager.IsOpen(_routineRetrievalDetailWindow))
            {
                CancelRoutineRetrievalDetail();
                return;
            }

            if (_routineRetrievalDetailWindow.DepartureParts == null || _routineRetrievalDetailWindow.RoutineRetrievalMission == null) { return; }

            _routineRetrievalDetailWindow.CrewCount = RmmUtil.CrewCount(_routineRetrievalDetailWindow.DepartureParts);

            List<RoutineRetrievalDetailWindow.ResourceItem> resources = new List<RoutineRetrievalDetailWindow.ResourceItem>();
            foreach (String propellant in _routineRetrievalDetailWindow.RoutineRetrievalMission.Propellants)
            {
                foreach (MissionResource missionResource in _routineRetrievalDetailWindow.RoutineRetrievalMission.Resources)
                {
                    if (missionResource.Name == propellant)
                    {
                        // Calculate fuel consumed (departure - arrival) as the required amount
                        double requiredAmount = missionResource.Amount; // fallback to departure amount
                        var mission = _routineRetrievalDetailWindow.RoutineRetrievalMission;
                        if (mission != null)
                        {
                            double consumed = mission.GetPropellantConsumedAmount(missionResource.Name);
                            requiredAmount = consumed;
                        }

                        RoutineRetrievalDetailWindow.ResourceItem resourceItem = new RoutineRetrievalDetailWindow.ResourceItem
                        {
                            Name = missionResource.Name,
                            RequiredAmount = requiredAmount,
                            CurrentAmount = RmmUtil.ReadResource(_routineRetrievalDetailWindow.DepartureParts, missionResource.Name),
                        };
                        resources.Add(resourceItem);
                    }
                }
            }
            _routineRetrievalDetailWindow.Resources = resources;

            // Calculate cargo returned (non-propellant resources gained: arrival amount - departure amount)
            List<RoutineRetrievalDetailWindow.CargoItem> cargoReturned = new List<RoutineRetrievalDetailWindow.CargoItem>();
            var retrievalMission = _routineRetrievalDetailWindow.RoutineRetrievalMission;
            if (retrievalMission != null && retrievalMission.ArrivalResources != null)
            {
                foreach (MissionResource arrivalResource in retrievalMission.ArrivalResources)
                {
                    if (retrievalMission.Propellants.Contains(arrivalResource.Name)) { continue; }
                    if (arrivalResource.Name == "ElectricCharge") { continue; }

                    // Find departure amount for this resource
                    double departureAmount = 0;
                    foreach (MissionResource depResource in retrievalMission.Resources)
                    {
                        if (depResource.Name == arrivalResource.Name) { departureAmount = depResource.Amount; break; }
                    }

                    double gained = arrivalResource.Amount - departureAmount;
                    if (gained > 0.01)
                    {
                        cargoReturned.Add(new RoutineRetrievalDetailWindow.CargoItem
                        {
                            Name = arrivalResource.Name,
                            Amount = gained,
                        });
                    }
                }
            }
            _routineRetrievalDetailWindow.CargoReturned = cargoReturned;
        }

        public void CancelRoutineRetrievalDetail()
        {
            if (_routineRetrievalDetailWindow != null)
            {
                WindowManager.Close(_routineRetrievalDetailWindow);
                _routineRetrievalDetailWindow = null;
            }
        }

        public void RoutineRetrievalDetail(string missionId, uint flightIdDepartureDockPart, IWindow parent = null)
        {
            CancelRoutineRetrievalDetail();
            _routineRetrievalDetailWindow = new RoutineRetrievalDetailWindow(this);
            WindowManager.Open(_routineRetrievalDetailWindow, parent: parent);

            RoutineRetrievalMission routineRetrievalMission = null;
            foreach (Mission mission in _missions)
            {
                if (mission.Info == null) { continue; }
                if (mission.Info.Type != 30) { continue; }
                if (mission.MissionId == missionId)
                {
                    routineRetrievalMission = RoutineMission.AssemblePotentialRoutineMission<RoutineRetrievalMission>(mission);
                    routineRetrievalMission.flightIdDepartureDockPart = flightIdDepartureDockPart;
                    if (!routineRetrievalMission.AllowedVessel(FlightGlobals.ActiveVessel).CheckSuccess) { continue; }
                }
            }
            _routineRetrievalDetailWindow.RoutineRetrievalMission = routineRetrievalMission;
            _routineRetrievalDetailWindow.DepartureParts = RmmUtil.GetDockedParts(FlightGlobals.ActiveVessel, RmmUtil.GetDockedPart(FlightGlobals.ActiveVessel, RmmUtil.GetVesselPart(FlightGlobals.ActiveVessel, flightIdDepartureDockPart)));
        }

        public void RoutineRetrievalDetail(RoutineRetrievalMission routineRetrievalMission, IWindow parent = null)
        {
            CancelRoutineRetrievalDetail();
            _routineRetrievalDetailWindow = new RoutineRetrievalDetailWindow(this);
            WindowManager.Open(_routineRetrievalDetailWindow, parent: parent);

            _routineRetrievalDetailWindow.RoutineRetrievalMission = routineRetrievalMission;
        }

        public CheckList OrderRetrievalMissionAllowed()
        {
            var checkList = new CheckList();

            checkList.Check(_routineRetrievalDetailWindow != null, "no window is open");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_routineRetrievalDetailWindow.RoutineRetrievalMission != null, "no mission selected");
            if (!checkList.CheckSuccess) { return checkList; }

            checkList.Check(_routineRetrievalDetailWindow.RoutineRetrievalMission.Allowed(FlightGlobals.ActiveVessel), "order not allowed");

            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                checkList.Check(Funding.Instance.Funds > _routineRetrievalDetailWindow.RoutineRetrievalMission.Price, "insufficient funds");
            }

            return checkList;
        }

        public void OrderRetrievalMission()
        {
            if (_routineRetrievalDetailWindow == null) { return; }

            if (_routineRetrievalDetailWindow.RoutineRetrievalMission != null)
            {
                var mission = _routineRetrievalDetailWindow.RoutineRetrievalMission;

                mission.DepartureTime = Planetarium.GetUniversalTime();
                mission.ArrivalTime = 0;
                mission.CrewCount = mission.MinimumCrew;

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    Funding.Instance.AddFunds(-mission.Price, TransactionReasons.VesselRollout);
                }

                mission.OrderRoutineMission();

                _routineRetrievalMissions = null;
            }
        }

        public void CancelRetrievalMission()
        {
            if (_routineRetrievalDetailWindow == null) { return; }

            if (_routineRetrievalDetailWindow.RoutineRetrievalMission != null)
            {
                var mission = _routineRetrievalDetailWindow.RoutineRetrievalMission;

                if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
                {
                    Funding.Instance.AddFunds(mission.Price, TransactionReasons.VesselRollout);
                }

                mission.UnorderRoutineMission();

                _routineRetrievalMissions = null;
            }
        }

        #endregion Routine Retrieval Detail


        #region Crew Selection

        private static CrewSelectionWindow _crewSelectionWindow = null;

        private void HandleCrewSelectionWindow()
        {
            if (_crewSelectionWindow == null) { return; }
            if (!WindowManager.IsOpen(_crewSelectionWindow))
            {
                CancelCrewSelection();
                return;
            }
        }

        public void CancelCrewSelection()
        {
            if (_crewSelectionWindow != null)
            {
                WindowManager.Close(_crewSelectionWindow);
                _crewSelectionWindow = null;
            }
        }

        public void CrewSelection(string crewSelectionString, IWindow parent = null)
        {
            CancelCrewSelection();
            _crewSelectionWindow = new CrewSelectionWindow(this, crewSelectionString, "Planned Crew");
            WindowManager.Open(_crewSelectionWindow, parent: parent);
        }

        public void SetCrewSelection(string crewSelectionString)
        {
            if (_routineDetailWindow != null)
            {
                if (_routineDetailWindow.RoutineArrivalMission != null)
                {
                    _routineDetailWindow.RoutineArrivalMission.CrewSelection = crewSelectionString;
                }
            }
        }

        #endregion Crew Selection

        #region Docking Port Selection

        private static DockingPortSelectionWindow _dockingPortSelectionWindow = null;

        private void HandleDockingPortSelectionWindow()
        {
            if (_dockingPortSelectionWindow == null) { return; }
            if (!WindowManager.IsOpen(_dockingPortSelectionWindow))
            {
                CancelDockingPortSelection();
                return;
            }
        }

        public void CancelDockingPortSelection()
        {
            if (_dockingPortSelectionWindow != null)
            {
                WindowManager.Close(_dockingPortSelectionWindow);
                _dockingPortSelectionWindow = null;
            }
        }

        public void DockingPortSelection(uint dockingPortFlightId, IWindow parent = null)
        {
            CancelDockingPortSelection();
            _dockingPortSelectionWindow = new DockingPortSelectionWindow(this, dockingPortFlightId);
            WindowManager.Open(_dockingPortSelectionWindow, parent: parent);

            List<RegisteredDockingPort> registeredDockingPorts = new List<RegisteredDockingPort>();
            foreach (Part part in FlightGlobals.ActiveVessel.parts)
            {
                string name = RmmScenario.Instance.GetRegisteredDockingPort(part.flightID);
                if (!String.IsNullOrEmpty(name))
                {
                    registeredDockingPorts.Add(new RegisteredDockingPort
                    {
                        flightId = part.flightID,
                        Name = name,
                    });
                }
            }
            _dockingPortSelectionWindow.DockingPorts = registeredDockingPorts;
        }

        public void SetDockingPortSelection(uint dockingPortFlightId)
        {
            if (_routineDetailWindow != null)
            {
                if (_routineDetailWindow.RoutineArrivalMission != null)
                {
                    _routineDetailWindow.RoutineArrivalMission.flightIdArrivalDockPart = dockingPortFlightId;
                }
            }
        }

        #endregion Docking Port Selection


        #region Ordered Missions

        private static OrderedMissionsWindow _orderedMissionsWindow = null;

        private void HandleOrderedMissionsWindow()
        {
            if (_orderedMissionsWindow == null) { return; }
            if (!WindowManager.IsOpen(_orderedMissionsWindow))
            {
                CancelOrderedMissions();
                return;
            }

            if (_routineArrivalMissions != null &&
                (_orderedMissionsWindow.RoutineArrivalMissions == null || _orderedMissionsWindow.RoutineArrivalMissions.Count != _routineArrivalMissions.Count))
            {
                _orderedMissionsWindow.RoutineArrivalMissions = _routineArrivalMissions;
            }
            if (_routineDepartureMissions != null &&
                (_orderedMissionsWindow.RoutineDepartureMissions == null || _orderedMissionsWindow.RoutineDepartureMissions.Count != _routineDepartureMissions.Count))
            {
                _orderedMissionsWindow.RoutineDepartureMissions = _routineDepartureMissions;
            }
            if (_routineRetrievalMissions != null &&
                (_orderedMissionsWindow.RoutineRetrievalMissions == null || _orderedMissionsWindow.RoutineRetrievalMissions.Count != _routineRetrievalMissions.Count))
            {
                _orderedMissionsWindow.RoutineRetrievalMissions = _routineRetrievalMissions;
            }
        }

        public void CancelOrderedMissions()
        {
            if (_orderedMissionsWindow != null)
            {
                WindowManager.Close(_orderedMissionsWindow);
                _orderedMissionsWindow = null;
            }
        }

        public void OrderedMissions(IWindow parent = null)
        {
            CancelOrderedMissions();
            _orderedMissionsWindow = new OrderedMissionsWindow(this);
            WindowManager.Open(_orderedMissionsWindow, parent: parent);

            _orderedMissionsWindow.RoutineArrivalMissions = _routineArrivalMissions;
            _orderedMissionsWindow.RoutineDepartureMissions = _routineDepartureMissions;
            _orderedMissionsWindow.RoutineRetrievalMissions = _routineRetrievalMissions;
        }

        public void SetOrderedMissions(uint dockingPortFlightId)
        {
            if (_routineDetailWindow != null)
            {
                if (_routineDetailWindow.RoutineArrivalMission != null)
                {
                    _routineDetailWindow.RoutineArrivalMission.flightIdArrivalDockPart = dockingPortFlightId;
                }
            }
        }

        #endregion Ordered Missions

        private void LoadRoutineArrivalMissions()
        {
            if (_missions == null) { return; }
            if (_routineArrivalMissions == null)
            {
                _routineArrivalMissions = new List<RoutineArrivalMission>();
            }
            else
            {
                _routineArrivalMissions.Clear();
            }

            List<string> orders = RmmScenario.Instance.GetOrdersOfType(10);
            foreach (string orderId in orders)
            {
                RoutineArrivalMission routineArrivalMission = RoutineArrivalMission.AssembleRoutineMission<RoutineArrivalMission>(orderId, _missions);
                if (routineArrivalMission == null) { continue; }
                _routineArrivalMissions.Add(routineArrivalMission);
            }
        }

        private void LoadRoutineDepartureMissions()
        {
            if (_missions == null) { return; }

            if (_routineDepartureMissions == null)
            {
                _routineDepartureMissions = new List<RoutineDepartureMission>();
            }
            else
            {
                _routineDepartureMissions.Clear();
            }

            List<string> orders = RmmScenario.Instance.GetOrdersOfType(20);
            foreach (string orderId in orders)
            {
                RoutineDepartureMission routineDepartureMission = RoutineDepartureMission.AssembleRoutineMission<RoutineDepartureMission>(orderId, _missions);
                if (routineDepartureMission == null) { continue; }
                _routineDepartureMissions.Add(routineDepartureMission);
            }
        }

        private void LoadRoutineRetrievalMissions()
        {
            if (_missions == null) { return; }

            if (_routineRetrievalMissions == null)
            {
                _routineRetrievalMissions = new List<RoutineRetrievalMission>();
            }
            else
            {
                _routineRetrievalMissions.Clear();
            }

            List<string> orders = RmmScenario.Instance.GetOrdersOfType(30);
            foreach (string orderId in orders)
            {
                RoutineRetrievalMission routineRetrievalMission = RoutineRetrievalMission.AssembleRoutineMission<RoutineRetrievalMission>(orderId, _missions);
                if (routineRetrievalMission == null) { continue; }
                _routineRetrievalMissions.Add(routineRetrievalMission);
            }
        }

        private bool SkyClear(Vessel currentVessel)
        {
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (vessel.id != currentVessel.id &&
                    !vessel.packed && vessel.loaded)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
