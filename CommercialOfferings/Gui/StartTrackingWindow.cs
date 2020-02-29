using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class StartTrackingWindow : WindowBase
    {
        private TrackingControl _trackingControl = null;
        private int _type = 0;

        private const int WINDOW_WIDTH = 300;
        private const int WINDOW_HEIGHT = 100;

        public string VesselName = "";
        public double Price = 0.0;
        public int MinimumCrew = 0;
        public List<Resource> MinimumResources = null;

        string trackName = "";
        string trackStrName = "";

        public StartTrackingWindow(TrackingControl trackingControl, int type) : base("Start Tracking", new Rect((Screen.width- WINDOW_WIDTH)/2, (Screen.height - WINDOW_HEIGHT) / 2, WINDOW_WIDTH, WINDOW_HEIGHT), WINDOW_WIDTH, WINDOW_HEIGHT)
        {
            _trackingControl = trackingControl;
            _type = type;
        }

        public override void Close()
        {
            _trackingControl.CancelStartTracking();
            base.Close();
        }

        public override void WindowUpdate()
        {
            if (trackName != trackStrName)
            {
                trackName = RmmUtil.CleanString(trackStrName);
                trackStrName = trackName;
            }
        }

        public override void WindowUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Mission Name:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
            trackStrName = GUILayout.TextField(trackStrName, 20, RmmStyle.Instance.TextFieldStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            if (_type != 20)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Vessel:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(VesselName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }

            if (_type != 20)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Price:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(Price.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Minimum Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
            GUILayout.Label(MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
            GUILayout.EndHorizontal();

            if (MinimumResources != null && MinimumResources.Count > 0)
            {
                GUILayout.Label("Minimum Resources:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));

                GUILayout.BeginHorizontal();
                GUILayout.Label("Resource", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                GUILayout.Label("Amount", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                foreach (Resource resource in MinimumResources)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(RmmUtil.DisplayName(resource.Name), RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label(Math.Round(resource.Amount, 2).ToString(), GUILayout.Width(150));
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Tracking", RmmStyle.Instance.ButtonStyle, GUILayout.Width(200), GUILayout.Height(22)))
            {
                _trackingControl.StartTracking(trackName.Trim());
            }
            GUILayout.EndHorizontal();
        }
    }
}
