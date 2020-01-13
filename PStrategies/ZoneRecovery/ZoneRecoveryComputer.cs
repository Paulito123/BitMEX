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
        private double MaxExposurePerc { get; }
        private double Leverage { get; }
        private double MinimumProfitPercentage { get; }

        private IBitmexApiService bitmexApiServiceA;
        private IBitmexApiService bitmexApiServiceB;

        #endregion Core variables and classes

        #region Static variables

        private static int StandardPegDistance { get; } = 5;
        private static int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        #endregion Static variables

        #region Working Variables

        private double InitPrice { get; set; }
        private long UnitSize { get; set; }
        private double TotalBalance { get; set; }

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
        private static Mutex EvaluationMutex = new Mutex();

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
            double mostRecentPrice,
            double totalWalletBalance,
            string symbol = "XBTUSD",
            int maxDepthIndex = 1,
            int zoneSize = 20,
            double maxExposurePerc = 0.01,
            double leverage = 1,
            double minPrftPerc = 0.01)
        {

            // Should never be reset during the lifetime of this ZoneRecoveryComputer instance...
            ZROrderLedger = new SortedDictionary<long, ZoneRecoveryOrderBatch>();
            RunningBatchNr = 0;

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

        private double GetMaximumDepth()
        {
            double sum = 0;

            for (int i = 0; i < MaxDepthIndex; i++)
            {
                sum = sum + FactorArray[i];
            }
            return sum;
        }

        private void SetUnitSizeForPrice(double refPrice)
        {
            try
            {
                double totalDepthMaxExposure = GetMaximumDepth();

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

        public void Reset(double mostRecentPrice, double totalWalletBalance)
        {
            try
            {
                //Random rand = new Random();
                //if ((rand.NextDouble() * (1 - -1) - 1) >= 0)
                //    CurrentDirection = ZoneRecoveryDirection.Up;
                //else
                //    CurrentDirection = ZoneRecoveryDirection.Down;

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

                LiveOrders[zra] = lo.ToList();
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

        // TODO 
        // At the time a new Batch is created, also the possible best-case scenarios can be created. Like this, the input and the possible outcome are
        // coded close together, which makes readability better. The outcome scenarios make the evaluation and anomaly detection easier.

        public void Evaluate(List<Order> lo = null)
        {
            // Check if one of the expected scenarios has happened and take action.
            // If an unexpected scenario happened, handle the anomaly.

            try
            {
                EvaluationMutex.WaitOne();

                if (ZROrderLedger.Count == 0)
                {
                    EvaluateInitiate();
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
            finally
            {
                EvaluationMutex.ReleaseMutex();
            }
        }

        private void EvaluateInitiate()
        {
            PositionMutex.WaitOne();
            bool isPositionAClosed = (LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.CurrentQty == 0).Count() == 1) ? true : false;
            bool isPositionBClosed = (LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.CurrentQty == 0).Count() == 1) ? true : false;
            PositionMutex.ReleaseMutex();
            
            bool noMoreOrdersOnA = (LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Count() > 0) ? false : true;
            bool noMoreOrdersOnB = (LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Count() > 0) ? false : true;

            if (noMoreOrdersOnA && noMoreOrdersOnB && isPositionAClosed && isPositionBClosed)
            {
                // Create a batchNr
                RunningBatchNr = CreateBatchNr(DateTime.Now);
                var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, new ZoneRecoveryScenario(ZoneRecoveryScenarioSetup.PDNI_TwoSides));
                ZROrderLedger.Add(RunningBatchNr, zrob);
                ZoneRecoveryOrder zro;

                // Create initial Orders
                var idA = CreatedNewClOrdID();
                var OrderParamsA =
                    OrderPOSTRequestParams
                        .CreateSimpleLimitWithID(Symbol, idA, UnitSize, (decimal)(InitPrice - StandardPegDistance), OrderSide.Buy);
                zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.A, OrderParamsA, bitmexApiServiceA, ZoneRecoveryOrderType.FB);
                ZROrderLedger[RunningBatchNr].OrdersList.Add(zro);
                bitmexApiServiceA
                    .Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA)
                    .ContinueWith(ZROrderLedger[RunningBatchNr].ProcessPostOrderResult, TaskContinuationOptions.AttachedToParent);

                var idB = CreatedNewClOrdID();
                var OrderParamsB =
                    OrderPOSTRequestParams
                        .CreateSimpleLimitWithID(Symbol, idB, UnitSize, (decimal)(InitPrice + StandardPegDistance), OrderSide.Sell);
                zro = new ZoneRecoveryOrder(ZoneRecoveryAccount.B, OrderParamsB, bitmexApiServiceB, ZoneRecoveryOrderType.FB);
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
                //ZROrderLedger[RunningBatchNr]
                



                //if (CurrentStatus == ZoneRecoveryStatus.Init)
                //{

                //}
                //else if (CurrentStatus == ZoneRecoveryStatus.Winding)
                //{

                //}
                //else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                //{

                //}
            }


        }

        //private ZoneRecoveryOrderBatchStatus ServerAppSync()
        //{
        //    OrderMutex.WaitOne();
        //    List<string> idList = ZROrderLedger[RunningBatchNr].GetClOrdIDList();

        //    foreach (string clOrdID in idList)
        //    {
        //        if (LiveOrders[ZoneRecoveryAccount.A].Where(x => x.ClOrdId == clOrdID).Count() > 0)
        //            ZROrderLedger[RunningBatchNr].OrdersList.Where(x => x.PostParams.ClOrdID == clOrdID).Single().LastServerResponse =
        //                LiveOrders[ZoneRecoveryAccount.A].Where(x => x.ClOrdId == clOrdID).Single();

        //        else if (LiveOrders[ZoneRecoveryAccount.B].Where(x => x.ClOrdId == clOrdID).Count() > 0)
        //            ZROrderLedger[RunningBatchNr].OrdersList.Where(x => x.PostParams.ClOrdID == clOrdID).Single().LastServerResponse =
        //                LiveOrders[ZoneRecoveryAccount.B].Where(x => x.ClOrdId == clOrdID).Single();
        //    }

        //    OrderMutex.ReleaseMutex();

        //    return ZROrderLedger[RunningBatchNr].EvaluateStatusAfterServerUpdate();
        //}

        private string CreatedNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void EvaluateWinding()
        {

        }

        private void EvaluateUnwinding()
        {

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

    }
}




//var nrOfFilledOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();
//var nrOfFilledOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();

//var nrOfNewOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();
//var nrOfNewOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();

//var nrOfOpenPositionsA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
//var nrOfOpenPositionsB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
