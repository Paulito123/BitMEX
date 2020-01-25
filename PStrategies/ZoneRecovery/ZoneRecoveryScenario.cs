using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitmex.Client.Websocket.Responses.Orders;

namespace PStrategies.ZoneRecovery
{
    public class ZoneRecoveryScenario
    {
        //private ZoneRecoveryScenarioSetup Setup { get; }
        private int CurrentID = 0; 
        public readonly Dictionary<int, List<Order>> Orders;
        public readonly Dictionary<int, ZoneRecoveryDirection> DirectionByScenario;

        public ZoneRecoveryScenario(/*ZoneRecoveryScenarioSetup setup*/)
        {
            //Setup = setup;
            Orders              = new Dictionary<int, List<Order>>();
            DirectionByScenario = new Dictionary<int, ZoneRecoveryDirection>();
        }
        
        public void AddSuccessOrder(Order o, bool isNew = false)
        {
            if (isNew)
            {
                CurrentID++;
                Orders.Add(CurrentID, new List<Order>());
            }

            Orders[CurrentID].Add(o);
        }

        public void AddSuccessOrderList(List<Order> ol, ZoneRecoveryDirection dir)
        {
            CurrentID++;
            Orders.Add(CurrentID, ol);
            DirectionByScenario.Add(CurrentID, dir);
        }

    }
}
