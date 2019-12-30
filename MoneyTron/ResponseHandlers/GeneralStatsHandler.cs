using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Reflection;
//using Bitmex.Client.Websocket.Responses.Orders;
using Serilog;

namespace MoneyTron.ResponseHandlers
{
    class GeneralStatsHandler
    {
        private readonly DateTime ConnectionStartDateTime;

        public GeneralStatsHandler(DateTime dt)
        {
            ConnectionStartDateTime = dt;
        }

        public DateTime GetConnectionStartDateTime()
        {
            return ConnectionStartDateTime;
        }

        public string GetTimeActive()
        {
            var days = ((int)(DateTime.Now - ConnectionStartDateTime).TotalDays).ToString();
            var hours = ((int)((DateTime.Now - ConnectionStartDateTime).TotalHours % 24)).ToString();
            var mins = ((int)((DateTime.Now - ConnectionStartDateTime).TotalMinutes % 60)).ToString();
            var secs = ((int)((DateTime.Now - ConnectionStartDateTime).TotalSeconds % 60)).ToString();
            return $"{days}d {hours}h {mins}m {secs}s";
        }
    }
}
