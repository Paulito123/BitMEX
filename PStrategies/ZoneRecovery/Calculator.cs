using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BitMEX.Model;

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
        /// Reflects how deep inside the FactorArray, this instance of the Calculator class, is allowed to go.
        /// </summary>
        private int MaxDepthIndex { get; }

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
        /// A list of ZoneRecoveryPositions that are taken in the lifecycle of this instance of the Calculator class.
        /// Idea: Use Stack<T> here
        /// </summary>
        private List<ZoneRecoveryPosition> OpenPositions;

        /// <summary>
        /// CurrentZRPosition reflects the position withing the Zone Recovery strategy.
        /// When 0 > Strategy has been initialized or is completely unwound. There should be no open positions.
        /// When 1 > First Winding / last Unwinding
        /// When CurrentZRPosition = MaxDepthIndex > ZoneRecoveryStatus is switched and winding process reversed
        /// </summary>
        private int CurrentZRPosition;

        /// <summary>
        /// The maximum unit size used for each trade within this instance of the Calculator class. 
        /// To calculate this measure: InitialPrice * Leverage * TotalBalance * MaxExposurePerc / totalDepthMaxExposure
        /// Basically it comes down to calculating the maximum amount of exposure you would want for one instance of 
        /// the Calculator class. The key parameter to adjust the risk is MaxExposurePerc. The volume of the first 
        /// position, represents the MaximumUnitSize.
        /// </summary>
        //private double MaximumUnitSize;
        
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
        public Calculator(double maxExposurePerc, double totalBalance, double leverage, double pipSize, int maxDepthIndex, int zoneSize, double minPrftPerc)
        {
            // Initialize main variables
            MaxExposurePerc = maxExposurePerc;
            TotalBalance = totalBalance;
            Leverage = leverage;
            PipSize = pipSize;
            MaxDepthIndex = maxDepthIndex;
            ZoneSize = zoneSize;
            MinimumProfitPercentage = minPrftPerc;

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
            OpenPositions = new List<ZoneRecoveryPosition>();
            CurrentZRPosition = 0;
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
            else if (CurrentStatus == ZoneRecoveryStatus.Unwinding) // Unwinding...
            {
                CurrentZRPosition--;

                // Reset the calculator
                if (CurrentZRPosition == 0)
                    InitializeCalculator();
            }
            else if (CurrentStatus == ZoneRecoveryStatus.Init) // Winding...
            {
                CurrentStatus = ZoneRecoveryStatus.Winding;
                CurrentZRPosition++;
            }
        }

        /// <summary>
        /// Return the actual unit size. (= volume of the first position)
        /// This only work with the assumptions: 
        ///   - that the first PositionIndex of the open positions = 1  !!
        ///   - that the first value in the FactorArray = 1             !!
        /// </summary>
        /// <returns></returns>
        private double GetUnitSize()
        {
            return OpenPositions.Single(s => s.PositionIndex == 1).TotalQty;
        }

        /// <summary>
        /// Check if a clOrdID already exists among the historic positions taken.
        /// </summary>
        /// <param name="ordID">The orderID to be checked in the open positions</param>
        /// <returns></returns>
        private bool CheckClOrdIdExists(string ordID)
        {
            if (OpenPositions.Where(s => s.OrderID.Equals(ordID)).Count() == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Calculate the theoretical price at which closing all open positions results in a break even. 
        /// Returns 0 when there are no open positions.
        /// </summary>
        /// <returns>The theoretical break even price</returns>
        private double CalculateBreakEvenPrice()
        {
            if(OpenPositions.Count > 0)
            {
                double v_numerator = 0.0;
                double v_denominator = 0.0;
                double bePrice = 0.0;

                // Calculate the break even price
                foreach (ZoneRecoveryPosition zp in OpenPositions)
                {
                    v_numerator = v_numerator + (zp.TotalQty * zp.AVGPrice);
                    v_denominator = v_denominator + (zp.TotalQty);
                }

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
        private double CalculateTotalOpenExposure()
        {
            if (OpenPositions.Count > 0)
            {
                double e = 0;

                // Sum absolute values of all open quantities
                foreach (ZoneRecoveryPosition zp in OpenPositions)
                {
                    e = e + Math.Abs(zp.TotalQty);
                }
                return e;
            }
            else
            {
                return 0.0;
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
            if (OpenPositions.Count > 0)
            {
                // Determine the direction of next step relative to the first position
                if (OpenPositions.Single(s => s.PositionIndex == 1).TotalQty > 0) // First position = LONG position
                    return (OpenPositions.Count % 2 == 1) ? -1 : 1;
                else                                                                 // First position = SHORT position
                    return (OpenPositions.Count % 2 == 1) ? 1 : -1;
            }
            else
                return 0;
        }
        
        /// <summary>
        /// Calculates the total Qty of all the open positions for a given direction
        /// </summary>
        /// <param name="direction">The direction for which the total Qty needs to be summed</param>
        /// <returns></returns>
        private double CalculateOpenQtyForDirection(int direction)
        {
            double outp = 0;
            foreach (ZoneRecoveryPosition zrp in OpenPositions)
            {
                outp = outp + ((zrp.TotalQty / Math.Abs(zrp.TotalQty) == direction) ? zrp.TotalQty : 0);
            }
            return outp;
        }

        /// <summary>
        /// Calculate the next price at which a reverse order should be placed.
        /// </summary>
        /// <returns>The next reverse price</returns>
        private double CalculateNextReversePrice()
        {
            ZoneRecoveryPosition zrp = OpenPositions.Single(s => s.PositionIndex == 1);
            int firstDir = (int)(zrp.TotalQty / Math.Abs(zrp.TotalQty));
            if (GetNextDirection() == firstDir)
                return zrp.AVGPrice;
            else
                return zrp.AVGPrice + ((firstDir * -1) * PipSize * ZoneSize);
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
        /// SetNewPosition should be called when a new position is taken on the exchange. The Zone Recovery
        /// strategy proceeds one step in its logic. The List of OrderResponses passed should be the 
        /// OrderResponses returned for one specific order.
        /// Example:
        ///     MaxDepthIndex = 4
        ///     CurrentZRPosition >  0   1   2   3   4   3   2   1   0   X
        ///     Position L/S      >  -   L   S   L   S   L4  S3  L2  S1  X
        ///     (Un)Winding       >  W   W   W   W   U   U   U   U   U   X
        /// 
        /// When the TP is reached at the exchange, a new position should not be set. The current Calculator 
        /// instance should be disposed.
        /// 
        /// TODO: Check how WebSocket returns updates on resting orders. This function makes the assumption 
        /// that a list of OrderResponses is returned.
        /// Turning the wheel, advance one step further in the ZR winding process...
        /// </summary>
        /// <param name="orderResp">The OrderResponse object returned by the Exchange.</param>
        public bool TryNewPosition(OrderResponse orderResp)
        {
            bool isPositionSet = false;
            bool acquiredLock = false;
            
            if (orderResp.OrdStatus.Equals("Filled"))
            {
                try
                {
                    Monitor.TryEnter(_Lock, 0, ref acquiredLock);
                    if (acquiredLock && !CheckClOrdIdExists(orderResp.ClOrdId))
                    {
                        // The critical section.
                        int elementID = OpenPositions.Count + 1;

                        // Add the new position to the OpenPositions List
                        OpenPositions.Add(new ZoneRecoveryPosition(orderResp.ClOrdId, orderResp.Account, this.PipSize, elementID, orderResp.AvgPx, orderResp.OrderQty));

                        // Turn the wheel of internal calculation variables
                        this.TakeStepForward();

                        isPositionSet = true;

                        // TODO: The controlling application should call GetNextAction() next.
                        // IDEA: Let this function already return the next action.
                    }
                    //else
                    //{
                    //    // The lock was not acquired.
                    //}
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

            // Return [true / false] when [something / nothing] happened
            return isPositionSet;
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
        /// </summary>
        /// <returns>ZoneRecoveryAction</returns>
        public ZoneRecoveryAction GetNextAction()
        {
            // TODO: Add account to ZoneRecoveryAction

            ZoneRecoveryAction zra = new ZoneRecoveryAction(OpenPositions.Count + 1);

            if (CurrentStatus == ZoneRecoveryStatus.Winding)
            {
                if (CurrentZRPosition > 0)
                {
                    // Calculate the next take profit price
                    zra.TPPrice = CalculateTakeProfitPrice();
                    zra.TPVolumeSell = CalculateOpenQtyForDirection(1);
                    zra.TPVolumeBuy = CalculateOpenQtyForDirection(-1);
                    
                    // Calculate the next reverse price
                    zra.ReverseVolume = GetUnitSize() * FactorArray[OpenPositions.Count];
                    zra.ReversePrice = CalculateNextReversePrice();
                    return zra;
                }
                else
                    // Happens when no initial position has been taken
                    return null;
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
 */
