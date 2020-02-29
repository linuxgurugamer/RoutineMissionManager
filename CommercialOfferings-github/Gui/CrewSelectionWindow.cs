using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class CrewSelectionWindow : WindowBase
    {
        RoutineControl _routineControl = null;
        string _crewSelectionString = null;

        List<ProtoCrewMember> preferredCrewList = new List<ProtoCrewMember>();
        private Vector2 scrollPositionPreferredCrew;
        private Vector2 scrollPositionAvailableCrew;

        public CrewSelectionWindow(RoutineControl routineControl, string crewSelectionString, string title) : base(title, new Rect(), 200, 60)
        {
            _routineControl = routineControl;
            _crewSelectionString = crewSelectionString;

            preferredCrewList.Clear();
            string[] prefCrewNames = GetPreferredCrewNames(_crewSelectionString);
            foreach (String name in prefCrewNames)
            {
                foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
                {
                    if (name == cr.name) { preferredCrewList.Add(cr); }
                }
                foreach (ProtoCrewMember to in HighLogic.CurrentGame.CrewRoster.Tourist)
                {
                    if (name == to.name) { preferredCrewList.Add(to); }
                }
            }
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {
            GUILayout.Label("Preferred:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));

            scrollPositionPreferredCrew = GUILayout.BeginScrollView(scrollPositionPreferredCrew, false, true, GUILayout.Width(200), GUILayout.Height(200));
            foreach (ProtoCrewMember cr in preferredCrewList)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(cr.type == ProtoCrewMember.KerbalType.Tourist ? cr.name + " T" : cr.name, RmmStyle.Instance.LabelStyle, GUILayout.Width(135));
                if (GUILayout.Button("-", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                {
                    preferredCrewList.Remove(cr);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.Label("Roster:", RmmStyle.Instance.LabelStyle, GUILayout.Width(100));


            scrollPositionAvailableCrew = GUILayout.BeginScrollView(scrollPositionAvailableCrew, false, true, GUILayout.Width(200), GUILayout.Height(300));
            foreach (ProtoCrewMember cr in HighLogic.CurrentGame.CrewRoster.Crew)
            {
                GUILayout.BeginHorizontal();
                if (cr.rosterStatus != ProtoCrewMember.RosterStatus.Dead)
                {
                    GUILayout.Label(cr.name, RmmStyle.Instance.LabelStyle, GUILayout.Width(135));
                    if (GUILayout.Button("^", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                    {
                        bool alreadyAdded = false;
                        foreach (ProtoCrewMember cre in preferredCrewList)
                        {
                            if (cre.name == cr.name)
                            {
                                alreadyAdded = true;
                            }
                        }
                        if (!alreadyAdded) { preferredCrewList.Add(cr); }
                    }
                }
                GUILayout.EndHorizontal();
            }
            foreach (ProtoCrewMember to in HighLogic.CurrentGame.CrewRoster.Tourist)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(to.name + " T", RmmStyle.Instance.LabelStyle, GUILayout.Width(135));
                if (GUILayout.Button("^", RmmStyle.Instance.ButtonStyle, GUILayout.Width(20), GUILayout.Height(22)))
                {
                    bool alreadyAdded = false;
                    foreach (ProtoCrewMember cre in preferredCrewList)
                    {
                        if (cre.name == to.name)
                        {
                            alreadyAdded = true;
                        }
                    }
                    if (!alreadyAdded) { preferredCrewList.Add(to); }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set", RmmStyle.Instance.ButtonStyle, GUILayout.Width(100), GUILayout.Height(22)))
            {
                string missionPreferedCrew = "";
                foreach (ProtoCrewMember cr in preferredCrewList)
                {
                    missionPreferedCrew = missionPreferedCrew + cr.name + ",";
                }
                _routineControl.SetCrewSelection(missionPreferedCrew);
                Close();
            }

            GUILayout.EndHorizontal();
        }


        private string[] GetPreferredCrewNames(string missionPreferedCrew)
        {
            if (String.IsNullOrEmpty(missionPreferedCrew))
            {
                return new string[0];
            }
            return missionPreferedCrew.Split(',');
        }
    }
}
