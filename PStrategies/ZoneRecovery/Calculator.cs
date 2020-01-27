using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using PStrategies.ZoneRecovery.State;

using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

using Serilog;

namespace PStrategies.ZoneRecovery
{
    
    public class Calculator
    {
        #region Core variables

        private static int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        private string Symbol;
        private int MaxDepthIndex;
        private decimal ZoneSize;
        private decimal MaxExposurePerc;
        private decimal Leverage;
        private decimal MinimumProfitPercentage;

        private long RunningBatchNr;
        private ZoneRecoveryState State;

        private IBitmexApiService bitmexApiServiceA { get; set; }
        private IBitmexApiService bitmexApiServiceB { get; set; }
        
        private readonly SortedDictionary<long, ZoneRecoveryOrderBatch> ZROrderLedger;
        private Dictionary<ZoneRecoveryAccount, List<Order>> Orders;
        private Dictionary<ZoneRecoveryAccount, List<Position>> Positions;

        #endregion Core variables

        public Calculator(IBitmexApiService apiSA, IBitmexApiService apiSB)
        {
            ZROrderLedger = new SortedDictionary<long, ZoneRecoveryOrderBatch>();

            if (apiSA == null || apiSB == null)
                throw new Exception("bitmexApiService cannot be null");

            bitmexApiServiceA = apiSA;
            bitmexApiServiceB = apiSB;
        }

        public void Initialize(
            string symbol = "XBTUSD", int maxDepthIndex = 1, decimal zoneSize = 20, decimal maxExposurePerc = (decimal)0.01, decimal leverage = 1, 
            decimal minPrftPerc = (decimal)0.01)
        {
            Symbol = symbol;
            MaxDepthIndex = maxDepthIndex;
            ZoneSize = zoneSize;
            MaxExposurePerc = maxExposurePerc;
            Leverage = leverage;
            MinimumProfitPercentage  = minPrftPerc;

        }

        private string CreateNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static long CreateBatchNr(DateTime dt)
        {
            return dt.Ticks;
        }
    }
}

/*

Hey I'm coming from Bitfinex where we can manually set OCO orders. I'm trying to find the equivalent on Bitmex so when I open a position I can set my targets and walk away.
For Long:
Buy 1 XBT at 10,000
Set a limit sell order at 10,100 and check off Reduce Only
Set a stop market at 9900 and check off Close On Trigger
If it hits 10,100 then you will take your ~1% profit and position will close. The stop market order will still be open, but will not execute even if it hits 9900 because "Close On Trigger" prevents it from doing so
If it hits 9900, then you wlll realize your ~1% loss and position will close. The limit sell order will still be open, but will not execute even if it hits 1100 because "Reduce Only" prevents it from opening a new position
For Short, do the opposite.
Is that the right way to do it?

        /// <summary>
        /// SetNewPosition should be called when a new position is taken on the exchange. The Zone Recovery
        /// strategy proceeds one step in its logic. The List of OrderResponses passed should be the 
        /// OrderResponses returned for one specific order.
        /// Example:
        ///     MaxDepthIndex = 4
        ///     CurrentZRPosition >  0   1   2   3   4   3   2   1   0   X
        ///     Position L/S      >  -   L   S   L   S   L4  S3  L2  S1  X
        ///     (Un)Winding       >  I   W   W   W   U   U   U   U   U   X
        /// 
        /// When the TP is reached at the exchange, a new position should not be set. The current Calculator 
        /// instance should be disposed.
        /// 
        /// Turning the wheel, advance one step further in the ZR winding process...
        /// </summary>
   
orderResp.OrdStatus.Equals("New")
 */
