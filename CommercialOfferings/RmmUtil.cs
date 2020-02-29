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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommercialOfferings
{
    public static class RmmUtil
    {
        public static string GamePath
        {
            get { return KSPUtil.ApplicationRootPath; }
        }
        public static string CommercialOfferingsPath
        {
            get { return "GameData" + Path.DirectorySeparatorChar + "RoutineMissionManager" + Path.DirectorySeparatorChar; }
        }
        public static System.Random Rand = new System.Random();

        public static double OrbitAltitude(Vessel vessel)
        {
            return ((vessel.orbit.semiMajorAxis - vessel.mainBody.Radius));
        }




        public static double OrbitAltitude(double semiMajorAxis, string body)
        {
            foreach (CelestialBody celestialBody in FlightGlobals.Bodies)
            {
                if (celestialBody.name == body)
                {
                    return ((semiMajorAxis - celestialBody.Radius));
                }
            }

            return 0;
        }

        public static bool IsPreLaunch(Vessel ves)
        {
            if (ves.situation == Vessel.Situations.PRELAUNCH ||
                (ves.situation == Vessel.Situations.LANDED &&
                 (ves.landedAt == "KSC_LaunchPad_Platform" ||
                  ves.landedAt.Contains("Runway"))))
            {
                return (true);
            }
            else
            {
                return (false);
            }
        }

        public static bool IsTrackingActive(Vessel vessel)
        {
            return IsTrackingActive(vessel.parts);
        }

        public static bool IsTrackingActive(List<Part> parts)
        {
            foreach (Part p in parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.GetType() == typeof(RMMModule))
                    {
                        RMMModule aRMMModule;
                        aRMMModule = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        if (aRMMModule.trackingActive && aRMMModule.trackingPrimary) { return true; }
                    }
                }
            }
            return false;
        }

        public static bool HasDockingPorts(Vessel vessel)
        {
            return HasDockingPorts(vessel.parts);
        }

        public static bool HasDockingPorts(List<Part> parts)
        {
            foreach (Part p in parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "RMMModule")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static int CountVesselParts(Vessel ves)
        {
            int count = 0;
            foreach (Part p in ves.parts)
            {
                count = count + 1;
            }
            return (count);
        }

        public static bool IsDocking(Part part)
        {
            ModuleDockingNode dockingPort = part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
            if (dockingPort.state.Length >= 7 && dockingPort.state.Substring(0, 7) == "Acquire")
                return (true);
            else
                return (false);
        }

        public static bool IsDocked(Vessel vessel, Part part)
        {
            ModuleDockingNode dockingPort = part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();

            //this port is docked
            if (dockingPort.state.Length >= 6 && dockingPort.state.Substring(0, 6) == "Docked" && null != dockingPort.vesselInfo.name)
                return (true);

            //no joined port filled
            if (dockingPort.dockedPartUId == 0) { return (false); }

            //find joined port
            foreach (Part p in vessel.parts)
            {
                if (p.flightID == dockingPort.dockedPartUId)
                {
                    ModuleDockingNode pDockingPort = p.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                    if (pDockingPort != null && pDockingPort.state.Length >= 6 && pDockingPort.state.Substring(0, 6) == "Docked")
                    {
                        return (true);
                    }
                }
            }
            return (false);
        }

        public static bool PartExistsInVessel(Vessel vessel, uint partFlightId)
        {
            foreach (Part part in vessel.parts)
            {
                if (part.flightID == partFlightId)
                {
                    return true;
                }
            }
            return false;
        }

        public static Part GetVesselPart(Vessel vessel, uint partFlightId)
        {
            foreach (Part part in vessel.parts)
            {
                if (part.flightID == partFlightId)
                {
                    return part;
                }
            }
            return null;
        }

        public static Part GetDockedPart(Vessel vessel, Part part)
        {
            ModuleDockingNode dockingPort = part.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
            if (dockingPort == null) { return null; }

            if (dockingPort.state.Length >= 6 && dockingPort.state.Substring(0, 6) == "Docked" && null != dockingPort.vesselInfo.name)
            {
                //this port is docked
                foreach (Part p in vessel.parts)
                {
                    ModuleDockingNode pDockingPort = p.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                    if (pDockingPort != null && pDockingPort.dockedPartUId == part.flightID)
                    {
                        return p;
                    }
                }
            }
            else if (dockingPort.dockedPartUId != 0)
            {
                //find joined port
                foreach (Part p in vessel.parts)
                {
                    if (p.flightID == dockingPort.dockedPartUId)
                    {
                        ModuleDockingNode pDockingPort = p.Modules.OfType<ModuleDockingNode>().FirstOrDefault();
                        if (pDockingPort != null && pDockingPort.state.Length >= 6 && pDockingPort.state.Substring(0, 6) == "Docked")
                        {
                            return p;
                        }
                    }
                }
            }
            return null;
        }

        public static List<Part> GetDockedParts(Vessel vessel, Part dockedPort)
        {
            Part parentPart = dockedPort;
            List<Part> childParts = new List<Part>();
            List<Part> linkedParts = null;
            linkedParts = GetLinkedParts(parentPart);
            uint dockedToPartId = 0;
            foreach (Part linkedPart in linkedParts)
            {
                var dockedPart = RmmUtil.GetDockedPart(vessel, linkedPart);
                if (!(dockedPart != null && dockedPart.flightID == parentPart.flightID))
                {
                    childParts.Add(linkedPart);
                }
                else
                {
                    dockedToPartId = linkedPart.flightID;
                }
            }
            int childPartIndex = 0;
            while (childPartIndex < childParts.Count)
            {
                linkedParts = GetLinkedParts(childParts[childPartIndex]);
                foreach (Part linkedPart in linkedParts)
                {
                    // don't add parent part
                    if (linkedPart.flightID == parentPart.flightID)
                    {
                        continue;
                    }

                    // don't add parts twice
                    bool present = false;
                    foreach (Part childPart in childParts)
                    {
                        if (linkedPart.flightID == childPart.flightID)
                        {
                            present = true;
                            break;
                        }
                    }
                    if (present)
                    {
                        continue;
                    }

                    childParts.Add(linkedPart);
                }
                childPartIndex++;
            }
            // return one list with all parts
            List<Part> allParts = null;
            allParts = childParts;
            allParts.Add(parentPart);
            return allParts;
        }

        public static List<Part> GetLinkedParts(Part part)
        {
            List<Part> linkedParts = new List<Part>();

            if (part.parent != null)
            {
                linkedParts.Add(part.parent);
            }
            foreach (Part child in part.children)
            {
                linkedParts.Add(child);
            }
            return linkedParts;
        }

        public static bool ForeignDockingPorts(Vessel ves, string trackID)
        {
            foreach (Part p in ves.parts)
            {
                foreach (PartModule pm in p.Modules)
                {
                    if (pm.ClassName == "ModuleDockingNode")
                    {
                        RMMModule ComOffMod = p.Modules.OfType<RMMModule>().FirstOrDefault();
                        if (ComOffMod.trackMissionId != trackID)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        

        #region Crew

        public static int CrewCount(Vessel vessel)
        {
            return CrewCount(vessel.parts);
        }

        public static int CrewCount(List<Part> parts)
        {
            int crew = 0;

            foreach (Part p in parts)
            {
                if (p.protoModuleCrew.Count > 0)
                {
                    crew = crew + p.protoModuleCrew.Count;
                }
            }

            return (crew);
        }

        public static int AstronautCrewCount(Vessel vessel)
        {
            return AstronautCrewCount(vessel.parts);
        }

        public static int AstronautCrewCount(List<Part> parts)
        {
            int crew = 0;

            foreach (Part part in parts)
            {
                if (part.protoModuleCrew.Count > 0)
                {
                    foreach (ProtoCrewMember cr in part.protoModuleCrew)
                    {
                        if (cr.type == ProtoCrewMember.KerbalType.Crew)
                        {
                            crew++;
                        }

                    }
                }
            }

            return (crew);
        }

        public static int CrewCapacityCount(Vessel ves)
        {
            int capacity = 0;

            foreach (Part p in ves.parts)
            {
                if (p.CrewCapacity > 0)
                {
                    capacity = capacity + p.CrewCapacity;
                }
            }

            return (capacity);
        }

        #endregion Crew

        #region Body

        private static string[] allowedBodies = null;
        private static string[] allowedHomeBodies = null;

        public static bool AllowedBody(string bodyName)
        {
            if (allowedBodies == null)
            {
                allowedBodies = System.IO.File.ReadAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "AllowedBodies.txt");
            }

            foreach (String allowedBody in allowedBodies)
            {
                if (bodyName == allowedBody.Trim()) { return (true); }
            }

            return (false);
        }

        public static bool HomeBody(string bodyName)
        {
            if (allowedHomeBodies == null)
            {
                allowedHomeBodies = System.IO.File.ReadAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "HomeBodies.txt");
            }

            foreach (String allowedHomeBody in allowedHomeBodies)
            {
                if (bodyName == allowedHomeBody.Trim()) { return (true); }
            }

            return (false);
        }

        public static string HomeBodyName()
        {
            if (allowedHomeBodies == null)
            {
                allowedHomeBodies = System.IO.File.ReadAllLines(RmmUtil.GamePath + RmmUtil.CommercialOfferingsPath + "HomeBodies.txt");
            }

            foreach (String allowedHomeBody in allowedHomeBodies)
            {
                return allowedHomeBody.Trim();
            }

            return ("Home");
        }

        #endregion Body

        #region Coordinates
        // adapted from: www.consultsarath.com/contents/articles/KB000012-distance-between-two-points-on-globe--calculation-using-cSharp.aspx
        public static double GetDistanceBetweenPoints(double lat1, double long1, double lat2, double long2, double bodyRadius)
        {
            double distance = 0;

            double dLat = (lat2 - lat1) / 180 * Math.PI;
            double dLong = (long2 - long1) / 180 * Math.PI;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                        + Math.Cos(lat2) * Math.Sin(dLong / 2) * Math.Sin(dLong / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            //Calculate radius of planet
            // For this you can assume any of the two points.
            double radiusE = bodyRadius; // Equatorial radius, in metres
            double radiusP = bodyRadius; // Polar Radius

            //Numerator part of function
            double nr = Math.Pow(radiusE * radiusP * Math.Cos(lat1 / 180 * Math.PI), 2);
            //Denominator part of the function
            double dr = Math.Pow(radiusE * Math.Cos(lat1 / 180 * Math.PI), 2)
                            + Math.Pow(radiusP * Math.Sin(lat1 / 180 * Math.PI), 2);
            double radius = Math.Sqrt(nr / dr);

            //Calaculate distance in metres.
            distance = radius * c;
            return distance;
        }

        #endregion Coordinates

        #region Resources

        public static List<string> GetCargoArray(Vessel vessel, List<string> proppelants)
        {
            return GetCargoArray(vessel.parts, proppelants);
        }

        public static List<string> GetCargoArray(List<Part> parts, List<string> proppelants)
        {
            List<string> cargoArray = new List<string>();

            foreach (Part part in parts)
            {
                foreach (PartResource resource in part.Resources)
                {
                    if (!proppelants.Contains(resource.info.name) && !cargoArray.Contains(resource.info.name) && resource.info.name != "ElectricCharge")
                    {
                        cargoArray.Add(resource.info.name);
                    }
                }
            }

            return cargoArray;
        }

        public static void GetProppellantArray(string returnResourcesString, ref string[] propellantArray)
        {
            string[] SplitArray = returnResourcesString.Split(',');

            foreach (String st in SplitArray)
            {
                string[] SplatArray = st.Split(':');
                string resourceName = SplatArray[0].Trim();
                double amount = Convert.ToDouble(SplatArray[1].Trim());

                Array.Resize(ref propellantArray, propellantArray.Length + 1);
                propellantArray[propellantArray.Length - 1] = resourceName;
            }
        }


        public static List<string> DetermineProppellantArray(Vessel vessel)
        {
            return DetermineProppellantArray(vessel.parts);
        }

        public static List<string> DetermineProppellantArray(List<Part> parts)
        {
            List<string> propellants = new List<string>();

            List<string> excludedPropellants = new List<string> { "ElectricCharge", "IntakeAir" };

            foreach (Part part in parts)
            {
                foreach (PartModule partModule in part.Modules)
                {
                    if (partModule.GetType() == typeof(ModuleEngines))
                    {
                        ModuleEngines mer = part.Modules.OfType<ModuleEngines>().FirstOrDefault();
                        foreach (Propellant pr in mer.propellants)
                        {
                            if (!propellants.Contains(pr.name) && !excludedPropellants.Contains(pr.name))
                            {
                                propellants.Add(pr.name);
                            }
                        }
                    }

                    if (partModule.GetType() == typeof(ModuleEnginesFX))
                    {
                        ModuleEnginesFX mefxr = part.Modules.OfType<ModuleEnginesFX>().FirstOrDefault();
                        foreach (Propellant pr in mefxr.propellants)
                        {
                            if (!propellants.Contains(pr.name) && !excludedPropellants.Contains(pr.name))
                            {
                                propellants.Add(pr.name);
                            }
                        }
                    }

                    if (partModule.GetType() == typeof(ModuleRCS))
                    {
                        ModuleRCS mrcs = part.Modules.OfType<ModuleRCS>().FirstOrDefault();
                        if (!propellants.Contains(mrcs.resourceName) && !excludedPropellants.Contains(mrcs.resourceName))
                        {
                            propellants.Add(mrcs.resourceName);
                        }
                    }

                    if (partModule.GetType() == typeof(ModuleRCSFX))
                    {
                        ModuleRCS mrcs = part.Modules.OfType<ModuleRCSFX>().FirstOrDefault();
                        if (!propellants.Contains(mrcs.resourceName) && !excludedPropellants.Contains(mrcs.resourceName))
                        {
                            propellants.Add(mrcs.resourceName);
                        }
                    }

                    if (partModule.GetType() == typeof(ModuleAblator))
                    {
                        ModuleAblator mabl = part.Modules.OfType<ModuleAblator>().FirstOrDefault();
                        if (!propellants.Contains(mabl.ablativeResource) && !excludedPropellants.Contains(mabl.ablativeResource))
                        {
                            propellants.Add(mabl.ablativeResource);
                        }
                    }
                }
            }
            return propellants;
        }

        public static List<string> DetermineResourceArray(Vessel vessel)
        {
            return DetermineResourceArray(vessel.parts);
        }

        public static List<string> DetermineResourceArray(List<Part> parts)
        {
            List<string> resources = new List<string>();

            List<string> excludedPropellants = new List<string> { "ElectricCharge", "IntakeAir" };

            foreach (Part part in parts)
            {
                foreach(PartResource partResource in part.Resources)
                {
                    if (!resources.Contains(partResource.resourceName))
                    {
                        resources.Add(partResource.resourceName);
                    }
                }
            }
            return resources;
        }

        public static double ReadResource(Vessel vessel, string resourceName)
        {
            return ReadResource(vessel.parts, resourceName);
        }

        public static double ReadResource(List<Part> parts, string resourceName)
        {
            double amountCounted = 0;

            foreach (Part p in parts)
            {
                foreach (PartResource r in p.Resources)
                {
                    if (r.resourceName == resourceName)
                    {
                        amountCounted = amountCounted + r.amount;
                    }
                }
            }

            return amountCounted;
        }

        public static double Mass(string resourceName, double amount)
        {
            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return (amount * prd.density);
        }
        public static double Cost(string resourceName, double amount)
        {
            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return (amount * prd.unitCost);
        }
        public static string DisplayName(string resourceName)
        {
            PartResourceDefinition prd = PartResourceLibrary.Instance.GetDefinition(resourceName);
            return prd.displayName;
        }

        #endregion Resources

        #region Price

        public static float CalculateVesselPrice(Vessel vessel)
        {
            double price = 0.0f;

            //cost parts
            foreach (Part part in vessel.parts)
            {
                double missingResCost = 0;
                foreach (PartResource r in part.Resources)
                {
                        missingResCost = missingResCost + RmmUtil.Cost(r.info.name, (r.maxAmount - r.amount));
                }

                price = price + (part.partInfo.cost - missingResCost + part.GetModuleCosts(0));
            }
            return ((float)price);
        }

        #endregion Price

        #region Time String

        public static string TimeString(double time)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string strT = "";

            bool positive;

            if (time >= 0)
            {
                positive = true;
            }
            else
            {
                positive = false;
            }

            days = (int)Math.Floor(time / 21600);
            time = time - (days * 21600);

            hours = (int)Math.Floor(time / 3600);
            time = time - (hours * 3600);

            minutes = (int)Math.Floor(time / 60);
            time = time - (minutes * 60);

            seconds = (int)Math.Floor(time);

            if (days > 0)
            {
                strT = days.ToString() + "d";
                strT = (hours != 0) ? strT + hours.ToString() + "h" : strT;
                strT = (minutes != 0) ? strT + minutes.ToString() + "m" : strT;
                strT = (seconds != 0) ? strT + seconds.ToString() + "s" : strT;
            }
            else if (hours > 0)
            {
                strT = hours.ToString() + "h";
                strT = (minutes != 0) ? strT + minutes.ToString() + "m" : strT;
                strT = (seconds != 0) ? strT + seconds.ToString() + "s" : strT;
            }
            else if (minutes > 0)
            {
                strT = minutes.ToString() + "m";
                strT = (seconds != 0) ? strT + seconds.ToString() + "s" : strT;
            }
            else if (seconds > 0)
            {
                strT = seconds.ToString() + "s";
            }

            //strT = days.ToString() + "d" + hours.ToString() + "h" + minutes.ToString() + "m" + seconds + "s";

            if (positive)
                return (strT);
            else
                return ("-" + strT);
        }

        public static string TimeEtaString(double time)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string strT = "";

            if (time >= 0)
            {
                days = (int)Math.Floor(time / 21600);
                time = time - (days * 21600);

                hours = (int)Math.Floor(time / 3600);
                time = time - (hours * 3600);

                minutes = (int)Math.Floor(time / 60);
                time = time - (minutes * 60);

                seconds = (int)Math.Floor(time);

                if (days > 1)
                {
                    strT = days.ToString() + "d";
                }
                else if (days > 0)
                {

                    strT = days.ToString() + "d" + hours.ToString() + "h";
                }
                else if (hours > 0)
                {
                    strT = hours.ToString() + "h";
                }
                else
                {
                    strT = "soon(TM)";
                }
            }
            else
            {
                if (time > -3600)
                {
                    strT = "come back later";
                }
                else
                {
                    strT = "maybe later";
                }
            }

            return (strT);
        }

        #endregion Time String


        public static float GetDockingDistance(Part dockingPort)
        {
            return 0.15f;
        }

        public static void ToMapView()
        {
            if (MapView.MapIsEnabled) { return; }
            MapView.EnterMapView();
        }

        public static string CleanString(String inputString)
        {
            String[] arr = Regex.Matches(inputString, @"([\w\s/+-/(/)/./,]+)")
                .OfType<Match>()
                .Select(m => m.Groups[0].Value)
                .ToArray();

            String outputString = "";

            StringBuilder builder = new StringBuilder();
            foreach (string value in arr)
            {
                builder.Append(value);
            }
            outputString = builder.ToStringAndRelease();
            return outputString;
        }
    }
}
