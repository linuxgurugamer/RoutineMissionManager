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
using System.Xml.Serialization;
using System.Xml;

namespace CommercialOfferings.MissionData
{
    public class Mission
    {
        public const string MISSION_FILE = "mission.xml";

        public string MissionId;
        public string FolderPath = "";

        public string Name = "";
        public string CompanyName = "";
        public string VehicleName = "";
        public string LaunchSystemName = "";

        public double Price = 0;
        public double VehicleReturnFee = 0;
        public double Time = 0;
        public string Body = "";
        public double MaxOrbitAltitude = 0;

        public string Description = "";

        public int MinimumCrew = 0;
        public int MaximumCrew = 0;

        public bool ReturnEnabled = false;
        public bool SafeReturn = false;
        public string ReturnResources = "";
        public double ReturnCargoMass = 0.0;

        public uint flightIDDockPart
        {
            get
            {
                if (_arrival != null)
                {
                    return _arrival.flightIDDockPart;
                }
                return 0;
            }
        }

        public MissionInfo Info
        {
            get { return _info; }
        }
        protected MissionInfo _info = null;

        public MissionLaunch Launch
        {
            get { return _launch; }
        }
        private MissionLaunch _launch = null;

        public MissionArrival Arrival
        {
            get { return _arrival; }
        }
        private MissionArrival _arrival = null;

        public MissionDeparture Departure
        {
            get { return _departure; }
        }
        private MissionDeparture _departure = null;

        public List<MissionLanding> Landings
        {
            get { return _landings; }
        }
        private List<MissionLanding> _landings = new List<MissionLanding>();

        public Mission()
        {

        }

        public static Mission NewMission(string missionId, int type, string name)
        {
            Mission mission = new Mission();
            mission.MissionId = missionId;
            mission.FolderPath = RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + missionId;

            Directory.CreateDirectory(RmmUtil.GamePath + mission.FolderPath);


            MissionInfo info = new MissionInfo
            {
                Type = type,
                Name = name,
                Campaign = HighLogic.SaveFolder
            };
            mission._info = info;

            mission.SaveMission();

            return mission;
        }

        public static Mission GetMissionById(string missionId)
        {
            Mission mission = new Mission();
            mission.MissionId = missionId;
            mission.FolderPath = RmmUtil.CommercialOfferingsPath + "Missions" + Path.DirectorySeparatorChar + missionId;

            mission.LoadMission(mission.FolderPath);

            return mission;
        }

        public static Mission GetMissionByPath(String FolderPath)
        {
            Mission mission = new Mission();
            mission.MissionId = Path.GetFileName(FolderPath);
            mission.FolderPath = FolderPath;

            mission.LoadMission(mission.FolderPath);

            return mission;
        }

        private void LoadMission(String FolderPath)
        {
            XmlDocument doc = new XmlDocument();
            String filePath = RmmUtil.GamePath + Path.DirectorySeparatorChar + FolderPath + Path.DirectorySeparatorChar + MISSION_FILE;
            if (!File.Exists(filePath)) { _info = null; return; }

            doc.Load(filePath);

            _info = null;
            _launch = null;
            _arrival = null;
            _departure = null;
            _landings.Clear();

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Name == typeof(MissionInfo).Name)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(MissionInfo));
                    _info = (MissionInfo)ser.Deserialize(new XmlNodeReader(node));
                }

                if (node.Name == typeof(MissionLaunch).Name)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(MissionLaunch));
                    _launch = (MissionLaunch)ser.Deserialize(new XmlNodeReader(node));
                }

                if (node.Name == typeof(MissionArrival).Name)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(MissionArrival));
                    _arrival = (MissionArrival)ser.Deserialize(new XmlNodeReader(node));
                }

                if (node.Name == typeof(MissionDeparture).Name)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(MissionDeparture));
                    _departure = (MissionDeparture)ser.Deserialize(new XmlNodeReader(node));
                }

                if (node.Name == typeof(MissionLanding).Name)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(MissionLanding));
                    _landings.Add((MissionLanding)ser.Deserialize(new XmlNodeReader(node)));
                }
            }
        }


        private void SaveMission()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("Mission");
            doc.AppendChild(root);
            XmlNode missionNode = doc.FirstChild;

            if (Info != null)
            {
                XmlNode node = doc.ImportNode(SerializeObjectToXmlNode(Info), true);
                missionNode.AppendChild(node);
            }

            if (Launch != null)
            {
                XmlNode node = doc.ImportNode(SerializeObjectToXmlNode(Launch), true);
                missionNode.AppendChild(node);
            }

            if (Arrival != null)
            {
                XmlNode node = doc.ImportNode(SerializeObjectToXmlNode(Arrival), true);
                missionNode.AppendChild(node);
            }
            if (Departure != null)
            {
                XmlNode node = doc.ImportNode(SerializeObjectToXmlNode(Departure), true);
                missionNode.AppendChild(node);
            }
            foreach (MissionLanding landing in Landings)
            {
                XmlNode node = doc.ImportNode(SerializeObjectToXmlNode(landing), true);
                missionNode.AppendChild(node);
            }

            doc.Save(RmmUtil.GamePath + Path.DirectorySeparatorChar + FolderPath + Path.DirectorySeparatorChar + MISSION_FILE);
        }

        public void TrackLaunch(MissionLaunch launch)
        {
            LoadMission(FolderPath);
            if (_info == null) { return; }

            // delete later tracked mission events
            _arrival = null;
            _landings.Clear();


            _launch = launch;
            SaveMission();
        }

        public void TrackArrival(MissionArrival arrival)
        {
            LoadMission(FolderPath);
            if (_info == null) { return; }

            // find landings with indentical parts
            List<MissionLanding> overlappingLandings = new List<MissionLanding>();
            foreach (MissionLanding storedLanding in _landings)
            {
                foreach (MissionPart storedMissionPart in storedLanding.Parts)
                {
                    foreach (MissionPart missionPart in arrival.Parts)
                    {
                        if (storedMissionPart.flightID == missionPart.flightID)
                        {
                            overlappingLandings.Add(storedLanding);
                        }
                    }
                }
            }
            // delete landings with indentical parts
            foreach (MissionLanding overlappingLanding in overlappingLandings)
            {
                _landings.Remove(overlappingLanding);
            }

            _arrival = arrival;
            SaveMission();
        }

        public void TrackDeparture(MissionDeparture departure)
        {
            LoadMission(FolderPath);
            if (_info == null) { return; }

            // delete later tracked mission events
            _landings.Clear();

            _departure = departure;
            SaveMission();
        }

        public void TrackLanding(MissionLanding landing)
        {
            LoadMission(FolderPath);
            if (_info == null) { return; }

            // find landings with indentical parts
            List<MissionLanding> overlappingLandings = new List<MissionLanding>();
            foreach (MissionLanding storedLanding in _landings)
            {
                foreach (MissionPart storedMissionPart in storedLanding.Parts)
                {
                    foreach (MissionPart missionPart in landing.Parts)
                    {
                        if (storedMissionPart.flightID == missionPart.flightID)
                        {
                            overlappingLandings.Add(storedLanding);
                        }
                    }
                }
            }
            // delete landings with indentical parts
            foreach (MissionLanding overlappingLanding in overlappingLandings)
            {
                _landings.Remove(overlappingLanding);
            }

            // find arrival with identical parts
            bool overlappingArrival = false;
            if (_arrival != null)
            {
                foreach (MissionPart arrivalMissionPart in _arrival.Parts)
                {
                    foreach (MissionPart missionPart in landing.Parts)
                    {
                        if (arrivalMissionPart.flightID == missionPart.flightID)
                        {
                            overlappingArrival = true;
                        }
                    }
                }
            }
            // delete arrival with indentical parts
            if (overlappingArrival)
            {
                _arrival = null;
            }


            _landings.Add(landing);
            SaveMission();
        }

        public static bool PartIsMissionPart(Part part, List<MissionPart> missionParts)
        {
            foreach(MissionPart missionPart in missionParts)
            {
                if (missionPart.flightID == part.flightID) { return true; }
            }
            return false;
        }


        private static XmlNode SerializeObjectToXmlNode(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("Argument cannot be null");

            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            XmlNode resultNode = null;
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                try
                {
                    xmlSerializer.Serialize(memoryStream, obj, ns);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
                memoryStream.Position = 0;
                XmlDocument doc = new XmlDocument();
                doc.Load(memoryStream);
                resultNode = doc.DocumentElement;
            }
            return resultNode;
        }



        public static List<Mission> LoadMissions()
        {
            List<Mission> missions = new List<Mission>();

            LoadMissionsDirectory(RmmUtil.GamePath + Path.DirectorySeparatorChar + "GameData", ref missions);

            return missions;
        }

        private static void LoadMissionsDirectory(string searchDirectory, ref List<Mission> missions)
        {

            if (File.Exists(searchDirectory + Path.DirectorySeparatorChar + "CommercialOfferingsPackMarkerFile"))
            {
                string[] directoryOfferings = Directory.GetDirectories(searchDirectory);

                foreach (String dir in directoryOfferings)
                {
                    if (File.Exists(dir + Path.DirectorySeparatorChar + Mission.MISSION_FILE))
                    {
                        string folderPath = dir.Substring(RmmUtil.GamePath.ToString().Length, dir.Length - RmmUtil.GamePath.ToString().Length);
                        Mission mission = Mission.GetMissionByPath(folderPath);

                        if (mission.Info != null &&
                            (String.IsNullOrEmpty(mission.Info.Campaign) || mission.Info.Campaign == HighLogic.SaveFolder))
                        {
                            missions.Add(mission);
                        }
                    }
                }
            }
            else
            {
                string[] searchDirectories = Directory.GetDirectories(searchDirectory);

                foreach (String dir in searchDirectories)
                {
                    LoadMissionsDirectory(dir, ref missions);
                }
            }
        }

        public void DeleteMission()
        {
            System.IO.Directory.Delete(RmmUtil.GamePath + Path.DirectorySeparatorChar + FolderPath, true);
        }
    }
}