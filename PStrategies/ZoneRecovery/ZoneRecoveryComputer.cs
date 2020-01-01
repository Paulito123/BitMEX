using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMEX.Model;
using BitMEX.Client;

namespace PStrategies.ZoneRecovery
{
    /// <summary>
    /// An enumeration used for expressing in which part of the Zone Recovery algorithm the class is at a given moment.
    /// </summary>
    public enum ZoneRecoveryStatus { Winding, Unwinding, Init, Finish, Alert, Undefined }

    /// <summary>
    /// The direction in which profits (or lesser loss) can be made for the current positions.
    /// </summary>
    public enum ZoneRecoveryDirection { Up, Down, Undefined }

    class ZoneRecoveryComputer
    {
        #region Private variables

        /// <summary>
        /// The trading symbol for this instance of the Calculator.
        /// </summary>
        private string Symbol { get; }

        /// <summary>
        /// An array of factors used to calculate the size of each winding.
        /// </summary>
        private static long[] FactorArray = new long[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        /// <summary>
        /// The minimum unit size of each order. The result of the UnitSize multiplied by a value of the FactorArray 
        /// results in the quantity of a respective order.
        /// </summary>
        private long UnitSize { get; set; }

        /// <summary>
        /// Reflects how deep inside the FactorArray, this instance of the Calculator class, is allowed to go.
        /// </summary>
        private int MaxDepthIndex { get; }

        /// <summary>
        /// The list of available connections.
        /// </summary>
        private Dictionary<long, MordoR> Connections;
        
        /// <summary>
        /// At any time, this measure reflects the maximum percentage of the TotalBalance that can be exposed to 
        /// the market. 
        /// </summary>
        private double MaxExposurePerc { get; }

        /// <summary>
        /// The current leverage used for calculating other measures
        /// </summary>
        private double Leverage { get; }

        /// <summary>
        /// The size of the zone expressed in number of pips. ZoneSize * PipSize = Real Zone Size
        /// </summary>
        private int ZoneSize { get; }

        /// <summary>
        /// When the mathematical break even price has been reached, this percentage defines how far in profit
        /// the strategy needs to be before closing all related positions. 
        /// Should be a decimal between 0 and 1!!!
        /// </summary>
        private double MinimumProfitPercentage { get; }

        /// <summary>
        /// Object used to lock a piece of code to prevent it from being executed multiple times within one instance of the calculator class.
        /// </summary>
        private readonly Object _Lock = new Object();

        /// <summary>
        /// The value of the current ZoneRecoveryStatus
        /// </summary>
        private ZoneRecoveryStatus CurrentStatus;

        /// <summary>
        /// CurrentZRPosition reflects the position withing the Zone Recovery strategy.
        /// When 0 > Strategy has been initialized or is completely unwound. There should be no open positions.
        /// When 1 > First Winding / last Unwinding
        /// When CurrentZRPosition = MaxDepthIndex > ZoneRecoveryStatus is switched and winding process reversed
        /// </summary>
        private int CurrentZRPosition;

        /// <summary>
        /// The current direction for the known positions.
        /// </summary>
        private ZoneRecoveryDirection CurrentDirection;

        #endregion Private variables

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="maxExposurePerc"></param>
        /// <param name="leverage"></param>
        /// <param name="maxDepthIndex"></param>
        /// <param name="zoneSize"></param>
        /// <param name="minPrftPerc"></param>
        /// <param name="connA"></param>
        /// <param name="connB"></param>
        public ZoneRecoveryComputer(string symbol, double maxExposurePerc, double leverage, int maxDepthIndex, int zoneSize, double minPrftPerc, MordoR connA, MordoR connB)
        {
            // Initialize main variables
            Symbol = symbol;
            MaxExposurePerc = maxExposurePerc;
            Leverage = leverage;
            MaxDepthIndex = maxDepthIndex;
            ZoneSize = zoneSize;
            MinimumProfitPercentage = minPrftPerc;

            Connections = new Dictionary<long, MordoR>();
            Connections.Add(connA.Account, connA);
            Connections.Add(connB.Account, connB);
        }

        /// <summary>
        /// Turning the wheel, advance one step further in the ZR winding process...
        /// </summary>
        private void TakeStepForward()
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
        }

        /// <summary>
        /// Calculates the break even price of two positions.
        /// </summary>
        /// <returns>The calculated break even price</returns>
        private static double CalculateBreakEvenPrice(double volA, double priceA, double volB, double priceB)
        {
            double outp;

            try
            {
                outp = ((volA * priceA) + (volB * priceB)) / (volA + volB);
            }
            catch (Exception exc)
            {
                outp = 0;
            }

            return outp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //public void EvaluatePositions()
        //{
        //    long exitCode = 0;

        //    // Check if server can be queried
        //    if (DateTime.Now >= NextServerReleaseDateTime && Connections[AccountLong].LastKnownRateLimit > 5 && Connections[AccountShort].LastKnownRateLimit > 5)
        //    {
        //        // Create a variable used for locking.
        //        bool acquiredLock = false;
        //        long timeOutMS = 0;

        //        try
        //        {
        //            // ----------
        //            exitCode = 1;
        //            // ----------

        //            // Try enter the lock.
        //            Monitor.TryEnter(_Lock, 0, ref acquiredLock);

        //            // Test if entering the lock was successful.
        //            if (acquiredLock)
        //            {
        //                // ----------
        //                exitCode = 2;
        //                // ----------

        //                if (CurrentStatus == ZoneRecoveryStatus.Unwinding || CurrentStatus == ZoneRecoveryStatus.Winding)
        //                {
        //                    // ----------
        //                    exitCode = 3;
        //                    // ----------
        //                    List<OrderResponse> ServerOrders = new List<OrderResponse>();

        //                    // Cancel resting orders
        //                    if (ApplicationOrders != null)
        //                    {
        //                        // ----------
        //                        exitCode = 4;
        //                        // ----------
        //                        string l;
        //                        string s;

        //                        MessageBox.Show(ApplicationOrders.Select(p => ((OrderResponse)p.ServerResponseInitial).Account).First().ToString());
        //                        MessageBox.Show(ApplicationOrders.Count().ToString());

        //                        // Get the clOrdIds that are supposed to be live
        //                        if (ApplicationOrders.Where(p => ((OrderResponse)p.ServerResponseInitial).Account == AccountLong).Count() > 0)
        //                        {
        //                            l = string.Join(",", ApplicationOrders.Where(p => ((OrderResponse)p.ServerResponseInitial).Account == AccountLong).Select(p => p.ClOrdId));
        //                            var ol = Connections[AccountLong].GetOrdersForCSId(l);

        //                            // Add OrderResponses to ServerOrders
        //                            if (ol is List<OrderResponse>)
        //                                ServerOrders.AddRange((List<OrderResponse>)ol);
        //                            else
        //                                throw new Exception("Error: Cannot update OrderResponse Long");
        //                        }

        //                        // Get the clOrdIds that are supposed to be live
        //                        if (ApplicationOrders.Where(p => ((OrderResponse)p.ServerResponseInitial).Account == AccountShort).Count() > 0)
        //                        {
        //                            s = string.Join(",", ApplicationOrders.Where(p => ((OrderResponse)p.ServerResponseInitial).Account == AccountShort).Select(p => p.ClOrdId));
        //                            var os = Connections[AccountShort].GetOrdersForCSId(s);

        //                            // Add OrderResponses to ServerOrders
        //                            if (os is List<OrderResponse>)
        //                                ServerOrders.AddRange((List<OrderResponse>)os);
        //                            else
        //                                throw new Exception("Error: Cannot update OrderResponse Short");
        //                        }
        //                    }

        //                    if (ServerOrders.Count() == 0)
        //                    {
        //                        // ----------
        //                        exitCode = 5;
        //                        // ----------

        //                    }
        //                    else if (ServerOrders.Where(o => o.OrdStatus == "Filled").Count() > 0)
        //                    {
        //                        // ----------
        //                        exitCode = 6;
        //                        // ----------

        //                        // Synchronize the internal positions
        //                        SyncPositions();

        //                        timeOutMS = 5000;

        //                        // Match the orders on the server with the orders known in the application
        //                        foreach (ZoneRecoveryOrder ao in ApplicationOrders)
        //                        {
        //                            ao.ServerResponseCompare = ServerOrders.Where(n => n.ClOrdId == ao.ClOrdId).Single();
        //                        }

        //                        string REVStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.REV).Select(o => o.ServerResponseCompare.OrdStatus).Single();
        //                        string TPStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponseCompare.OrdStatus).Single();
        //                        string TLStatus = "N/A";
        //                        if (ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponseCompare.OrdStatus).Count() == 1)
        //                            TLStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponseCompare.OrdStatus).Single();

        //                        long TPAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponseCompare.Account).Single();
        //                        long TLAccount = Connections.Where(c => c.Key != TPAccount).Select(c => c.Key).First();

        //                        // Check all possible cases + response to the case
        //                        if (TPStatus == "New" && (TLStatus == "New" || TLStatus == "N/A") && REVStatus == "Filled")            // Reversed
        //                        {
        //                            // ----------
        //                            exitCode = 5;
        //                            // ----------

        //                            var cancelTP = Connections[TPAccount].CancelAllOrders(Symbol, "", "Reversed. Close all open orders.");
        //                            var cancelTL = Connections[TLAccount].CancelAllOrders(Symbol, "", "Reversed. Close all open orders.");

        //                            // Throw an exception if not all orders could have been canceled
        //                            if (!cancelTP || !cancelTL)
        //                                throw new Exception("Unable to cancel orders.");

        //                            // Go one step forward in the Calculator status.
        //                            TakeStepForward();

        //                            // Create App orders and send to server. TP is the old TL.
        //                            UpdateAppOrderAndSync(TLAccount);

        //                        }
        //                        else if (TPStatus == "Filled" && (TLStatus == "Filled" || TLStatus == "N/A") && REVStatus == "New")     // Profit taken
        //                        {
        //                            // ----------
        //                            exitCode = 6;
        //                            // ----------

        //                            // Try to cancel a previous order.
        //                            var cancelRev = Connections[TLAccount].CancelAllOrders(Symbol, "", "TP Reached. Close all open orders.");

        //                            // Throw an exception if not all orders could have been canceled
        //                            if (!cancelRev)
        //                                throw new Exception("Unable to cancel orders.");

        //                            // Reset the Calculator internal variables
        //                            InitializeCalculator();

        //                        }
        //                        else if (TPStatus == "Filled" && TLStatus == "New" && REVStatus == "New")
        //                        {
        //                            // ----------
        //                            exitCode = 7;
        //                            // ----------
        //                            // Profit side taken without loss side

        //                        }
        //                        else if (TPStatus == "New" && TLStatus == "Filled" && REVStatus == "New")
        //                        {
        //                            // ----------
        //                            exitCode = 8;
        //                            // ----------
        //                            // Loss side taken without profit side

        //                        }
        //                        else if (TPStatus == "Filled" && (TLStatus == "New" || TLStatus == "N/A") && REVStatus == "Filled")
        //                        {
        //                            // ----------
        //                            exitCode = 9;
        //                            // ----------
        //                            // Profit and reverse side taken without loss side

        //                        }
        //                        else if (TPStatus == "New" && TLStatus == "Filled" && REVStatus == "Filled")
        //                        {
        //                            // ----------
        //                            exitCode = 10;
        //                            // ----------
        //                            // Profit side take without loss side

        //                        }
        //                        else if (TPStatus == "Filled" && TLStatus == "Filled" && REVStatus == "Filled")
        //                        {
        //                            // ----------
        //                            exitCode = 11;
        //                            // ----------
        //                            // Profit side take without loss side

        //                        }
        //                    }
        //                }
        //                else if (CurrentStatus == ZoneRecoveryStatus.Init)    // No Application orders OR ZoneRecoveryStatus = Init
        //                {
        //                    // ----------
        //                    exitCode = 12;
        //                    // ----------

        //                    // Synchronize the internal positions
        //                    SyncPositions();

        //                    long errorCounter = 0;

        //                    if (Math.Abs(Positions[AccountShort].CurrentQty) == 0 && Math.Abs(Positions[AccountLong].CurrentQty) == 0)
        //                    {
        //                        // Do nothing
        //                        // ----------
        //                        exitCode = 13;
        //                        // ----------
        //                    }
        //                    else if (Math.Abs(Positions[AccountShort].CurrentQty) == 0 && Math.Abs(Positions[AccountLong].CurrentQty) > 0)
        //                    {
        //                        // ----------
        //                        exitCode = 14;
        //                        // ----------

        //                        // Set the initial unit size
        //                        UnitSize = Positions[AccountLong].CurrentQty;

        //                        // Create App orders and send to server
        //                        errorCounter = UpdateAppOrderAndSync(Positions[AccountLong].Account);

        //                        // check if Orders are placed with success
        //                        if (errorCounter == 0)
        //                            // Turn the wheel
        //                            TakeStepForward();
        //                        else
        //                            throw new Exception("Error: UpdateAppOrderAndSync(Positions[AccountLong].Account)");

        //                    }
        //                    else if (Math.Abs(Positions[AccountLong].CurrentQty) == 0 && Math.Abs(Positions[AccountShort].CurrentQty) > 0)
        //                    {
        //                        // ----------
        //                        exitCode = 15;
        //                        // ----------

        //                        // Set the initial unit size
        //                        UnitSize = Math.Abs(Positions[AccountShort].CurrentQty);

        //                        // Create App orders and send to server
        //                        errorCounter = UpdateAppOrderAndSync(Positions[AccountShort].Account);

        //                        // check if Orders are placed with success
        //                        if (errorCounter == 0)
        //                            // Turn the wheel
        //                            TakeStepForward();
        //                        else
        //                            throw new Exception("Error: UpdateAppOrderAndSync(Positions[AccountShort].Account)");

        //                    }
        //                    else
        //                    {
        //                        // ----------
        //                        exitCode = 16;
        //                        // ----------
        //                        // Cancel all open orders
        //                        //var closeLongOrders = Connections[AccountLong].CancelAllOrders(Symbol);
        //                        //var closeShortOrders = Connections[AccountShort].CancelAllOrders(Symbol);
        //                        //var closeLongPosition = Connections[AccountLong].ClosePosition(Symbol);
        //                        //var closeShortPosition = Connections[AccountShort].ClosePosition(Symbol);

        //                        ////TODO: handle whatever this returns

        //                        //// Throw an exception if not all orders could have been canceled
        //                        //if (!closeLongOrders || !closeShortOrders)
        //                        //    throw new Exception("Unable to cancel orders.");
        //                        //else
        //                        //    InitializeCalculator();

        //                        //TODO: Send mail
        //                    }
        //                }
        //                else if (CurrentStatus == ZoneRecoveryStatus.Alert)
        //                {
        //                    // ----------
        //                    exitCode = 17;
        //                    // ----------
        //                }
        //                else if (CurrentStatus == ZoneRecoveryStatus.Finish)
        //                {
        //                    // ----------
        //                    exitCode = 18;
        //                    // ----------
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            CurrentStatus = ZoneRecoveryStatus.Alert;
        //            MessageBox.Show("[" + exitCode.ToString() + "]:" + e.Message);
        //        }
        //        finally
        //        {
        //            // Ensure that the lock is released.
        //            if (acquiredLock)
        //            {
        //                // Reset server query timer
        //                DateTime dt = DateTime.Now;
        //                NextServerReleaseDateTime = dt.AddMilliseconds((timeOutMS > 0) ? timeOutMS : StandardTimeOut);

        //                // Exit the lock
        //                Monitor.Exit(_Lock);
        //            }
        //        }
        //    }

        //    return exitCode;
        //}
    }
}
