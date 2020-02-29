using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RoutineOverviewWindow : WindowBase
    {
        RoutineControl _routineControl = null;

        private Vector2 scrollPosition;

        public List<RoutineArrivalMission> RoutineArrivalMissions = null;

        public RoutineOverviewWindow(RoutineControl routineControl) : base("Launch Missions", new Rect(), 300, 60)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(300), GUILayout.Height(200));
            if (RoutineArrivalMissions != null && RoutineArrivalMissions.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission Name", RmmStyle.Instance.LabelStyle, GUILayout.Width(230));
                GUILayout.EndHorizontal();
                foreach (RoutineArrivalMission routineArrivalMission in RoutineArrivalMissions)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(routineArrivalMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(230));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                    {
                        _routineControl.RoutineDetail(routineArrivalMission.MissionId, this);
                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
