using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommercialOfferings.OrderData
{
    public class OrderedMission
    {
        [Persistent]
        public string OrderId;
        [Persistent]
        public string MissionId;
        [Persistent]
        public int Type;

        private List<OrderValue> _orderValues = new List<OrderValue>();

        public ConfigNode SaveNode()
        {
            ConfigNode node = ConfigNode.CreateConfigFromObject(this);
            node.name = "ORDEREDMISSION";

            foreach (OrderValue orderValue in _orderValues)
            {
                ConfigNode orderValueConfig = ConfigNode.CreateConfigFromObject(orderValue);
                orderValueConfig.name = "ORDERVALUE";
                node.AddNode(orderValueConfig);
            }

            return node;
        }

        public static OrderedMission LoadNode(ConfigNode node)
        {
            OrderedMission orderedMission = ConfigNode.CreateObjectFromConfig<OrderedMission>(node);

            foreach (ConfigNode orderValueConfig in node.GetNodes("ORDERVALUE"))
            {
                OrderValue orderValue = ConfigNode.CreateObjectFromConfig<OrderValue>(orderValueConfig);
                orderedMission._orderValues.Add(orderValue);
            }

            return orderedMission;
        }

        public void WriteValues(Dictionary<string, object> values)
        {
            _orderValues.Clear();
            foreach (KeyValuePair<string, object> entry in values)
            {
                OrderValue newOrderValue = new OrderValue();
                newOrderValue.Name = entry.Key;
                newOrderValue.Type = entry.Value.GetType().Name;
                newOrderValue.Value = entry.Value.ToString();
                _orderValues.Add(newOrderValue);
            }
        }

        public Dictionary<string, object> ReadValues()
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (OrderValue orderValue in _orderValues)
            {
                values.Add(orderValue.Name, ValueObject(orderValue));
            }
            return values;
        }

        private object ValueObject(OrderValue orderValue)
        {
            if (orderValue.Type == typeof(int).Name)
            {
                int value = 0;
                int.TryParse(orderValue.Value, out value);
                return value;
            }
            else if (orderValue.Type == typeof(uint).Name)
            {
                uint value = 0;
                uint.TryParse(orderValue.Value, out value);
                return value;
            }
            else if (orderValue.Type == typeof(double).Name)
            {
                double value = 0.0;
                double.TryParse(orderValue.Value, out value);
                return value;
            }
            else if (orderValue.Type == typeof(string).Name)
            {
                return orderValue.Value;
            }
            else if (orderValue.Type == typeof(bool).Name)
            {
                bool value = false;
                bool.TryParse(orderValue.Value, out value);
                return value;
            }
            return null;
        }

        private class OrderValue
        {
            [Persistent]
            public string Name;
            [Persistent]
            public string Type;
            [Persistent]
            public string Value;
        }
    }
}
