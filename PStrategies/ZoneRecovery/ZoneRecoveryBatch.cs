using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Dto;
using BitMEXRest.Client;

using Serilog;

namespace PStrategies.ZoneRecovery
{
    class ZoneRecoveryBatch
    {
        #region Core variables

        internal long BatchNumber;
        internal ZoneRecoveryBatchType BatchType;
        internal ZoneRecoveryBatchStatus BatchStatus;
        private readonly List<ZoneRecoveryBatchOrder> ZROrdersList;

        #endregion Core variables

        #region Constructors

        public ZoneRecoveryBatch(ZoneRecoveryBatchType batchType, ZoneRecoveryBatchStatus stat)
        {
            BatchNumber = CreateBatchNr();
            ZROrdersList = new List<ZoneRecoveryBatchOrder>();
            BatchStatus = stat;
            BatchType = batchType;
        }

        #endregion Constructors

        #region Setters, getters and spetters

        internal void HandleOrderResponse(Task<BitmexApiResult<OrderDto>> task)
        {
            string message = "";
            
            if (task.Exception != null)
            {
                message = $"HandleOrderResponse: {(task.Exception.InnerException ?? task.Exception).Message}";
                Log.Error(message);
            }
            else
            {
                if (ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.ClOrdId).Count() == 1)
                {
                    ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.ClOrdId).Single().SetLastStatus(task.Result.Result);
                    message = $"HandleOrderResponse: order [{task.Result.Result.ClOrdId}] returned status [{task.Result.Result.OrdStatus}]";
                    Log.Information(message);
                }
                else
                {
                    message = $"HandleOrderResponse(impossible): order [{task.Result.Result.ClOrdId}] could not be found in the list, or has multiple instances";
                    Log.Error(message);
                }
            }
            Console.WriteLine(message);
        }

        public void AddOrder(ZoneRecoveryBatchOrder zrbo)
        {
            if (zrbo != null)
                ZROrdersList.Add(zrbo);
        }

        #endregion Setters, getters and spetters

        #region Helpers

        private static long CreateBatchNr()
        {
            return DateTime.Now.Ticks;
        }

        #endregion Helpers

        //// Add the scenarios that need to be checked with every update
        //List<Order> tmpList;
        //tmpList = new List<Order>();
        //tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.Filled });
        //tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.New });
        //ZRBatchLedger[RunningBatchNr].SuccessScenario.AddOrderList(tmpList, ZoneRecoveryDirection.Up);
        //tmpList = new List<Order>();
        //tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.New });
        //tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.Filled });
        //ZRBatchLedger[RunningBatchNr].SuccessScenario.AddOrderList(tmpList, ZoneRecoveryDirection.Down);

        //tmpList = new List<Order>();
        //tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.Filled });
        //tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.Filled });
        //ZRBatchLedger[RunningBatchNr].FailureScenario.AddOrderList(tmpList);
        //tmpList = new List<Order>();
        //tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.Canceled });
        //ZRBatchLedger[RunningBatchNr].FailureScenario.AddOrderList(tmpList, ZoneRecoveryDirection.Undefined);
        //tmpList = new List<Order>();
        //tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.Canceled });
        //ZRBatchLedger[RunningBatchNr].SuccessScenario.AddOrderList(tmpList, ZoneRecoveryDirection.Undefined);

    }
}

