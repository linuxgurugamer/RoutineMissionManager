/*Copyright (c) 2014, Flip van Toly
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are 
permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of 
conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of 
conditions and the following disclaimer in the documentation and/or other materials provided with 
the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to 
endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS 
OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY 
AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR 
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY 
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR 
OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
OF SUCH DAMAGE.*/

//Namespace Declaration 
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.UI.Screens;
using CommercialOfferings.Gui;
using ClickThroughFix;
using ToolbarControl_NS;
using SpaceTuxUtility;

namespace CommercialOfferings
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        void Start()
        {
            ToolbarControl.RegisterMod(RmmMonoBehaviour.MODID, RmmMonoBehaviour.MODNAME);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RmmMonoBehaviour : MonoBehaviour
    {
        private double _nextLogicTime = 1;

        private WindowManager _windowManager = new WindowManager();
        private TrackingControl _trackingControl = new TrackingControl();
        private RoutineControl _routineControl = new RoutineControl();



        //stock toolbar button
        //private ApplicationLauncherButton toolBarButton;
        //private Texture2D toolBarButtonTexture;

        ToolbarControl toolbarControl;
        internal const string MODID = "RMM";
        internal const string MODNAME = "Routine Mission Manager";

        //private string[] allowedBodies = null;

        //GUI

#if false
        //GUI Location
        private bool renderGUILocation = false;
       private Rect windowPosGUILocation = new Rect(450, 200, 350, 30);
        private Vector2 scrollPositionModules;
        private RMMModule rmmmSelectGUI = null;
#endif
        //GUI TermsCondi
        //private bool renderGUITermsCondi = false;
        private Rect windowPosGUITermsCondi = new Rect(400, 250, 250, 300);
        private Vector2 scrollPositionDisc;

        private List<RMMModule> RegisteredModuleList = new List<RMMModule>();

        public static RmmMonoBehaviour Instance = null;

        public void Awake()
        {
#if false
            renderGUILocation = false;
#endif
            //renderGUITermsCondi = false;
            addToolbarButton();
        }

        void onDestroy()
        {
            //if (toolBarButton != null) { removeToolbarButton(); }
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                GameObject.Destroy(toolbarControl);
                toolbarControl = null;
            }

        }


        private void OnGUI()
        {
            if (Instance == null) { Instance = this; }
#if false
            if (Event.current.type == EventType.Repaint || Event.current.isMouse)
            {
                // preDraw code
            }
#endif
            OnUpdate();
            _windowManager.OnGUI();
            _trackingControl.OnUpdate();
            _routineControl.OnUpdate();

            //UpdateToolBar();
        }

#if false
        private void UpdateToolBar()
        {

            if (HighLogic.LoadedScene != GameScenes.FLIGHT || (lastUpdateToolBarTime + 3) > Planetarium.GetUniversalTime() || !ApplicationLauncher.Ready) { return; }
            if (toolBarButton == null && ToolBarButtonVisible()) { addToolbarButton(); }
            if (toolBarButton != null && !ToolBarButtonVisible()) { removeToolbarButton(); }

            lastUpdateToolBarTime = Planetarium.GetUniversalTime();
        }
#endif
        private void addToolbarButton()
        {
#if false
            toolBarButtonTexture = GameDatabase.Instance.GetTexture("RoutineMissionManager/PluginData/Textures/RMMbutton", false);
            toolBarButton = ApplicationLauncher.Instance.AddModApplication(
                                onToggleOn,
                                onToggleOff,
                                null,
                                null,
                                null,
                                null,
                                ApplicationLauncher.AppScenes.FLIGHT,
                                toolBarButtonTexture);
#endif
            if (toolbarControl == null)
            {
                GameObject gameObject = new GameObject();
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(onToggleOn, onToggleOff,
                    ApplicationLauncher.AppScenes.FLIGHT,
                    MODID,
                    "rmmBtn",
                    "RoutineMissionManager/PluginData/Textures/RMMbutton-38",
                    "RoutineMissionManager/PluginData/Textures/RMMbutton-24",
                    MODNAME
                );

            }
        }

#if false
        private void removeToolbarButton()
        {
            ApplicationLauncher.Instance.RemoveModApplication(toolBarButton);
            toolBarButton = null;
        }
#endif

        void onToggleOn()
        {
            Menu();

            // Add docking port modules
            if (RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name) && FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING)
            {
                RmmDockingPortModule.AddToVessel(FlightGlobals.ActiveVessel);
            }
        }

        void onToggleOff()
        {
            CancelMenu();
        }

        private bool ToolBarButtonVisible()
        {
            if (RmmUtil.IsPreLaunch(FlightGlobals.ActiveVessel) ||
                (FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING &&
                 RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name) &&
                 RmmUtil.HasDockingPorts(FlightGlobals.ActiveVessel)) ||
                RmmUtil.IsTrackingActive(FlightGlobals.ActiveVessel))
                return (true);

            return (false);
        }

#if false
        private void startRoutine()
        {
            if (!File.Exists(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "UserAcknowledgesKnownBugs")) 
            { 
                openGUITermsCondi(); 
                return; 
            }
        }
#endif
        public void CreateDepartureTracking(Part dockingPort)
        {
            _trackingControl.CreateDepartureTracking(dockingPort);
        }

        public void RegistrationVisible()
        {
            RmmDockingPortModule.AddToVessel(FlightGlobals.ActiveVessel);
        }

        public void EnableDepartureTracking()
        {
            RmmDockingPortModule.AddToVessel(FlightGlobals.ActiveVessel);
            RmmDockingPortModule.SetEnableTracking(FlightGlobals.ActiveVessel, true);
        }

        public void EnableDockingPortRegistration()
        {
            RmmDockingPortModule.AddToVessel(FlightGlobals.ActiveVessel);
            RmmDockingPortModule.SetEnableRegistration(FlightGlobals.ActiveVessel, true);
        }

        /// <summary>
        /// GUITermsCondi
        /// </summary>
        /// <param name="windowID"></param>
        private void WindowGUITermsCondi(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 250, 30));

            GUILayout.BeginVertical();

            scrollPositionDisc = GUILayout.BeginScrollView(scrollPositionDisc, false, true, GUIStyle.none, RmmStyle.Instance.VertiScrollBarStyle, GUILayout.Width(240), GUILayout.Height(300));

            GUILayout.Label(
            "Our scientist have discovered docking ports are in fact very round in nature. This new-found knowledge explains why ships can dock in so many different ways to a docking port. In practice ships have been seen to dock straight, canted slightly to the left, a full quarter to the right and even almost upside down with a two degree counterclockwise offset." + Environment.NewLine +
            "Based on these observations some theoretical scientist have argued there could be infinite many ways to connect two docking ports." + Environment.NewLine +
            "But all this metaphysical nonsense aside, docking is already super hard on our Kerbonauts. They can't be expected to align two docking ports together and also control the relative rotation of the approach. That's like totally doing two things at the same time!" + Environment.NewLine +
            Environment.NewLine +
            "So when you order a mission to be executed unsupervised through the Routine Mission Manager, you should make sure every possible rotation between the current target docking port and the mission vessel does not lead to a collision between parts of the station and the approaching vessel. If you can not make sure of this you should not order this mission in combination with this docking port."
            , RmmStyle.Instance.LabelTextStyle, GUILayout.Width(205));

            if (GUILayout.Button("I understand and acknowledge", RmmStyle.Instance.ButtonStyle, GUILayout.Height(30)))
            {
                System.IO.File.Create(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "UserAcknowledgesKnownBugs");
                //closeGUITermsCondi();
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private static int winPosGUITermsCondiID = WindowHelper.NextWindowId("windowPosGUITermsCondi");
        private void drawGUITermsCondi()
        {
            windowPosGUITermsCondi = ClickThruBlocker.GUILayoutWindow(winPosGUITermsCondiID, windowPosGUITermsCondi, WindowGUITermsCondi, "New science breakthrough!", RmmStyle.Instance.WindowStyle);
        }

#if false
        public void openGUITermsCondi()
        {
            //renderGUITermsCondi = true;
        }

        public void closeGUITermsCondi()
        {
            //renderGUITermsCondi = false;
        }

#endif

        internal void OnDestroy()
        {
            //if (toolBarButton != null) { removeToolbarButton(); }
            if (toolbarControl != null)
            {
                toolbarControl.OnDestroy();
                GameObject.Destroy(toolbarControl);
                toolbarControl = null;
            }
        }


        public void OnUpdate()
        {
            if (!HighLogic.LoadedSceneIsFlight) { return; }
            if (_nextLogicTime == 0 || _nextLogicTime > Planetarium.GetUniversalTime()) { return; }


            HandleMenuWindow();

            _nextLogicTime = Planetarium.GetUniversalTime() + 1;
        }

#region Menu

        private MenuWindow _menuWindow = null;

        private void HandleMenuWindow()
        {
            if (_menuWindow == null) { return; }
            if (!WindowManager.IsOpen(_menuWindow))
            {
                CancelMenu();
                return;
            }

            if (RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name) &&
                (RmmUtil.IsPreLaunch(FlightGlobals.ActiveVessel) || FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING))
            {
                _menuWindow.TrackingEnabled = true;
            }
            else
            {
                _menuWindow.TrackingEnabled = false;
            }

            if (RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name) &&
                FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING)
            {
                _menuWindow.RoutineArrivalEnabled = true;
                _menuWindow.RoutineDepartureEnabled = true;
                _menuWindow.RegisterDockingPortsEnabled = true;
            }
            else
            {
                _menuWindow.RoutineArrivalEnabled = false;
                _menuWindow.RoutineDepartureEnabled = false;
            }
        }

        public void CancelMenu()
        {
            if (_menuWindow != null)
            {
                WindowManager.Close(_menuWindow);
                _menuWindow = null;
            }
        }

        public void Menu()
        {
            CancelMenu();
            _menuWindow = new MenuWindow(this);
            WindowManager.Open(_menuWindow);
        }

        public void StartTrackingOption()
        {
            if (!RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name)) { return; }

            if (!RmmUtil.IsTrackingActive(FlightGlobals.ActiveVessel) && RmmUtil.IsPreLaunch(FlightGlobals.ActiveVessel)) { _trackingControl.CreateLaunchTracking(FlightGlobals.ActiveVessel); }

            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING)
            {
                EnableDepartureTracking();
            }
        }

        public void TrackedMissionsOption()
        {
            _trackingControl.TrackingOverview(_menuWindow);
        }


        public void RoutineLaunchOption()
        {
            if (!RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name)) { return; }
            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING)
            {
                _routineControl.RoutineOverview(_menuWindow);
            }
        }

        public void RoutineDepartureOption()
        {
            if (!RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name)) { return; }
            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING)
            {
                _routineControl.RoutineDepartureOverview(_menuWindow);
            }
        }

        public void OrderedMissionsOption()
        {
            _routineControl.OrderedMissions(_menuWindow);
        }

        public void RegisterDockingPortsOption()
        {
            if (!RmmUtil.AllowedBody(FlightGlobals.ActiveVessel.mainBody.name)) { return; }
            if (FlightGlobals.ActiveVessel.situation == Vessel.Situations.ORBITING)
            {
                EnableDockingPortRegistration();
            }
        }

        public void ManualOption()
        {
            Manual();
        }

#endregion Menu

#region Manual

        private ManualWindow _manualWindow = null;

        private void HandleManualWindow()
        {
            if (_manualWindow == null) { return; }
            if (!WindowManager.IsOpen(_manualWindow))
            {
                CancelMenu();
                return;
            }
        }

        public void CancelManual()
        {
            if (_manualWindow != null)
            {
                WindowManager.Close(_manualWindow);
                _manualWindow = null;
            }
        }

        public void Manual()
        {
            CancelMenu();
            _manualWindow = new ManualWindow(this);
            WindowManager.Open(_manualWindow);
        }

#endregion Manual
    }
}