using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMEX.Model;

namespace PStrategies.ZoneRecovery
{
    public class Calculator
    {
        #region Variables

        ///// <summary>
        ///// The guid for which the calculator needs to listen for take profit action
        ///// </summary>
        //private string nextGuidTP;

        ///// <summary>
        ///// The guid for which the calculator needs to listen for take reverse action
        ///// </summary>
        //private string nextGuidReverse;

        /// <summary>
        /// Object used to lock a piece of code to prevent it from being executed multiple times within one instance of the calculator class.
        /// </summary>
        private Object _Lock = new Object();

        /// <summary>
        /// An array of factors used to calculate the size of each winding.
        /// </summary>
        private static double[] FactorArray = new double[] { 1, 2, 3, 6, 12, 24, 48, 96 };

        /// <summary>
        /// An enumeration used for expressing in which part of the Zone Recovery algorithm the class is at a given moment.
        /// </summary>
        private enum ZoneRecoveryStatus { Winding, Unwinding, Unwound }

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
        /// The maximum unit size used for each trade within this instance of the Calculator class. 
        /// To calculate this measure: InitialPrice * Leverage * TotalBalance * MaxExposurePerc / totalDepthMaxExposure
        /// Basically it comes down to calculating the maximum amount of exposure you would want for one instance of 
        /// the Calculator class. The key parameter to adjust the risk is MaxExposurePerc.
        /// </summary>
        private double MaximumUnitSize;

        /// <summary>
        /// When the mathematical break even price has been reached, this percentage defines how far in profit
        /// the strategy needs to be before closing all related positions.
        /// </summary>
        private double MinimumProfitPercentage;

        /// <summary>
        /// CurrentZRPosition reflects the position withing the Zone Recovery strategy.
        /// When 0 > Strategy has been initialized or is completely unwound. There should be no open positions.
        /// When 1 > First Winding / last Unwinding
        /// When CurrentZRPosition = MaxDepthIndex > ZoneRecoveryStatus is switched and winding process reversed
        /// </summary>
        private int CurrentZRPosition;

        /// <summary>
        /// It is the price at the time the Calculator class is initialized and it is used as the reference price
        /// for calculating the zone.
        /// </summary>
        private double InitialPrice { get; set; }

        /// <summary>
        /// At any time, this measure reflects the maximum percentage of the TotalBalance that can be exposed to 
        /// the market. 
        /// </summary>
        private double MaxExposurePerc { get; set; }

        /// <summary>
        /// The total balance of the wallet at the time that the Calculator class is initialized.
        /// </summary>
        private double TotalBalance { get; set; }

        /// <summary>
        /// The current leverage used for calculating other measures
        /// </summary>
        private double Leverage { get; set; }

        /// <summary>
        /// The minimum pip size of the exchange, used for rounding prices.
        /// </summary>
        private double PipSize { get; set; }

        /// <summary>
        /// The size of the zone expressed in number of pips. ZoneSize * PipSize = Real Zone Size
        /// </summary>
        private int ZoneSize { get; set; }

        /// <summary>
        /// Reflects how deep inside the FactorArray, this instance of the Calculator class, is allowed to go.
        /// </summary>
        private int MaxDepthIndex { get; set; }

        #endregion Variables

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
        /// <param name="initOrder">The first order response in the ZR strategy</param>
        public Calculator(double initPrice, double maxExposurePerc, double totalBalance, double leverage, double pipSize, int maxDepthIndex, int zoneSize, double minPrftPerc)
        {
            CurrentStatus = ZoneRecoveryStatus.Winding;
            OpenPositions = new List<ZoneRecoveryPosition>();
            InitialPrice = initPrice;
            MaxExposurePerc = maxExposurePerc;
            TotalBalance = totalBalance;
            Leverage = leverage;
            PipSize = pipSize;
            CurrentZRPosition = 0;
            MaxDepthIndex = maxDepthIndex;
            ZoneSize = zoneSize;
            MinimumProfitPercentage = minPrftPerc;
            MaximumUnitSize = GetRelativeUnitSize();
        }
        #endregion Constructor(s)

        #region Private methods

        /// <summary>
        /// Calculates the total maximum exposure (depth) possible, according to the depths defines in FactorArray. Basically
        /// this is the sum of all the factors defined in FactorArray until the defined MaxDepth. It represents the 
        /// maximum exposure possible.
        /// Example: 
        ///     FactorArray = new double[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        ///     MaxDepth = 4
        ///     GetTotalDepthMaxExposure() returns 12 (1 + 2 + 3 + 6) 
        /// </summary>
        private double GetTotalDepthMaxExposure()
        {
            double sum = 0;

            for (int i = 0; i < MaxDepthIndex; i++)
            {
                sum = sum + FactorArray[i];
            }
            return sum;
        }

        /// <summary>
        /// For a given InitialPrice, Balance, MaxExposurePerc and Leverage, this function returns the calculated maximum unit size. 
        /// This unit size multiplied with the factors in FactorArray will give the unit size of each order.
        /// In other words, the value returned is the Volume by unit in the FactorArray.
        /// </summary>
        private double GetRelativeUnitSize()
        {
            double totalDepthMaxExposure = (double)this.GetTotalDepthMaxExposure();
            return Math.Round(InitialPrice * Leverage * TotalBalance * MaxExposurePerc / totalDepthMaxExposure, MidpointRounding.ToEven);
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
        /// Calculate the profit difference based on the profit percentage and the initial price
        /// </summary>
        private double CalculateProfitPriceDifference()
        {
            return InitialPrice * MinimumProfitPercentage;
        }

        /// <summary>
        /// Returns the price at which profit should be taken to be mathematically "Break Even". Commissions are not yet taken into account.
        /// TODO: Extend the calculation to take into account commissions and real profit using the MinimumProfitPercentage variable.
        /// </summary>
        /// <returns></returns>
        private double CalculateTPPrice()
        {
            double v_numerator = 0.0;
            double v_denominator = 0.0;
            double o = 0.0;

            // Calculate the break even price
            foreach(ZoneRecoveryPosition zp in OpenPositions)
            {
                v_numerator = v_numerator + (zp.TotalQty * zp.AVGPrice);
                v_denominator = v_denominator + (zp.TotalQty);
            }

            o = v_numerator / v_denominator * (1 / PipSize);

            // Add the calculated minimum profit margin
            o = o + (-this.GetNextDirection() * CalculateProfitPriceDifference());

            return Math.Round(o, MidpointRounding.AwayFromZero) / (1 / PipSize);
        }

        /// <summary>
        /// Calculates the total Qty of all the open positions for a given direction
        /// </summary>
        /// <param name="direction">The direction for which the total Qty needs to be summed</param>
        /// <returns></returns>
        private double CalculateOpenQtyForDirection(int direction)
        {
            double outp = 0;
            foreach(ZoneRecoveryPosition zrp in OpenPositions)
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
                CurrentZRPosition--;
            else
                CurrentZRPosition = 0; // Should not happen...
        }

        #endregion Private methods

        #region Public methods

        /// <summary>
        /// TODO TEST THIS FUNCTION
        /// </summary>
        /// <returns></returns>
        public double GetInitialVolume()
        {
            return GetRelativeUnitSize();
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
            lock (_Lock)
            {
                ZoneRecoveryAction zra = new ZoneRecoveryAction(OpenPositions.Count + 1);

                if (CurrentStatus == ZoneRecoveryStatus.Winding)
                {
                    if (CurrentZRPosition > 0)
                    {
                        // Calculate the next take profit price
                        zra.TPPrice         = CalculateTPPrice();
                        zra.TPVolumeSell    = CalculateOpenQtyForDirection(1);
                        zra.TPVolumeBuy     = CalculateOpenQtyForDirection(-1);
                        zra.ReverseVolume   = MaximumUnitSize * FactorArray[OpenPositions.Count];
                        zra.ReversePrice    = CalculateNextReversePrice();
                        return zra;
                    }
                    else
                        // Happens when no initial position has been taken
                        return null;
                }
                else if (CurrentStatus == ZoneRecoveryStatus.Unwinding) // Unwinding
                {
                    // Calculate the next take profit price
                    zra.TPPrice         = CalculateTPPrice();
                    zra.TPVolumeSell    = CalculateOpenQtyForDirection(1);
                    zra.TPVolumeBuy     = CalculateOpenQtyForDirection(-1);
                    zra.ReverseVolume   = -OpenPositions.Single(s => s.PositionIndex == (CurrentZRPosition + 1)).TotalQty;
                    zra.ReversePrice    = CalculateNextReversePrice();
                    return zra;
                }
                else
                    return null;
            }
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
 */
