using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RoutineDepartureOverviewWindow : WindowBase
    {
        RoutineControl _routineControl = null;

        private Vector2 scrollPosition;

        public List<RoutineDepartureMission> RoutineDepartureMissions = null;

        public RoutineDepartureOverviewWindow(RoutineControl routineControl) : base("Departure Missions", new Rect(), 300, 60)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300), GUILayout.Height(200));
            if (RoutineDepartureMissions != null && RoutineDepartureMissions.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission Name", RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                GUILayout.Label("Docking Port", RmmStyle.Instance.LabelStyle, GUILayout.Width(90));
                GUILayout.EndHorizontal();
                foreach (RoutineDepartureMission routineDepartureMission in RoutineDepartureMissions)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(routineDepartureMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                    GUILayout.Label(RmmScenario.Instance.GetRegisteredDockingPort(routineDepartureMission.flightIdDepartureDockPart), RmmStyle.Instance.LabelStyle, GUILayout.Width(90));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _routineControl.RoutineDepartureDetail(routineDepartureMission.MissionId, routineDepartureMission.flightIdDepartureDockPart, this);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
