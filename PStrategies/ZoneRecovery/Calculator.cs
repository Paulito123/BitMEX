using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitMEX.Model;
using BitMEX.Client;

namespace PStrategies.ZoneRecovery
{
    public class Calculator
    {
        #region Private variables
        
        /// <summary>
        /// An array of factors used to calculate the size of each winding.
        /// </summary>
        private static double[] FactorArray = new double[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        
        /// <summary>
        /// The minimum unit size of each order. The result of the UnitSize multiplied by a value of the FactorArray 
        /// results in the quantity of a respective order.
        /// </summary>
        private double UnitSize;

        /// <summary>
        /// Reflects how deep inside the FactorArray, this instance of the Calculator class, is allowed to go.
        /// </summary>
        private int MaxDepthIndex { get; }

        /// <summary>
        /// The account number of the account used for LONG positions.
        /// </summary>
        private long AccountLong;

        /// <summary>
        /// The account number of the account used for SHORT positions.
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
        /// 
        /// </summary>
        /// <param name="maxExposure">Percentage of the maximum market exposure when completely wound</param>
        /// <param name="totalBalance">Most recent total balance</param>
        /// <param name="leverage">Leverage used to calculate other parameters</param>
        /// <param name="pipSize">The minimum pip size possible on the exchange</param>
        /// <param name="maxDepthIndex">Maximum dept allowed in the Zone Recovery system</param>
        /// <param name="zoneSize">The size of the zone in nr of pips</param>
        /// <param name="minPrftPerc">Minimum required profit margin</param>
        public Calculator(double maxExposurePerc, double totalBalance, double leverage, double pipSize, int maxDepthIndex, int zoneSize, double minPrftPerc
                          , PositionResponse longPos, PositionResponse shortPos, long accountLong, long accountShort)
        {
            // Initialize main variables
            AccountLong = accountLong;
            AccountShort = accountShort;
            MaxExposurePerc = maxExposurePerc;
            TotalBalance = totalBalance;
            Leverage = leverage;
            PipSize = pipSize;
            MaxDepthIndex = maxDepthIndex;
            ZoneSize = zoneSize;
            MinimumProfitPercentage = minPrftPerc;
            LongPosition = longPos;
            ShortPosition = shortPos;

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
            //OpenOrders = new List<OrderResponse>();
            CurrentZRPosition = 0;
            //TradeIndex = 0;
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
        private void TakeStepForward(List<PositionResponse> pos = null)
        {
            if (CurrentStatus == ZoneRecoveryStatus.Winding)
            {
                CurrentZRPosition++;

                // Set the new value for the internal positions
                if(pos != null)
                    SetPositions(pos);

                // Zone Recovery logic is reversed
                if (CurrentZRPosition == MaxDepthIndex)
                    CurrentStatus = ZoneRecoveryStatus.Unwinding;
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
            {
                CurrentZRPosition--;

                // Set the new value for the internal positions
                if (pos != null)
                    SetPositions(pos);

                // Reset the calculator
                if (CurrentZRPosition == 0)
                    InitializeCalculator();
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Init)
            {
                CurrentStatus = ZoneRecoveryStatus.Winding;
                CurrentZRPosition = 1;
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
                return LongPosition.CurrentQty + ShortPosition.CurrentQty;
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
                if (LongPosition.CurrentQty > ShortPosition.CurrentQty)
                    return -1;
                else
                    return 1;
            }
            else
                return 0;
        }
        
        /// <summary>
        /// Calculates the total Qty of all the open positions for a given direction
        /// </summary>
        /// <param name="direction">The direction for which the total Qty needs to be summed</param>
        /// <returns></returns>
        private long CalculateOpenQtyForDirection(int direction)
        {
            if (direction == 1)
                return LongPosition.CurrentQty;
            else if (direction == -1)
                return ShortPosition.CurrentQty;
            else
                return 0;
        }

        /// <summary>
        /// Calculate the next price at which a reverse order should be placed.
        /// </summary>
        /// <returns>The next reverse price</returns>
        private double CalculateNextReversePrice()
        {
            if (LongPosition.IsOpen && ShortPosition.IsOpen)
                return RoundToPipsize((GetNextDirection()== 1) ? LongPosition.MarkPrice : ShortPosition.MarkPrice);
            else if (LongPosition.IsOpen)
                return RoundToPipsize(LongPosition.MarkPrice - ZoneSize);
            else if (ShortPosition.IsOpen)
                return RoundToPipsize(ShortPosition.MarkPrice + ZoneSize);
            else
                return 0;
        }

        /// <summary>
        /// Set the internal position from a list of positions. 
        /// Make sure every account in the list is unique. It will return the first occurrance.
        /// </summary>
        /// <param name="pos">A list of position responses.</param>
        private void SetPositions(List<PositionResponse> pos)
        {
            if(pos.First(p => p.Account == AccountLong) != null)
                this.LongPosition = pos.First(p => p.Account == AccountLong);

            if (pos.First(p => p.Account == AccountShort) != null)
                this.ShortPosition = pos.First(p => p.Account == AccountShort);
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// For a given reference price, Balance, MaxExposurePerc and Leverage, this function returns the calculated maximum unit size. 
        /// This unit size multiplied with the factors in FactorArray will give the unit size of each order.
        /// In other words, the value returned is the Volume by unit in the FactorArray.
        /// </summary>
        /// <param name="refPrice">reference price used for unit size calculation</param>
        /// <returns>The absolute minimum unit size for the given parameters</returns>
        public double GetUnitSizeForPrice(double refPrice)
        {
            if (refPrice > 0)
            {
                double totalDepthMaxExposure = (double)this.GetMaximumDepth();
                return Math.Round(refPrice * Leverage * TotalBalance * MaxExposurePerc / totalDepthMaxExposure, MidpointRounding.ToEven);
            }
            else
                return 0;
        }

        /// <summary>
        /// This method initiates the calculator with an initial position. Therefore it can 
        /// only be used when CurrentStatus == ZoneRecoveryStatus.Init.
        /// </summary>
        /// <param name="startPosition">The first position taken for this instance of the calculator.</param>
        /// <returns></returns>
        public List<ZoneRecoveryAction> EvaluatePosition(PositionResponse startPosition)
        {
            // Create a variable used for locking.
            bool acquiredLock = false;

            // Test if prerequisites have been met.
            if (CurrentStatus == ZoneRecoveryStatus.Init && 
                startPosition.CurrentQty != 0 &&
                CurrentZRPosition == 0)
            {
                try
                {
                    // Try enter the lock.
                    Monitor.TryEnter(_Lock, 0, ref acquiredLock);

                    // Test if entering the lock was successful.
                    if (acquiredLock)
                    {
                        // Set the unit size to the size of the very first position taken.
                        UnitSize = Math.Abs(startPosition.CurrentQty);
                        
                        // Initialize internal positions.
                        var pos = new List<PositionResponse>();
                        pos.Add(startPosition);
                        SetPositions(pos);

                        // Turn the wheel of internal calculation variables
                        this.TakeStepForward();
                    }
                }
                finally
                {
                    // Ensure that the lock is released.
                    if (acquiredLock)
                    {
                        // Exit the lock
                        Monitor.Exit(_Lock);
                    }
                }
            }

            if (acquiredLock)
                return this.GetNextAction();
            else
                return null;
        }

        /// <summary>
        /// Calculates the parameters for the next orders, TP & Reverse > InitialPrice & Volume and returns it as a ZoneRecoveryAction.
        /// Zone calculation strategy = "Stick with initial zone". This means when a new position diverts from the planned order price,
        /// the next order price is still set at a price calculated relative to the initial price. 
        /// Example for PipSize = 1 and ZoneSize = 50:
        ///     Buy     1   at  1000$
        ///     Sell    2   at   950$
        ///     Buy     3   at  1025$   > DISCREPANCY between planned vs actual price
        ///     
        /// According to the "plan", the Buy price here should have been 1000$ but due to extreme market volatility, the first price 
        /// at which the volume (3) could get filled was 1025$. Rather than average calculating the next reverse price this step 
        /// calculator will stick to the original plan and try to keep buying at 1000$ and keep selling at 950$.
        /// OpenPositions.Single(s => s.PositionIndex == 1).AVGPrice returns the initial reference price (= price of first position)
        /// REMARK: This function does not validate the current position. It assumes the next action based on 2 internal variables:
        /// 1. CurrentStatus
        /// 2. CurrentZRPosition
        /// </summary>
        /// <returns>ZoneRecoveryAction</returns>
        public List<ZoneRecoveryAction> GetNextAction()
        {
            // TODO: 
            // Close the irrelevant open orders
            // Create 3 actions:
            //  1. TP: Close current direction
            //  2. TL: Close previous direction
            //  3. REV: Add to current direction

            //LongPosition;
            //ShortPosition;
            //List<ZoneRecoveryAction> instructionList = new List<ZoneRecoveryAction>();
            //LastInstructionList

            // Determine next Action based on 
            //  CurrentStatus
            //  CurrentZRPosition


            if (CurrentStatus == ZoneRecoveryStatus.Winding)
            {
                if (CurrentZRPosition < MaxDepthIndex)
                {

                }


                ZoneRecoveryAction zraLong = new ZoneRecoveryAction(AccountLong, , );
                ZoneRecoveryAction zraShort = new ZoneRecoveryAction(AccountShort);

                // Calculate the next take profit price
                zra.TPPrice = CalculateTakeProfitPrice();
                zra.TPVolumeSell = CalculateOpenQtyForDirection(1);
                zra.TPVolumeBuy = CalculateOpenQtyForDirection(-1);

                // Calculate the next reverse price
                zra.ReverseVolume = UnitSize * FactorArray[OpenPositions.Count];
                zra.ReversePrice = CalculateNextReversePrice();
                return zra;
                
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Unwinding) // Unwinding
            {
                // Calculate the next take profit price
                zra.TPPrice = CalculateTakeProfitPrice();
                zra.TPVolumeSell = CalculateOpenQtyForDirection(1);
                zra.TPVolumeBuy = CalculateOpenQtyForDirection(-1);

                // Calculate the next reverse price
                zra.ReverseVolume = -OpenPositions.Single(s => s.PositionIndex == (CurrentZRPosition + 1)).TotalQty;
                zra.ReversePrice = CalculateNextReversePrice();
                return zra;
            }
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentPositions">A list with the positions of both accounts.</param>
        public List<ZoneRecoveryAction> EvaluatePosition(List<PositionResponse> currentPositions)
        {
            bool l = currentPositions.Single(p => p.Account == AccountLong).CurrentQty == LongPosition.CurrentQty;
            bool s = currentPositions.Single(p => p.Account == AccountShort).CurrentQty == ShortPosition.CurrentQty;

            // Check if the CurrentQty changed on the exchange compared to what is know in the application...
            if (!l || !s)
            {
                bool acquiredLock = false;
                try
                {
                    Monitor.TryEnter(_Lock, 0, ref acquiredLock);
                    if (acquiredLock)
                    {

                        // TODO: log result both positions

                        if (currentPositions.Single(p => p.Account == AccountLong).CurrentQty == 0 && 
                            currentPositions.Single(p => p.Account == AccountShort).CurrentQty == 0)
                        {
                            // TP was reached and both positions were closed.
                            // CLOSE all open orders that might be a result of this instance of the Calculator
                            // RESET this instance of the Calculator

                            // CurrentZRPosition
                        }
                        else if (CurrentStatus == ZoneRecoveryStatus.Winding)
                        {
                            
                            
                            // Turn the wheel of internal calculation variables
                            this.TakeStepForward(currentPositions);

                        }
                        else if (CurrentStatus == ZoneRecoveryStatus.Unwinding)
                        {

                            // Turn the wheel of internal calculation variables
                            this.TakeStepForward(currentPositions);
                        }
                        else
                        {

                        }
                    }
                }
                finally
                {
                    // Ensure that the lock is released.
                    if (acquiredLock)
                    {
                        // Exit the lock
                        Monitor.Exit(_Lock);
                    }
                }

                if (acquiredLock)
                    return this.GetNextAction();
                else
                    return null;
            }
            else
                return null;

        }

        
        //public bool FeedOrderResponse(OrderResponse orderResp)
        //{
        //    bool isPositionSet = false;
        //    bool acquiredLock = false;
            
        //    if (orderResp.OrdStatus.Equals("Filled"))
        //    {
        //        try
        //        {
        //            Monitor.TryEnter(_Lock, 0, ref acquiredLock);
        //            if (acquiredLock)
        //            {
        //                // The critical section.
        //                int elementID = OpenPositions.Count + 1;

        //                // Add the new position to the OpenPositions List
        //                OpenPositions.Add(new ZoneRecoveryPosition(orderResp.ClOrdId, orderResp.Account, this.PipSize, elementID, orderResp.AvgPx, orderResp.OrderQty));

        //                // Turn the wheel of internal calculation variables
        //                this.TakeStepForward();

        //                isPositionSet = true;

        //                // TODO: The controlling application should call GetNextAction() next.
        //                // IDEA: Let this function already return the next action.
        //            }
        //        }
        //        finally
        //        {
        //            // Ensure that the lock is released.
        //            if (acquiredLock)
        //            {
        //                // Exit the lock
        //                Monitor.Exit(_Lock);
        //            }
        //        }

        //    }

        //    // Return [true / false] when [something / nothing] happened
        //    return isPositionSet;
        //}
        
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
