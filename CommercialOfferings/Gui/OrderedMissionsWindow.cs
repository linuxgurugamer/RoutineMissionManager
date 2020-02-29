using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class OrderedMissionsWindow : WindowBase
    {
        private Vector2 scrollPositionMissions;

        private RoutineControl _routineControl = null;

        public List<RoutineArrivalMission> RoutineArrivalMissions = null;
        public List<RoutineDepartureMission> RoutineDepartureMissions = null;

        public OrderedMissionsWindow(RoutineControl routineControl) : base("Ordered Missions", new Rect(), 400)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            GUILayout.BeginVertical();


            scrollPositionMissions = GUILayout.BeginScrollView(scrollPositionMissions, false, false, RmmStyle.Instance.HoriScrollBarStyle, RmmStyle.Instance.VertiScrollBarStyle, GUILayout.Width(390), GUILayout.Height(300));

            GUILayout.BeginHorizontal();
            GUILayout.Label("Order", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            GUILayout.Label("Mission Name", RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
            GUILayout.EndHorizontal();

            if (RoutineArrivalMissions != null)
            {
                foreach (RoutineArrivalMission routineArrivalMission in RoutineArrivalMissions)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(routineArrivalMission.OrderId, RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(routineArrivalMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                    {
                        _routineControl.RoutineDetail(routineArrivalMission, this);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (RoutineDepartureMissions != null)
            {
                foreach (RoutineDepartureMission routineDepartureMission in RoutineDepartureMissions)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(routineDepartureMission.OrderId, RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(routineDepartureMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                    GUILayout.Label(RmmScenario.Instance.GetRegisteredDockingPort(routineDepartureMission.flightIdDepartureDockPart), RmmStyle.Instance.LabelStyle, GUILayout.Width(90));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _routineControl.RoutineDepartureDetail(routineDepartureMission, this);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
