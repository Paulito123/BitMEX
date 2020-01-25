using System;
using System.IO;
using System.Collections.Generic;
//using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

using Serilog;

namespace PStrategies.ZoneRecovery
{
    public class ZoneRecoveryComputer
    {
        #region Core variables and classes

        private bool IsLive { get; set; }
        private string Symbol { get; set; }
        private int MaxDepthIndex { get; set; }
        private decimal ZoneSize { get; set; }
        private decimal MaxExposurePerc { get; set; }
        private decimal Leverage { get; set; }
        private decimal MinimumProfitPercentage { get; set; }

        private IBitmexApiService bitmexApiServiceA { get; set; }
        private IBitmexApiService bitmexApiServiceB { get; set; }

        #endregion Core variables and classes

        #region Static variables

        private static int StandardPegDistance { get; } = 10;
        private static int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        #endregion Static variables

        #region Working Variables

        private decimal InitPrice { get; set; }
        public long UnitSize { get; set; }
        private decimal TotalBalance { get; set; }
        public ZoneRecoveryDirection CurrentDirection { get; set; }
        public DateTime LastWorkDone { get; set; }

        #endregion Working Variables

        #region Containers

        /// <summary>
        /// A ledger that keeps all the created (and sent) orders. It takes a batch number and a ZoneRecoveryOrder object.
        /// TODO: Add functionality to export the ledger to files or DB...
        /// </summary>
        private readonly SortedDictionary<long, ZoneRecoveryOrderBatch> ZROrderLedger;

        /// <summary>
        /// A reference to the Orders Dictionary in the OrderStatsHandler
        /// </summary>
        private Dictionary<ZoneRecoveryAccount, List<Order>> LiveOrders;

        /// <summary>
        /// A reference to the Position Dictionary in the PositionStatsHandler
        /// </summary>
        private Dictionary<ZoneRecoveryAccount, List<Position>> LivePositions;

        #endregion Containers

        #region Mutexes

        private static Mutex OrderMutex;
        private static Mutex PositionMutex;

        #endregion Mutexes

        #region Variables used to define the current "state"

        /// <summary>
        /// The last BatchNr that is currently considered active.
        /// </summary>
        private long RunningBatchNr;
        public int CurrentZRPosition;
        public ZoneRecoveryStatus CurrentStatus;

        #endregion Variables used to define the current "state"

        #region Constructors and Initializers

        public ZoneRecoveryComputer()
        {
            ZROrderLedger = new SortedDictionary<long, ZoneRecoveryOrderBatch>();
            IsLive = false;
            Console.WriteLine("ZRComputer constructed.");
        }

        public bool Initialise(
            IBitmexApiService apiSA,
            IBitmexApiService apiSB,
            decimal bidPrice,
            decimal askPrice,
            decimal totalWalletBalance,
            Dictionary<ZoneRecoveryAccount, List<Order>> liveOrders,
            Mutex orderMutex,
            Dictionary<ZoneRecoveryAccount, List<Position>> livePositions,
            Mutex positionMutex,
            string symbol = "XBTUSD",
            int maxDepthIndex = 1,
            decimal zoneSize = 20,
            decimal maxExposurePerc = (decimal)0.01,
            decimal leverage = 1,
            decimal minPrftPerc = (decimal)0.01)
        {
            // RunningBatchNr should never be reset during the lifetime of this ZoneRecoveryComputer instance...
            RunningBatchNr = 0;

            try
            {
                if (apiSA == null || apiSB == null)
                    throw new Exception("bitmexApiService cannot be null");

                bitmexApiServiceA = apiSA;
                bitmexApiServiceB = apiSB;

                if (liveOrders != null)
                    LiveOrders = liveOrders;
                else
                    throw new Exception("Empty reference to live orders");

                if (orderMutex != null)
                    OrderMutex = orderMutex;
                else
                    throw new Exception("Empty reference to OrderMutex");

                if (livePositions != null)
                    LivePositions = livePositions;
                else
                    throw new Exception("Empty reference to live Positions");

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

                // TODO Da klopt ier nie eh moat!
                if (bidPrice <= 0 || askPrice <= 0)
                    throw new Exception("MostRecentPrice does not have a permitted value");

                if (totalWalletBalance <= 0)
                    throw new Exception("TotalWalletBalance does not have a permitted value");

                Reset(bidPrice, totalWalletBalance);

                IsLive = true;
                Console.WriteLine("ZRComputer is Live.");
            }
            catch (Exception exc)
            {
                Log.Error($"ZoneRecoveryComputer: {exc.Message}");
                return false;
            }
            return true;
        }

        #endregion Constructors

        #region ZoneRecovery core methods

        private void TakeStepForward(ZoneRecoveryDirection sideHint = ZoneRecoveryDirection.Undefined)
        {
            if (CurrentDirection == ZoneRecoveryDirection.Undefined && sideHint == ZoneRecoveryDirection.Undefined)
                return;

            if (CurrentStatus == ZoneRecoveryStatus.Winding)
            {
                CurrentZRPosition++;

                // Zone Recovery logic is reversed
                if (CurrentZRPosition == MaxDepthIndex)
                    CurrentStatus = ZoneRecoveryStatus.Unwinding;
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
            {
                CurrentZRPosition--;

                // Reset the calculator
                if (CurrentZRPosition == 0)
                    CurrentStatus = ZoneRecoveryStatus.Finish;
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Init)
            {
                CurrentStatus = ZoneRecoveryStatus.Winding;
                CurrentZRPosition++;
            }

            // Determine new direction
            if (CurrentDirection == ZoneRecoveryDirection.Down)
                CurrentDirection = ZoneRecoveryDirection.Up;
            else if (CurrentDirection == ZoneRecoveryDirection.Up)
                CurrentDirection = ZoneRecoveryDirection.Down;
            else
                CurrentDirection = sideHint;
        }

        public bool Live()
        {
            return IsLive;
        }

        public void BringDown()
        {
            IsLive = false;
        }

        public void Enable()
        {
            IsLive = true;
        }

        #endregion

        #region Calculators

        private decimal CalculateMaximumDepth()
        {
            decimal sum = 0;

            for (int i = 0; i < MaxDepthIndex; i++)
            {
                sum = sum + FactorArray[i];
            }
            return sum;
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
                Log.Error($"CalculateQtyForOrderType: {exc.Message}");
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }
            return output;
        }

        private decimal CalculateReversePrice()
        {
            decimal result = 0;
            try
            {
                PositionMutex.WaitOne();

                var posA = LivePositions[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                var posB = LivePositions[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

                if (posA == null || posB == null)
                    throw new Exception($"one or more positions do not exist");

                if ((posA.IsOpen ?? false) && (posB.IsOpen ?? false))
                    result = Math.Round((-GetDirectionForCalculation() == 1) ? (decimal)posB.AvgEntryPrice : (decimal)posA.AvgEntryPrice, 2);
                else if (posA.IsOpen ?? false)
                    result = Math.Round((decimal)posA.AvgEntryPrice - ZoneSize, 2);
                else if (posB.IsOpen ?? false)
                    result = Math.Round((decimal)posB.AvgEntryPrice + ZoneSize, 2);
            }
            catch (Exception exc)
            {
                Log.Error($"GetReversePrice: {exc.Message}");
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }

            return result;
        }

        private decimal CalculateBreakEvenPrice()
        {
            decimal result = 0;
            try
            {
                PositionMutex.WaitOne();

                var posA = LivePositions[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                var posB = LivePositions[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

                if (posA == null || posB == null)
                    throw new Exception($"one or more positions do not exist");

                decimal beA = 0, beB = 0;

                if (posA.CurrentQty > 0)
                    beA = (decimal)((posA.BreakEvenPrice != null) ? posA.BreakEvenPrice : posA.AvgEntryPrice);

                if (posB.CurrentQty > 0)
                    beB = (decimal)((posB.BreakEvenPrice != null) ? posB.BreakEvenPrice : posB.AvgEntryPrice);

                if (beA > 0 || beB > 0)
                    result = ((beA * (decimal)posA.CurrentQty) + (beB * (decimal)posB.CurrentQty)) / ((decimal)posA.CurrentQty + (decimal)posB.CurrentQty);

            }
            catch (Exception exc)
            {
                Log.Error($"CalculateBreakEvenPrice: {exc.Message}");
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }

            return result;
        }

        private long CalculateTotalOpenExposure()
        {
            Position posA = new Position();
            Position posB = new Position();

            try
            {
                PositionMutex.WaitOne();

                posA = LivePositions[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                posB = LivePositions[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

                if (posA == null || posB == null)
                    throw new Exception($"one or more positions do not exist");
            }
            catch (Exception exc)
            {
                Log.Error($"CalculateTotalOpenExposure: {exc.Message}");
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }

            return (posA.CurrentQty ?? 0) + (posB.CurrentQty ?? 0);
        }

        #endregion Calculators

        #region Getter and Setters

        public void SetUnitSizeForPrice(decimal refPrice)
        {
            try
            {
                decimal totalDepthMaxExposure = CalculateMaximumDepth();

                if (refPrice > 0 && totalDepthMaxExposure > 0 && totalDepthMaxExposure < FactorArray.Sum())
                {
                    UnitSize = (long)Math.Round(refPrice * Leverage * TotalBalance * MaxExposurePerc / totalDepthMaxExposure, MidpointRounding.ToEven);
                }
                else
                    throw new Exception("Cannot determine UnitSize");
            }
            catch (Exception exc)
            {
                Log.Error($"SetUnitSizeForPrice: {exc.Message}");
                throw exc;
            }
        }

        public void Reset(decimal mostRecentPrice, decimal totalWalletBalance)
        {
            try
            {
                CurrentDirection = ZoneRecoveryDirection.Undefined;
                CurrentStatus = ZoneRecoveryStatus.Init;
                CurrentZRPosition = 0;

                if (totalWalletBalance > 0)
                    TotalBalance = totalWalletBalance;
                else
                    throw new Exception("TotalWalletBalance does not have a permitted value");

                if (mostRecentPrice > 0)
                {
                    InitPrice = mostRecentPrice;
                    SetUnitSizeForPrice(mostRecentPrice);
                }
                else
                    throw new Exception("MostRecentPrice does not have a permitted value");
            }
            catch (Exception exc)
            {
                Log.Error($"Reset: {exc.Message}");
                throw exc;
            }
        }

        private int GetNextZRPosition()
        {
            if (CurrentStatus == ZoneRecoveryStatus.Init)
                return 1;
            else if (CurrentStatus == ZoneRecoveryStatus.Winding)
                return CurrentZRPosition + 1;
            else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                return CurrentZRPosition - 1;
            else if (CurrentStatus == ZoneRecoveryStatus.Alert)
                return -1;
            else
                return 0;
        }

        private int GetDirectionForCalculation()
        {
            if (CurrentDirection == ZoneRecoveryDirection.Up)
                return 1;
            else if (CurrentDirection == ZoneRecoveryDirection.Down)
                return -1;
            else
                return 0;
        }

        private int GetNextDirectionForCalculation()
        {
            if (GetDirectionForCalculation() == 1)
                return -1;
            else if (GetDirectionForCalculation() == -1)
                return 1;
            else
                return 0;
        }

        // To be called within the parent PositionMutex context !!!!!!
        private long GetOpenQtyForAccount(ZoneRecoveryAccount acc)
        {
            if (LivePositions != null && LivePositions.ContainsKey(acc) && LivePositions[acc] != null && LivePositions[acc].Where(x => x.Symbol == Symbol).Count() > 0)
                return (LivePositions[acc].Where(x => x.Symbol == Symbol).First().CurrentQty ?? 0);
            else
                return 0;
        }

        #endregion Getter and Setters

        #region Evaluators and Initiators

        public void Evaluate()
        {
            // Check if one of the expected scenarios has happened and take action.
            // If an unexpected scenario happened, handle the anomaly.
            if (IsLive)
            {
                try
                {
                    OrderMutex.WaitOne();

                    Log.Debug($"Evaluate: {DateTime.Now}");

                    if (LiveOrders == null || LiveOrders.Count() == 0)
                        return;

                    List<Order> listConcat = new List<Order>();

                    if (LiveOrders.ContainsKey(ZoneRecoveryAccount.A) && LiveOrders[ZoneRecoveryAccount.A] != null && LiveOrders[ZoneRecoveryAccount.A].Count() > 0)
                        listConcat.AddRange(LiveOrders[ZoneRecoveryAccount.A]);

                    if (LiveOrders.ContainsKey(ZoneRecoveryAccount.B) && LiveOrders[ZoneRecoveryAccount.B] != null && LiveOrders[ZoneRecoveryAccount.B].Count() > 0)
                        listConcat.AddRange(LiveOrders[ZoneRecoveryAccount.B]);

                    if (listConcat.Count() > 0 && (CurrentStatus == ZoneRecoveryStatus.Init ||
                                                   CurrentStatus == ZoneRecoveryStatus.Winding ||
                                                   CurrentStatus == ZoneRecoveryStatus.Unwinding))
                        EvaluateNormal(listConcat);

                    //else if (CurrentStatus == ZoneRecoveryStatus.Finish)
                    //{
                    //    HandleFinish();
                    //}
                    //else // Alert || Undefined
                    //{
                    //    EvaluateAnomaly();
                    //}

                    if (CurrentStatus == ZoneRecoveryStatus.Init)
                        Initiate(listConcat);
                }
                catch (Exception exc)
                {
                    Log.Error($"Evaluate: {exc.Message}");
                }
                finally
                {
                    OrderMutex.ReleaseMutex();
                }
            }
        }

        private bool AccountsFlat()
        {
            bool areAccountsFlat = false;

            try
            {
                PositionMutex.WaitOne();

                if (GetOpenQtyForAccount(ZoneRecoveryAccount.A) == 0 || GetOpenQtyForAccount(ZoneRecoveryAccount.B) == 0)
                    areAccountsFlat = true;
            }
            catch (Exception exc)
            {
                Log.Error($"AccountsFlat: {exc.Message}");
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }
            return areAccountsFlat;
        }
        
        private void Initiate(List<Order> listConcat)
        {
            if (AccountsFlat() && listConcat.Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New).Count() == 0)
            {
                LastWorkDone = DateTime.Now;

                // Create a batchNr
                RunningBatchNr = CreateBatchNr(DateTime.Now);
                var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                ZROrderLedger.Add(RunningBatchNr, zrob);
                ZoneRecoveryOrder zro;
                
                // Create initial Orders
                var idA = CreateNewClOrdID();
                var OrderParamsA =
                    OrderPOSTRequestParams
                        .CreateSimpleLimit(Symbol, idA, UnitSize, (InitPrice - StandardPegDistance), OrderSide.Buy);
                zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsA, bitmexApiServiceA, ZoneRecoveryOrderType.FS);
                ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                bitmexApiServiceA
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA)
                    .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                var idB = CreateNewClOrdID();
                var OrderParamsB =
                    OrderPOSTRequestParams
                        .CreateSimpleLimit(Symbol, idB, UnitSize, (InitPrice + StandardPegDistance), OrderSide.Sell);
                zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsB, bitmexApiServiceB, ZoneRecoveryOrderType.FS);
                ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                bitmexApiServiceB
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsB)
                    .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                // Add the scenarios that need to be checked with every update
                List<Order> tmpList;
                tmpList = new List<Order>();
                tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.Filled });
                tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.New });
                ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Up);
                tmpList = new List<Order>();
                tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.New });
                tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.Filled });
                ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Down);

                
            }
            else
            {
                // TODO: Check LastWorkDone if a possible anomaly needs to be checked
                Log.Information($"Initiate: cannot initiate ZR strategy because the preferred conditions are not met");
                return;
            }
        }

        private void RemoveFromOrderList(List<string> clOrdIdList)
        {
            if (LiveOrders == null || !LiveOrders.ContainsKey(ZoneRecoveryAccount.A) || !LiveOrders.ContainsKey(ZoneRecoveryAccount.B))
                throw new Exception("RemoveFromOrderList: LiveOrders == null");

            foreach (string id in clOrdIdList)
            {
                int cnt;

                cnt = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.ClOrdId == id).Count();
                if (cnt > 0)
                {
                    LiveOrders[ZoneRecoveryAccount.A].Remove(LiveOrders[ZoneRecoveryAccount.A].Single(s => s.ClOrdId == id));
                }
                else
                {
                    cnt = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.ClOrdId == id).Count();

                    if (cnt > 0)
                        LiveOrders[ZoneRecoveryAccount.B].Remove(LiveOrders[ZoneRecoveryAccount.B].Single(s => s.ClOrdId == id));
                }
            }
        }

        private void EvaluateNormal(List<Order> orders)
        {
            Log.Debug($"EvaluateNormal started and the RunningBatchNr=[{RunningBatchNr}]");

            if (ZROrderLedger == null || !ZROrderLedger.ContainsKey(RunningBatchNr))
                return;
            
            Log.Debug("EvaluateAndInitiate going to start");

            // TODO: Retrun a specific status instead of a bool
            // Check if action needs to be taken 
            bool isBatchClosed = ZROrderLedger[RunningBatchNr].EvaluateAndInitiate(orders);

            Log.Debug($"EvaluateNormal: {isBatchClosed}");
            
            if (isBatchClosed)
            {
                RemoveFromOrderList(ZROrderLedger[RunningBatchNr].GetClOrdIDList());

                // Turn the wheel...
                TakeStepForward(ZROrderLedger[RunningBatchNr].Direction);

                // Create the condition that allows TL orders to be created or not.
                bool tlCondition =
                    (CurrentStatus == ZoneRecoveryStatus.Winding && CurrentZRPosition > 1) ||
                    (CurrentStatus == ZoneRecoveryStatus.Unwinding && CurrentZRPosition > 1);

                // Open orders for the next step. Volume is added to the current position.
                if (CurrentStatus == ZoneRecoveryStatus.Winding)
                {
                    if (CurrentDirection == ZoneRecoveryDirection.Up)
                    {
                        // Create a batchNr
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // A => TP
                        // B => REV,TL

                        // Create TP
                        var idTP = CreateNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTP, bitmexApiServiceA, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreateNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsREV, bitmexApiServiceB, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreateNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Buy);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTL, bitmexApiServiceB, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                            bitmexApiServiceB
                                .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTL)
                                .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);
                        }

                        // Add the scenarios that need to be checked with every update
                        List<Order> tmpList;
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.Filled });
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.New });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.Filled });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Up);

                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.New });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Down);
                    }
                    else if (CurrentDirection == ZoneRecoveryDirection.Down)
                    {
                        // Create a new batch
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // B => TP
                        // A => REV,TL

                        // Create TP
                        var idTP = CreateNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTP, bitmexApiServiceB, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreateNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsREV, bitmexApiServiceA, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreateNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Sell);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTL, bitmexApiServiceA, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                            bitmexApiServiceA
                                .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTL)
                                .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);
                        }

                        // Add the scenarios that need to be checked with every update
                        List<Order> tmpList;
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.Filled });
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.New });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.New });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Up);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.Filled });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Down);
                    }
                    else
                    {
                        // Handle other cases...
                    }
                }
                // Open orders for the next step. Volume is removed from the current position.
                else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                {
                    if (CurrentDirection == ZoneRecoveryDirection.Up)
                    {
                        // Create a batchNr
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // A => TP
                        // B => REV,TL

                        // Create TP
                        var idTP = CreateNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTP, bitmexApiServiceA, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreateNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsREV, bitmexApiServiceB, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreateNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Buy);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTL, bitmexApiServiceB, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                            bitmexApiServiceB
                                .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTL)
                                .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);
                        }

                        // Add the scenarios that need to be checked with every update
                        List<Order> tmpList;
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.Filled });
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.New });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.Filled });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Up);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.New });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Down);
                    }
                    else if (CurrentDirection == ZoneRecoveryDirection.Down)
                    {
                        // Create a new batch
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // B => TP
                        // A => REV,TL

                        // Create TP
                        var idTP = CreateNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTP, bitmexApiServiceB, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreateNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsREV, bitmexApiServiceA, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreateNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Sell);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTL, bitmexApiServiceA, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].ZROrdersList.Add(zro);
                            bitmexApiServiceA
                                .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTL)
                                .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);
                        }

                        // Add the scenarios that need to be checked with every update
                        List<Order> tmpList;
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.Filled });
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.New });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.New });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Up);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.Filled });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList, ZoneRecoveryDirection.Down);
                    }
                    else
                    {
                        // Handle other cases...
                    }
                }
            }
            else
            {
                // TODO Check for anomaly or wait for next OrderResponse?
            }


        }

        private void HandleFinish()
        {

        }

        private void EvaluateAnomaly()
        {

        }

        #endregion Evaluators

        #region Helpers

        private string CreateNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private static long CreateBatchNr(DateTime dt)
        {
            return dt.Ticks;
        }

        #endregion Helpers
        
    }
}


//public void EvaluateOrders()
//{
//    try
//    {
//        OrderMutex.WaitOne();

//        Evaluate();
//    }
//    catch (Exception exc)
//    {
//        Log.Error($"EvaluateOrders: {exc.Message}");
//    }
//    finally
//    {
//        OrderMutex.ReleaseMutex();
//    }
//}

//private void EvaluatePositions(List<Position> posList, ZoneRecoveryAccount zra)
//{
//    try
//    {
//        PositionMutex.WaitOne();

//        var currentQty = LastKnownPosition[zra].Where(x => x.Symbol == Symbol).First().CurrentQty ?? -1;

//        if (currentQty >= 0 && currentQty != posList.Where(x => x.Symbol == Symbol).First().CurrentQty)
//        {
//            LastKnownPosition[zra] = posList.ToList();
//        }
//    }
//    catch (Exception exc)
//    {
//        Log.Error($"EvaluatePositions: {exc.Message}");
//    }
//    finally
//    {
//        PositionMutex.ReleaseMutex();
//    }
//}

//private int GetPreviousZRPosition()
//{
//    if (CurrentStatus == ZoneRecoveryStatus.Init)
//        return 0;
//    else if (CurrentStatus == ZoneRecoveryStatus.Winding)
//        return CurrentZRPosition - 1;
//    else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
//        return CurrentZRPosition + 1;
//    else if (CurrentStatus == ZoneRecoveryStatus.Alert)
//        return -1;
//    else
//        return 0;
//}

//var nrOfFilledOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();
//var nrOfFilledOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();

//var nrOfNewOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();
//var nrOfNewOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();

//var nrOfOpenPositionsA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
//var nrOfOpenPositionsB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
