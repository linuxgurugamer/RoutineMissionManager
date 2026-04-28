using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class RoutineRetrievalDetailWindow : WindowBase
    {
        public struct ResourceItem
        {
            public string Name;
            public double RequiredAmount;
            public double CurrentAmount;
        }

        public struct CargoItem
        {
            public string Name;
            public double Amount;
        }

        RoutineControl _routineControl = null;

        public RoutineRetrievalMission RoutineRetrievalMission = null;
        public List<Part> DepartureParts = null;
        public List<ResourceItem> Resources = null;
        public List<CargoItem> CargoReturned = null;

        public int CrewCount = 0;
        private uint departureDockingPortFlightId = 0;
        private string strDepartureDockingPortName = "";
        private List<string> messages;

        public RoutineRetrievalDetailWindow(RoutineControl routineControl) : base("Retrieval Mission", new Rect(), 200, 60)
        {
            _routineControl = routineControl;
        }

        public override void WindowUpdate()
        {
            if (RoutineRetrievalMission != null)
            {
                if (RoutineRetrievalMission.flightIdDepartureDockPart != departureDockingPortFlightId)
                {
                    departureDockingPortFlightId = RoutineRetrievalMission.flightIdDepartureDockPart;
                    strDepartureDockingPortName = RmmScenario.Instance.GetRegisteredDockingPort(departureDockingPortFlightId);
                }
            }
        }

        public override void WindowUI()
        {
            if (RoutineRetrievalMission != null)
            {
                if (RoutineRetrievalMission.Kind == MissionKind.Ordered)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Order:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RoutineRetrievalMission.OrderId, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("Mission:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RoutineRetrievalMission.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("System:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RoutineRetrievalMission.VesselName, RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Duration:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(RmmUtil.TimeString(RoutineRetrievalMission.Duration), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Price:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                GUILayout.Label(Math.Round(RoutineRetrievalMission.Price).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(250));
                GUILayout.EndHorizontal();

                if (RoutineRetrievalMission.Kind == MissionKind.Ordered)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("ETA:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(RmmUtil.TimeEtaString(RoutineRetrievalMission.ArrivalTime - Planetarium.GetUniversalTime()), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();
                }

                if (RoutineRetrievalMission.MinimumCrew > 0)
                {
                    if (RoutineRetrievalMission.MinimumCrew < RoutineRetrievalMission.CrewCapacity)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Minimal crew required:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(RoutineRetrievalMission.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew capacity:", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(RoutineRetrievalMission.CrewCapacity.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label("Crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                        GUILayout.Label(RoutineRetrievalMission.MinimumCrew.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(200));
                        GUILayout.EndHorizontal();
                    }
                }

                if (RoutineRetrievalMission.MinimumCrew < RoutineRetrievalMission.CrewCapacity)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Current crew:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.Label(CrewCount.ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(50));
                    GUILayout.EndHorizontal();
                }

                if (Resources != null && Resources.Count > 0)
                {
                    GUILayout.Label("Propellants:", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
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

                if (CargoReturned != null && CargoReturned.Count > 0)
                {
                    GUILayout.Label("Cargo Returned:", RmmStyle.Instance.LabelStyle, GUILayout.Width(300));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Resource", RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                    GUILayout.Label("Amount", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                    GUILayout.EndHorizontal();
                    foreach (CargoItem cargo in CargoReturned)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(cargo.Name, RmmStyle.Instance.LabelStyle, GUILayout.Width(150));
                        GUILayout.Label(Math.Round(cargo.Amount, 2).ToString(), RmmStyle.Instance.LabelStyle, GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                    }
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

                if (RoutineRetrievalMission.Kind == MissionKind.Potential)
                {
                    if (GUILayout.Button("Order Retrieval", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
                    {
                        CheckList checkList = _routineControl.OrderRetrievalMissionAllowed();
                        if (checkList.CheckSuccess)
                        {
                            _routineControl.OrderRetrievalMission();
                            base.Close();
                        }
                        else
                        {
                            messages = checkList.Messages;
                        }
                    }
                }
                if (RoutineRetrievalMission.Kind == MissionKind.Ordered)
                {
                    if (GUILayout.Button("Cancel Retrieval", RmmStyle.Instance.ButtonStyle, GUILayout.Width(300), GUILayout.Height(22)))
                    {
                        _routineControl.CancelRetrievalMission();
                        base.Close();
                    }
                }
            }
        }
    }
}
