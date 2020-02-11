using System.Collections.Generic;
using System.Linq;
using Bitmex.Client.Websocket.Responses.Orders;
using BitMEXRest.Model;
using BitMEXRest.Dto;
using BitMEXRest.Client;
using Serilog;

namespace PStrategies.ZoneRecovery
{
    class ZoneRecoveryBatchOrder
    {

        #region Core variables and containers
        
        internal ZoneRecoveryAccount Account { get; }
        internal OrderPOSTRequestParams PostParams { get; }
        //internal OrderDto ServerResponse
        //{
        //    get => ServerResponse;
        //    set => SetServerResponse(value);
        //}
        internal ZoneRecoveryOrderStatus CurrentStatus { get; set; }

        #endregion Core variables and containers

        #region Constructors

        public ZoneRecoveryBatchOrder(ZoneRecoveryAccount acc, OrderPOSTRequestParams postParams)
        {
            PostParams = postParams;
            Account = acc;
            CurrentStatus = ZoneRecoveryOrderStatus.Undefined;
        }

        #endregion Constructors

        #region Handle statusses
        
        internal OrderDto SetLastStatus(OrderDto o)
        {
            CurrentStatus = GetOrderStatus(o);
            return o;
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