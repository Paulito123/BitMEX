using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System;
using Bitmex.Client.Websocket.Responses.Positions;
using Serilog;
using PStrategies.ZoneRecovery;

namespace MoneyTron.ResponseHandlers
{
    class PositionStatsHandler
    {
        private readonly Dictionary<ZoneRecoveryAccount, List<Position>> _posList = new Dictionary<ZoneRecoveryAccount, List<Position>>();
        bool DontKeepEmptyPositions;

        public PositionStatsHandler()
        {
            _posList.Add(ZoneRecoveryAccount.A, new List<Position>());
            _posList.Add(ZoneRecoveryAccount.B, new List<Position>());
            DontKeepEmptyPositions = true;
        }

        internal void HandleNewPosition(Position newPos, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };

            // If there is already an entry in the list for the given symbol and account, remove the entry.
            if (_posList[acc].Where(o => o.Symbol == newPos.Symbol).Count() > 0 || (DontKeepEmptyPositions && newPos.CurrentQty == 0))
                _posList[acc].RemoveAll(r => removeList.Any(a => a == r.Symbol));

            // Add the new position to the list
            if (!(DontKeepEmptyPositions && newPos.CurrentQty == 0))
            _posList[acc].Add(newPos);
        }

        internal void HandleUpdatePosition(Position newPos, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };

            if (_posList[acc].Where(p => p.Symbol == newPos.Symbol).Count() == 0 || (DontKeepEmptyPositions && newPos.CurrentQty == 0))
                HandleNewPosition(newPos, acc);
            
            else
            {
                Position oldPos = _posList[acc].Where(p => p.Symbol == newPos.Symbol).First();
                
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

        internal void HandleDeletePosition(Position newPos, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };
            _posList[acc].RemoveAll(r => removeList.Any(a => a == r.Symbol));
        }

        internal BindingSource GetBindingSource(ZoneRecoveryAccount acc)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = _posList[acc];
            return bs;
        }

        internal Dictionary<ZoneRecoveryAccount, List<Position>> GetPositionDictionary()
        {
            return _posList;
        }
    }
}
