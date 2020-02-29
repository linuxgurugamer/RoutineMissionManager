using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class MenuWindow : WindowBase
    {
        private RmmMonoBehaviour _rmmMonoBehaviour = null;

        public bool TrackingEnabled = false;
        public bool RoutineArrivalEnabled = false;
        public bool RoutineDepartureEnabled = false;
        public bool RegisterDockingPortsEnabled = false;


        public MenuWindow(RmmMonoBehaviour rmmMonoBehaviour) : base("Routine Mission Manager    ", new Rect(250, 250, 200, 100), 200, 100)
        {
            _rmmMonoBehaviour = rmmMonoBehaviour;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label("Tracking", RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            if (TrackingEnabled)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Start Tracking", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.StartTrackingOption();
                }
                GUILayout.EndHorizontal();
            }

            if (true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Tracked Missions", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.TrackedMissionsOption();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("Ordering", RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            if (RoutineArrivalEnabled)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Order Launch", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.RoutineLaunchOption();
                }
                GUILayout.EndHorizontal();
            }

            if (RoutineDepartureEnabled)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Order Departure", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.RoutineDepartureOption();
                }
                GUILayout.EndHorizontal();
            }

            if (true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Ordered Missions", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.OrderedMissionsOption();
                }
                GUILayout.EndHorizontal();
            }

            if (RegisterDockingPortsEnabled)
            {
                GUILayout.Label("Locations", RmmStyle.Instance.LabelStyle, GUILayout.Width(200));

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Register Docking Ports", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.RegisterDockingPortsOption();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Label(" ", RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            if (true)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Manual", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
                {
                    _rmmMonoBehaviour.ManualOption();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}
