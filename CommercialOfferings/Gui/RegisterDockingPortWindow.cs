using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RegisterDockingPortWindow : WindowBase
    {
        private RmmDockingPortModule _dockingPort;
        private string portCode = "";
        private string strPortCode = "";

        public RegisterDockingPortWindow(RmmDockingPortModule dockingPort) : base("Register Docking Port", new Rect(((Screen.width - 300) / 2), ((Screen.height - 60) / 2), 300, 60), 300, 60)
        {
            _dockingPort = dockingPort;

            strPortCode = _dockingPort.Name;
        }

        public override void WindowUpdate()
        {
            if (portCode != strPortCode)
            {
                portCode = RmmUtil.CleanString(strPortCode);
                strPortCode = portCode;
            }
        }

        public override void WindowUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Docking Port Code:", GUILayout.Width(150));
            strPortCode = GUILayout.TextField(strPortCode, 15, GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Register", GUILayout.Width(70)))
            {
                _dockingPort.RegisterDockingPort(portCode.Trim());
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}
