using System;
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

        private string Symbol { get; }
        private int MaxDepthIndex { get; }
        private int ZoneSize { get; }
        private decimal MaxExposurePerc { get; }
        private decimal Leverage { get; }
        private decimal MinimumProfitPercentage { get; }

        private IBitmexApiService bitmexApiServiceA;
        private IBitmexApiService bitmexApiServiceB;

        #endregion Core variables and classes

        #region Static variables

        private static int StandardPegDistance { get; } = 5;
        private static int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        #endregion Static variables

        #region Working Variables

        private decimal InitPrice { get; set; }
        private long UnitSize { get; set; }
        private decimal TotalBalance { get; set; }
        private ZoneRecoveryDirection CurrentProfitDirection { get; set; }

        #endregion Working Variables

        #region Containers

        /// <summary>
        /// A ledger that keeps all the created (and sent) orders. It takes a batch number and a ZoneRecoveryOrder object.
        /// TODO: Add functionality to export the ledger to files or DB...
        /// </summary>
        private readonly SortedDictionary<long, ZoneRecoveryOrderBatch> ZROrderLedger;
        private Dictionary<ZoneRecoveryAccount, List<Order>> LiveOrders;
        private Dictionary<ZoneRecoveryAccount, List<Position>> LastKnownPosition;

        #endregion Containers

        #region Mutexes

        private static Mutex OrderMutex = new Mutex();
        private static Mutex PositionMutex = new Mutex();

        #endregion Mutexes

        #region Variables used to define the current "state"

        /// <summary>
        /// The last BatchNr that is currently considered active.
        /// </summary>
        private long RunningBatchNr;
        private int CurrentZRPosition;
        private ZoneRecoveryStatus CurrentStatus;
        //private ZoneRecoveryDirection CurrentDirection;

        #endregion Variables used to define the current "state"

        #region Constructors

        public ZoneRecoveryComputer(
            IBitmexApiService apiSA,
            IBitmexApiService apiSB,
            decimal mostRecentPrice,
            decimal totalWalletBalance,
            string symbol = "XBTUSD",
            int maxDepthIndex = 1,
            int zoneSize = 20,
            decimal maxExposurePerc = (decimal)0.01,
            decimal leverage = 1,
            decimal minPrftPerc = (decimal)0.01)
        {
            // Should never be reset during the lifetime of this ZoneRecoveryComputer instance...
            ZROrderLedger = new SortedDictionary<long, ZoneRecoveryOrderBatch>();
            RunningBatchNr = 0;
            CurrentProfitDirection = ZoneRecoveryDirection.Undefined;

            try
            {
                if (apiSA == null || apiSB == null)
                    throw new Exception("bitmexApiService cannot be null");

                bitmexApiServiceA = apiSA;
                bitmexApiServiceB = apiSB;

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

                if (mostRecentPrice <= 0)
                    throw new Exception("MostRecentPrice does not have a permitted value");

                if (totalWalletBalance <= 0)
                    throw new Exception("TotalWalletBalance does not have a permitted value");

                Reset(mostRecentPrice, totalWalletBalance);
            }
            catch (Exception exc)
            {
                Log.Error($"ZoneRecoveryComputer: {exc.Message}");
                throw exc;
            }
        }

        #endregion Constructors

        private decimal GetMaximumDepth()
        {
            decimal sum = 0;

            for (int i = 0; i < MaxDepthIndex; i++)
            {
                sum = sum + FactorArray[i];
            }
            return sum;
        }

        private void SetUnitSizeForPrice(decimal refPrice)
        {
            try
            {
                decimal totalDepthMaxExposure = GetMaximumDepth();

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
                CurrentProfitDirection = ZoneRecoveryDirection.Undefined;
                CurrentStatus = ZoneRecoveryStatus.Init;
                CurrentZRPosition = 0;

                LiveOrders = new Dictionary<ZoneRecoveryAccount, List<Order>>();
                LiveOrders.Add(ZoneRecoveryAccount.A, new List<Order>());
                LiveOrders.Add(ZoneRecoveryAccount.B, new List<Order>());

                LastKnownPosition = new Dictionary<ZoneRecoveryAccount, List<Position>>();
                LastKnownPosition.Add(ZoneRecoveryAccount.A, new List<Position>());
                LastKnownPosition.Add(ZoneRecoveryAccount.B, new List<Position>());

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

        public void EvaluateOrders(List<Order> lo, ZoneRecoveryAccount zra)
        {
            try
            {
                OrderMutex.WaitOne();

                LiveOrders[zra] = lo;
            }
            catch (Exception exc)
            {
                Log.Error($"EvaluateOrders: {exc.Message}");
            }
            finally
            {

                Evaluate(lo);

                OrderMutex.ReleaseMutex();
            }
        }

        private void EvaluatePositions(List<Position> posList, ZoneRecoveryAccount zra)
        {
            try
            {
                PositionMutex.WaitOne();

                var currentQty = LastKnownPosition[zra].Where(x => x.Symbol == Symbol).First().CurrentQty ?? -1;

                if (currentQty >= 0 && currentQty != posList.Where(x => x.Symbol == Symbol).First().CurrentQty)
                {
                    LastKnownPosition[zra] = posList.ToList();
                }
            }
            catch (Exception exc)
            {
                Log.Error($"EvaluatePositions: {exc.Message}");
            }
            finally
            {
                PositionMutex.ReleaseMutex();
            }
        }

        public void Evaluate(List<Order> lo = null)
        {
            // Check if one of the expected scenarios has happened and take action.
            // If an unexpected scenario happened, handle the anomaly.
            try
            {
                if (ZROrderLedger.Count == 0)
                {   // Dit moet achteraan samen met wanneer er een batch status => Finish heeft bereikt.
                    Initiate();
                }
                else if (CurrentStatus == ZoneRecoveryStatus.Init || CurrentStatus == ZoneRecoveryStatus.Winding || CurrentStatus == ZoneRecoveryStatus.Unwinding)
                {
                    EvaluateNormal(lo);
                }
                else if (CurrentStatus == ZoneRecoveryStatus.Finish)
                {
                    HandleFinish();
                }
                else // Alert || Undefined
                {
                    EvaluateAnomaly();
                }
            }
            catch (Exception exc)
            {
                Log.Error($"Evaluate: {exc.Message}");
            }
        }

        public int TestMe (int i)
        {
            switch (i)
            {
                case 1:
                    return LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && (x.CurrentQty ?? 0) == 0).Count();
                case 2:
                    return LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && (x.CurrentQty ?? 0) == 0).Count();
                case 3:
                    return LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Count();
                default:
                    return LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Count();
            }
        }

        private void Initiate()
        {
            PositionMutex.WaitOne();
            bool isPositionAClosed = (LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && (x.CurrentQty ?? 0) == 0).Count() == 1) ? true : false;
            bool isPositionBClosed = (LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && (x.CurrentQty ?? 0) == 0).Count() == 1) ? true : false;
            PositionMutex.ReleaseMutex();
            
            bool noMoreOrdersOnA = (LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Count() > 0) ? false : true;
            bool noMoreOrdersOnB = (LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Count() > 0) ? false : true;

            if (noMoreOrdersOnA && noMoreOrdersOnB && isPositionAClosed && isPositionBClosed)
            {
                // Create a batchNr
                RunningBatchNr = CreateBatchNr(DateTime.Now);
                var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                ZROrderLedger.Add(RunningBatchNr, zrob);
                ZoneRecoveryOrder zro;

                // Create initial Orders
                var idA = CreatedNewClOrdID();
                var OrderParamsA =
                    OrderPOSTRequestParams
                        .CreateSimpleLimit(Symbol, idA, UnitSize, (decimal)(InitPrice - StandardPegDistance), OrderSide.Buy);
                zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsA, bitmexApiServiceA, ZoneRecoveryOrderType.FS);
                ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                bitmexApiServiceA
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA)
                    .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                var idB = CreatedNewClOrdID();
                var OrderParamsB =
                    OrderPOSTRequestParams
                        .CreateSimpleLimit(Symbol, idB, UnitSize, (decimal)(InitPrice + StandardPegDistance), OrderSide.Sell);
                zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsB, bitmexApiServiceB, ZoneRecoveryOrderType.FS);
                ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                bitmexApiServiceB
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsB)
                    .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                // Add the scenarios that need to be checked with every update
                List<Order> tmpList;
                tmpList = new List<Order>();
                tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.Filled });
                tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.New });
                ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                tmpList = new List<Order>();
                tmpList.Add(new Order() { ClOrdId = idA, OrdStatus = OrderStatus.New });
                tmpList.Add(new Order() { ClOrdId = idB, OrdStatus = OrderStatus.Filled });
                ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
            }
            else
            {
                // Anomaly
                Log.Warning($"EvaluateInitStart: cannot initiate ZR strategy because the preferred conditions are not met");
                return;
                // TODO: Handle anomaly
            }
        }

        private void EvaluateNormal(List<Order> lo)
        {
            // Add the new shit
            ZROrderLedger[RunningBatchNr].SetMultipleLastResponse(lo);

            // Check if action needs to be taken
            if(ZROrderLedger[RunningBatchNr].EvaluateAndInitiate())
            {
                // Check the last TP result
                var lastRespTP =
                        ZROrderLedger[RunningBatchNr]
                            .OrdersList
                            .Where(x => x.ZROrderType == ZoneRecoveryOrderType.TP || (x.ZROrderType == ZoneRecoveryOrderType.FS && x.CurrentStatus == ZoneRecoveryOrderStatus.Filled))
                            .Single();

                // Turn the wheel...
                TakeStepForward(lastRespTP.LastServerResponse.Side ?? Bitmex.Client.Websocket.Responses.BitmexSide.Undefined);

                // Create the condition that allows TL orders to be created or not.
                bool tlCondition =
                    (CurrentStatus == ZoneRecoveryStatus.Winding && CurrentZRPosition > 1) ||
                    (CurrentStatus == ZoneRecoveryStatus.Unwinding && CurrentZRPosition > 1);

                // Open orders for the next step. Volume is added to the current position.
                if (CurrentStatus == ZoneRecoveryStatus.Winding)
                {
                    if (CurrentProfitDirection == ZoneRecoveryDirection.Up)
                    {
                        // Create a batchNr
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // Create TP
                        var idTP = CreatedNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTP, bitmexApiServiceA, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreatedNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsREV, bitmexApiServiceB, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreatedNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Buy);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTL, bitmexApiServiceB, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
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
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.New });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                    }
                    else if (CurrentProfitDirection == ZoneRecoveryDirection.Down)
                    {
                        // Create a new batch
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // Create TP
                        var idTP = CreatedNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTP, bitmexApiServiceB, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreatedNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsREV, bitmexApiServiceA, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreatedNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Sell);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTL, bitmexApiServiceA, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
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
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.Filled });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                    }
                    else
                    {
                        // Handle other cases...
                    }
                }
                // Open orders for the next step. Volume is removed from the current position.
                else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                {
                    if (CurrentProfitDirection == ZoneRecoveryDirection.Up)
                    {
                        // Create a batchNr
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // Create TP
                        var idTP = CreatedNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTP, bitmexApiServiceA, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreatedNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsREV, bitmexApiServiceB, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreatedNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Buy);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTL, bitmexApiServiceB, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
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
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.New });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                    }
                    else if (CurrentProfitDirection == ZoneRecoveryDirection.Down)
                    {
                        // Create a new batch
                        RunningBatchNr = CreateBatchNr(DateTime.Now);
                        var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario());
                        ZROrderLedger.Add(RunningBatchNr, zrob);
                        ZoneRecoveryOrder zro;

                        // Create TP
                        var idTP = CreatedNewClOrdID();
                        var OrderParamsTP =
                            OrderPOSTRequestParams
                                .CreateSimpleLimit(Symbol, idTP, CalculateQtyForOrderType(ZoneRecoveryOrderType.TP), CalculatePriceForOrderType(ZoneRecoveryOrderType.TP), OrderSide.Sell);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsTP, bitmexApiServiceB, ZoneRecoveryOrderType.TP);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceB
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsTP)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Create REV
                        var idREV = CreatedNewClOrdID();
                        var OrderParamsREV =
                            OrderPOSTRequestParams
                                .CreateMarketStopOrder(Symbol, idREV, CalculateQtyForOrderType(ZoneRecoveryOrderType.REV), CalculatePriceForOrderType(ZoneRecoveryOrderType.REV), OrderSide.Buy);
                        zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsREV, bitmexApiServiceA, ZoneRecoveryOrderType.REV);
                        ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                        bitmexApiServiceA
                            .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsREV)
                            .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                        // Check if the condition to add a TL is met
                        var idTL = CreatedNewClOrdID();
                        if (tlCondition)
                        {
                            var OrderParamsTL =
                                OrderPOSTRequestParams
                                    .CreateSimpleLimit(Symbol, idTL, CalculateQtyForOrderType(ZoneRecoveryOrderType.TL), CalculatePriceForOrderType(ZoneRecoveryOrderType.TL), OrderSide.Sell);
                            zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsTL, bitmexApiServiceA, ZoneRecoveryOrderType.TL);
                            ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
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
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
                        tmpList = new List<Order>();
                        tmpList.Add(new Order() { ClOrdId = idREV, OrdStatus = OrderStatus.New });
                        tmpList.Add(new Order() { ClOrdId = idTP, OrdStatus = OrderStatus.Filled });
                        if (tlCondition)
                            tmpList.Add(new Order() { ClOrdId = idTL, OrdStatus = OrderStatus.Filled });
                        ZROrderLedger[RunningBatchNr].Scenario.AddSuccessOrderList(tmpList);
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

        private string CreatedNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }
        
        private void HandleFinish()
        {
            
        }

        private void EvaluateAnomaly()
        {

        }

        private static long CreateBatchNr(DateTime dt)
        {
            return dt.Ticks;
        }

        private decimal CalculatePriceForOrderType(ZoneRecoveryOrderType ot)
        {
            var dir = GetDirection();
            
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

                    return GetReversePrice();

                default:
                    return 0;
            }
        }

        private decimal CalculateQtyForOrderType(ZoneRecoveryOrderType ot)
        {
            var dir = GetDirection();
            decimal output = 0;

            if (dir == 0)
                return 0;

            try
            {
                PositionMutex.WaitOne();

                var posA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                var posB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

                if (posA == null || posB == null)
                    throw new Exception($"one or more positions do not exist");

                switch (ot)
                {
                    case ZoneRecoveryOrderType.TP:
                        if (GetNextDirection() == 1)
                            output = posB.CurrentQty ?? 0;
                        else
                            output = -(posA.CurrentQty ?? 0);
                        break;
                    case ZoneRecoveryOrderType.TL:
                        if (GetNextDirection() == 1)
                            output = -(posA.CurrentQty ?? 0);
                        else
                            output = posB.CurrentQty ?? 0;
                        break;
                    case ZoneRecoveryOrderType.REV:
                        output = GetNextDirection() * UnitSize * FactorArray[GetNextZRPosition()];
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

        private decimal GetReversePrice()
        {
            decimal result = 0;
            try
            {
                PositionMutex.WaitOne();

                var posA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                var posB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

                if (posA == null || posB == null)
                    throw new Exception($"one or more positions do not exist");

                if ((posA.IsOpen ?? false) && (posB.IsOpen ?? false))
                    result = Math.Round((-GetDirection() == 1) ? (decimal)posB.AvgEntryPrice : (decimal)posA.AvgEntryPrice, 2);
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

        private int GetDirection()
        {
            if (CurrentProfitDirection == ZoneRecoveryDirection.Up)
                return 1;
            else if (CurrentProfitDirection == ZoneRecoveryDirection.Down)
                return -1;
            else
                return 0;
        }

        private int GetNextDirection()
        {
            if (GetDirection() == 1)
                return -1;
            else if (GetDirection() == -1)
                return 1;
            else
                return 0;
        }

        private decimal CalculateBreakEvenPrice()
        {
            decimal result = 0;
            try
            {
                PositionMutex.WaitOne();

                var posA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                var posB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

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

                posA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Single();
                posB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Single();

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

            return (posA.CurrentQty ?? 0)  + (posB.CurrentQty ?? 0);
        }

        private void TakeStepForward(Bitmex.Client.Websocket.Responses.BitmexSide sideHint = Bitmex.Client.Websocket.Responses.BitmexSide.Undefined)
        {
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

            if (sideHint != Bitmex.Client.Websocket.Responses.BitmexSide.Undefined)
            {
                if (sideHint == Bitmex.Client.Websocket.Responses.BitmexSide.Buy)
                    CurrentProfitDirection = ZoneRecoveryDirection.Up;
                else
                    CurrentProfitDirection = ZoneRecoveryDirection.Down;
            }
            else
            {
                if (GetDirection() == 1)
                    CurrentProfitDirection = ZoneRecoveryDirection.Down;
                else if (GetDirection() == -1)
                    CurrentProfitDirection = ZoneRecoveryDirection.Up;
            }
        }

    }
}




//var nrOfFilledOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();
//var nrOfFilledOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();

//var nrOfNewOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();
//var nrOfNewOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();

//var nrOfOpenPositionsA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
//var nrOfOpenPositionsB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
