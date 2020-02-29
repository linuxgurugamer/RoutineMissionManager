using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RoutineDepartureDetailWindow : WindowBase
    {
        public struct ResourceItem
        {
            public string Name;
            public double RequiredAmount;
            public double CurrentAmount;
        }

        RoutineControl _routineControl = null;

        public RoutineDepartureMission RoutineDepartureMission = null;
        public List<Part> DepartureParts = null;
        public List<ResourceItem> Resources = null;
        public double CurrentCargoMass = 0;
        public double CurrentCargoFunds = 0;

        public int CrewCount = 0;
        private uint departureDockingPortFlightId = 0;
        private string strDepartureDockingPortName = "";
        private List<string> messages;

        public RoutineDepartureDetailWindow(RoutineControl routineControl) : base("Departure Mission", new Rect(), 200, 60)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {
            if (RoutineDepartureMission != null)
            {
                if (RoutineDepartureMission.flightIdDepartureDockPart != departureDockingPortFlightId)
                {
                    departureDockingPortFlightId = RoutineDepartureMission.flightIdDepartureDockPart;
                    strDepartureDockingPortName = RmmScenario.Instance.GetRegisteredDockingPort(departureDockingPortFlightId);
                }
            }
        }

        public override void WindowUI()
        {
            if (RoutineDepartureMission != null)
            {
                if (RoutineDepartureMission.Kind == MissionKind.Ordered)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Order:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RoutineDepartureMission.OrderId, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RoutineDepartureMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("System:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RoutineDepartureMission.VesselName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Return Fee:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(Math.Round(RoutineDepartureMission.Price).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(250));
                GUILayout.EndHorizontal();

                if (RoutineDepartureMission.CargoMass > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Cargo Fee:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(Math.Round(CurrentCargoFunds, 2).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Duration:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RmmUtil.TimeString(RoutineDepartureMission.Duration), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                if (RoutineDepartureMission.Kind == MissionKind.Ordered)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ETD:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RmmUtil.TimeEtaString(RoutineDepartureMission.DepartureTime - Planetarium.GetUniversalTime()), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }

                if (RoutineDepartureMission.MinimumCrew > 0)
                {
                    if (RoutineDepartureMission.MinimumCrew < RoutineDepartureMission.CrewCapacity)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Minimal crew required:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(RoutineDepartureMission.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(RoutineDepartureMission.CrewCapacity.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                        GUILayout.Label(RoutineDepartureMission.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                }

                if (RoutineDepartureMission.MinimumCrew < RoutineDepartureMission.CrewCapacity)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Current crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(CrewCount.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                }

                if (Resources != null && Resources.Count > 0)
                {
                    GUILayout.Label("Resources:", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Resource", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label("Required", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label("Current", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                    foreach (ResourceItem resource in Resources)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(resource.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(Math.Round(resource.RequiredAmount, 2).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                        GUILayout.Label(Math.Round(resource.CurrentAmount, 2).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                    }
                }


                if (RoutineDepartureMission.CargoMass > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Cargo mass:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label(Math.Round(RoutineDepartureMission.CargoMass, 2).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Current cargo mass:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label(Math.Round(CurrentCargoMass, 2).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Docking Port:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(strDepartureDockingPortName, RmmStyle.Instance.LabelStyle, GUILayout.Width(50));
                GUILayout.EndHorizontal();

                if (messages != null && messages.Count > 0)
                {
                    GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                    foreach (String message in messages)
                    {
                        GUILayout.Label(message, RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                    }
                }

                if (RoutineDepartureMission.Kind == MissionKind.Potential)
                {
                    if (GUILayout.Button("Order Return", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
                    {
                        CheckList checkList = _routineControl.OrderDepartureMissionAllowed();
                        if (checkList.CheckSuccess)
                        {
                            _routineControl.OrderDepartureMission();
                            base.Close();
                        }
                        else
                        {
                            messages = checkList.Messages;
                        }
                    }
                }
                if (RoutineDepartureMission.Kind == MissionKind.Ordered)
                {
                    if (GUILayout.Button("Cancel Return", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
                    {
                        _routineControl.CancelDepartureMission();
                        base.Close();
                    }
                }
            }
        }
    }
}
