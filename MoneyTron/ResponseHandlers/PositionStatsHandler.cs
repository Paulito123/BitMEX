using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System;
using Bitmex.Client.Websocket.Responses.Positions;

namespace MoneyTron.ResponseHandlers
{
    class PositionStatsHandler
    {
        private readonly List<Position> _posList = new List<Position>();

        public void HandleNewPosition(Position newPos)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };

            if (newPos.CurrentQty == 0 && _posList.Where(o => o.Symbol == newPos.Symbol).Count() == 0)
                return;
            else if (newPos.CurrentQty == 0 && _posList.Where(o => o.Symbol == newPos.Symbol).Count() > 0)
                _posList.RemoveAll(r => removeList.Any(a => a == r.Symbol));
            else if (_posList.Where(o => o.Symbol == newPos.Symbol).Count() == 0)
                _posList.Add(newPos);
            else
            {
                _posList.RemoveAll(r => removeList.Any(a => a == r.Symbol));
                _posList.Add(newPos);
            }
        }

        public void HandleUpdatePosition(Position newPos)
        {
            Position oldPos = _posList.FirstOrDefault(x => x.Symbol == newPos.Symbol);

            if (oldPos != null && newPos != null)
            {
                var posType = typeof(Position);
                var properties = posType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanRead && prop.CanWrite);

                foreach (var property in properties)
                {
                    if (property.Name != "Account" && property.Name != "Symbol" && 
                        property.Name != "Currency" && property.Name != "Underlying" && 
                        property.Name != "QuoteCurrency" && property.Name.IndexOf("Simple") == -1)
                    {
                        var value = property.GetValue(newPos, null);
                        if (value != null)
                        {
                            if (value is double? && (double)value != 0)
                            {
                                property.SetValue(oldPos, value, null);
                            }
                            else if (value is long? && (long)value != 0)
                            {
                                property.SetValue(oldPos, value, null);
                            }
                            else if (value is string && value.ToString() != string.Empty)
                            {
                                property.SetValue(oldPos, value, null);
                            }
                        }
                    }
                }
            }
            else
                return;
        }

        public void HandleDeletePosition(Position newPos)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };
            _posList.RemoveAll(r => removeList.Any(a => a == r.Symbol));
        }
        
        public BindingSource GetBindingSource()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = _posList;
            return bs;
        }
    }
}
