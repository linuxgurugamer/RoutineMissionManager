using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RoutineRetrievalOverviewWindow : WindowBase
    {
        RoutineControl _routineControl = null;

        private Vector2 scrollPosition;

        public List<RoutineRetrievalMission> RoutineRetrievalMissions = null;

        public RoutineRetrievalOverviewWindow(RoutineControl routineControl) : base("Retrieval Missions", new Rect(), 300, 60)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300), GUILayout.Height(200));
            if (RoutineRetrievalMissions != null && RoutineRetrievalMissions.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission Name", RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                GUILayout.Label("Docking Port", RmmStyle.Instance.LabelStyle, GUILayout.Width(90));
                GUILayout.EndHorizontal();
                foreach (RoutineRetrievalMission routineRetrievalMission in RoutineRetrievalMissions)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(routineRetrievalMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                    GUILayout.Label(RmmScenario.Instance.GetRegisteredDockingPort(routineRetrievalMission.flightIdDepartureDockPart), RmmStyle.Instance.LabelStyle, GUILayout.Width(90));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _routineControl.RoutineRetrievalDetail(routineRetrievalMission.MissionId, routineRetrievalMission.flightIdDepartureDockPart, this);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
