using CommercialOfferings.MissionData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings
{
    public enum MissionKind { Potential, Ordered}

    public class RoutineMission
    {
        protected string _orderId = null;
        protected Mission _mission = null;
        private Dictionary<string, object> _orderValues = new Dictionary<string, object>();

        public string OrderId
        {
            get
            {
                return _orderId;
            }
        }

        public string MissionId
        {
            get
            {
                return _mission?.MissionId;
            }
        }

        public MissionKind Kind 
        {
            get
            {
                if (String.IsNullOrEmpty(_orderId))
                {
                    return MissionKind.Potential;
                }
                else
                {
                    return MissionKind.Ordered;
                }
            }
        }

        public string FolderPath
        {
            get { return _mission?.FolderPath; }
        }

        protected void SetOrderValue(string name, object value)
        {
            if (_orderValues.ContainsKey(name))
            {
                _orderValues[name] = value;
            }
            else
            {
                _orderValues.Add(name, value);
            }
        }

        protected T GetOrderValue<T>(string name)
        {
            if (_orderValues.ContainsKey(name))
            {
                return (T)_orderValues[name];
            }
            else
            {
                return default(T);
            }
        }

        public virtual CheckList Valid() { return new CheckList(); }

        public void OrderRoutineMission()
        {
            RmmScenario.Instance.SetOrderedMission(_orderId, _mission.MissionId, _mission.Info.Type, _orderValues);
        }

        public void UnorderRoutineMission()
        {
            RmmScenario.Instance.SetOrderedMission(_orderId, null, 0, null);
        }



        public static T AssembleRoutineMission<T>(string orderId, List<Mission> missions) where T : RoutineMission, new()
        {
            string missionId = null;
            Dictionary<string, object> orderValues = null;
            RmmScenario.Instance.GetOrderedMission(orderId, out missionId, out orderValues);
            if (String.IsNullOrEmpty(missionId)) { return default(T); }

            Mission missionIdMission = null;
            foreach (Mission mission in missions)
            {
                if (mission.MissionId == missionId)
                {
                    missionIdMission = mission;
                }
            }

            if (missionIdMission == null ) { return default(T); }

            T routineMission = new T();
            routineMission._orderId = orderId;
            routineMission._mission = missionIdMission;
            routineMission._orderValues = orderValues;

            return routineMission;
        }

        public static T AssemblePotentialRoutineMission<T>(Mission mission) where T : RoutineMission, new()
        {
            if (mission == null) return null;
            if (mission.Info == null) return null;
            if (!String.IsNullOrEmpty(mission.Info.Campaign) && mission.Info.Campaign != HighLogic.SaveFolder) return null;

            T routineMission = new T();
            routineMission._mission = mission;

            if (!routineMission.Valid().CheckSuccess) return null;

            return routineMission;
        }

        public static CheckList RoutineMissionValid<T>(Mission mission) where T : RoutineMission, new()
        {
            T routineMission = new T();
            routineMission._mission = mission;

            CheckList checkList = routineMission.Valid();

            return checkList;
        }
    }
}
