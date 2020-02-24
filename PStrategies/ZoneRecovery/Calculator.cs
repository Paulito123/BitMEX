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

        public bool SwitchedOn
        {
            get => switchedOn;
            set => SwitchMeOn(value);
        }
        private bool switchedOn;

        internal static readonly int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        private static decimal PegDistance = 5;

        internal string Symbol;
        internal int MaxDepthIndex;
        internal decimal ZoneSize;
        internal decimal MaxExposurePerc;
        internal decimal Leverage;
        internal decimal MinimumProfitPercentage;
        public long RunningBatchNr;
        public long UnitSize;
        internal decimal WalletBalance;
        public decimal Ask;
        public decimal Bid;

        public IZoneRecoveryState State;

        private Dictionary<ZoneRecoveryAccount, IBitmexApiService> ApiServiceByAccount;
        
        internal static Mutex PositionMutex;

        public readonly SortedDictionary<long, ZoneRecoveryBatch> ZRBatchLedger;
        internal Dictionary<ZoneRecoveryAccount, List<Order>> Orders;
        internal Dictionary<ZoneRecoveryAccount, List<Position>> Positions;
        internal Dictionary<ZoneRecoveryAccount, int> RateLimitsRemaining;
        internal int RateLimitTimeOutInMSec = 2000;

        #endregion Core variables

        private void SwitchMeOn(bool s)
        {
            if (s)
            {
                // things to do before switching on this shit
            }
            else
            {
                // things to do before switching off this shit
            }
            switchedOn = s;
        }

        internal void RemoveOrderForAccount(ZoneRecoveryAccount acc, string clOrdId)
        {
            if (RateLimitsRemaining[acc] >= 5)
            {
                var OrderParams = new OrderDELETERequestParams() { ClOrdID = clOrdId };
                var result = ApiServiceByAccount[acc]
                    .Execute(BitmexApiUrls.Order.DeleteOrder, OrderParams)
                    .ContinueWith(HandleDeleteOrderResponse, TaskContinuationOptions.AttachedToParent);
            }
            else
            {
                RemoveOrderForAccount(acc, clOrdId);
            }
        }

        public Calculator()
        {
            ZRBatchLedger = new SortedDictionary<long, ZoneRecoveryBatch>();
            RateLimitsRemaining = new Dictionary<ZoneRecoveryAccount, int>();
            UpdateRateLimitsRemaining(ZoneRecoveryAccount.A, 60);
            UpdateRateLimitsRemaining(ZoneRecoveryAccount.B, 60);
            State = new ZRSInitiating(this);
        }

        public CalculatorStats GetStats()
        {
            CalculatorStats c = new CalculatorStats();
            c.Direction = ZRBatchLedger[RunningBatchNr].Direction;
            c.Leverage = Leverage;
            c.State = State;
            c.UnitSize = UnitSize;

            return c;
        }

        public void UpdatePrices(Dictionary<string, decimal> dict)
        {
            if (dict.ContainsKey("Ask") && dict["Ask"] > 0)
                Ask = dict["Ask"];
            if (dict.ContainsKey("Bid") && dict["Bid"] > 0)
                Bid = dict["Bid"];
        }

        public void Initialize(IBitmexApiService apiSA, IBitmexApiService apiSB,
            decimal walletBalance,
            Dictionary<ZoneRecoveryAccount, List<Order>> ordersContainer,
            Dictionary<ZoneRecoveryAccount, List<Position>> positionContainer, Mutex positionMutex,
            string symbol = "XBTUSD", 
            int maxDepthIndex = 1, decimal zoneSize = 20, decimal maxExposurePerc = (decimal)0.01, decimal leverage = 1, decimal minPrftPerc = (decimal)0.01)
        {
            // RunningBatchNr should never be reset during the lifetime of this Calculator instance...
            RunningBatchNr = 0;

            try
            {
                
                if (apiSA == null || apiSB == null)
                    throw new Exception("bitmexApiService cannot be null");

                ApiServiceByAccount = new Dictionary<ZoneRecoveryAccount, IBitmexApiService>();
                ApiServiceByAccount.Add(ZoneRecoveryAccount.A, apiSA);
                ApiServiceByAccount.Add(ZoneRecoveryAccount.B, apiSB);

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

                Log.Information("Calculator initialized");
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
            if (switchedOn)
            {
                //Console.WriteLine($"Calculator.Evaluate for Account {acc}");

                // Push the latest updates to the Batch
                var orderList = Orders[acc].Where(o => clOrdIdList.Any(s => s == o.OrderId)).ToList();

                if (ZRBatchLedger.ContainsKey(RunningBatchNr))
                    ZRBatchLedger[RunningBatchNr].CheckStatusses(acc, orderList);
                else
                    throw new Exception($"Evaluate: ZRBatchLedger does not contain RunningBatchNr");

                // Evaluate the new state
                State.Evaluate();
            }
        }

        public void Evaluate()
        {
            if (switchedOn)
            {
                // Evaluate the new state
                State.Evaluate();
            }
        }

        public string GetState()
        {
            return State.GetType().Name;
        }

        public async void CreateNewBatch(ZoneRecoveryBatchType typeToCreate, int direction = 0)
        {
            Log.Debug($"StartNewZRSession");

            if (switchedOn)
            {
                try
                {
                    switch (typeToCreate)
                    {
                        case ZoneRecoveryBatchType.PeggedStart:
                        {
                            if (!ZRBatchLedger.ContainsKey(RunningBatchNr) || (ZRBatchLedger.ContainsKey(RunningBatchNr) && ZRBatchLedger[RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.Closed))
                            {
                                // Set UnitSize
                                CalculateUnitSize(Bid);

                                // Create a new batch
                                var zrob = new ZoneRecoveryBatch(typeToCreate, ZoneRecoveryBatchStatus.Working);
                                RunningBatchNr = zrob.BatchNumber;
                                ZRBatchLedger.Add(RunningBatchNr, zrob);
                                ZoneRecoveryBatchOrder zro;

                                // Create unique ids
                                var idA = CreateNewClOrdID();
                                var idB = CreateNewClOrdID();

                                // Create initial Orders
                                var OrderParamsA = OrderPOSTRequestParams.CreateSimpleLimit(Symbol, idA, UnitSize, (Bid - PegDistance), OrderSide.Buy);
                                zro = new ZoneRecoveryBatchOrder(ZoneRecoveryAccount.A, OrderParamsA, ZoneRecoveryOrderType.FS);
                                ZRBatchLedger[RunningBatchNr].AddOrder(zro);
                                //bitmexApiServiceA
                                //    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA)
                                //    .ContinueWith(HandleOrderResponse, TaskContinuationOptions.AttachedToParent);
                                PlaceOrder(ApiServiceByAccount[ZoneRecoveryAccount.A], ZoneRecoveryAccount.A, OrderParamsA);

                                var OrderParamsB = OrderPOSTRequestParams.CreateSimpleLimit(Symbol, idB, UnitSize, (Ask + PegDistance), OrderSide.Sell);
                                zro = new ZoneRecoveryBatchOrder(ZoneRecoveryAccount.B, OrderParamsB, ZoneRecoveryOrderType.FS);
                                ZRBatchLedger[RunningBatchNr].AddOrder(zro);
                                //bitmexApiServiceB
                                //    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsB)
                                //    .ContinueWith(HandleOrderResponse, TaskContinuationOptions.AttachedToParent);
                                PlaceOrder(ApiServiceByAccount[ZoneRecoveryAccount.B], ZoneRecoveryAccount.B, OrderParamsB);
                            }
                            break;
                        }
                        case ZoneRecoveryBatchType.WindingFirst:
                        {
                            // Create a new batch
                            var zrob = new ZoneRecoveryBatch(typeToCreate, ZoneRecoveryBatchStatus.Working);
                            RunningBatchNr = zrob.BatchNumber;
                            ZRBatchLedger.Add(RunningBatchNr, zrob);
                            ZoneRecoveryBatchOrder zro;

                            // Create unique ids
                            var idTP = CreateNewClOrdID();
                            var idREV = CreateNewClOrdID();

                            OrderSide sideTP, sideREV;
                            ZoneRecoveryAccount apiServiceAccountTP, apiServiceAccountREV;

                            if (direction == 1)
                            {
                                sideTP = OrderSide.Sell;
                                sideREV = OrderSide.Buy;
                                apiServiceAccountTP = ZoneRecoveryAccount.A;
                                apiServiceAccountREV = ZoneRecoveryAccount.B;
                            }
                            else if (direction == -1)
                            {
                                sideTP = OrderSide.Buy;
                                sideREV = OrderSide.Sell;
                                apiServiceAccountTP = ZoneRecoveryAccount.B;
                                apiServiceAccountREV = ZoneRecoveryAccount.A;
                            }


                            // Create 
                            var OrderParamsTP =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), sideTP);
                            zro = new ZoneRecoveryOrder(apiServiceAccountTP, OrderParamsTP, ApiServiceByAccount[apiServiceAccountTP], ZoneRecoveryOrderType.TP);
                            ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                            PlaceOrder(ApiServiceByAccount[apiServiceAccountTP], apiServiceAccountTP, OrderParamsTP);

                            // Create REV
                            var OrderParamsREV =
                                OrderPOSTRequestParams
                                    .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), sideREV);
                            zro = new ZoneRecoveryOrder(apiServiceAccountREV, OrderParamsREV, ApiServiceByAccount[apiServiceAccountREV], ZoneRecoveryOrderType.REV);
                            ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                            PlaceOrder(ApiServiceByAccount[apiServiceAccountREV], apiServiceAccountREV, OrderParamsREV);

                                
                            break;
                        }
                    }
                    
                }
                catch (Exception exc)
                {
                    Log.Error(exc.Message);
                    Console.WriteLine(exc.Message);
                }
            }
            else
            {
                Console.WriteLine($"StartNewZRSession: Tried to start but Calculator is switched off...");
            }
        }

        private decimal CalculateQtyForOrderType(ZoneRecoveryOrderType ot)
        {
            var dir = GetDirectionForCalculation();
            decimal output = 0;

            if (dir == 0)
                return 0;

            try
            {
                PositionMutex.WaitOne();

                var posA = LivePositions[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                var posB = LivePositions[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

                if (posA == null || posB == null)
                    throw new Exception($"one or more positions do not exist");

                switch (ot)
                {
                    case ZoneRecoveryOrderType.TP:
                        if (GetNextDirectionForCalculation() == 1)
                            output = posB.CurrentQty ?? 0;
                        else
                            output = -(posA.CurrentQty ?? 0);
                        break;
                    case ZoneRecoveryOrderType.TL:
                        if (GetNextDirectionForCalculation() == 1)
                            output = -(posA.CurrentQty ?? 0);
                        else
                            output = posB.CurrentQty ?? 0;
                        break;
                    case ZoneRecoveryOrderType.REV:
                        output = GetNextDirectionForCalculation() * UnitSize * FactorArray[GetNextZRPosition()];
                        break;
                    default:
                        output = 0;
                        break;
                }
            }
            catch (Exception exc)
            {
                string message = $"CalculateQtyForOrderType: {exc.Message}";
                Log.Error(message);
                Console.WriteLine(message);
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }
            return output;
        }

        private decimal CalculatePriceForOrderType(ZoneRecoveryOrderType ot)
        {
            var dir = GetDirectionForCalculation();

            if (dir == 0)
                return 0;

            switch (ot)
            {
                case ZoneRecoveryOrderType.TL:
                case ZoneRecoveryOrderType.TP:

                    // TODO Fix Bug: Infinite price is returned
                    decimal breakEvenPrice = CalculateBreakEvenPrice();
                    decimal totalExposure = CalculateTotalOpenExposure();

                    return Math.Round(breakEvenPrice + (dir * (totalExposure * MinimumProfitPercentage)), 2);

                case ZoneRecoveryOrderType.REV:

                    return CalculateReversePrice();

                default:
                    return 0;
            }
        }

        private void PlaceOrder(IBitmexApiService api, ZoneRecoveryAccount acc, OrderPOSTRequestParams paramz, bool bypass = false)
        {
            if (!bypass && RateLimitsRemaining[acc] <= 5)
            {
                Thread.Sleep(2000);
                PlaceOrder(api, acc, paramz, true);
            }
            else
                api.Execute(BitmexApiUrls.Order.PostOrder, paramz).ContinueWith(HandleOrderResponse, TaskContinuationOptions.AttachedToParent);
        }
        
        private void HandleDeleteOrderResponse(Task<BitmexApiResult<List<OrderDto>>> task)
        {
            // Update RateLimitsRemaining
            ZoneRecoveryAccount acc;
            if (ZRBatchLedger[RunningBatchNr].ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.First().ClOrdId).Count() == 1)
                acc = ZRBatchLedger[RunningBatchNr].ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.First().ClOrdId).Select(y => y.Account).Single();
            else
                throw new Exception($"HandleOrderResponse[1]: Shit happening....");

            UpdateRateLimitsRemaining(acc, task.Result.RateLimitRemaining);
                
            Console.WriteLine($"HandleDeleteOrderResponse: [{task.Result.Result.Count()}] order(s) canceled");

            ZRBatchLedger[RunningBatchNr].CheckBatchStatus();
        }

        private void HandleOrderResponse(Task<BitmexApiResult<OrderDto>> task)
        {
            // Update RateLimitsRemaining
            ZoneRecoveryAccount acc;
            if (ZRBatchLedger[RunningBatchNr].ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.ClOrdId).Count() == 1)
                acc = ZRBatchLedger[RunningBatchNr].ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.ClOrdId).Select(y => y.Account).Single();
            else
                throw new Exception($"HandleOrderResponse[1]: Shit happening....");

            UpdateRateLimitsRemaining(acc, task.Result.RateLimitRemaining);

            // Increase ResponsesReceived
            ZRBatchLedger[RunningBatchNr].ResponsesReceived++;

            string message = "";

            if (task.Exception != null)
            {
                message = $"HandleOrderResponse: {(task.Exception.InnerException ?? task.Exception).Message}";
                Log.Error(message);
            }
            else
            {
                if (ZRBatchLedger[RunningBatchNr].ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.ClOrdId).Count() == 1)
                {
                    ZRBatchLedger[RunningBatchNr].ZROrdersList.Where(x => x.PostParams.ClOrdID == task.Result.Result.ClOrdId).Single().SetLastStatus(task.Result.Result);
                    message = $"HandleOrderResponse: order [{task.Result.Result.ClOrdId}] returned status [{task.Result.Result.OrdStatus}]";
                    Log.Information(message);
                }
                else
                {
                    message = $"HandleOrderResponse(impossible): order [{task.Result.Result.ClOrdId}] could not be found in the list, or has multiple instances";
                    Log.Error(message);
                }
            }
            Console.WriteLine(message);

            ZRBatchLedger[RunningBatchNr].CheckBatchStatus();
        }

        private string CreateNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void UpdateRateLimitsRemaining(ZoneRecoveryAccount acc, int rateLimitRemaining)
        {
            if (RateLimitsRemaining.ContainsKey(acc))
                RateLimitsRemaining[acc] = rateLimitRemaining;
            else
                RateLimitsRemaining.Add(acc, rateLimitRemaining);
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
