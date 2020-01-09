using System.Collections.Generic;
using System.Linq;
using System;
using Bitmex.Client.Websocket.Responses.Orders;
using BitMEXRest.Dto;

namespace PStrategies.ZoneRecovery
{
    class ZoneRecoveryOrderBatch
    {
        private long BatchNumber { get; }
        private List<ZoneRecoveryOrder> OrdersList;
        private int NrOfOrdersExpected { get; }
        public ZoneRecoveryOrderBatchStatus CurrentZROBStatus { get; set; }

        public ZoneRecoveryOrderBatch(long batchNumber, int nrOfOrdersExpected)
        {
            BatchNumber = batchNumber;
            OrdersList = new List<ZoneRecoveryOrder>();
            NrOfOrdersExpected = nrOfOrdersExpected;
            CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.Init;
        }

        public void AddOrder(ZoneRecoveryOrder ordr)
        {
            if (OrdersList.Count() < NrOfOrdersExpected)
                OrdersList.Add(ordr);
            else
                throw new Exception("ZoneRecoveryOrderBatch: Order is trying to be added while container reached its max capacity");
        }

        /// <summary>
        /// Attach the order response to the initial order.
        /// </summary>
        /// <param name="ordr"></param>
        public void SetLastResponse(OrderDto ordr)
        {
            OrdersList.Where(x => x.PostParams.ClOrdID == ordr.ClOrdId).Single().LastResponse = ordr;
        }

        public void EvaluateStatus()
        {
            int newCounter = OrdersList.Where(x => x.LastResponse.OrdStatus == "New,Triggered" || x.LastResponse.OrdStatus == "New").Count();
            int filledCounter = OrdersList.Where(x => x.LastResponse.OrdStatus == "Filled").Count();
            int canceledCounter = OrdersList.Where(x => x.LastResponse.OrdStatus == "Canceled").Count();
            int nullCounter = OrdersList.Where(x => x.LastResponse == null).Count();

            if (nullCounter == NrOfOrdersExpected)
            {
                CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.Init;
            }
            else if (newCounter == NrOfOrdersExpected)
            {
                CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.Resting;
            }
            else if (canceledCounter == 0 && filledCounter == 0 && newCounter > 0 && nullCounter > 0)
            {
                CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.PartialResting;
            }
            else if (canceledCounter == 0 && filledCounter > 0)
            {
                CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.OrderFilled;
            }
            else
                CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.Alert;
        }
    }
}
