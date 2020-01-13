using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Dto;
using BitMEXRest.Client;

using Serilog;

/* TODO
 * Er zijn maar 5 mogelijke succesvolle scenarios:
 * 1. 1 van de twee FB orders is filled
 * 2. Een TP is filled
 * 3. Een TP is filled + TL is filled
 * 4. Een reverse is filled
 * 5. Alle orders zijn gesloten
 * 
 * --> Maak een klasse 
 * */

namespace PStrategies.ZoneRecovery
{
    /// <summary>
    /// ZoneRecoveryOrderBatch is a wrapper class that takes a batch number as ID. 
    /// BatchNr should be uniquely identifying every instance of a batch within parent structures.
    /// </summary>
    class ZoneRecoveryOrderBatch
    {
        #region Core variables

        private long BatchNumber { get; }
        private int NrOfOrdersExpected { get; set; }

        #endregion Core variables
        
        #region Containers

        public List<ZoneRecoveryOrder> OrdersList;
        public ZoneRecoveryScenario Scenario;

        #endregion Containers
        
        #region Constructors

        public ZoneRecoveryOrderBatch(long batchNumber, ZoneRecoveryScenario scenario)
        {
            BatchNumber = batchNumber;
            OrdersList = new List<ZoneRecoveryOrder>();
            Scenario = scenario;
        }

        #endregion Constructors

        #region Setters, Getters, Adders, Updaters and Deleters
        
        /// <summary>
        /// Attach the initial order response.
        /// </summary>
        /// <param name="ordr"></param>
        public void SetFirstResponse(OrderDto ordr)
        {
            OrdersList.Where(x => x.PostParams.ClOrdID == ordr.ClOrdId).Single().FirstServerResponse = ordr;
            //TODO OrdersList.Where(x => x.PostParams.ClOrdID == ordr.ClOrdId).Single().CurrentStatus;
        }

        /// <summary>
        /// Attach the last known order response.
        /// </summary>
        /// <param name="ordr"></param>
        public void SetSingleLastResponse(Order ordr)
        {
            OrdersList.Where(x => x.PostParams.ClOrdID == ordr.ClOrdId).Single().LastServerResponse = ordr;
        }

        public void SetMultipleLastResponse(List<Order> lo)
        {
            if (lo != null)
            {
                foreach (Order o in lo)
                {
                    SetSingleLastResponse(o);
                }
            }
        }

        public List<string> GetClOrdIDList()
        {
            return OrdersList.Select(x => x.PostParams.ClOrdID).ToList();
        }

        public void KillRestingOrders()
        {
            // Iterate all known orders
            foreach (ZoneRecoveryOrder zro in OrdersList)
            {
                // If order is New (resting), kill it.
                if (zro.CurrentStatus == ZoneRecoveryOrderStatus.New)
                {
                    //var result = zro.KillOrder();

                    //if (result.Result.Single().OrdStatus != "Canceled")
                    //{
                    //    throw new Exception($"KillRestingOrders: order {result.Result.Single().ClOrdId} did not return a Canceled status after trying to kill it");
                    //}
                }
                //else if (zro.CurrentStatus == ZoneRecoveryOrderStatus.)
            }
        }

        public void ProcessPostOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
            // TODO Handle Exception !!!
            if (task.Exception != null)
            {
                Log.Error($"ProcessPostOrderResult: {(task.Exception.InnerException ?? task.Exception).Message}");
            }
            else
            {
                SetFirstResponse(task.Result.Result);
                Log.Information($"ProcessPostOrderResult: order placed with Id [{task.Result.Result.OrderId}] and status [{task.Result.Result.OrdStatus}]");
            }
        }

        #endregion Setters, Getters, Adders, Updaters and Deleters

        #region Evaluators

        public bool EvaluateAndInitiate()
        {
            bool isSuccess = false;

            // Iterate the scenarios
            foreach (int k in Scenario.Orders.Keys)
            {
                // Iterate the orders
                int successCounter = 0;
                foreach (Order o in Scenario.Orders[k])
                {
                    // Check if conditions are met
                    if (OrdersList.Where(x => x.LastServerResponse.ClOrdId == o.ClOrdId).Single().LastServerResponse.OrdStatus == o.OrdStatus)
                        successCounter++;
                    else
                        break;
                }
                if (OrdersList.Count() == successCounter)
                {
                    isSuccess = true;
                    break;
                }   
            }

            if (isSuccess)
            {

            }

            // TODO do not return anything but initiate the actions required
            return isSuccess;
        }

        #endregion Evaluators

    }
}
