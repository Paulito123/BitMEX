using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Reflection;
using Bitmex.Client.Websocket.Responses.Orders;
using Serilog;
using PStrategies.ZoneRecovery;

namespace MoneyTron.ResponseHandlers
{
    class OrdersStatsHandler
    {
        internal readonly Dictionary<ZoneRecoveryAccount, List<Order>> _orderList = new Dictionary<ZoneRecoveryAccount, List<Order>>();

        public OrdersStatsHandler ()
        {
            _orderList.Add(ZoneRecoveryAccount.A, new List<Order>());
            _orderList.Add(ZoneRecoveryAccount.B, new List<Order>());
        }

        internal void HandleNewOrder(Order newOrder, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newOrder.OrderId };
            
            // If the there is already an entry, remove the entry.
            if (_orderList[acc].Where(o => o.OrderId == newOrder.OrderId).Count() > 0)
                _orderList[acc].RemoveAll(r => removeList.Any(a => a == r.OrderId));

            // Add the new entry
            _orderList[acc].Add(newOrder);
        }

        internal bool HandleUpdateOrder(Order newOrder, ZoneRecoveryAccount acc)
        {
            bool statusChanged = false;
            List<string> removeList = new List<string>() { newOrder.OrderId };

            if (!_orderList.ContainsKey(acc) || _orderList[acc] == null || _orderList[acc].Where(p => p.OrderId == newOrder.OrderId).Count() == 0)
            { 
                HandleNewOrder(newOrder, acc);
                statusChanged = true;
            }
            else
            {
                Order oldOrd = _orderList[acc].Where(p => p.OrderId == newOrder.OrderId).First();
                
                // Update the existing order with the new property values.
                var posType = typeof(Order);
                var properties = posType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanRead && prop.CanWrite);

                foreach (var property in properties)
                {
                    if (property.Name != "OrderId" && property.Name != "ClOrdID" &&
                        property.Name != "Account" && property.Name != "Symbol" &&
                        property.Name.IndexOf("Simple") == -1)
                    {
                        var value = property.GetValue(newOrder, null);

                        if (value != null)
                        {
                            if (property.Name == "OrdStatus")
                            {
                                if (value is OrderStatus && (OrderStatus)value != OrderStatus.Undefined)
                                {
                                    statusChanged = true;
                                    property.SetValue(oldOrd, value, null);
                                }
                            }
                            else
                            {
                                property.SetValue(oldOrd, value, null);
                            }
                        }
                    }
                }
                
            }
            return statusChanged;
        }

        internal void HandleDeleteOrder(Order newOrder, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newOrder.OrderId };
            _orderList[acc].RemoveAll(r => removeList.Any(a => a == r.OrderId));
        }

        internal BindingSource GetBindingSource(ZoneRecoveryAccount acc)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = _orderList[acc];
            return bs;
        }

        internal Dictionary<ZoneRecoveryAccount, List<Order>> GetOrderDictionary()
        {
            return _orderList;
        }
    }
}