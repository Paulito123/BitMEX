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
        public ZoneRecoveryDirection Direction { get; set; }
        public static DateTime StartDateTime { get; set; }
        public static DateTime StopDateTime { get; set; }

        #endregion Core variables

        #region Containers

        public List<ZoneRecoveryOrder> ZROrdersList;
        public ZoneRecoveryScenario SuccessScenario;
        public ZoneRecoveryScenario FailureScenario;

        #endregion Containers

        #region Constructors

        public ZoneRecoveryOrderBatch(long batchNumber, ZoneRecoveryDirection direction = ZoneRecoveryDirection.Undefined)
        {
            BatchNumber = batchNumber;
            ZROrdersList = new List<ZoneRecoveryOrder>();
            SuccessScenario = new ZoneRecoveryScenario();
            FailureScenario = new ZoneRecoveryScenario();
            Direction = direction;
            StartDateTime = DateTime.Now;
        }

        #endregion Constructors

        #region Setters, Getters, Adders, Updaters and Deleters
        
        /// <summary>
        /// Attach the initial order response.
        /// </summary>
        /// <param name="ordr"></param>
        public void SetFirstResponse(OrderDto ordr)
        {
            ZROrdersList.Where(x => x.PostParams.ClOrdID == ordr.ClOrdId).Single().FirstServerResponse = ordr;
            //TODO OrdersList.Where(x => x.PostParams.ClOrdID == ordr.ClOrdId).Single().CurrentStatus;
        }

        public List<string> GetClOrdIDList()
        {
            return ZROrdersList.Select(x => x.PostParams.ClOrdID).ToList();
        }

        public bool KillRestingOrders()
        {
            int errorCounter = 0;
            // Iterate all known orders
            foreach (ZoneRecoveryOrder zro in ZROrdersList)
            {
                try
                {
                    // If order is New (resting), kill it.
                    if (zro.CurrentStatus == ZoneRecoveryOrderStatus.New)
                    {
                        Log.Debug($"KillRestingOrders: before zro.KillMe() > {zro.CurrentStatus}");

                        zro.KillMe();

                        Log.Debug($"KillRestingOrders: after zro.KillMe() > {zro.CurrentStatus}");

                        //if (zro.CurrentStatus != ZoneRecoveryOrderStatus.Canceled)
                        //    throw new Exception($"order {zro.DeleteServerResponse.ClOrdId} did not return a Canceled status after trying to kill it");
                        
                                                    
                    }
                    StopDateTime = DateTime.Now;
                }
                catch (Exception exc)
                {
                    errorCounter++;
                    Log.Error($"KillRestingOrders: { exc.Message}");
                }
                //finally
                //{
                //    if (errorCounter == 0)
                //    {

                //    }
                //}
            }
            return (errorCounter > 0) ? false : true;
        }

        public void ProcessPostOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
            string message = "";
            // TODO Handle Exception !!!
            if (task.Exception != null)
            {
                message = $"ProcessPostOrderResult: {(task.Exception.InnerException ?? task.Exception).Message}";
                Log.Error(message);
            }
            else
            {
                SetFirstResponse(task.Result.Result);
                message = $"ProcessPostOrderResult: order placed with Id [{task.Result.Result.ClOrdId}] and status [{task.Result.Result.OrdStatus}]";
                Log.Information(message);
            }
            Console.WriteLine(message);
        }

        #endregion Setters, Getters, Adders, Updaters and Deleters

        #region Evaluators

        public bool EvaluateAndInitiate(List<Order> allOrders)
        {
            // Iterate the failure scenarios
            foreach (int k in FailureScenario.Orders.Keys)
            {
                // Iterate the orders
                int failureCounter = 0;
                foreach (Order o in FailureScenario.Orders[k])
                {
                    // Check if conditions are met
                    if (allOrders.Where(x => x.ClOrdId == o.ClOrdId && x.OrdStatus == o.OrdStatus).Count() == 1)
                        failureCounter++;
                    else
                        break;

                    Console.WriteLine($"FailureScenario[{k}]: ClOrdId={o.ClOrdId}, OrdStatus={o.OrdStatus.ToString()} has [{allOrders.Where(x => x.ClOrdId == o.ClOrdId && x.OrdStatus == o.OrdStatus).Count()}] hits bring total to [{failureCounter}]");
                    //Log.Debug($"FailureScenario[{k}]: ClOrdId={o.ClOrdId}, OrdStatus={o.OrdStatus.ToString()} has [{allOrders.Where(x => x.ClOrdId == o.ClOrdId && x.OrdStatus == o.OrdStatus).Count()}] hits bring total to [{failureCounter}]");
                }
                if (allOrders.Count() == failureCounter)
                {
                    // TODO: flatten positions
                    Direction = SuccessScenario.DirectionByScenario[k];
                    return KillRestingOrders();
                }
            }

            // Iterate the scenarios
            foreach (int k in SuccessScenario.Orders.Keys)
            {
                // Iterate the orders
                int successCounter = 0;
                foreach (Order o in SuccessScenario.Orders[k])
                {
                    // Check if conditions are met
                    if (allOrders.Where(x => x.ClOrdId == o.ClOrdId && x.OrdStatus == o.OrdStatus).Count() == 1)
                        successCounter++;  
                    else
                        break;

                    Console.WriteLine($"SuccessScenario[{k}]: ClOrdId={o.ClOrdId}, OrdStatus={o.OrdStatus.ToString()} has [{allOrders.Where(x => x.ClOrdId == o.ClOrdId && x.OrdStatus == o.OrdStatus).Count()}] hits bring total to [{successCounter}]");
                    //Log.Debug($"Scenario[{k}]: ClOrdId={o.ClOrdId}, OrdStatus={o.OrdStatus.ToString()} has [{allOrders.Where(x => x.ClOrdId == o.ClOrdId && x.OrdStatus == o.OrdStatus).Count()}] hits bring total to [{successCounter}]");
                }
                if (allOrders.Count() == successCounter)
                {
                    Direction = SuccessScenario.DirectionByScenario[k];
                    return KillRestingOrders();
                }
            }
            // TODO do not return anything but initiate the actions required
            return false;
        }

        #endregion Evaluators

    }
}
