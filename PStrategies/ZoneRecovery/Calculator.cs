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
        /// </summary>
        private List<ZoneRecoveryPosition> OpenPositions;

        /// <summary>
        /// The maximum unit size used for each trade within this instance of the Calculator class. 
        /// To calculate this measure: InitialPrice * Leverage * TotalBalance * MaxExposure / totalDepthMaxExposure
        /// Basically it comes down to calculating the maximum amount of exposure you would want for one instance of 
        /// the Calculator class. The key parameter to adjust the risk is MaxExposure.
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
        private double MaxExposure { get; set; }

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
        /// TODO: InitialPrice and its derived calculations should be recalculated when the first order is filled. Zone is 
        ///       determined relative to the price of the first position.
        /// </summary>
        /// <param name="initialPrice">The price at the time the class is initialized</param>
        /// <param name="maxExposure">Percentage of the maximum market exposure when completely wound</param>
        /// <param name="totalBalance">Most recent total balance</param>
        /// <param name="leverage">Leverage used to calculate other parameters</param>
        /// <param name="pipSize">The minimum pip size possible on the exchange</param>
        /// <param name="maxDepthIndex">Maximum dept allowed in the Zone Recovery system</param>
        /// <param name="zoneSize">The size of the zone in nr of pips</param>
        /// <param name="minPrftPerc">Minimum required profit margin</param>
        public Calculator(double initialPrice, double maxExposure, double totalBalance, double leverage, double pipSize, int maxDepthIndex, int zoneSize, double minPrftPerc)
        {
            CurrentStatus = ZoneRecoveryStatus.Winding;
            OpenPositions = new List<ZoneRecoveryPosition>();
            InitialPrice = initialPrice;
            MaxExposure = maxExposure;
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
        /// For a given InitialPrice, Balance, MaxExposure and Leverage, this function returns the calculated maximum unit size. 
        /// This unit size multiplied with the factors in FactorArray will give the unit size of each order.
        /// </summary>
        private double GetRelativeUnitSize()
        {
            double totalDepthMaxExposure = (double)this.GetTotalDepthMaxExposure();
            return Math.Round((double)(InitialPrice * Leverage * TotalBalance * MaxExposure / totalDepthMaxExposure) * (1 / (double)PipSize), MidpointRounding.AwayFromZero) / (1 / (double)PipSize);
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
                if (OpenPositions.Single(s => s.PositionIndex == 1).TotalVolume > 0) // First position = LONG position
                    return (OpenPositions.Count % 2 == 1) ? -1 : 1;
                else                                                                 // First position = SHORT position
                    return (OpenPositions.Count % 2 == 1) ? 1 : -1;
            }
            else
                return 0;
        }

        /// <summary>
        /// Calculate the difference in price relative to the InitialPrice.
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

            foreach(ZoneRecoveryPosition zp in OpenPositions)
            {
                v_numerator = v_numerator + (zp.TotalVolume * zp.AVGPrice);
                v_denominator = v_denominator + (zp.TotalVolume);
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
                outp = outp + ((zrp.TotalVolume / Math.Abs(zrp.TotalVolume) == direction) ? zrp.TotalVolume : 0);
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
            int firstDir = (int)(zrp.TotalVolume / Math.Abs(zrp.TotalVolume));
            if (GetNextDirection() == firstDir)
                return zrp.AVGPrice;
            else
                return zrp.AVGPrice + ((firstDir * -1) * PipSize * ZoneSize);
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
        /// </summary>
        /// <param name="orderResp">The List object with all the OrderResponses for one order returned by the Exchange.</param>
        public void SetNewPosition(List<BitMEX.Model.OrderResponse> orderResp)
        {
            lock(_Lock)
            {
                // Create a new position object for the calculation of the average position size
                ZoneRecoveryPosition newPos = new ZoneRecoveryPosition(orderResp[1].OrderId, (double)PipSize, OpenPositions.Count + 1);

                // Loop all the OrderResponse objects related to a specific filled previously resting or market order.
                foreach (BitMEX.Model.OrderResponse resp in orderResp)
                {
                    // TODO: Check if assumption is correct that AvgPx is the average price for the filled OrderQty...
                    newPos.AddToPosition((double)resp.AvgPx, (double)resp.OrderQty);
                }

                // Add the new averaged position to the positions collection
                this.OpenPositions.Add(newPos);

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
                        zra.TPPrice = CalculateTPPrice();
                        zra.TPVolumeSell = CalculateOpenQtyForDirection(1);
                        zra.TPVolumeBuy = CalculateOpenQtyForDirection(-1);
                        zra.ReverseVolume = MaximumUnitSize * FactorArray[OpenPositions.Count];
                        zra.ReversePrice = CalculateNextReversePrice();
                        return zra;
                    }
                    else
                        //Should never happen because GetNextStep is called only after the first position is taken...
                        return null;
                }
                else if (CurrentStatus == ZoneRecoveryStatus.Unwinding) // Unwinding
                {
                    // Calculate the next take profit price
                    zra.TPPrice = CalculateTPPrice();
                    zra.TPVolumeSell = CalculateOpenQtyForDirection(1);
                    zra.TPVolumeBuy = CalculateOpenQtyForDirection(-1);
                    zra.ReverseVolume = -OpenPositions.Single(s => s.PositionIndex == (CurrentZRPosition + 1)).TotalVolume;
                    zra.ReversePrice = CalculateNextReversePrice();
                    return zra;
                }
                else
                    return null;
            }
        }
    }

    /// <summary>
    /// ZoneRecoveryAction class serves merely the purpose of transporting all the parameters needed for creating the orders in the
    /// application that uses this library.
    /// TODO: Make ZoneRecoveryAction class dual account friendly by extending it with TPVolumeSellAccount and TPVolumeBuyAccount.
    /// </summary>
    public class ZoneRecoveryAction
    {
        /// <summary>
        /// PositionIndex represents the number of positions taken in the past within the current strategy instance. It can be used 
        /// as a unique identifier by the application that uses this library to make sure the same action is not taken twice...
        /// </summary>
        public int PositionIndex { set; get; }

        /// <summary>
        /// The price where a profit is taken. TP = Take Profit
        /// </summary>
        public double TPPrice { set; get; }

        /// <summary>
        /// Volume to Sell all your Buy positions
        /// </summary>
        public double TPVolumeSell { set; get; }

        /// <summary>
        /// Volume to Buy all your Sell positions
        /// </summary>
        public double TPVolumeBuy { set; get; }

        /// <summary>
        /// The price at which the position should be reversed
        /// </summary>
        public double ReversePrice { set; get; }
        public double ReverseVolume { set; get; }

        public ZoneRecoveryAction(int posIndex)
        {
            PositionIndex = posIndex;
        }
    }

    /// <summary>
    /// Class that represents a taken position. Since one order could be filled by a number of OrderResponses, it is
    /// easier to have a calculated object with only the information needed rather than the collection of
    /// OrderResponses.
    /// When Volume is negative, the position is a Sell position.
    /// When Volume is position, the position is a Long position.
    /// </summary>
    public class ZoneRecoveryPosition
    {
        public string OrderID { get; set; }
        public double AVGPrice { get; set; }
        public double TotalVolume { get; set; }
        public double PipSize { get; set; }

        /// <summary>
        /// PositionIndex keeps the sequence number of a position within its container. (followup number)
        /// Could be deleted later on if not used...
        /// </summary>
        public int PositionIndex { get; set; }

        public void AddToPosition(double executionPrice, double executionVolume)
        {
            this.AVGPrice = CalculateAveragePrice(AVGPrice, executionPrice, TotalVolume, executionVolume, PipSize);
            this.TotalVolume = this.TotalVolume + executionVolume;
        }

        public ZoneRecoveryPosition(string ordID, double pipSize, int posIndex)
        {
            this.OrderID = ordID;
            this.AVGPrice = 0.0;
            this.TotalVolume = 0.0;
            this.PipSize = pipSize;
            this.PositionIndex = posIndex;
        }

        public static double CalculateAveragePrice(double price1, double price2, double vol1, double vol2, double pipSize)
        {
            return Math.Round(((price1 * vol1) + (price2 * vol2)) / (vol1 + vol2) * (1 / pipSize), MidpointRounding.AwayFromZero) / (1 / pipSize);
        }
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
