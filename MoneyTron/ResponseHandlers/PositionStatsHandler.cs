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

        public void HandleNewPosition(Position newPos, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };

            // If the list does not yet exist in the dictionary, create it.
            if (!_posList.ContainsKey(acc))
                _posList.Add(acc, new List<Position>());

            // If the there is already an entry in the list for the given symbol and account, remove the entry.
            if (_posList[acc].Where(o => o.Symbol == newPos.Symbol).Count() > 0)
                _posList[acc].RemoveAll(r => removeList.Any(a => a == r.Symbol));

            // Add the new position to the list
            _posList[acc].Add(newPos);
        }

        public void HandleUpdatePosition(Position newPos, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };

            if (!_posList.ContainsKey(acc) || _posList[acc] == null || _posList[acc].Where(p => p.Symbol == newPos.Symbol).Count() == 0)
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

        public void HandleDeletePosition(Position newPos, ZoneRecoveryAccount acc)
        {
            List<string> removeList = new List<string>() { newPos.Symbol };
            _posList[acc].RemoveAll(r => removeList.Any(a => a == r.Symbol));
        }
        
        public BindingSource GetBindingSource(ZoneRecoveryAccount acc)
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = _posList[acc];
            return bs;
        }

        private Dictionary<ZoneRecoveryAccount, List<Position>> GetPositionDictionary()
        {
            return _posList;
        }
    }
}
