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
        private ZoneRecoveryDirection ProfitDirection { get; set; }

        #endregion Core variables

        #region Containers

        public List<ZoneRecoveryOrder> OrdersList;
        public ZoneRecoveryScenario Scenario;

        #endregion Containers

        #region Constructors

        public ZoneRecoveryOrderBatch(long batchNumber, ZoneRecoveryScenario scenario, ZoneRecoveryDirection direction = ZoneRecoveryDirection.Undefined)
        {
            BatchNumber = batchNumber;
            OrdersList = new List<ZoneRecoveryOrder>();
            Scenario = scenario;
            ProfitDirection = direction;
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

        public bool KillRestingOrders()
        {
            int errorCounter = 0;
            // Iterate all known orders
            foreach (ZoneRecoveryOrder zro in OrdersList)
            {
                try
                {
                    // If order is New (resting), kill it.
                    if (zro.CurrentStatus == ZoneRecoveryOrderStatus.New)
                    {
                        zro.KillMe();

                        if (zro.CurrentStatus != ZoneRecoveryOrderStatus.Canceled)
                        {
                            throw new Exception($"order {zro.DeleteServerResponse.ClOrdId} did not return a Canceled status after trying to kill it");
                        }
                    }
                }
                catch (Exception exc)
                {
                    errorCounter++;
                    Log.Error($"KillRestingOrders: { exc.Message}");
                }
            }
            return (errorCounter > 0) ? false : true;
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
                    if (ProfitDirection == ZoneRecoveryDirection.Undefined)
                        ProfitDirection = 
                            (OrdersList
                                .Where(x => 
                                    x.LastServerResponse.OrdStatus == OrderStatus.Filled && 
                                    (x.ZROrderType == ZoneRecoveryOrderType.FS || x.ZROrderType == ZoneRecoveryOrderType.TP))
                                .Single()
                                .ZRAccount == ZoneRecoveryAccount.A) ? ZoneRecoveryDirection.Up : ZoneRecoveryDirection.Down;

                    return KillRestingOrders();
                } 
            }
            // TODO do not return anything but initiate the actions required
            return false;
        }

        #endregion Evaluators

    }
}
