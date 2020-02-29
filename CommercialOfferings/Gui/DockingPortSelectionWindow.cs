using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class DockingPortSelectionWindow : WindowBase
    {
        RoutineControl _routineControl = null;
        uint _dockingPortFlightId = 0;

        private Vector2 scrollPosition;

        public List<RegisteredDockingPort> DockingPorts = null;


        public DockingPortSelectionWindow(RoutineControl routineControl, uint dockingPortFlightId) : base("Docking Port", new Rect(), 200, 60)
        {
            _routineControl = routineControl;
            _dockingPortFlightId = dockingPortFlightId;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true, GUILayout.Width(200), GUILayout.Height(200));
            if (DockingPorts != null && DockingPorts.Count > 0)
            {
                
                foreach (RegisteredDockingPort dockingPort in DockingPorts)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("<<", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                    {
                        _routineControl.SetDockingPortSelection(dockingPort.flightId);
                        Close();
                    }
                    GUILayout.Label(dockingPort.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(140));
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();
        }
    }
}
