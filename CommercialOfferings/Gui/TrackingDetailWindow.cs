using CommercialOfferings.MissionData;
using System;
using KSP.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class TrackingDetailWindow : WindowBase
    {
        private Vector2 scrollPositionTracking;
        private TrackingControl _trackingControl;

        public Mission TrackingMission;
        public CheckList ValidCheckList;

        public Tracking VesselTracking;

        public TrackingDetailWindow(TrackingControl trackingControl) : base("Tracking", new Rect(0, 0, 400, 300), 400, 300)
        {
            _trackingControl = trackingControl;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            GUILayout.BeginVertical();

            scrollPositionTracking = GUILayout.BeginScrollView(scrollPositionTracking, false, false, RmmStyle.Instance.HoriScrollBarStyle, RmmStyle.Instance.VertiScrollBarStyle, GUILayout.Width(390), GUILayout.Height(500));

            if (VesselTracking != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("End Current Vessel Tracking", RmmStyle.Instance.ButtonStyle, GUILayout.Width(250), GUILayout.Height(22)))
                {
                    _trackingControl.EndVesselTracking(VesselTracking);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            float labelWidth = 175;

            if (TrackingMission != null)
            {
                if (TrackingMission.Info != null)
                {
                    MissionInfo info = TrackingMission.Info;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("INFO", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Type:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(info.Type.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Name:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(info.Name.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                if (TrackingMission.Launch != null)
                {
                    MissionLaunch launch = TrackingMission.Launch;

                    //GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("LAUNCH", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Time:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(RmmUtil.TimeString(launch.Time), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Body:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(launch.Body.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Vessel:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(Localizer.Format(launch.VesselName), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Value:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(launch.Funds.ToString("F1"), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(launch.Crew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Parts:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label((launch.Parts == null ? "0" : launch.Parts.Count.ToString()), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.Space(10);
                }
                if (TrackingMission.Departure != null)
                {
                    MissionDeparture departure = TrackingMission.Departure;

                    //GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("DEPARTURE", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Time:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(RmmUtil.TimeString(departure.Time), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Body:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(departure.Body.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(departure.Crew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(departure.CrewCapacity.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Parts:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label((departure.Parts == null ? "no parts tracked" : departure.Parts.Count.ToString()), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();

                    if (departure.Resources != null)
                    {
                        foreach (MissionResource resource in departure.Resources)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(labelWidth);
                            GUILayout.Label(resource.Name + " " + resource.Amount.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                            GUILayout.EndHorizontal();
                        }
                    }
                    if (departure.Propellants != null)
                    {
                        foreach (String propellant in departure.Propellants)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(labelWidth);
                            GUILayout.Label("Propellant " + propellant, RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.Space(10);
                }
                if (TrackingMission.Landings != null)
                {
                    foreach (MissionLanding landing in TrackingMission.Landings)
                    {
                        //GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("LANDING", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Time:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.Label(RmmUtil.TimeString(landing.Time), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Body:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.Label(landing.Body.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Value:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.Label(landing.Funds.ToString("F1"), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.Label(landing.Crew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.Label(landing.CrewCapacity.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Parts:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.Label((landing.Parts == null ? "no parts tracked" : landing.Parts.Count.ToString()), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();

                        if (landing.Resources != null)
                        {
                            foreach (MissionResource resource in landing.Resources)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Space(labelWidth);
                                GUILayout.Label(resource.Name + " " + resource.Amount.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                                GUILayout.EndHorizontal();
                            }
                        }
                    }
                    GUILayout.Space(10);
                }
                if (TrackingMission.Arrival != null)
                {
                    MissionArrival arrival = TrackingMission.Arrival;

                    //GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ARRIVAL", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Time:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(RmmUtil.TimeString(arrival.Time), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Body:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(arrival.Body.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(arrival.Crew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label(arrival.CrewCapacity.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Parts:", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.Label((arrival.Parts == null ? "no parts tracked" : arrival.Parts.Count.ToString()), RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
            }

            if (ValidCheckList != null)
            {
                //GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));

                if (ValidCheckList.CheckSuccess)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Valid", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    GUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.Space(10);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Invalid", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                    bool first = true;
                    foreach (String message in ValidCheckList.Messages)
                    {
                        if (!first)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(labelWidth);
                        }
                        first = false;
                        GUILayout.Label(message, RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                        GUILayout.EndHorizontal();
                    }
                    if (first)
                        GUILayout.EndHorizontal();

                }
                GUILayout.Space(10);
            }

            if (VesselTracking == null)
            {
                //GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(labelWidth));
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Tracking", RmmStyle.Instance.ButtonStyle, GUILayout.Width(150), GUILayout.Height(22)))
                {
                    _trackingControl.DeleteTracking(TrackingMission);
                    Close();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}
