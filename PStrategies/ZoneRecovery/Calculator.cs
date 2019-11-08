namespace PStrategies.ZoneRecovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BitMEX.Model;
    using BitMEX.Client;

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
        /// The account used for LONG transactions.
        /// </summary>
        private long AccountLong;

        /// <summary>
        /// The account used for SHORT transactions.
        /// </summary>
        private long AccountShort;

        /// <summary>
        /// The last known long position.
        /// </summary>
        private PositionResponse LongPosition;

        /// <summary>
        /// The last known short position.
        /// </summary>
        private PositionResponse ShortPosition;
        
        /// <summary>
        /// The orders that are known in the application and are suposed to be resting on the server.
        /// </summary>
        private List<ZoneRecoveryOrder> ApplicationOrders;

        /// <summary>
        /// An enumeration used for expressing in which part of the Zone Recovery algorithm the class is at a given moment.
        /// </summary>
        private enum ZoneRecoveryStatus { Winding, Unwinding, Init }

        /// <summary>
        /// At any time, this measure reflects the maximum percentage of the TotalBalance that can be exposed to 
        /// the market. 
        /// </summary>
        private double MaxExposurePerc { get; }

        /// <summary>
        /// The total balance of the wallet at the time that the Calculator class is initialized.
        /// </summary>
        private double TotalBalance { get; }

        /// <summary>
        /// The current leverage used for calculating other measures
        /// </summary>
        private double Leverage { get; }

        /// <summary>
        /// The minimum pip size of the exchange, used for rounding prices.
        /// </summary>
        private double PipSize { get; }

        /// <summary>
        /// The size of the zone expressed in number of pips. ZoneSize * PipSize = Real Zone Size
        /// </summary>
        private int ZoneSize { get; }

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
        /// Keeps track of the previously sent instructions.
        /// </summary>
        private List<ZoneRecoveryAction> LastInstructionList = new List<ZoneRecoveryAction>();

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
        /// <param name="pipSize">The minimum pip size possible on the exchange</param>
        /// <param name="maxDepthIndex">Maximum dept allowed in the Zone Recovery system</param>
        /// <param name="zoneSize">The size of the zone in nr of pips</param>
        /// <param name="minPrftPerc">Minimum required profit margin</param>
        /// <param name="connLong">A MordoR connection configured for the Long account</param>
        /// <param name="connShort">A MordoR connection configured for the Short account</param>
        public Calculator(string symbol, double maxExposurePerc, double totalBalance, double leverage, double pipSize, int maxDepthIndex, int zoneSize, double minPrftPerc
                          , MordoR connA, MordoR connB)
        {
            // Initialize main variables
            Symbol = symbol;
            Connections = new Dictionary<long, MordoR>();
            AccountLong = connA.Account;
            AccountShort = connB.Account;
            Connections.Add(connA.Account, connA);
            Connections.Add(connB.Account, connB);
            MaxExposurePerc = maxExposurePerc;
            TotalBalance = totalBalance;
            Leverage = leverage;
            PipSize = pipSize;
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
            CurrentStatus = ZoneRecoveryStatus.Init;
            SyncPositions();
            CurrentZRPosition = 0;
            UnitSize = 0;
            ApplicationOrders = new List<ZoneRecoveryOrder>();
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
        /// Output the rounded input for the current pipsize
        /// </summary>
        /// <param name="amount">The amount to be rounded</param>
        /// <returns></returns>
        private double RoundToPipsize(double amount)
        {
            return Math.Round(amount, MidpointRounding.AwayFromZero) / (1 / PipSize);
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
            if(LongPosition.IsOpen || ShortPosition.IsOpen)
            {
                double v_numerator = (LongPosition.CurrentQty * LongPosition.MarkPrice) + (ShortPosition.CurrentQty * ShortPosition.MarkPrice);
                double v_denominator = LongPosition.CurrentQty + ShortPosition.CurrentQty;
                double bePrice = 0.0;
                
                bePrice = v_numerator / v_denominator;

                return RoundToPipsize(bePrice);
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Calculate the total exposure of the open positions. This is the sum of all absolute quantities of all open positions.
        /// </summary>
        /// <returns>The total exposure </returns>
        private long CalculateTotalOpenExposure()
        {
            if (LongPosition.IsOpen || ShortPosition.IsOpen)
            {
                return LongPosition.CurrentQty + Math.Abs(ShortPosition.CurrentQty);
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Calculates the profit price for the currently open positions. When all open positions are closed at this price,
        /// a minimum theoretical profit has been made as defined in the MinimumProfitPercentage parameter.
        /// </summary>
        /// <returns>The profit price</returns>
        private double CalculateTakeProfitPrice()
        {
            double breakEvenPrice = this.CalculateBreakEvenPrice();
            double direction = -this.GetNextDirection();
            double totalExposure = this.CalculateTotalOpenExposure();

            if (breakEvenPrice > 0 && direction != 0 && totalExposure != 0)
            {
                return breakEvenPrice + (direction * (totalExposure * MinimumProfitPercentage) );
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Returns the direction of the next trade in this instance of the Zone Recovery strategy.
        /// </summary>
        /// <returns>1 = LONG, -1 = SHORT, 0 = UNDEFINED</returns>
        private int GetNextDirection()
        {
            if (LongPosition.IsOpen || ShortPosition.IsOpen)
            {
                if (Math.Abs(LongPosition.CurrentQty) > Math.Abs(ShortPosition.CurrentQty))
                    return -1;
                else
                    return 1;
            }
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
            if(GetNextDirection() == 0)
                return 0;

            switch (ot)
            {
                case ZoneRecoveryOrderType.TP:
                    if (GetNextDirection() == 1)
                        return Math.Abs(ShortPosition.CurrentQty);
                    else
                        return -LongPosition.CurrentQty;
                case ZoneRecoveryOrderType.TL:
                    if (GetNextDirection() == 1)
                        return -LongPosition.CurrentQty; 
                    else
                        return Math.Abs(ShortPosition.CurrentQty);
                case ZoneRecoveryOrderType.REV:
                    return GetNextDirection() * UnitSize * FactorArray[CurrentZRPosition];
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculate the next price at which a reverse order should be placed.
        /// </summary>
        /// <returns>The next reverse price</returns>
        private double CalculateNextReversePrice()
        {
            // TODO Test the equalized Reverse price for optimal risk
            if (GetNextDirection() == 1)
            {
                return RoundToPipsize(((LongPosition.MarkPrice + (ZoneSize * GetNextDirection())) * (ShortPosition.CurrentQty + (FactorArray[CurrentZRPosition] * GetNextDirection())) - (ShortPosition.MarkPrice * ShortPosition.CurrentQty)) / FactorArray[CurrentZRPosition]);
            }
            else if (GetNextDirection() == -1)
            {
                return RoundToPipsize(((ShortPosition.MarkPrice + (ZoneSize * GetNextDirection())) * (LongPosition.CurrentQty + (FactorArray[CurrentZRPosition] * GetNextDirection())) - (LongPosition.MarkPrice * LongPosition.CurrentQty)) / FactorArray[CurrentZRPosition]);                
            }
            else
                return 0;

            //if (LongPosition.IsOpen && ShortPosition.IsOpen)
            //    return RoundToPipsize((GetNextDirection()== 1) ? LongPosition.MarkPrice : ShortPosition.MarkPrice);
            //else if (LongPosition.IsOpen)
            //    return RoundToPipsize(LongPosition.MarkPrice - ZoneSize);
            //else if (ShortPosition.IsOpen)
            //    return RoundToPipsize(ShortPosition.MarkPrice + ZoneSize);
            //else
            //    return 0;
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
                LongPosition = lp[1];
                List<PositionResponse> sp = (List<PositionResponse>)Connections[AccountShort].GetPositionsForSymbols(new string[] { Symbol });
                ShortPosition = sp[1];
            }
            catch (Exception e)
            {
                inSync = false;
            }
            return inSync;
        }

        /// <summary>
        /// Update the internal ApplicationOrders list and send the ApplicationOrders that have been created to the server.
        /// </summary>
        /// <param name="accNewTP">The Account number of the connection used to create the next TP order.</param>
        /// <param name="accNewREV">The Account number of the connection used to create the next REV and optionally TL order.</param>
        private void UpdateAppOrderAndSync(long accNewTP, long accNewREV)
        {
            // Create new Application Orders
            ApplicationOrders = new List<ZoneRecoveryOrder>();

            // Check if orders still need to be placed.
            if (CurrentZRPosition > 0)
            {
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewTP,
                    CalculateTakeProfitPrice(),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.TP),
                    ZoneRecoveryOrderType.TP));
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewREV,
                    CalculateTakeProfitPrice(),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.TL),
                    ZoneRecoveryOrderType.TL));
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewREV,
                    CalculateNextReversePrice(),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.REV),
                    ZoneRecoveryOrderType.REV));
            }
            else
            {
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewTP,
                    CalculateTakeProfitPrice(),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.TP),
                    ZoneRecoveryOrderType.TP));
                ApplicationOrders.Add(new ZoneRecoveryOrder(
                    MordoR.GenerateGUID(),
                    Symbol,
                    accNewREV,
                    CalculateNextReversePrice(),
                    CalculateQtyForOrderType(ZoneRecoveryOrderType.REV),
                    ZoneRecoveryOrderType.REV));
            }

            // Send the orders to the server
            foreach (ZoneRecoveryOrder zorro in ApplicationOrders)
            {
                var resp = (OrderResponse)zorro.SendOrderToServer(Connections[zorro.Account]);
                // TODO Double check if the orders are live
            }
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// Evaluates the current situation on the server. When something changed compared with the known situation in 
        /// the application, the appropriate action is taken.
        /// </summary>
        /// <returns>TODO: Let it return information needed to draw an action on the plot of NinjaTrader.</returns>
        public bool Evaluate()
        {
            // Check if server can be queried
            if (DateTime.Now >= NextServerReleaseDateTime)
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

                        if (ApplicationOrders.Count > 0 && CurrentStatus != ZoneRecoveryStatus.Init)
                        {
                            // Try getting the order status on the server...
                            try
                            {

                                List<OrderResponse> ServerOrders = new List<OrderResponse>();

                                var l = string.Join(",", ApplicationOrders.Where(p => p.Account == AccountLong).Select(p => p.ClOrdId));
                                var s = string.Join(",", ApplicationOrders.Where(p => p.Account == AccountShort).Select(p => p.ClOrdId));

                                ServerOrders.AddRange((List<OrderResponse>)Connections[AccountLong].GetOrdersForCSId(l));
                                ServerOrders.AddRange((List<OrderResponse>)Connections[AccountShort].GetOrdersForCSId(s));

                                if (ServerOrders.Where(o => o.OrdStatus == "Filled").Count() > 0)
                                {
                                    timeOutMS = 5000;

                                    // Synchronize the internal positions
                                    SyncPositions();

                                    // Match the orders on the server with the orders known in the application
                                    foreach (ZoneRecoveryOrder ao in ApplicationOrders)
                                    {
                                        ao.ServerResponse = ServerOrders.Where(n => n.ClOrdId == ao.ClOrdId).Single();
                                    }

                                    string REVStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.REV).Select(o => o.ServerResponse.OrdStatus).Single();
                                    string TPStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponse.OrdStatus).Single();
                                    string TLStatus;
                                    if (ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponse.OrdStatus).Count() == 1)
                                        TLStatus = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponse.OrdStatus).Single();
                                    else
                                        TLStatus = "N/A";

                                    long REVAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.REV).Select(o => o.ServerResponse.Account).Single();
                                    long TPAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponse.Account).Single();
                                    long TLAccount;
                                    if (ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponse.OrdStatus).Count() == 1)
                                        TLAccount = ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponse.Account).Single();
                                    else
                                        TLAccount = 0;

                                    // Check all possible cases + response to the case
                                    if (TPStatus == "New" && (TLStatus == "New" || TLStatus == "N/A") && REVStatus == "Filled")            // Reversed
                                    {
                                        List<ZoneRecoveryOrder> cancelTP;
                                        List<ZoneRecoveryOrder> cancelTL;

                                        // Close the remaining TP order.
                                        cancelTP = (List<ZoneRecoveryOrder>)Connections[TPAccount].CancelOrders(new string[] {
                                            ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TP).Select(o => o.ServerResponse.ClOrdId).Single() });

                                        // Close the remaining TL order.
                                        if (TLStatus != "N/A")
                                            cancelTL = (List<ZoneRecoveryOrder>)Connections[TLAccount].CancelOrders(new string[] {
                                                ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.TL).Select(o => o.ServerResponse.ClOrdId).Single() });

                                        // TODO check if all orders are closed

                                        // Go one step forward in the Calculator status.
                                        TakeStepForward();

                                        // Create App orders and send to server
                                        UpdateAppOrderAndSync(REVAccount, TPAccount);
                                        
                                    }
                                    else if (TPStatus == "Filled" && (TLStatus == "Filled" || TLStatus == "N/A") && REVStatus == "New")    // TP and TL
                                    {
                                        List<ZoneRecoveryOrder> cancelREV;

                                        // Close the remaining REV order.
                                        cancelREV = (List<ZoneRecoveryOrder>)Connections[REVAccount].CancelOrders(new string[] {
                                            ApplicationOrders.Where(o => o.OrderType == ZoneRecoveryOrderType.REV).Select(o => o.ServerResponse.ClOrdId).Single() });

                                        // Reset the Calculator internal variables
                                        InitializeCalculator();
                                    }
                                    else if (TPStatus == "Filled" && TLStatus == "New" && REVStatus == "New")       // Profit side taken without loss side
                                    {


                                    }
                                    else if (TPStatus == "New" && TLStatus == "Filled" && REVStatus == "New")       // Loss side taken without profit side
                                    {


                                    }
                                    else if (TPStatus == "Filled" && (TLStatus == "New" || TLStatus == "N/A") && REVStatus == "Filled")    // Profit and reverse side taken without loss side
                                    {


                                    }
                                    else if (TPStatus == "New" && TLStatus == "Filled" && REVStatus == "Filled")    // Profit side take without loss side
                                    {


                                    }
                                    else if (TPStatus == "Filled" && TLStatus == "Filled" && REVStatus == "Filled") // Profit side take without loss side
                                    {


                                    }

                                }

                            }
                            catch (Exception e)
                            {

                            }
                        }
                        else    // No Application orders
                        {
                            try
                            {
                                // Synchronize the internal positions
                                SyncPositions();

                                if (ShortPosition.CurrentQty == 0 && LongPosition.CurrentQty == 0)
                                {
                                    // Do nothing
                                }
                                else if (ShortPosition.CurrentQty == 0 && LongPosition.CurrentQty > 0)
                                {
                                    // Set the initial unit size
                                    UnitSize = LongPosition.CurrentQty;
                                    
                                    // Create App orders and send to server
                                    UpdateAppOrderAndSync(LongPosition.Account, ShortPosition.Account);

                                    // Turn the wheel
                                    TakeStepForward();
                                }
                                else if(LongPosition.CurrentQty == 0 && ShortPosition.CurrentQty < 0)
                                {
                                    // Set the initial unit size
                                    UnitSize = Math.Abs(ShortPosition.CurrentQty);

                                    // Create App orders and send to server
                                    UpdateAppOrderAndSync(ShortPosition.Account, LongPosition.Account);

                                    // Turn the wheel
                                    TakeStepForward();
                                }
                                else
                                {
                                    // TODO Close all open positions in the most cost saving way...
                                }
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
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
                return true;
            }
            else return false;
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
                double totalDepthMaxExposure = (double)this.GetMaximumDepth();
                return (long)Math.Round(refPrice * Leverage * TotalBalance * MaxExposurePerc / totalDepthMaxExposure, MidpointRounding.ToEven);
            }
            else
                return 0;
        }
        
        #endregion Public methods
    }
}

/*
 * BreakEvenPrice
 * i    A[i]    V[i]   A[i]*V[i]
 * --------------------------------
 * 1    1.13    -1000    -1130
 * 2    1.125    2000     2250
 * 3    1.129   -3000    -3387   +
 * --------------------------------
 * SOM:         -2000    -2267
 * BREAK_EVEN_PRICE = SOM(A[i]*V[i]) / SOM(V[i]) = -2267 / -2000 = 1.1335

private static double CalculateBEP(double[] orders, double[] vol, int tradeindex, double dir) {
	// BEP = Sum(Vol R P) / Sum(Vol R)
	double v_numerator 		= 0.0;
	double v_denominator 	= 0.0;
	double o 				= 0.0;
	double cDir 				= dir;
			
	for (int i = tradeindex-1; i >= 0; i--) {
		v_numerator = Math.Round(v_numerator + (vol[i] * orders[i] * cDir), 5);
		v_denominator = Math.Round(v_denominator + (vol[i] * cDir), 5);
		// Change direction for next calculation
		cDir = (cDir < 0)? 1 : -1;
	}
			
	o = v_numerator / v_denominator;
			
	return Math.Round(o, 4);
}

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
