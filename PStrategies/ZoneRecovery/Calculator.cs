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

        internal static readonly int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        internal string Symbol
        {
            get => Symbol;
            set => Symbol = value;
        }
        internal int MaxDepthIndex
        {
            get => MaxDepthIndex;
            set => MaxDepthIndex = value;
        }
        internal decimal ZoneSize
        {
            get => ZoneSize;
            set => ZoneSize = value;
        }
        internal decimal MaxExposurePerc
        {
            get => MaxExposurePerc;
            set => MaxExposurePerc = value;
        }
        internal decimal Leverage
        {
            get => Leverage;
            set => Leverage = value;
        }
        internal decimal MinimumProfitPercentage
        {
            get => MinimumProfitPercentage;
            set => MinimumProfitPercentage = value;
        }
        internal long RunningBatchNr
        {
            get => RunningBatchNr;
            set => RunningBatchNr = value;
        }
        internal long UnitSize { get; set; }
        internal decimal WalletBalance { get; set; }
        internal decimal Ask { get; set; }
        internal decimal Bid { get; set; }

        internal ZoneRecoveryState State;

        private IBitmexApiService bitmexApiServiceA { get; set; }
        private IBitmexApiService bitmexApiServiceB { get; set; }
        
        internal static Mutex PositionMutex;

        internal readonly SortedDictionary<long, ZoneRecoveryBatch> ZRBatchLedger;
        internal Dictionary<ZoneRecoveryAccount, List<Order>> Orders;
        internal Dictionary<ZoneRecoveryAccount, List<Position>> Positions;

        #endregion Core variables

        public Calculator()
        {
            ZRBatchLedger = new SortedDictionary<long, ZoneRecoveryBatch>();
            State = new ZRSInitiating(this);
        }

        public void UpdatePrices(Dictionary<string, decimal> dict)
        {
            if (dict.ContainsKey("Ask") && dict["Ask"] > 0)
                Ask = dict["Ask"];
            if (dict.ContainsKey("Bid") && dict["Bid"] > 0)
                Ask = dict["Bid"];
        }

        public void Initialize(IBitmexApiService apiSA, IBitmexApiService apiSB,
            decimal walletBalance,
            Dictionary<ZoneRecoveryAccount, List<Order>> ordersContainer,
            Dictionary<ZoneRecoveryAccount, List<Position>> positionContainer, Mutex positionMutex,
            string symbol = "XBTUSD", 
            int maxDepthIndex = 1, decimal zoneSize = 20, decimal maxExposurePerc = (decimal)0.01, decimal leverage = 1, decimal minPrftPerc = (decimal)0.01)
        {
            // RunningBatchNr should never be reset during the lifetime of this ZoneRecoveryComputer instance...
            RunningBatchNr = 0;

            try
            {
                if (apiSA == null || apiSB == null)
                    throw new Exception("bitmexApiService cannot be null");

                bitmexApiServiceA = apiSA;
                bitmexApiServiceB = apiSB;

                if (ordersContainer != null)
                    Orders = ordersContainer;
                else
                    throw new Exception("No order container defined in parent");

                if (positionContainer != null)
                    Positions = positionContainer;
                else
                    throw new Exception("No position container defined in parent");

                if (positionMutex != null)
                    PositionMutex = positionMutex;
                else
                    throw new Exception("Empty reference to PositionMutex");

                if (symbol == "XBTUSD")
                    Symbol = symbol;
                else
                    throw new Exception("Symbol has a value that is not permitted");

                if (maxExposurePerc > 0 && maxExposurePerc <= 1)
                    MaxExposurePerc = maxExposurePerc;
                else
                    throw new Exception("MaxExposurePerc not within permitted bounds");

                if (leverage > 0 && leverage <= 100)
                    Leverage = leverage;
                else
                    throw new Exception("Leverage not within permitted bounds");

                if (maxDepthIndex > 0 && maxDepthIndex <= FactorArray.Count())
                    MaxDepthIndex = maxDepthIndex;
                else
                    throw new Exception("MaxDepthIndex not within permitted bounds");

                if (zoneSize >= 2 && zoneSize < 1000)
                    ZoneSize = zoneSize;
                else
                    throw new Exception("ZoneSize not within permitted bounds");

                if (minPrftPerc > 0 && minPrftPerc <= 1)
                    MinimumProfitPercentage = minPrftPerc;
                else
                    throw new Exception("MinimumProfitPercentage not within permitted bounds");

                if (walletBalance > 0)
                    WalletBalance = walletBalance;
                else
                    throw new Exception("WalletBalance does not have a permitted value");

            }
            catch (Exception exc)
            {
                string message = $"Initialize: {exc.Message}";
                Log.Error(message);
                Console.WriteLine(message);
            }
        }
        
        public void Evaluate(ZoneRecoveryAccount acc, List<string> clOrdIdList)
        {
            //Console.WriteLine($"Calculator.Evaluate for Account {acc}");

            // Push the latest updates to the Batch
            var orderList = Orders[acc].Where(o => clOrdIdList.Any(s => s == o.OrderId)).ToList();
            ZRBatchLedger[RunningBatchNr].CheckStatusses(acc, orderList);

            // Evaluate the new state
            State.Evaluate();
        }

        internal bool StartNewZRSession()
        {
            if (ZRBatchLedger[RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.Closed)
            {
                // Create a new batch
                var zrob = new ZoneRecoveryBatch(ZoneRecoveryBatchType.PeggedStart, ZoneRecoveryBatchStatus.Working);
                ZRBatchLedger.Add(zrob.BatchNumber, zrob);
                ZoneRecoveryBatchOrder zro;

                // Create unique ids
                var idA = CreateNewClOrdID();
                var idB = CreateNewClOrdID();
                
                // Create initial Orders
                var OrderParamsA = OrderPOSTRequestParams.CreateSimpleLimit(Symbol, idA, UnitSize, (Bid - 1), OrderSide.Buy);
                zro = new ZoneRecoveryBatchOrder(ZoneRecoveryAccount.A, OrderParamsA);
                ZRBatchLedger[RunningBatchNr].AddOrder(zro);
                bitmexApiServiceA
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA)
                    .ContinueWith(ZRBatchLedger[RunningBatchNr].HandleOrderResponse, TaskContinuationOptions.AttachedToParent);

                var OrderParamsB = OrderPOSTRequestParams.CreateSimpleLimit(Symbol, idB, UnitSize, (Ask + 1), OrderSide.Sell);
                zro = new ZoneRecoveryBatchOrder(ZoneRecoveryAccount.B, OrderParamsB);
                ZRBatchLedger[RunningBatchNr].AddOrder(zro);
                bitmexApiServiceB
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsB)
                    .ContinueWith(ZRBatchLedger[RunningBatchNr].HandleOrderResponse, TaskContinuationOptions.AttachedToParent);

                return true;
            }

            return false;
        }

        internal void CreateOrders()
        {

        }

        internal void CreateOrder(ZoneRecoveryAccount acc, ZoneRecoveryOrderType typ, decimal price, decimal qty)
        {

        }

        private string CreateNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private decimal CalculateMaximumDepth()
        {
            decimal sum = 0;

            for (int i = 0; i < MaxDepthIndex; i++)
            {
                sum = sum + FactorArray[i];
            }
            return sum;
        }

        private void CalculateUnitSize(decimal refPrice)
        {
            try
            {
                decimal totalDepthMaxExposure = CalculateMaximumDepth();

                if (refPrice > 0 && totalDepthMaxExposure > 0 && totalDepthMaxExposure < FactorArray.Sum())
                {
                    UnitSize = (long)Math.Round(refPrice * Leverage * WalletBalance * MaxExposurePerc / totalDepthMaxExposure, MidpointRounding.ToEven);
                }
                else
                    throw new Exception("Cannot determine UnitSize");
            }
            catch (Exception exc)
            {
                string message = $"SetUnitSizeForPrice: {exc.Message}";
                Log.Error(message);
                Console.WriteLine(message);
            }
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
