using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.Localization;


namespace CommercialOfferings.Gui
{
    class TrackingWindow : WindowBase
    {
        private Vector2 scrollPositionTrackings;

        public List<Tracking> ActiveTrackings = new List<Tracking>();

        TrackingControl _trackingControl = null;

        private const int WINDOW_HEIGHT = 300;

        public TrackingWindow(TrackingControl trackingControl) : base("Active Tracking", new Rect(0, (Screen.height - WINDOW_HEIGHT)/2, 200, WINDOW_HEIGHT), 200, WINDOW_HEIGHT)
        {
            IsCloseButtonVisible = false;

            _trackingControl = trackingControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            GUILayout.BeginVertical();

            scrollPositionTrackings = GUILayout.BeginScrollView(scrollPositionTrackings, false, false, RmmStyle.Instance.HoriScrollBarStyle, RmmStyle.Instance.VertiScrollBarStyle, GUILayout.Width(200), GUILayout.Height(WINDOW_HEIGHT));

            if (ActiveTrackings != null && ActiveTrackings.Count != 0)
            {
                foreach (Tracking tracking in ActiveTrackings)
                {

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(tracking.MissionName, RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                    {
                        _trackingControl.TrackingDetail(tracking.Mission, this, vesselTracking:tracking);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Label(tracking.VesselName, RmmStyle.Instance.LabelStyle, GUILayout.Width(180));

                    GUILayout.Label(tracking.Status, RmmStyle.Instance.LabelStyle, GUILayout.Width(90));

                    GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(60));
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
