﻿namespace PStrategies.ZoneRecovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using BitMEX.Model;
    using BitMEX.Client;

    /// <summary>
    /// An enumeration used for expressing in which part of the Zone Recovery algorithm the class is at a given moment.
    /// </summary>
    public enum ZoneRecoveryStatus { Winding, Unwinding, Init, Alert }

    // TESTLONG  [51091]    : "QbpGewiOyIYMbyQ-ieaTKfOJ" - "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf"
    // TESTSHORT [170591]   : "xEuMT-y7ffwxrvHA2yDwL1bZ" - "3l0AmJz7l3P47-gK__LwgZQQ23uOKCFhYJG4HeTLlGXadRm6"

    public class Calculator
    {
        #region Private variables

        /// <summary>
        /// The trading symbol for this instance of the Calculator.
        /// </summary>
        private string Symbol;
        
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
        /// The last timestamp the server was queried.
        /// </summary>
        private DateTime NextServerReleaseDateTime;

        /// <summary>
        /// Minimum number of miliseconds between 2 server queries
        /// </summary>
        private long StandardTimeOut { get; set; }

        /// <summary>
        /// Reflects how deep inside the FactorArray, this instance of the Calculator class, is allowed to go.
        /// </summary>
        private int MaxDepthIndex { get; }

        /// <summary>
        /// The list of available connections.
        /// </summary>
        private Dictionary<long, MordoR> Connections;

        /// <summary>
        /// List of Wallets.
        /// </summary>
        private Dictionary<long, WalletResponse> Wallets;

        /// <summary>
        /// List of last known positions by AccountNumber.
        /// </summary>
        private Dictionary<long, PositionResponse> Positions;

        /// <summary>
        /// The account used for LONG transactions.
        /// </summary>
        private long AccountLong;

        /// <summary>
        /// The account used for SHORT transactions.
        /// </summary>
        private long AccountShort;
        
        /// <summary>
        /// The orders that are known in the application and are suposed to be resting on the server.
        /// </summary>
        private List<ZoneRecoveryOrder> ApplicationOrders;
        
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
        private int ZoneSize;

        /// <summary>
        /// When the mathematical break even price has been reached, this percentage defines how far in profit
        /// the strategy needs to be before closing all related positions. 
        /// Should be a decimal between 0 and 1!!!
        /// </summary>
        private double MinimumProfitPercentage;

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

        #endregion Private variables

        #region Constructor(s)

        /// <summary>
        /// Initializes the Zone Recovery Calculator.
        /// </summary>
        /// <param name="maxExposure">Percentage of the maximum market exposure when completely wound</param>
        /// <param name="totalBalance">Most recent total balance</param>
        /// <param name="leverage">Leverage used to calculate other parameters</param>
        /// <param name="maxDepthIndex">Maximum dept allowed in the Zone Recovery system</param>
        /// <param name="zoneSize">The size of the zone in nr of pips</param>
        /// <param name="minPrftPerc">Minimum required profit margin</param>
        /// <param name="connLong">A MordoR connection configured for the Long account</param>
        /// <param name="connShort">A MordoR connection configured for the Short account</param>
        public Calculator(string symbol, double maxExposurePerc, double leverage, int maxDepthIndex, int zoneSize, double minPrftPerc
                          , MordoR connA, MordoR connB)
        {
            // Initialize main variables
            Symbol = symbol;
            AccountLong = connA.Account;
            AccountShort = connB.Account;
            Connections = new Dictionary<long, MordoR>();
            Connections.Add(connA.Account, connA);
            Connections.Add(connB.Account, connB);
            Wallets = new Dictionary<long, WalletResponse>();
            Wallets.Add(connA.Account, (WalletResponse)connA.GetWalletInfo());
            Wallets.Add(connB.Account, (WalletResponse)connB.GetWalletInfo());
            Positions = new Dictionary<long, PositionResponse>();
            Positions.Add(connA.Account, null);
            Positions.Add(connB.Account, null);
            MaxExposurePerc = maxExposurePerc;
            Leverage = leverage;
            MaxDepthIndex = maxDepthIndex;
            ZoneSize = zoneSize;
            MinimumProfitPercentage = minPrftPerc;
            StandardTimeOut = 1500;

            // Initialize calculation variables
            InitializeCalculator();
        }
        
        #endregion Constructor(s)

        #region Private methods

        /// <summary>
        /// Initializes the variables used for the calculation
        /// </summary>
        private void InitializeCalculator()
        {
            ApplicationOrders = new List<ZoneRecoveryOrder>();
            CurrentStatus = ZoneRecoveryStatus.Init;
            SyncPositions();
            CurrentZRPosition = 0;
            UnitSize = 0;
        }

        /// <summary>
        /// Calculates the total maximum depth (and exposure) possible, according to the depths defines in FactorArray. 
        /// Basically this is the sum of all the factors defined in FactorArray until the defined MaxDepth. It 
        /// represents the maximum number of units possible for the given parameters.
        /// Example: 
        ///     FactorArray = new double[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        ///     MaxDepth = 4
        ///     GetMaximumDepth() returns 12 (1 + 2 + 3 + 6) 
        /// </summary>
        private double GetMaximumDepth()
        {
            double sum = 0;

            for (int i = 0; i < MaxDepthIndex; i++)
            {
                sum = sum + FactorArray[i];
            }
            return sum;
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
                    CurrentStatus = ZoneRecoveryStatus.Init;
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Init)
            {
                CurrentStatus = ZoneRecoveryStatus.Winding;
                CurrentZRPosition++;
            }
        }

        /// <summary>
        /// Calculate the theoretical price at which closing all open positions results in a break even. 
        /// Returns 0 when there are no open positions.
        /// </summary>
        /// <returns>The theoretical break even price</returns>
        private double CalculateBreakEvenPrice()
        {
            if (Positions[AccountLong].IsOpen && Positions[AccountShort].IsOpen)
            {
                // Calculate and return the break even price when there is a position in both directions.
                double v_numerator = (Positions[AccountLong].CurrentQty * Positions[AccountLong].AvgEntryPrice) + (Positions[AccountShort].CurrentQty * Positions[AccountShort].AvgEntryPrice);
                double v_denominator = Positions[AccountLong].CurrentQty + Positions[AccountShort].CurrentQty;
                double bePrice = 0.0;

                bePrice = v_numerator / v_denominator;

                return Math.Round(bePrice);
            }
            else if (Positions[AccountLong].IsOpen || Positions[AccountShort].IsOpen)
            {
                // Return the MarkPrice as Break even price.
                if (Positions[AccountLong].IsOpen)
                    return Math.Round(double.Parse(Positions[AccountLong].AvgEntryPrice.ToString()));
                else
                    return Math.Round(double.Parse(Positions[AccountShort].AvgEntryPrice.ToString()));
            }
            else
                // Default return value.
                return 0.0;
        }

        /// <summary>
        /// Calculate the total exposure of the open positions. This is the sum of all absolute quantities of all open positions.
        /// </summary>
        /// <returns>The total exposure </returns>
        private long CalculateTotalOpenExposure()
        {
            if (Positions[AccountLong].IsOpen || Positions[AccountShort].IsOpen)
            {
                return Positions[AccountLong].CurrentQty + Math.Abs(Positions[AccountShort].CurrentQty);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the direction of the next trade in this instance of the Zone Recovery strategy.
        /// </summary>
        /// <returns>1 = LONG, -1 = SHORT, 0 = UNDEFINED</returns>
        private int GetNextDirection()
        {
            if (Positions[AccountLong].IsOpen || Positions[AccountShort].IsOpen)
            {
                if (Math.Abs(Positions[AccountLong].CurrentQty) > Math.Abs(Positions[AccountShort].CurrentQty))
                    return -1;
                else
                    return 1;
            }
            else
                return 0;
        }

        /// <summary>
        /// Get the next ZRPosition.
        /// </summary>
        /// <returns>CurrentZRPosition +1 step</returns>
        private long GetNextZRPosition()
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

        /// <summary>
        /// Calculate the Qty for the next orders by ZoneRecoveryOrderType.
        /// </summary>
        /// <param name="ot">ZoneRecoveryOrderType</param>
        /// <returns>Qty by ZoneRecoveryOrderType.</returns>
        private long CalculateQtyForOrderType(ZoneRecoveryOrderType ot)
        {
            // No open position...
            if (GetNextDirection() == 0)
                return 0;

            switch (ot)
            {
                case ZoneRecoveryOrderType.TP:
                    if (GetNextDirection() == 1)
                        return Math.Abs(Positions[AccountShort].CurrentQty);
                    else
                        return -Positions[AccountLong].CurrentQty;
                case ZoneRecoveryOrderType.TL:
                    if (GetNextDirection() == 1)
                        return -Positions[AccountLong].CurrentQty; 
                    else
                        return Math.Abs(Positions[AccountShort].CurrentQty);
                case ZoneRecoveryOrderType.REV:
                    if (CurrentStatus == ZoneRecoveryStatus.Winding || CurrentStatus == ZoneRecoveryStatus.Init)
                        return GetNextDirection() * UnitSize * FactorArray[GetNextZRPosition()];
                    else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                        return GetNextDirection() * UnitSize * FactorArray[GetNextZRPosition()]; // TODO: Check if offset is needed for GetNextZRPosition() here
                    else
                        return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ot"></param>
        /// <returns></returns>
        private double CalculatePriceForOrderType(ZoneRecoveryOrderType ot)
        {
            // No open position...
            if (GetNextDirection() == 0)
                return 0;

            switch (ot)
            {
                case ZoneRecoveryOrderType.TL:
                case ZoneRecoveryOrderType.TP:
                    // TODO Fix Bug: Infinite price is returned
                    double breakEvenPrice = CalculateBreakEvenPrice();
                    double direction = -GetNextDirection();
                    double totalExposure = CalculateTotalOpenExposure();

                    return Math.Round(breakEvenPrice + (direction * (totalExposure * MinimumProfitPercentage)));

                case ZoneRecoveryOrderType.REV:

                    if (Positions[AccountLong].IsOpen && Positions[AccountShort].IsOpen)
                        return Math.Round((GetNextDirection() == 1) ? (double)Positions[AccountShort].AvgEntryPrice : (double)Positions[AccountLong].AvgEntryPrice);
                    else if (Positions[AccountLong].IsOpen)
                        return Math.Round((double)Positions[AccountLong].AvgEntryPrice - ZoneSize);
                    else if (Positions[AccountShort].IsOpen)
                        return Math.Round((double)Positions[AccountShort].AvgEntryPrice + ZoneSize);
                    else
                        return 0;

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Sync the positions known by the application with the positions know on the server.
        /// </summary>
        private bool SyncPositions()
        {
            bool inSync = true;
            
            try
            {
                List<PositionResponse> lp = (List<PositionResponse>)Connections[AccountLong].GetPositionsForSymbols(new string[] { Symbol });
                Positions[AccountLong] = lp[0];
                List<PositionResponse> sp = (List<PositionResponse>)Connections[AccountShort].GetPositionsForSymbols(new string[] { Symbol });
                Positions[AccountShort] = sp[0];
            }
            catch (Exception e)
            {
                CurrentStatus = ZoneRecoveryStatus.Alert;
                inSync = false;
            }
            return inSync;
        }

        /// <summary>
        /// Update the internal ApplicationOrders list and send the ApplicationOrders that have been created to the server.
        /// </summary>
        /// <param name="accNewTP">The Account number of the connection used to create the next TP order.</param>
        /// <param name="accNewREV">The Account number of the connection used to create the next REV and optionally TL order.</param>
        private long UpdateAppOrderAndSync(long accNewTP, long accNewREV)
        {
            // Create new Application Orders
            ApplicationOrders = new List<ZoneRecoveryOrder>();

            // Check if orders still need to be placed.
            if (CurrentStatus == ZoneRecoveryStatus.Winding || CurrentStatus == ZoneRecoveryStatus.Unwinding)
            {
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewTP,
                    CalculatePriceForOrderType(ZoneRecoveryOrderType.TP),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.TP),
                    ZoneRecoveryOrderType.TP));
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewREV,
                    CalculatePriceForOrderType(ZoneRecoveryOrderType.TL),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.TL),
                    ZoneRecoveryOrderType.TL));
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewREV,
                    CalculatePriceForOrderType(ZoneRecoveryOrderType.REV),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.REV),
                    ZoneRecoveryOrderType.REV));
            }
            else if(CurrentStatus == ZoneRecoveryStatus.Init)
            {
                ApplicationOrders.Add(new ZoneRecoveryOrder(MordoR.GenerateGUID(),Symbol,
                    accNewTP,
                    CalculatePriceForOrderType(ZoneRecoveryOrderType.TP),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.TP),
                    ZoneRecoveryOrderType.TP));
                ApplicationOrders.Add(new ZoneRecoveryOrder(MordoR.GenerateGUID(),Symbol,
                    accNewREV,
                    CalculatePriceForOrderType(ZoneRecoveryOrderType.REV),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.REV),
                    ZoneRecoveryOrderType.REV));
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Alert)
            {
                return 9;
            }

            // TODO: reorganize this mess!!!

            long errorCounter = 0;

            //Send the orders to the server
            foreach (ZoneRecoveryOrder zorro in ApplicationOrders)
            {
                var resp = zorro.SendOrderToServer(Connections[zorro.Account]);
                if (resp is BaseError)
                    errorCounter++;
            }

            // Undo all orders when one order failed.
            if (errorCounter > 0)
            {
                //Undo orders
                foreach (ZoneRecoveryOrder zorro in ApplicationOrders)
                {
                    if (zorro.ServerResponseInitial is OrderResponse)
                        zorro.UndoOrder(Connections[zorro.Account]);
                }
            }

            return errorCounter;
        }
        
        #endregion Private methods

        #region Public methods

        /// <summary>
        /// Evaluates the current situation on the server. When something changed compared with the known situation in 
        /// the application, the appropriate action is taken.
        /// </summary>
        /// <returns>TODO: Let it return information needed to draw an action on the plot of NinjaTrader.</returns>
        public void Evaluate()
        {
            //if (CurrentStatus == ZoneRecoveryStatus.Alert)
            //    return null;
            
            // Check if server can be queried
            if (DateTime.Now >= NextServerReleaseDateTime && Connections[AccountLong].LastKnownRateLimit > 15 && Connections[AccountShort].LastKnownRateLimit > 15)
            {
                // Create a variable used for locking.
                bool acquiredLock = false;
                long timeOutMS = 0;
                
                try
                {
                    // Try enter the lock.
                    Monitor.TryEnter(_Lock, 0, ref acquiredLock);

                    // Test if entering the lock was successful.
                    if (acquiredLock)
                    {

                        if (CurrentStatus == ZoneRecoveryStatus.Unwinding || CurrentStatus == ZoneRecoveryStatus.Winding)
                        {
                            
                            List<OrderResponse> ServerOrders = new List<OrderResponse>();

                            // Get the clOrdIds that are supposed to be live
                            var l = string.Join(",", ApplicationOrders.Where(p => p.Account == AccountLong).Select(p => p.ClOrdId));
                            var s = string.Join(",", ApplicationOrders.Where(p => p.Account == AccountShort).Select(p => p.ClOrdId));

                            // Check the status of these clOrdIds
                            var ol = Connections[AccountLong].GetOrdersForCSId(l);
                            var os = Connections[AccountShort].GetOrdersForCSId(s);

                            // Add OrderResponses to ServerOrders
                            if (ol is List<OrderResponse>)
                                ServerOrders.AddRange((List<OrderResponse>)ol);
                            else
                                throw new Exception("Error: OrderResponse Long not valid");

                            if(os is List<OrderResponse>)
                                ServerOrders.AddRange((List<OrderResponse>)os);
                            else
                                throw new Exception("Error: OrderResponse Short not valid");
                            
                            if (ServerOrders.Where(o => o.OrdStatus == "Filled").Count() > 0)
                            {
                                // Synchronize the internal positions
                                SyncPositions();

                                timeOutMS = 5000;
                                    
                                // Match the orders on the server with the orders known in the application
                                foreach (ZoneRecoveryOrder ao in ApplicationOrders)
                                {
                                    ao.ServerResponseCompare = ServerOrders.Where(n => n.ClOrdId == ao.ClOrdId).Single();
                                }

                                string REVStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.REV).Select(o => o.ServerResponseCompare.OrdStatus).Single();
                                string TPStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponseCompare.OrdStatus).Single();
                                string TLStatus = "N/A";
                                if (ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponseCompare.OrdStatus).Count() == 1)
                                    TLStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponseCompare.OrdStatus).Single();

                                long REVAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.REV).Select(o => o.ServerResponseCompare.Account).Single();
                                long TPAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponseCompare.Account).Single();
                                long TLAccount = 0;
                                if (ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponseCompare.OrdStatus).Count() == 1)
                                    TLAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponseCompare.Account).Single();

                                // Check all possible cases + response to the case
                                if (TPStatus == "New" && (TLStatus == "New" || TLStatus == "N/A") && REVStatus == "Filled")            // Reversed
                                {
                                    var cancelTP = Connections[TPAccount].CancelAllOrders(Symbol);
                                    var cancelTL = Connections[TLAccount].CancelAllOrders(Symbol);

                                    // Throw an exception if not all orders could have been canceled
                                    if (!cancelTP || !cancelTL)
                                        throw new Exception("Unable to cancel orders.");
                                        
                                    // Go one step forward in the Calculator status.
                                    TakeStepForward();

                                    // Create App orders and send to server
                                    UpdateAppOrderAndSync(REVAccount, TPAccount);

                                }
                                else if (TPStatus == "Filled" && (TLStatus == "Filled" || TLStatus == "N/A") && REVStatus == "New")
                                {
                                    // Try to cancel a previous order.
                                    var cancelRev = Connections[REVAccount].CancelAllOrders(Symbol, "", "TP Reached. Close all open orders.");

                                    // Throw an exception if not all orders could have been canceled
                                    if (!cancelRev)
                                        throw new Exception("Unable to cancel orders.");
                                    
                                    // Reset the Calculator internal variables
                                    InitializeCalculator();

                                }
                                else if (TPStatus == "Filled" && TLStatus == "New" && REVStatus == "New")       
                                {
                                    // Profit side taken without loss side

                                }
                                else if (TPStatus == "New" && TLStatus == "Filled" && REVStatus == "New")       
                                {
                                    // Loss side taken without profit side

                                }
                                else if (TPStatus == "Filled" && (TLStatus == "New" || TLStatus == "N/A") && REVStatus == "Filled")    
                                {
                                    // Profit and reverse side taken without loss side

                                }
                                else if (TPStatus == "New" && TLStatus == "Filled" && REVStatus == "Filled")    
                                {
                                    // Profit side take without loss side

                                }
                                else if (TPStatus == "Filled" && TLStatus == "Filled" && REVStatus == "Filled") 
                                {
                                    // Profit side take without loss side

                                }
                            }
                        }
                        else if (CurrentStatus == ZoneRecoveryStatus.Init)    // No Application orders OR ZoneRecoveryStatus = Init
                        {
                            // Synchronize the internal positions
                            SyncPositions();

                            long errorCounter = 0;

                            if (Math.Abs(Positions[AccountShort].CurrentQty) == 0 && Math.Abs(Positions[AccountLong].CurrentQty) == 0)
                            {
                                // Do nothing
                            }
                            else if (Math.Abs(Positions[AccountShort].CurrentQty) == 0 && Math.Abs(Positions[AccountLong].CurrentQty) > 0)
                            {
                                // Set the initial unit size
                                UnitSize = Positions[AccountLong].CurrentQty;
                                    
                                // Create App orders and send to server
                                errorCounter = UpdateAppOrderAndSync(Positions[AccountLong].Account, Positions[AccountShort].Account);

                                // check if Orders are placed with success
                                if (errorCounter == 0)
                                    // Turn the wheel
                                    TakeStepForward();

                            }
                            else if (Math.Abs(Positions[AccountLong].CurrentQty) == 0 && Math.Abs(Positions[AccountShort].CurrentQty) > 0)
                            {
                                // Set the initial unit size
                                UnitSize = Math.Abs(Positions[AccountShort].CurrentQty);
                                    
                                // Create App orders and send to server
                                errorCounter = UpdateAppOrderAndSync(Positions[AccountShort].Account, Positions[AccountLong].Account);

                                // check if Orders are placed with success
                                if (errorCounter == 0)
                                    // Turn the wheel
                                    TakeStepForward();

                            }
                            else
                            {
                                // Cancel all open orders
                                var l = Connections[AccountLong].CancelAllOrders(Symbol);
                                var s = Connections[AccountShort].CancelAllOrders(Symbol);
                                
                                /* TODO: Close all positions POST /order with execInst:"Close"
                                 * Close: 'Close' implies 'ReduceOnly'. A 'Close' order will cancel other active limit orders with the same side and symbol 
                                 * if the open quantity exceeds the current position. This is useful for stops: by canceling these orders, a 'Close' Stop 
                                 * is ensured to have the margin required to execute, and can only execute up to the full size of your position. If orderQty 
                                 * is not specified, a 'Close' order has an orderQty equal to your current position's size.
                                 */

                                // Throw an exception if not all orders could have been canceled
                                if (!l || !s)
                                    throw new Exception("Unable to cancel orders.");
                                else
                                    InitializeCalculator();

                                //TODO: Send mail
                            }
                        }
                    }
                }
                catch(Exception e)
                {
                    CurrentStatus = ZoneRecoveryStatus.Alert;
                    MessageBox.Show(e.Message);
                }
                finally
                {
                    // Ensure that the lock is released.
                    if (acquiredLock)
                    {
                        // Reset server query timer
                        DateTime dt = DateTime.Now;
                        NextServerReleaseDateTime = dt.AddMilliseconds((timeOutMS > 0) ? timeOutMS : StandardTimeOut);
                        
                        // Exit the lock
                        Monitor.Exit(_Lock);
                    }
                }
            }

            //return ApplicationOrders;
        }

        /// <summary>
        /// Get the position for a specific connection.
        /// </summary>
        /// <param name="conn">A MordoR connection</param>
        /// <returns>A PositionResponse for the connection</returns>
        public PositionResponse GetPositionForConnection(MordoR conn)
        {
            return Positions[conn.Account];
        }

        /// <summary>
        /// Get the wallet for a specific connection.
        /// </summary>
        /// <param name="conn">A MordoR connection</param>
        /// <returns>A WalletResponse for the connection</returns>
        public WalletResponse GetWalletInfoForConnection(MordoR conn)
        {
            return Wallets[conn.Account];
        }

        /// <summary>
        /// For a given reference price, Balance, MaxExposurePerc and Leverage, this function returns the calculated maximum unit size. 
        /// This unit size multiplied with the factors in FactorArray will give the unit size of each order.
        /// In other words, the value returned is the Volume by unit in the FactorArray.
        /// This function is particulary usefull in the "calling" application to determine the initial UnitSize for the first position.
        /// </summary>
        /// <param name="refPrice">reference price used for unit size calculation</param>
        /// <returns>The absolute minimum unit size for the given parameters</returns>
        public long GetUnitSizeForPrice(double refPrice)
        {
            if (refPrice > 0)
            {
                double totalBalance = (Wallets[AccountLong].Amount + Wallets[AccountShort].Amount) / 100000000.0;
                double totalDepthMaxExposure = (double)this.GetMaximumDepth();
                return (long)Math.Round(refPrice * Leverage * totalBalance * MaxExposurePerc / totalDepthMaxExposure, MidpointRounding.ToEven);
            }
            else
                return 0;
        }

        /// <summary>
        /// Return the status of the Calculator.
        /// </summary>
        /// <returns>ZoneRecoveryStatus CurrentStatus</returns>
        public ZoneRecoveryStatus GetStatus()
        {
            return CurrentStatus;
        }

        //public void SetUnitSize(long unitSize)
        //{
        //    UnitSize = unitSize;
        //}

        //public long SetUnitSizeForPrice(double? refPrice = null)
        //{
        //    if (refPrice is double)
        //        UnitSize = GetUnitSizeForPrice((double)refPrice);

        //    return UnitSize;
        //}

        #endregion Public methods
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
        /// TODO: Check how WebSocket returns updates on resting orders. This function makes the assumption 
        /// that a list of OrderResponses is returned.
        /// Turning the wheel, advance one step further in the ZR winding process...
        /// </summary>
   
orderResp.OrdStatus.Equals("New")
 */
