using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Dto;
using BitMEXRest.Client;

using Serilog;

// TODO: 
// Implement check procedures in the batch that can assess whether all orders within a setup have been filled (static by Batch type)

namespace PStrategies.ZoneRecovery
{
    class ZoneRecoveryBatch
    {
        #region Core variables

        internal long BatchNumber;
        internal ZoneRecoveryBatchType BatchType;
        internal ZoneRecoveryBatchStatus BatchStatus;
        internal ZoneRecoveryDirection Direction;
        private readonly List<ZoneRecoveryBatchOrder> ZROrdersList;
        private int ResponsesReceived;
        private static int MaxWorkingDelayAllowedInSec = 10;

        #endregion Core variables

        #region Constructors

        public ZoneRecoveryBatch(ZoneRecoveryBatchType batchType, ZoneRecoveryBatchStatus stat)
        {
            BatchNumber = CreateBatchNr();
            ZROrdersList = new List<ZoneRecoveryBatchOrder>();
            BatchStatus = stat;
            BatchType = batchType;
            ResponsesReceived = 0;
            Direction = ZoneRecoveryDirection.Undefined;
        }

        #endregion Constructors

        #region Vanalles
        
        internal void CheckStatusses(ZoneRecoveryAccount acc, List<Order> orderList)
        {
            foreach(Order o in orderList)
            {
                ZROrdersList.Where(x => x.PostParams.ClOrdID == o.ClOrdId).Single().SetLastStatus(o);
            }
            CheckBatchStatus();
        }

        internal void HandleOrderResponse(Task<BitmexApiResult<OrderDto>> task)
        {
            ResponsesReceived++; 
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
            CheckBatchStatus();
        }

        private void CheckBatchStatus()
        {
            switch (BatchType)
            {
                case ZoneRecoveryBatchType.PeggedStart:
                    if (ResponsesReceived < 2)
                    {
                        if (ZROrdersList.Max(x => x.LastUpdateReceived) < DateTime.Now.AddSeconds(-MaxWorkingDelayAllowedInSec))
                            BatchStatus = ZoneRecoveryBatchStatus.Error;
                        
                        else
                            BatchStatus = ZoneRecoveryBatchStatus.Working;
                    }
                    else if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.New).Count() == 2)
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.Waiting;
                    }
                    else if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.Filled).Count() == 1)
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.ReadyForNext;
                        if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.Filled).Single().Account == ZoneRecoveryAccount.A)
                            Direction = ZoneRecoveryDirection.Up;
                        else
                            Direction = ZoneRecoveryDirection.Down;                            
                    }
                    else
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.Error;
                        throw new Exception("ZoneRecoveryBatch.CheckBatchStatus: ZoneRecoveryBatchStatus.Error[1]");
                    }
                    break;
                case ZoneRecoveryBatchType.WindingFirst:
                case ZoneRecoveryBatchType.UnwindingLast:
                    if (ResponsesReceived < 2)
                    {
                        if (ZROrdersList.Max(x => x.LastUpdateReceived) < DateTime.Now.AddSeconds(-MaxWorkingDelayAllowedInSec))
                            BatchStatus = ZoneRecoveryBatchStatus.Error;

                        else
                            BatchStatus = ZoneRecoveryBatchStatus.Working;
                    }
                    else if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.New).Count() == 2)
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.Waiting;
                    }
                    else if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.Filled).Count() == 1)
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.ReadyForNext;
                    }
                    else
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.Error;
                        throw new Exception("ZoneRecoveryBatch.CheckBatchStatus: ZoneRecoveryBatchStatus.Error[2]");
                    }
                    break;
                case ZoneRecoveryBatchType.WindingUp:
                case ZoneRecoveryBatchType.WindingDown:
                case ZoneRecoveryBatchType.UnwindingUp:
                case ZoneRecoveryBatchType.UnwindingDown:
                    if (ResponsesReceived < 3)
                    {
                        if (ZROrdersList.Max(x => x.LastUpdateReceived) < DateTime.Now.AddSeconds(-MaxWorkingDelayAllowedInSec))
                            BatchStatus = ZoneRecoveryBatchStatus.Error;

                        else
                            BatchStatus = ZoneRecoveryBatchStatus.Working;
                    }
                    else if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.New).Count() == 3)
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.Waiting;
                    }
                    else if (ZROrdersList.Where(x => x.CurrentStatus == ZoneRecoveryOrderStatus.Filled).Count() == 1)
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.ReadyForNext;
                    }
                    else
                    {
                        BatchStatus = ZoneRecoveryBatchStatus.Error;
                        throw new Exception("ZoneRecoveryBatch.CheckBatchStatus: ZoneRecoveryBatchStatus.Error[2]");
                    }
                    break;
                default:
                    BatchStatus = ZoneRecoveryBatchStatus.Error;
                    break;
            }
            var message = $"CheckBatchStatus: BatchStatus [{BatchStatus.ToString()}]";
            Console.WriteLine(message);
            Log.Verbose(message);
        }

        internal void AddOrder(ZoneRecoveryBatchOrder zrbo)
        {
            if (zrbo != null)
                ZROrdersList.Add(zrbo);
        }
        
        #endregion Vanalles

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

