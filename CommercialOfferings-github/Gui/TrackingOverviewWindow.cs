using CommercialOfferings.MissionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class TrackingOverviewWindow : WindowBase
    {
        private Vector2 scrollPositionMissions;

        public struct MissionItem
        {
            public Mission Mission;
            public CheckList ValidCheckList;
        }

        TrackingControl _trackingControl = null;

        public List<MissionItem> MissionItems = null;

        public TrackingOverviewWindow(TrackingControl trackingControl) : base("Trackings", new Rect(), 400, 300)
        {
            _trackingControl = trackingControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            GUILayout.BeginVertical();

            scrollPositionMissions = GUILayout.BeginScrollView(scrollPositionMissions, false, false, RmmStyle.Instance.HoriScrollBarStyle, RmmStyle.Instance.VertiScrollBarStyle, GUILayout.Width(390), GUILayout.Height(500));

            if (MissionItems != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Type", RmmStyle.Instance.LabelStyle, GUILayout.Width(80));
                GUILayout.Label("Name", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                foreach (MissionItem missionItem in MissionItems)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(missionItem.Mission.Info.Type.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(80));
                    GUILayout.Label(missionItem.Mission.Info.Name.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label(missionItem.ValidCheckList.CheckSucces ? "Valid" : "Invalid", RmmStyle.Instance.LabelStyle, GUILayout.Width(80));
                    if (GUILayout.Button(">", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        _trackingControl.TrackingDetail(missionItem.Mission, this);
                    }
                    GUILayout.EndHorizontal();
                }
            }


            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
