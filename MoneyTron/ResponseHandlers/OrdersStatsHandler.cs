using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Reflection;
using Bitmex.Client.Websocket.Responses.Orders;
using Serilog;

namespace MoneyTron.ResponseHandlers
{
    class OrdersStatsHandler
    {
        private readonly List<Order> _orderList = new List<Order>();

        public void HandleOrder(Order newOrder)
        {
            List<string> removeList = new List<string>() { newOrder.OrderId };

            switch (newOrder.OrdStatus)
            {
                case OrderStatus.Undefined:
                //case OrderStatus.Filled:
                case OrderStatus.Canceled:
                    _orderList.RemoveAll(r => removeList.Any(a => a == r.OrderId));
                    break;
                case OrderStatus.New:
                case OrderStatus.Filled:
                case OrderStatus.PartiallyFilled:
                    if (_orderList.Where(o => o.OrderId == newOrder.OrderId).Count() == 0)
                        _orderList.Add(newOrder);
                    else
                    {
                        _orderList.RemoveAll(r => removeList.Any(a => a == r.OrderId));
                        _orderList.Add(newOrder);
                    }
                    break;
            }
        }

        public void HandleNewOrder(Order newOrder)
        {
            if (_orderList.Where(o => o.OrderId == newOrder.OrderId).Count() == 0)
                _orderList.Add(newOrder);
            else if (_orderList.Where(o => o.OrderId == newOrder.OrderId).Count() > 0)
            {
                Log.Error($"HandleNewOrder: multipe Orders in list for single OrderId [{newOrder.OrderId}].");
                List<string> removeList = new List<string>() { newOrder.OrderId };
                _orderList.RemoveAll(r => removeList.Any(a => a == r.OrderId));
                _orderList.Add(newOrder);
            }
        }

        public void HandleUpdateOrder(Order newOrder)
        {
            List<string> removeList = new List<string>() { newOrder.OrderId };

            if (_orderList.Where(p => p.OrderId == newOrder.OrderId).Count() == 0)
                _orderList.Add(newOrder);
            else if (_orderList.Where(p => p.OrderId == newOrder.OrderId).Count() > 1)
            {
                Log.Error($"Multipe Orders in list for single OrderId.");
                _orderList.RemoveAll(r => removeList.Any(a => a == r.OrderId));
                throw new Exception("Multipe Orders in list for single OrderId.");
            }
            else
            {
                Order oldOrd = _orderList.Where(p => p.OrderId == newOrder.OrderId).First();

                if (newOrder.OrdStatus == OrderStatus.Canceled || /*newOrder.OrdStatus == OrderStatus.Filled || */newOrder.OrdStatus == OrderStatus.Rejected)
                    _orderList.RemoveAll(r => removeList.Any(a => a == r.OrderId));

                else
                {
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
            }
        }
        
        public void HandleDeleteOrder(Order newOrder)
        {
            List<string> removeList = new List<string>() { newOrder.OrderId };
            _orderList.RemoveAll(r => removeList.Any(a => a == r.OrderId));
        }

        public BindingSource GetBindingSource()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = _orderList;
            return bs;
        }

        public List<Order> Clone()
        {
            return _orderList.ToList();
        }
    }
}