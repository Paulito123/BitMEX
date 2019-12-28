using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Reflection;
using System;
using Bitmex.Client.Websocket.Responses.Positions;
using Serilog;

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
            List<string> removeList = new List<string>() { newPos.Symbol };

            if (_posList.Where(p => p.Symbol == newPos.Symbol).Count() == 0)
                HandleNewPosition(newPos);
            else if (_posList.Where(p => p.Symbol == newPos.Symbol).Count() > 1)
            {
                _posList.RemoveAll(r => removeList.Any(a => a == r.Symbol));
                Log.Error($"Multipe Positions in list for single Symbol.");
                throw new Exception("Multipe Positions in list for single Symbol.");
            }
            else
            {
                Position oldPos = _posList.Where(p => p.Symbol == newPos.Symbol).First();

                if (newPos.CurrentQty == 0)
                    _posList.RemoveAll(r => removeList.Any(a => a == r.Symbol));

                else
                {
                    var posType = typeof(Position);
                    var properties = posType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.CanRead && prop.CanWrite);

                    foreach (var property in properties)
                    {
                        if (property.Name != "Account" && property.Name != "Symbol" &&
                            property.Name != "Currency" && property.Name.IndexOf("Simple") == -1)
                        {
                            var value = property.GetValue(newPos, null);
                            if (value != null)
                            {
                                property.SetValue(oldPos, value, null);
                            }
                        }
                    }
                }
            }
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
