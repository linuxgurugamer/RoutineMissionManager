using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings.Gui
{
    class ManualWindow : WindowBase
    {
        private Vector2 scrollPositionMissions;

        private RmmMonoBehaviour _rmmMonoBehaviour = null;

        public bool TrackingEnabled = false;
        public bool RoutineArrivalEnabled = false;
        public bool RoutineDepartureEnabled = false;

        private const int TEXT_WIDTH = 465;

        public ManualWindow(RmmMonoBehaviour rmmMonoBehaviour) : base("Manual", new Rect(), 500, 200)
        {
            _rmmMonoBehaviour = rmmMonoBehaviour;
        }

        public override void WindowUpdate()
        {

        }

        public override void WindowUI()
        {

            scrollPositionMissions = GUILayout.BeginScrollView(scrollPositionMissions, false, false, RmmStyle.Instance.HoriScrollBarStyle, RmmStyle.Instance.VertiScrollBarStyle, GUILayout.Width(500), GUILayout.Height(600));

            GUILayout.Label(ManualText.Instance.ManualIntro, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.OverviewTitle, RmmStyle.Instance.LabelTextTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.Overview, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.TrackingTitle, RmmStyle.Instance.LabelTextTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.TrackingGeneral, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.TrackingMissionArchitectureTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.TrackingMissionArchitecture, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.StartTrackingTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.StartTracking, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.StartTrackingLaunchTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.StartTrackingLaunch, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.StartTrackingDepartureTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.StartTrackingDeparture, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.TrackingFlightTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.TrackingFlight, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.TrackingArrivalTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.TrackingArrival, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.TrackingLandingTitle, RmmStyle.Instance.LabelTextSubTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.TrackingLanding, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.OrderingTitle, RmmStyle.Instance.LabelTextTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.OrderGeneral, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label("", RmmStyle.Instance.LabelStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.Label(ManualText.Instance.LocationsTitle, RmmStyle.Instance.LabelTextTitleStyle, GUILayout.Width(TEXT_WIDTH));
            GUILayout.Label(ManualText.Instance.LocationsGeneral, RmmStyle.Instance.LabelTextStyle, GUILayout.Width(TEXT_WIDTH));

            GUILayout.EndScrollView();
        }

        class ManualText
        {
            private static ManualText _instance;

            public static ManualText Instance
            {
                get
                {
                    if (_instance == null) { _instance = new ManualText(); }
                    return _instance;
                }
            }

            public string ManualIntro =
                "Thank you for implementing the Routine Mission Manager. This application allows you, as a space program administrators, to track certain flights in a manner which allows you to order similar missions in the future, without requiring your continues hands-on guidance and oversight of these missions. "
                + "As such, the routine mission manager will save time executing the routine missions which will allow you to focus more time on pushing the borders of space exploration. "
                + Environment.NewLine + Environment.NewLine
                + "What type of flights can tracked? What are the variations of the subsequent orders of these mission? What are the details and conditions which define these operations? Is there a manual which explains any of this?" + Environment.NewLine + "You are reading it now. ";

            public string OverviewTitle = "Overview";
            public string Overview =
                "The routine mission manager usage can be subdivided in three domains: tracking, ordering and location management. "
                + Environment.NewLine + Environment.NewLine
                + "Tracking involves flying a new type of mission while the routine mission manager tracks the mission. "
                + Environment.NewLine + Environment.NewLine
                + "Ordering refers to the ordering of additionals missions through the routine mission manager. "
                + "These ordered missions are based on the missions which have been tracked by the routine mission manager. "
                + "Mission ordered through the routine mission manager will be executed without your direct involvement. "
                + Environment.NewLine + Environment.NewLine
                + "Location Management comprises the registering of locations. "
                + "These locations serve as starting points and destination for mission ordered through the routine mission manager. ";

            public string TrackingTitle = "Tracking";
            public string TrackingGeneral =
                "To allow a mission to be ordered through the routine mission mananger the mission needs to be executed once while being tracked. "
                + Environment.NewLine + Environment.NewLine
                + "In its simplest form it is only required to start the tracking and fly the mission. However to allow more variation on future ordered missions it is preferable to consider the original flight a model flight in which the vessels is rated instead. "
                + "There might yet other mission structures which could not be flown at all without the use of the routine mission manager. An example might be a launch in which some stage of the vessel returns to land and be recovered while another part continues an ascent to dock with a vessel in orbit. Since such a mission could require you to be at two places at the same time, you would not be able to fly such a mission in a conventionally manner. The routine mission manager will however allow you to complete different stages of the flight seperatly, and will subsequently allow you to order the entire mission through the routine mission manager. In such a case the original mission should be considered a simulation. "
                + Environment.NewLine + Environment.NewLine
                + "No matter whether you are simply tracking a flight you expect will need repeating again in the future, flying a model flight or flying the original mission as a simulation, the original flight should always be executed as clean and efficient as possible. ";

            public string TrackingMissionArchitectureTitle = "Mission Architecture";
            public string TrackingMissionArchitecture =
                "The following mission architectures are supported by the routine mission manager: "
                + Environment.NewLine
                + Environment.NewLine
                + "<b>Launch</b> " + Environment.NewLine
                + "A mission which start with a launch or takeof from the home planet and ends with an docking arrival at an orbiting vessel around a celestial body for which the routine mission manager is enabled. In addition this mission may contain one or more landings on the home planet of recovarable stages. "
                + Environment.NewLine
                + Environment.NewLine
                + "<B>Departure</B> " + Environment.NewLine
                + "A mission which starts with a departure from an orbiting vessel around a celestial body for which the routine mission manager is enabled and ends with one or more landings on the home planet. ";

            public string StartTrackingTitle = "Start Tracking";
            public string StartTracking =
                "To track a flight you are required to manually start the tracking of the flight. Missions can start as a launch from the launchpad, takeof from the runway or a depature from a larger orbiting vessel. "
                + Environment.NewLine + Environment.NewLine
                + "To fly a model flight consider using minimal crew, minimal proppelant and, if applicable, maximum cargo. "
                + Environment.NewLine + Environment.NewLine
                + "The crew on the orignal mission will be considered the minimal crew for any ordered mission based on the tracked mission. You will be able to increase the crew up to the maximum crew capacity in ordered missions. "
                + Environment.NewLine + Environment.NewLine
                + "The proppellant on board at the start of the mission will be considered the minimal resource requirement for any future ordered mission. "
                + Environment.NewLine + Environment.NewLine
                + "The cargo, which refers to non proppellant resources on board of the vessel, should be as high as possible to allow future missions to carry cargo up to the same mass as the original mission. "
                + Environment.NewLine
                + Environment.NewLine
                + "How tracking is started differs slightly on where the mission starts. ";

            public string StartTrackingLaunchTitle = "Start Tracking from Launch and Takeof";
            public string StartTrackingLaunch =
                "To start tracking at the launchpad or runway: "
                + Environment.NewLine
                + "-Open the main routine mission manager window using the \'rmm\' toolbar button."
                + Environment.NewLine
                + "-Select the 'Start Tracking' option. "
                + Environment.NewLine
                + "A window will open."
                + Environment.NewLine
                + "-Enter a name for the mission and press the 'Start Tracking' button.";

            public string StartTrackingDepartureTitle = "Start Tracking from Departure";
            public string StartTrackingDeparture =
                "To start tracking from a departures from a larger vessel in orbit: "
                + Environment.NewLine
                + "-Open the main routine mission manager window using the \'rmm\' toolbar button." 
                + Environment.NewLine
                + "-Select the 'Start Tracking' option. "
                + Environment.NewLine
                + "Every docking port on the vessel will momentarily get a 'Start Tracking' in the part menu. (This option can take a few seconds to become visible.)"
                + Environment.NewLine
                + "-On the docking port of the departing vessel select the 'Start Tracking' option from the part menu. Note: this should be the docking port which will be part of the departing vessel."
                + Environment.NewLine
                + "A window will open."
                + Environment.NewLine
                + "-Enter a name for the mission and press the 'Start Tracking' button.";

            public string TrackingFlightTitle = "Tracking the Flight";
            public string TrackingFlight =
                "After tracking is started no actions are required while the mission is underway. "
                + "A 'Tracking' window will appear listing all nearby vessels being tracked by the routine mission manager. "
                + "In the tracking window pressing the '>' button will open a window which will show detailed information of the mission. "
                + "Pressing the 'abort' button will stop the tracking for the respective vessel. "
                + Environment.NewLine + Environment.NewLine
                + "Tracking will also be ended automatically when crew is added after the start or the vessel docks with non mission vessels, except of course when the docking can be part of a mission arrival. "
                + Environment.NewLine + Environment.NewLine
                + "Tracking will persist over saves. ";

            public string TrackingArrivalTitle = "Tracking an Arrival";
            public string TrackingArrival =
                "An arrival will be tracked when the vessel docks with another vessel. "
                + Environment.NewLine + Environment.NewLine
                + "The other vessel needs to be in orbit around a celestial body for which the routine mission manager is enabled. ";
                
            public string TrackingLandingTitle = "Tracking a Landing";
            public string TrackingLanding =
                "A landing will be tracked automatically when the vessel lands on the home planet. "
                + Environment.NewLine + Environment.NewLine
                + "The vessel is required to remain on the ground and come to a complete stop for some duration before a landing is tracked. ";

            public string OrderingTitle = "Ordering a mission";
            public string OrderGeneral =
                "To order a mission the mission first needs to be tracked. In addition the location which serves as the starting point or destination of the mission needs to be registered. Both are covered elsewhere in this manual. "
                + Environment.NewLine
                + "To order a launch to or a departure from an orbiting vessel: "
                + Environment.NewLine
                + "-Switch to the vessel."
                + Environment.NewLine
                + "-Open the main routine mission manager window using the \'rmm\' toolbar button."
                + Environment.NewLine
                + "-Select the 'Order Launch' or 'Order Departure' option. "
                + Environment.NewLine
                + "A overview window will open listing the available missions from and to this location. For departure missions an identical departing vessel needs to be docked to the orbiting vessel, for it to be listed. "
                + Environment.NewLine
                + "-Press the '>' to open a detail window of the mission. "
                + Environment.NewLine
                + "-For launch mission the destination docking port will need to be set and for manned missions it will also possible to change the crew count and set a preferred crew for the mission. "
                + Environment.NewLine
                + "-Review the mission details. "
                + Environment.NewLine
                + "-Press the 'Order' button. "
                + Environment.NewLine
                + "The mission will now be ordered."
                + Environment.NewLine
                + Environment.NewLine
                + "All ordered missions can be seen when pressing the 'Ordered Missions' option in the main routine manager window. Here missions can also be cancelled. ";

            public string LocationsTitle = "Locations";
            public string LocationsGeneral =
                "To set the docking port on wich an ordered mission should arrive the docking port should first be named and registered within the routine mission manager. "
                + Environment.NewLine
                + "To register a docking port: "
                + Environment.NewLine
                + "-Open the main routine mission manager window using the \'rmm\' toolbar button."
                + Environment.NewLine
                + "-Select the 'Register Docking Ports' option. "
                + Environment.NewLine
                + "Every docking port on the vessel will momentarily get a 'Register Docking Port' in the part menu. (This option can take a few seconds to become visible.)"
                + Environment.NewLine
                + "Select the 'Register Docking Port' in the part menu of the docking port you want to register. "
                + Environment.NewLine
                + "A window will open.  the 'Register Docking Port' in the part menu of the docking port you want to register. "
                + Environment.NewLine
                + "Enter a name or code for the docking port and press the 'Register' button. "
                + Environment.NewLine
                + "The docking port will now be registered. ";
        }
    }
}
