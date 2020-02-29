using CommercialOfferings.Gui;
using CommercialOfferings.MissionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings
{
    class TrackingControl
    {
        private TrackingWindow _trackingWindow = null;

        private double _nextLogicTime = 1;

        public void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }

            HandleTrackingWindow();
            HandleStartTrackingWindow();

            HandleTrackingOverviewWindow();
            HandleTrackingDetailWindow();

            _nextLogicTime = Planetarium.GetUniversalTime() + 1;
        }

        #region Tracking

        private void HandleTrackingWindow()
        {
            List<Tracking> trackings = GetActiveTrackings();
            if (trackings.Count() == 0)
            {
                if (_trackingWindow != null)
                {
                    WindowManager.Close(_trackingWindow);
                    _trackingWindow = null;
                }
            }
            else
            {
                if (_trackingWindow == null)
                {
                    _trackingWindow = new TrackingWindow(this);
                    WindowManager.Open(_trackingWindow);
                }

                _trackingWindow.ActiveTrackings = trackings;
            }
        }


        private List<Tracking> GetActiveTrackings()
        {
            List<Tracking> trackings = new List<Tracking>();

            // Check active vessel
            Vessel activeVessel = FlightGlobals.ActiveVessel;
            if (activeVessel != null)
            {
                foreach (Part part in activeVessel.Parts)
                {
                    var trackingModule = part.Modules.OfType<RMMModule>().FirstOrDefault();
                    if (trackingModule != null && trackingModule.trackingPrimary)
                    {
                        trackings.Add(trackingModule.TrackingModule);
                    }
                }
            }

            // Check other vessels
            foreach (Vessel vessel in FlightGlobals.Vessels)
            {
                if (activeVessel != null && vessel == activeVessel) { continue; }
                foreach (Part part in vessel.Parts)
                {
                    var trackingModule = part.Modules.OfType<RMMModule>().FirstOrDefault();
                    if (trackingModule != null && trackingModule.trackingPrimary)
                    {
                        trackings.Add(trackingModule.TrackingModule);
                    }
                }
            }

            return trackings;
        }

        #endregion Tracking

        #region Create Tracking

        //private static bool _startTracking = false;
        private static StartTrackingWindow _startTrackingWindow = null;
        private static Vessel _startTrackingVessel = null;
        private static Part _startDockingPort = null;

        private void HandleStartTrackingWindow()
        {
            if (_startTrackingWindow == null) { return; }
            if (!WindowManager.IsOpen(_startTrackingWindow))
            {
                CancelStartTracking();
                return;
            }
            if (_startTrackingVessel != null)
            {
                if (RmmUtil.IsTrackingActive(FlightGlobals.ActiveVessel) ||
                    !RmmUtil.IsPreLaunch(FlightGlobals.ActiveVessel))
                {
                    CancelStartTracking();
                    return;
                }

                _startTrackingWindow.VesselName = _startTrackingVessel.vesselName;
                _startTrackingWindow.Price = RmmUtil.CalculateVesselPrice(_startTrackingVessel);
                _startTrackingWindow.MinimumCrew = RmmUtil.AstronautCrewCount(_startTrackingVessel);
            }
            else if (_startDockingPort != null)
            {
                if (FlightGlobals.ActiveVessel.situation != Vessel.Situations.ORBITING)
                {
                    CancelStartTracking();
                    return;
                }

                List<Part> dockedVesselParts = RmmUtil.GetDockedParts(_startDockingPort.vessel, _startDockingPort);
                // determine minimum crew
                _startTrackingWindow.MinimumCrew = RmmUtil.AstronautCrewCount(dockedVesselParts);

                // determine minimum resources
                List<string> propellants = RmmUtil.DetermineProppellantArray(dockedVesselParts);
                List<Resource> minimumResources = new List<Resource>();

                foreach (String propellant in propellants)
                {
                    var amount = RmmUtil.ReadResource(dockedVesselParts, propellant);
                    if (amount != 0)
                    {
                        Resource resource = new Resource
                        {
                            Name = propellant,
                            Amount = amount,
                        };
                        minimumResources.Add(resource);
                    }
                }

                _startTrackingWindow.MinimumResources = minimumResources;
            }
        }

        public void StartTracking(string name)
        {
            Tracking tracking = null;

            if (_startTrackingVessel != null)
            {
                foreach (Part part in _startTrackingVessel.parts)
                {


                    var trackingModule = part.Modules.OfType<RMMModule>().FirstOrDefault();
                    if (trackingModule != null)
                    {
                        tracking = trackingModule.TrackingModule;
                        break;
                    }
                }
            }

            if (_startDockingPort != null)
            {
                List<Part> dockedVesselParts = RmmUtil.GetDockedParts(_startDockingPort.vessel, _startDockingPort);
                foreach (Part part in dockedVesselParts)
                {
                    var trackingModule = part.Modules.OfType<RMMModule>().FirstOrDefault();
                    if (trackingModule != null)
                    {
                        tracking = trackingModule.TrackingModule;
                        break;
                    }
                }
            }

            if (tracking == null)
            {
                LoggerRmm.Warning("no tracking module on vessel");
                return;
            }

            if (_startTrackingVessel != null)
            {
                tracking.StartLaunchMission(name);
            }

            if (_startDockingPort != null)
            {
                tracking.StartDepartMission(name, _startDockingPort);
            }

            CancelStartTracking();
        }


        public void CancelStartTracking()
        {
            //_startTracking = false;
            if (_startTrackingWindow != null)
            {
                WindowManager.Close(_startTrackingWindow);
                _startTrackingWindow = null;
            }
            _startTrackingVessel = null;
            _startDockingPort = null;
        }

        public void CreateLaunchTracking(Vessel vessel)
        {
            //_startTracking = true;
            _startTrackingWindow = new StartTrackingWindow(this, 10);
            WindowManager.Open(_startTrackingWindow);
            _startTrackingVessel = vessel;
            _startDockingPort = null;
        }

        public void CreateDepartureTracking(Part dockingPort)
        {
            //_startTracking = true;
            _startTrackingWindow = new StartTrackingWindow(this, 20);
            WindowManager.Open(_startTrackingWindow);
            _startTrackingVessel = null;
            _startDockingPort = dockingPort;
        }

        #endregion Create Tracking

        #region Tracked Overview

        private static TrackingOverviewWindow _trackingOverviewWindow = null;

        private void HandleTrackingOverviewWindow()
        {
            if (_trackingOverviewWindow == null) { return; }
            if (!WindowManager.IsOpen(_trackingOverviewWindow))
            {
                CancelTrackingOverview();
                return;
            }
        }

        public void CancelTrackingOverview()
        {
            if (_trackingOverviewWindow != null)
            {
                WindowManager.Close(_trackingOverviewWindow);
                _trackingOverviewWindow = null;
            }
        }

        public void TrackingOverview(IWindow parent = null)
        {
            _trackingOverviewWindow = new TrackingOverviewWindow(this);
            WindowManager.Open(_trackingOverviewWindow, parent: parent);

            FillTrackingOverview();
        }

        public void FillTrackingOverview(IWindow parent = null)
        {
            if (_trackingOverviewWindow == null) { return; }

            List<Mission> missions = Mission.LoadMissions();
            List<TrackingOverviewWindow.MissionItem> missionItems = new List<TrackingOverviewWindow.MissionItem>();
            foreach (Mission mission in missions)
            {
                TrackingOverviewWindow.MissionItem missionItem = new TrackingOverviewWindow.MissionItem();
                missionItem.Mission = mission;
                switch (mission.Info.Type)
                {
                    case 10:
                        missionItem.ValidCheckList = RoutineMission.RoutineMissionValid<RoutineArrivalMission>(mission);
                        break;
                    case 20:
                        missionItem.ValidCheckList = RoutineMission.RoutineMissionValid<RoutineDepartureMission>(mission);
                        break;
                    default:
                        continue;
                }
                missionItems.Add(missionItem);
            }

            _trackingOverviewWindow.MissionItems = missionItems;
        }

        #endregion Tracked Overview

        #region Tracking Detail

        private static TrackingDetailWindow _trackingDetailWindow = null;

        private void HandleTrackingDetailWindow()
        {
            if (_trackingDetailWindow == null) { return; }
            if (!WindowManager.IsOpen(_trackingDetailWindow))
            {
                CancelTrackingDetail();
                return;
            }
        }

        public void CancelTrackingDetail()
        {
            if (_trackingDetailWindow != null)
            {
                WindowManager.Close(_trackingDetailWindow);
                _trackingDetailWindow = null;
            }
        }

        public void TrackingDetail(Mission trackingMission, IWindow parent = null, Tracking vesselTracking = null)
        {
            CancelTrackingDetail();
            _trackingDetailWindow = new TrackingDetailWindow(this);
            WindowManager.Open(_trackingDetailWindow, parent: parent);

            _trackingDetailWindow.TrackingMission = trackingMission;
            _trackingDetailWindow.VesselTracking = vesselTracking;
            switch (trackingMission.Info.Type)
            {
                case 10:
                    _trackingDetailWindow.ValidCheckList = RoutineMission.RoutineMissionValid<RoutineArrivalMission>(trackingMission);
                    break;
                case 20:
                    _trackingDetailWindow.ValidCheckList = RoutineMission.RoutineMissionValid<RoutineDepartureMission>(trackingMission);
                    break;
            }
        }

        public void EndVesselTracking(Tracking vesselTracking)
        {
            vesselTracking.AbortTracking();
            CancelTrackingDetail();
        }

        public void DeleteTracking(Mission trackingMission)
        {
            trackingMission.DeleteMission();
            CancelTrackingDetail();
            FillTrackingOverview();
        }

        #endregion Tracking Detail
    }
}
