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

        public ZoneRecoveryScenario(/*ZoneRecoveryScenarioSetup setup*/)
        {
            //Setup = setup;
            Orders = new Dictionary<int, List<Order>>();
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

        public void AddSuccessOrderList(List<Order> ol)
        {
            CurrentID++;
            Orders.Add(CurrentID, ol);
        }

        // TODO Add the parameters needed to set up all the posibble outcomes for the initial setup.
        // This would be (Filled + New) and (New + Filled) with the Unitsize calculated at the time
        // the Orders ar posted. 
        // Next steps should have the orders to be placed in case the conditions of Orders have been met... or something
        //public void SetUpInitial (ZoneRecoveryScenarioSetup setup)
        //{
        //    switch (setup)
        //    {
        //        case ZoneRecoveryScenarioSetup.PDNI_TwoSides:
        //            // Action: TP in same direction, REV in opposite direction
        //            break;
        //        case ZoneRecoveryScenarioSetup.UPWinding:

        //            break;
        //        case ZoneRecoveryScenarioSetup.DOWNWinding:

        //            break;
        //        case ZoneRecoveryScenarioSetup.UPUnwinding:

        //            break;
        //        case ZoneRecoveryScenarioSetup.DOWNUnwinding:

        //            break;
        //    }
        //}

        
    }
}
