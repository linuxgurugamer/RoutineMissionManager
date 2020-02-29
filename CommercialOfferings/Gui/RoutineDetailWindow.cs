using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RoutineDetailWindow : WindowBase
    {
        RoutineControl _routineControl = null;

        private Vector2 scrollPosition;

        public RoutineArrivalMission RoutineArrivalMission = null;

        private string strCrewCount = "";
        private uint arrivalDockingPortFlightId = 0;
        private string strArrivalDockingPortName = "";
        private List<string> messages;

        public RoutineDetailWindow(RoutineControl routineControl) : base("Launch Mission", new Rect(), 200, 60)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {
            if (RoutineArrivalMission != null)
            {
                if (RoutineArrivalMission.flightIdArrivalDockPart != arrivalDockingPortFlightId)
                {
                    arrivalDockingPortFlightId = RoutineArrivalMission.flightIdArrivalDockPart;
                    strArrivalDockingPortName = RmmScenario.Instance.GetRegisteredDockingPort(arrivalDockingPortFlightId);
                }

                if (RoutineArrivalMission.Kind == MissionKind.Potential)
                { 
                    if (RoutineArrivalMission.CrewCount < RoutineArrivalMission.MinimumCrew)
                    {
                        RoutineArrivalMission.CrewCount = RoutineArrivalMission.MinimumCrew;
                    }
                    if (RoutineArrivalMission.CrewCount > RoutineArrivalMission.CrewCapacity)
                    {
                        RoutineArrivalMission.CrewCount = RoutineArrivalMission.CrewCapacity;
                    }
                }
            }
        }

        public override void WindowUI()
        {
            if (RoutineArrivalMission != null)
            {
                if (RoutineArrivalMission.Kind == MissionKind.Ordered)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Order:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RoutineArrivalMission.OrderId, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RoutineArrivalMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("System:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RoutineArrivalMission.VesselName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Price:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(Math.Round(RoutineArrivalMission.Price).ToString() , RmmStyle.Instance.LabelStyle, GUILayout.Width(250));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Duration:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RmmUtil.TimeString(RoutineArrivalMission.Duration), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                if (RoutineArrivalMission.Kind == MissionKind.Ordered)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ETA:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RmmUtil.TimeEtaString(RoutineArrivalMission.ArrivalTime - Planetarium.GetUniversalTime()), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }

                if (RoutineArrivalMission.MinimumCrew > 0)
                {
                    if (RoutineArrivalMission.MinimumCrew < RoutineArrivalMission.CrewCapacity)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Minimal crew required:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(RoutineArrivalMission.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Maximum crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(RoutineArrivalMission.CrewCapacity.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                        GUILayout.Label(RoutineArrivalMission.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                }

                if (RoutineArrivalMission.MinimumCrew < RoutineArrivalMission.CrewCapacity)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Planned crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RoutineArrivalMission.CrewCount.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(50));
                    if (RoutineArrivalMission.Kind == MissionKind.Potential)
                    {
                        strCrewCount = GUILayout.TextField(strCrewCount, 3, GUILayout.Width(50));

                        if (GUILayout.Button("set", RmmStyle.Instance.ButtonStyle, GUILayout.Width(50), GUILayout.Height(22)))
                        {
                            int crewCount = 0;
                            int.TryParse(strCrewCount, out crewCount);
                            if (RoutineArrivalMission.CrewCount < RoutineArrivalMission.MinimumCrew) { crewCount = RoutineArrivalMission.MinimumCrew; }
                            if (RoutineArrivalMission.CrewCount > RoutineArrivalMission.CrewCapacity) { crewCount = RoutineArrivalMission.CrewCapacity; }
                            RoutineArrivalMission.CrewCount = crewCount;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (!String.IsNullOrEmpty(RoutineArrivalMission.CrewSelection))
                {
                    GUILayout.Label(RoutineArrivalMission.CrewSelection, RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                }

                if (RoutineArrivalMission.CrewCapacity > 0)
                {
                    GUILayout.BeginHorizontal();
                    if (RoutineArrivalMission.Kind == MissionKind.Potential)
                    {
                        if (GUILayout.Button("set preferred crew", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(20)))
                        {
                            _routineControl.CrewSelection(RoutineArrivalMission.CrewSelection, this);
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Label("   ", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Docking Port:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(strArrivalDockingPortName, RmmStyle.Instance.LabelStyle, GUILayout.Width(50));
                if (RoutineArrivalMission.Kind == MissionKind.Potential)
                {
                    if (GUILayout.Button("set docking port", RmmStyle.Instance.ButtonStyle, GUILayout.Width(150), GUILayout.Height(20)))
                    {
                        _routineControl.DockingPortSelection(RoutineArrivalMission.flightIdArrivalDockPart, this);
                    }
                }
                GUILayout.EndHorizontal();

                if (messages != null && messages.Count > 0)
                {
                    GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                    foreach (String message in messages)
                    {
                        GUILayout.Label(message, RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                    }
                }

                if (RoutineArrivalMission.Kind == MissionKind.Potential)
                {
                    if (GUILayout.Button("Order Launch", RmmStyle.Instance.ButtonStyle, GUILayout.Width(150), GUILayout.Height(22)))
                    {
                        CheckList checkList = _routineControl.OrderArrivalMissionAllowed();
                        if (checkList.CheckSuccess)
                        {
                            _routineControl.OrderArrivalMission();
                            base.Close();
                        }
                        else
                        {
                            messages = checkList.Messages;
                        }
                    }
                }

                if (RoutineArrivalMission.Kind == MissionKind.Ordered)
                {
                    if (GUILayout.Button("Cancel Launch", RmmStyle.Instance.ButtonStyle, GUILayout.Width(150), GUILayout.Height(22)))
                    {
                        _routineControl.CancelArrivalMission();
                        base.Close();
                    }
                }
            }
        }
    }
}
