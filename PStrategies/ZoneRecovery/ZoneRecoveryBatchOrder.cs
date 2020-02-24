using System;
using System.Collections.Generic;
using System.Linq;
using Bitmex.Client.Websocket.Responses.Orders;
using BitMEXRest.Model;
using BitMEXRest.Dto;
using BitMEXRest.Client;
using Serilog;

namespace PStrategies.ZoneRecovery
{
    public class ZoneRecoveryBatchOrder
    {

        #region Core variables and containers

        // internal...
        public ZoneRecoveryAccount Account { get; }
        public OrderPOSTRequestParams PostParams { get; set; }
        public ZoneRecoveryOrderStatus CurrentStatus { get; set; }
        public ZoneRecoveryOrderType OrderType { get; set; }
        internal DateTimeOffset? LastUpdateReceived { get; set; }

        #endregion Core variables and containers

        #region Constructors

        public ZoneRecoveryBatchOrder(ZoneRecoveryAccount acc, OrderPOSTRequestParams postParams, ZoneRecoveryOrderType orderType)
        {
            PostParams = postParams;
            Account = acc;
            CurrentStatus = ZoneRecoveryOrderStatus.Undefined;
            OrderType = orderType;
        }

        #endregion Constructors

        #region Handle statusses

        internal void UpdatePostParams (OrderPOSTRequestParams pp)
        {
            PostParams = pp;
            LastUpdateReceived = null;
        }

        internal void SetLastStatus(OrderDto o)
        {
            LastUpdateReceived = o.Timestamp;
            CurrentStatus = GetOrderStatus(o);
        }

        internal void SetLastStatus(Order o)
        {
            LastUpdateReceived = o.Timestamp;
            CurrentStatus = GetOrderStatus(o);
        }

        private ZoneRecoveryOrderStatus GetOrderStatus(object o)
        {
            if (o != null && o is Order)
            {
                switch (((Order)o).OrdStatus)
                {
                    case OrderStatus.Canceled:
                        return ZoneRecoveryOrderStatus.Canceled;
                    case OrderStatus.Filled:
                        return ZoneRecoveryOrderStatus.Filled;
                    case OrderStatus.New:
                        return ZoneRecoveryOrderStatus.New;
                    case OrderStatus.PartiallyFilled:
                        return ZoneRecoveryOrderStatus.PartiallyFilled;
                    case OrderStatus.Rejected:
                        return ZoneRecoveryOrderStatus.Rejected;
                }
            }
            else if (o != null && o is OrderDto)
            {
                switch (((OrderDto)o).OrdStatus)
                {
                    case "Canceled":
                        return ZoneRecoveryOrderStatus.Canceled;
                    case "Filled":
                        return ZoneRecoveryOrderStatus.Filled;
                    case "New":
                    case "New,Triggered":
                        return ZoneRecoveryOrderStatus.New;
                    case "Partially filled":
                        return ZoneRecoveryOrderStatus.PartiallyFilled;
                    case "Rejected":
                        return ZoneRecoveryOrderStatus.Rejected;
                }
            }
            else if (o == null)
            {
                return ZoneRecoveryOrderStatus.Error;
            }

            return ZoneRecoveryOrderStatus.Undefined;
        }

        #endregion Handle statusses

    }
}