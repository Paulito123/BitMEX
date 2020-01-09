using System;
using System.Collections.Generic;
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

    class ZoneRecoveryComputer
    {
        #region Private variables

        private IBitmexApiService bitmexApiServiceA;
        private IBitmexApiService bitmexApiServiceB;

        private string Symbol { get; }
        private int MaxDepthIndex { get; }
        private int ZoneSize { get; }
        private double MaxExposurePerc { get; }
        private double Leverage { get; }
        private double MinimumProfitPercentage { get; }
        
        private ZoneRecoveryStatus CurrentStatus;
        private int CurrentZRPosition;
        private ZoneRecoveryDirection CurrentDirection;
        
        private static int StandardPegDistance { get; } = 5;
        private static int[] FactorArray = new int[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        private double InitPrice { get; set; }
        private long UnitSize { get; set; }
        private double TotalBalance { get; set; }

        private Dictionary<ZoneRecoveryAccount, List<Order>> LiveOrders;
        private static Mutex OrderMutex = new Mutex();

        private Dictionary<ZoneRecoveryAccount, List<Position>> LastKnownPosition;
        private static Mutex PositionMutex = new Mutex();

        /// <summary>
        /// The main mutex for placing and removing orders.
        /// </summary>
        private static Mutex EvaluationMutex = new Mutex();

        /// <summary>
        /// A ledger that keeps all the created (and sent) orders. It takes a batch number and a ZoneRecoveryOrder object.
        /// TODO: Add functionality to export the ledger to files or DB...
        /// </summary>
        private readonly SortedDictionary<long, ZoneRecoveryOrderBatch> ZROrderLedger;

        /// <summary>
        /// The last BatchNr that is currently considered active.
        /// </summary>
        private long RunningBatchNr;

        #endregion Private variables

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
                Random rand = new Random();
                if ((rand.NextDouble() * (1 - -1) - 1) >= 0)
                    CurrentDirection = ZoneRecoveryDirection.Up;
                else
                    CurrentDirection = ZoneRecoveryDirection.Down;

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
                // Tell all other threads to wait here if the mutex is working...
                OrderMutex.WaitOne();

                LiveOrders[zra] = lo;
            }
            catch (Exception exc)
            {
                Log.Error($"EvaluateOrders: {exc.Message}");
            }
            finally
            {
                // Evaluate the new situation
                Evaluate();

                // Release the Mutex.
                OrderMutex.ReleaseMutex();
            }
        }

        private void EvaluatePositions(List<Position> posList, ZoneRecoveryAccount zra)
        {
            try
            {
                // Tell all other threads to wait here if the mutex is working...
                PositionMutex.WaitOne();

                var currentQty = LastKnownPosition[zra].Where(x => x.Symbol == Symbol).First().CurrentQty ?? -1;

                if (currentQty >= 0 && currentQty != posList.Where(x => x.Symbol == Symbol).First().CurrentQty)
                {
                    LastKnownPosition[zra] = posList;
                }
            }
            catch (Exception exc)
            {
                Log.Error($"EvaluatePositions: {exc.Message}");
            }
            finally
            {
                // Evaluate the new situation
                Evaluate();

                // Release the Mutex.
                PositionMutex.ReleaseMutex();
            }
        }

        // TODO: Evaluate should be called in both Position stream and Order stream because both orders and positions are evaluated here all the time.
        // The mutex makes sure they dont interfere with each other and wait for the previous action to be finished.
        // We will only be able to go to the next step if ALL REQUIREMENTS are met. The requirements can be related to orders and/or positions.
        // Therefore both streams need to call Evaluate().

        public void Evaluate()
        {
            // Check if one of the expected scenarios has happened and take action.
            // If an unexpected scenario happened, handle the anomaly.

            try
            {
                EvaluationMutex.WaitOne();

                if (ZROrderLedger.Count == 0)
                {
                    EvaluateInitStart();
                }
                else if (CurrentStatus == ZoneRecoveryStatus.Init)
                {
                    EvaluateInit();
                }
                //else if (CurrentStatus == ZoneRecoveryStatus.Winding)
                //{
                //    EvaluateWinding();
                //}
                //else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                //{
                //    EvaluateUnwinding();
                //}
                //else if (CurrentStatus == ZoneRecoveryStatus.Finish)
                //{
                //    HandleFinish();
                //}
                //else
                //{
                //    HandleAnomaly();
                //}
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

        private void EvaluateInitStart()
        {
            // Prepare variables for initial check.
            var nrOfOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol).Count();
            var nrOfOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol).Count();
            var nrOfClosedPositionA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.CurrentQty == 0).Count();
            var nrOfClosedPositionB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.CurrentQty == 0).Count();

            //var nrOfFilledOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();
            //var nrOfFilledOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.Filled && x.OrderQty == UnitSize).Count();

            //var nrOfNewOrdersA = LiveOrders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();
            //var nrOfNewOrdersB = LiveOrders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.OrdStatus == OrderStatus.New && x.OrderQty == UnitSize).Count();
            
            //var nrOfOpenPositionsA = LastKnownPosition[ZoneRecoveryAccount.A].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
            //var nrOfOpenPositionsB = LastKnownPosition[ZoneRecoveryAccount.B].Where(x => x.Symbol == Symbol && x.CurrentQty == UnitSize).Count();
            
            // No orders and both positions closed
            if (nrOfOrdersA == 0 && nrOfOrdersB == 0 && nrOfClosedPositionA == 1 && nrOfClosedPositionB == 1)
            {
                // Create a batchNr
                RunningBatchNr = CreateBatchNr(DateTime.Now);
                var zrob = new ZoneRecoveryOrderBatch(RunningBatchNr, 2);
                ZROrderLedger.Add(RunningBatchNr, zrob);
                ZoneRecoveryOrder zro;
                
                // Create initial Orders on both sides
                var OrderParamsA = OrderPOSTRequestParams.CreateSimpleLimitWithID(Symbol, CreatedNewClOrdID(), UnitSize, (decimal)(InitPrice - StandardPegDistance), OrderSide.Buy);
                zro = new ZoneRecoveryOrder(RunningBatchNr, ZoneRecoveryAccount.A, OrderParamsA);
                ZROrderLedger[RunningBatchNr].AddOrder(zro);
                bitmexApiServiceA.Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA).ContinueWith(ProcessPostOrderResult);

                var OrderParamsB = OrderPOSTRequestParams.CreateSimpleLimitWithID(Symbol, CreatedNewClOrdID(), UnitSize, (decimal)(InitPrice + StandardPegDistance), OrderSide.Sell);
                zro = new ZoneRecoveryOrder(RunningBatchNr, ZoneRecoveryAccount.B, OrderParamsB);
                ZROrderLedger[RunningBatchNr].AddOrder(zro);
                bitmexApiServiceB.Execute(BitmexApiUrls.Order.PostOrder, OrderParamsB).ContinueWith(ProcessPostOrderResult);
            }
            else
            {
                // Anomaly
            }
        }

        // TODO Compare the orders in ZROrderLedger with the copy of the orders on the server LiveOrders.
        private void EvaluateInit()
        {
            // TODO Handle Error
            if (ZROrderLedger[RunningBatchNr].CurrentZROBStatus == ZoneRecoveryOrderBatchStatus.Alert)
                return;

            switch (ZROrderLedger[RunningBatchNr].CurrentZROBStatus)
            {
                case ZoneRecoveryOrderBatchStatus.Resting:

                    break;
                case ZoneRecoveryOrderBatchStatus.Alert:
                    break;
                case ZoneRecoveryOrderBatchStatus.Init:
                    break;
                case ZoneRecoveryOrderBatchStatus.OrderFilled:
                    break;
                case ZoneRecoveryOrderBatchStatus.PartialResting:
                    break;
                default:
                    break;
            }
            
        }

        private string CreatedNewClOrdID()
        {
            return Guid.NewGuid().ToString("N");
        }

        private void ProcessPostOrderResult(Task<BitmexApiResult<OrderDto>> task)
        {
            // TODO: Perform some kind of check if all orders were placed successfully...
            if (task.Exception != null)
            {
                Log.Error((task.Exception.InnerException ?? task.Exception).Message);
                // TODO Handle Exception !!!
                //ZROrderLedger[RunningBatchNr].CurrentZROBStatus = ZoneRecoveryOrderBatchStatus.Alert;
            }
            else //if (task.Result.Result.OrdStatus == "New" || task.Result.Result.OrdStatus == "New,Triggered")
            {
                ZROrderLedger[RunningBatchNr].SetLastResponse(task.Result.Result);
                Log.Information($"Order placed with Id [{task.Result.Result.OrderId}] and status [{task.Result.Result.OrdStatus}]");
            }

            ZROrderLedger[RunningBatchNr].EvaluateStatus();
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

        private void HandleAnomaly()
        {

        }

        private static long CreateBatchNr(DateTime dt)
        {
            return dt.Ticks;
        }
    }
}
