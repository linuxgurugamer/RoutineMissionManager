using CommercialOfferings.OrderData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CommercialOfferings
{
    [KSPScenario(
        ScenarioCreationOptions.AddToNewGames,
        GameScenes.SPACECENTER,
        GameScenes.FLIGHT,
        GameScenes.TRACKSTATION
    )]
    class RmmScenario : ScenarioModule
    {
        private static RmmScenario _instance;

        private RmmScenario()
        {
            _instance = this;
            InitializeScenario();
        }

        public static RmmScenario Instance
        {
            get
            {
                return _instance;
            }
        }

        public List<RegisteredDockingPort> RegisteredDockingPorts = new List<RegisteredDockingPort>();
        public List<OrderedMission> OrderedMissions = new List<OrderedMission>();

        public bool trainingsInitialized = false;

        public void InitializeScenario() 
        {

        }

        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnLoad(ConfigNode gameNode)
        {
            base.OnLoad(gameNode);

            this.RegisteredDockingPorts.Clear();
            this.OrderedMissions.Clear();

            try
            {
                ConfigNode registeredLocations = gameNode.GetNode("REGISTEREDLOCATIONS");
                if (registeredLocations != null)
                {
                    foreach (ConfigNode dockingPortConfig in registeredLocations.GetNodes("DOCKINGPORT"))
                    {
                        RegisteredDockingPort dockingPort = ConfigNode.CreateObjectFromConfig<RegisteredDockingPort>(dockingPortConfig);
                        this.RegisteredDockingPorts.Add(dockingPort);
                    }
                }

                ConfigNode orderedMissions = gameNode.GetNode("ORDEREDMISSIONS");
                if (orderedMissions != null)
                {
                    foreach (ConfigNode orderedMissionConfig in orderedMissions.GetNodes("ORDEREDMISSION"))
                    {
                        OrderedMission orderedMission = OrderedMission.LoadNode(orderedMissionConfig);
                        this.OrderedMissions.Add(orderedMission);
                    }
                }
            }
            catch
            {
                LoggerRmm.Error("RMM: loading from savefile failed");
            }
        }

        public override void OnSave(ConfigNode gameNode)
        {
            base.OnSave(gameNode);

            try
            {
                ConfigNode registeredLocations = new ConfigNode("REGISTEREDLOCATIONS");
                foreach (RegisteredDockingPort registeredDockingPort in this.RegisteredDockingPorts)
                {
                    ConfigNode dockingPortConfig = ConfigNode.CreateConfigFromObject(registeredDockingPort);
                    dockingPortConfig.name = "DOCKINGPORT";
                    registeredLocations.AddNode(dockingPortConfig);
                }

                gameNode.AddNode(registeredLocations);

                ConfigNode orderedMissions = new ConfigNode("ORDEREDMISSIONS");
                foreach (OrderedMission orderedMission in this.OrderedMissions)
                {
                    ConfigNode orderedMissionConfig = orderedMission.SaveNode();
                    orderedMissions.AddNode(orderedMissionConfig);
                }

                gameNode.AddNode(orderedMissions);
            }
            catch
            {
                LoggerRmm.Error("Rmm: saving to savefile failed");
            }
        }

        public void SetRegisteredDockingPort(uint flightID, string name)
        {
            if (flightID == 0) return;

            // search for existing port
            RegisteredDockingPort existingDockingPort = null;
            foreach (RegisteredDockingPort dockingPort in RegisteredDockingPorts)
            {
                if (dockingPort.flightId == flightID)
                {
                    existingDockingPort = dockingPort;
                    break;
                }
            }

            // edit port
            if (existingDockingPort != null)
            {
                if (String.IsNullOrEmpty(name.Trim()))
                {
                    // unregister
                    RegisteredDockingPorts.Remove(existingDockingPort);
                }
                else
                {
                    // change
                    existingDockingPort.Name = name;
                }
                return;
            }

            // new port
            if (!String.IsNullOrEmpty(name.Trim()))
            {
                // register new
                RegisteredDockingPort newDockingPort = new RegisteredDockingPort();
                newDockingPort.flightId = flightID;
                newDockingPort.Name = name;
                RegisteredDockingPorts.Add(newDockingPort);
            }
        }

        public string GetRegisteredDockingPort(uint flightID)
        {
            if (flightID == 0) return null;

            foreach (RegisteredDockingPort dockingPort in RegisteredDockingPorts)
            {
                if (dockingPort.flightId == flightID)
                {
                    return dockingPort.Name;
                }
            }

            return null;
        }

        public List<RegisteredDockingPort> GetRegisteredDockingPortsList()
        {
            List<RegisteredDockingPort> list = new List<RegisteredDockingPort>();

            foreach (RegisteredDockingPort dockingPort in RegisteredDockingPorts)
            {
                RegisteredDockingPort dockingPortCopy = dockingPort.Copy();
                list.Add(dockingPortCopy);
            }

            return list;
        }

        public void SetOrderedMission(string orderId, string missionId, int type, Dictionary<string,object> orderValues)
        {
            // search for existing order
            OrderedMission existingOrderedMission = null;
            if (!String.IsNullOrEmpty(orderId))
            {
                foreach (OrderedMission orderdMission in OrderedMissions)
                {
                    if (orderdMission.OrderId == orderId)
                    {
                        existingOrderedMission = orderdMission;
                        break;
                    }
                }
            }

            // edit port
            if (existingOrderedMission != null)
            {
                if (String.IsNullOrEmpty(missionId) && orderValues == null)
                {
                    // unorder
                    OrderedMissions.Remove(existingOrderedMission);
                }
                else
                {
                    // edit order
                    existingOrderedMission.MissionId = missionId;
                    existingOrderedMission.Type = type;
                    existingOrderedMission.WriteValues(orderValues);
                }
                return;
            }

            // new order
            if (!String.IsNullOrEmpty(missionId) && orderValues != null)
            {
                // register new
                OrderedMission newOrderedMission = new OrderedMission();
                newOrderedMission.OrderId = RmmUtil.Rand.Next(1, 999999999).ToString();
                newOrderedMission.MissionId = missionId;
                newOrderedMission.Type = type;
                newOrderedMission.WriteValues(orderValues);
                OrderedMissions.Add(newOrderedMission);
            }
        }

        public void GetOrderedMission(string orderId, out string missionId, out Dictionary<string, object> orderValues)
        {
            missionId = null;
            orderValues = new Dictionary<string, object>();
            if (String.IsNullOrEmpty(orderId)) return;

            foreach (OrderedMission orderedMission in OrderedMissions)
            {
                if (orderedMission.OrderId == orderId)
                {
                    missionId = orderedMission.MissionId;
                    orderValues = orderedMission.ReadValues();
                    return;
                }
            }
        }

        public List<string> GetOrdersOfType(int type)
        {
            List<string> orders = new List<string>();

            foreach (OrderedMission orderedMission in OrderedMissions)
            {
                if (orderedMission.Type == type)
                {
                    orders.Add(orderedMission.OrderId);
                }
            }

            return orders;
        }
    }



    class Record : IConfigNode
    {
        public string Kerbonaut;
        public float TrainingExperience;
        public string CompletedTraining;

        public Record()
        { }

        public Record(ProtoCrewMember kerbal)
        {
            Kerbonaut = kerbal.name;
            TrainingExperience = 0.0f;
            CompletedTraining = "";
        }

        public void Load(ConfigNode node)
        {
            this.Kerbonaut = node.GetValue("Kerbonaut");
            this.TrainingExperience = (float)Convert.ToDouble(node.GetValue("TrainingExperience"));
            this.CompletedTraining = node.GetValue("CompletedTraining");
        }

        public void Save(ConfigNode node)
        {
            node.name = "RECORD";

            if (!node.HasValue("Kerbonaut"))
            {
                node.AddValue("Kerbonaut", this.Kerbonaut);
            }
            else
            {
                node.SetValue("Kerbonaut", this.Kerbonaut);
            }

            if (!node.HasValue("TrainingExperience"))
            {
                node.AddValue("TrainingExperience", this.TrainingExperience.ToString());
            }
            else
            {
                node.SetValue("TrainingExperience", this.TrainingExperience.ToString());
            }

            if (!node.HasValue("CompletedTraining"))
            {
                node.AddValue("CompletedTraining", this.CompletedTraining);
            }
            else
            {
                node.SetValue("CompletedTraining", this.CompletedTraining);
            }
        }
    }

    public class Training
    {
        public string Code = "";
        public string Name = "";
        public string Description = "";

        public string Location = "";

        public string Trait = "";
        public int Capacity = 1;

        public float Duration = 0f;
        public float Experience = 0f;
        //public float Courage = 0f;
        //public float Stupidity = 0f;

        //public void Load(ConfigNode node)
        //{
        //    this.Kerbonaut = node.GetValue("Kerbonaut");
        //    this.TrainingExperience = (float)Convert.ToDouble(node.GetValue("TrainingExperience"));
        //    this.CompletedTraining = node.GetValue("CompletedTraining");
        //}
        //
        //public void Save(ConfigNode node)
        //{
        //    node.name = "RECORD";
        //
        //    if (!node.HasValue("Kerbonaut"))
        //    {
        //        node.AddValue("Kerbonaut", this.Kerbonaut);
        //    }
        //    else
        //    {
        //        node.SetValue("Kerbonaut", this.Kerbonaut);
        //    }
        //
        //    if (!node.HasValue("TrainingExperience"))
        //    {
        //        node.AddValue("TrainingExperience", this.TrainingExperience.ToString());
        //    }
        //    else
        //    {
        //        node.SetValue("TrainingExperience", this.TrainingExperience.ToString());
        //    }
        //
        //    if (!node.HasValue("CompletedTraining"))
        //    {
        //        node.AddValue("CompletedTraining", this.CompletedTraining);
        //    }
        //    else
        //    {
        //        node.SetValue("CompletedTraining", this.CompletedTraining);
        //    }
        //}
    }
}
