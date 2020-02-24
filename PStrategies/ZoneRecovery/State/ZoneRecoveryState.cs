using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

using Serilog;

/* Wa moet da spel kunnen?
 * De status van de orders en posities zoals ze nu gekend zijn kunnen analyseren
 * Orders annuleren
 * Orders plaatsen
 * Anomalien oplossen
 * Bij het creeeren van een nieuwe state wordt de oude state doorgegeven
 * 
 * 
 */

namespace PStrategies.ZoneRecovery.State
{
    public abstract class IZoneRecoveryState
    {
        /// <summary>
        /// The context class for this state.
        /// </summary>
        protected Calculator calculator;

        /// <summary>
        /// Defines the step in the zone recovery strategy. Possible values:
        /// -1      : Not defined
        /// 0       : First orders are waiting to be filled
        /// 1..n    : Orders are waiting to be filled
        /// </summary>
        protected int step;

        /// <summary>
        /// 
        /// </summary>
        protected int currentZRPosition;

        /// <summary>
        /// The direction in which current TP order is resting.
        /// </summary>
        protected int tpDirection;

        public Calculator Calculator
        {
            get => calculator;
            set => calculator = value;
        }

        public int Step
        {
            get => step;
            set => step = value;
        }

        public int CurrentZRPosition
        {
            get => currentZRPosition;
            set => currentZRPosition = value;
        }
        
        public int TPDirection
        {
            get => tpDirection;
            set => tpDirection = value;
        }

        public abstract void Evaluate();

        public void InitiateState(string me)
        {
            Step = -1;
            TPDirection = 0;
            CurrentZRPosition = 0;
        }

        public string WhereAmI(string me)
        {
            return $"New state:{me} => [CurrentZRPosition]:[{CurrentZRPosition}], [Step]:[{Step}], [TPDirection]:[{TPDirection}]";
        }

        public ZoneRecoveryBatchType TurnTheWheel()
        {
            ZoneRecoveryBatchType nextType = ZoneRecoveryBatchType.Undefined;

            try
            {
                if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus != ZoneRecoveryBatchStatus.Closed)
                    throw new Exception("ZoneRecoveryBatchStatus not Closed");

                int direction;
                
                // TPDirection
                if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].Direction == ZoneRecoveryDirection.Down)
                    direction = 1;
                else if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].Direction == ZoneRecoveryDirection.Up)
                    direction = -1;
                else
                    direction = 0;

                switch (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchType)
                {
                    case ZoneRecoveryBatchType.PeggedStart:

                        // Correction of TPDirection because direction for PeggedStart and WindingFirst are equal!
                        direction = direction * -1;

                        // Next Type
                        nextType = ZoneRecoveryBatchType.WindingFirst;

                        // Current Zone Recovery Position
                        CurrentZRPosition++;

                        break;
                    case ZoneRecoveryBatchType.Unwinding:
                        
                        // Next Type
                        if (CurrentZRPosition == 1)
                            nextType = ZoneRecoveryBatchType.UnwindingLast;
                        else if (CurrentZRPosition == 0)
                            throw new Exception("UnwindingUp has CurrentZRPosition = 0");

                        // Current Zone Recovery Position
                        CurrentZRPosition--;

                        break;
                    case ZoneRecoveryBatchType.WindingFirst:
                    case ZoneRecoveryBatchType.Winding:
                        
                        if (CurrentZRPosition < Calculator.MaxDepthIndex)
                        {
                            // Current Zone Recovery Position
                            CurrentZRPosition++;
                        }
                        else if(CurrentZRPosition == Calculator.MaxDepthIndex)
                        {
                            // Current Zone Recovery Position
                            CurrentZRPosition--;

                            // Next Type
                            nextType = ZoneRecoveryBatchType.Unwinding;
                        }
                        else
                            throw new Exception("Winding: CurrentZRPosition > MaxDepthIndex error");
                        
                        break;
                    case ZoneRecoveryBatchType.UnwindingLast:
                        
                        // Next Type
                        nextType = ZoneRecoveryBatchType.PeggedStart;

                        // Current Zone Recovery Position
                        CurrentZRPosition--;

                        break;
                }

                // Step
                Step++;

                TPDirection = direction;
            }
            catch (Exception exc)
            {
                var message = $"{WhereAmI(GetType().Name)}[Error]:{exc.Message}";
                Console.WriteLine(message);
                Log.Error(message);
                nextType = ZoneRecoveryBatchType.Error;
            }
            
            return nextType;
        }
    }

    public class ZRSInitiating : IZoneRecoveryState
    {
        public ZRSInitiating(IZoneRecoveryState state, bool eval = false) : this(state.Calculator, eval) { }
        public ZRSInitiating(Calculator calc, bool eval = false)
        {
            Calculator = calc;
            Console.WriteLine(WhereAmI(GetType().Name));

            if (eval)
                Evaluate();
        }

        /// <summary>
        /// 1. Fetch 
        ///     a. positions 
        ///     b. orders
        /// 2. Check if there is a batch open
        ///     y. See what is the status of this batch and if it corresponds with the status on the server
        ///         y. continue with the batch as it is defined and Goto End
        ///         n. Repair
        ///     n. Goto 3.
        /// 3. Change status to Opening
        /// </summary>
        public override void Evaluate()
        {
            if (Calculator.ZRBatchLedger.ContainsKey(Calculator.RunningBatchNr))
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:{Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus}");
            else
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:<No status>");

            bool fail = false;
            decimal aPosition = 0, bPosition = 0;
            List<Order> mergedList = new List<Order>();

            // 1.b Fetch Orders
            try
            {
                if (Calculator.Orders[ZoneRecoveryAccount.A] == null || Calculator.Orders[ZoneRecoveryAccount.B] == null)
                    throw new Exception($"Orders list in Calculator is null");

                var aNewList = Calculator.Orders[ZoneRecoveryAccount.A].Where(x => x.Symbol == Calculator.Symbol && x.OrdStatus == OrderStatus.New).ToList();
                if (aNewList != null && aNewList.Count() > 0)
                    mergedList.AddRange(aNewList);

                var bNewList = Calculator.Orders[ZoneRecoveryAccount.B].Where(x => x.Symbol == Calculator.Symbol && x.OrdStatus == OrderStatus.New).ToList();
                if (bNewList != null && bNewList.Count() > 0)
                    mergedList.AddRange(bNewList);
            }
            catch (Exception exc)
            {
                string m = $"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}[2]: {exc.Message}";
                Log.Error(m);
                Console.WriteLine(m);
                fail = true;
            }

            if (fail)
                return;

            // 1.a Fetch Positions
            try
            {
                Calculator.PositionMutex.WaitOne();

                if (Calculator.Positions[ZoneRecoveryAccount.A] == null || Calculator.Positions[ZoneRecoveryAccount.B] == null)
                    throw new Exception($"Postitions list in Calculator is null");

                if (Calculator.Positions[ZoneRecoveryAccount.A].Where(x => x.Symbol == Calculator.Symbol).Count() == 1)
                    aPosition = Calculator.Positions[ZoneRecoveryAccount.A].Where(x => x.Symbol == Calculator.Symbol).Single().CurrentQty ?? 0;
                
                if (Calculator.Positions[ZoneRecoveryAccount.B].Where(x => x.Symbol == Calculator.Symbol).Count() == 1)
                    bPosition = Calculator.Positions[ZoneRecoveryAccount.B].Where(x => x.Symbol == Calculator.Symbol).Single().CurrentQty ?? 0;
            }
            catch (Exception exc)
            {
                string m = $"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}[1]: {exc.Message}";
                Log.Error(m);
                Console.WriteLine(m);
                fail = true;
            }
            finally
            {
                Calculator.PositionMutex.ReleaseMutex();
            }

            if (fail)
                return;

            // Check if there is a batch open
            if (Calculator.ZRBatchLedger.Count > 0)
            {
                // Check if there is a batch open
                if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.Closed)
                {
                    // Check if the positions are flat and no orders are resting
                    if (aPosition == 0 && bPosition == 0 && mergedList.Count == 0)
                    {
                        InitiateState(GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        Calculator.State = new ZRSOrdering(this, ZoneRecoveryBatchType.PeggedStart);
                    }
                    else if (aPosition == 0 && bPosition == 0)
                    {
                        InitiateState(GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
                        Calculator.State = new ZRSCanceling(this);
                    }
                    else
                    {
                        Console.WriteLine(WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name));
                        Calculator.State = new ZRSRepairing(this);
                    }
                }
                else
                {
                    // Open batch found, try to assess the situation
                    Calculator.State = new ZRSRepairing(this);
                }
            }
            else // No batch open...
            {
                InitiateState(GetType().Name + "." + MethodBase.GetCurrentMethod().Name);

                if (aPosition == 0 && bPosition == 0)
                {
                    Calculator.State = new ZRSOrdering(this, ZoneRecoveryBatchType.PeggedStart);
                    Calculator.Evaluate();
                }
                else
                {
                    Calculator.State = new ZRSRepairing(this, true);
                }
            }
        }
    }

    /*
     * Step = -1;
     * FactorPosition = 1;
     * TPDirection = 0;
     * CurrentZRPosition = 0;
     * 
     * TODO CHECK ZoneRecoveryBatchType and ZoneRecoveryBatchStatus
     * TODO Make up your mind which statusses and variables are needed to make this state machine work properly...
     */

    public class ZRSOrdering : IZoneRecoveryState
    {
        ZoneRecoveryBatchType ZRBType;

        public ZRSOrdering(IZoneRecoveryState state, ZoneRecoveryBatchType type)
        {
            Step = state.Step;
            Calculator = state.Calculator;
            TPDirection = state.TPDirection;
            ZRBType = type;

            Console.WriteLine(WhereAmI(GetType().Name));
        }

        public override void Evaluate()
        {
            if (Calculator.ZRBatchLedger.ContainsKey(Calculator.RunningBatchNr))
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:{Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus}");
            else
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:<No status>");

            switch(ZRBType)
            {
                case ZoneRecoveryBatchType.PeggedStart:
                    // Create the orders
                    Calculator.CreateNewBatch(ZRBType);

                    // Change state
                    Calculator.State = new ZRSWorking(this);
                    break;
                case ZoneRecoveryBatchType.Winding:
                    // Create the orders
                    Calculator.CreateNewBatch(ZRBType);

                    // Change state
                    Calculator.State = new ZRSWorking(this);
                    break;
            }

            if (Step == -1)
            {
                
            }
            else if (Step == 0)
            {
                // TODO: Shizzle manizzle
                // Change state
                Calculator.State = new ZRSWorking(this);
            }
            else if (Step == Calculator.MaxDepthIndex)
            {

                // Change state
                Calculator.State = new ZRSWorking(this);
            }
            else if (Step == Calculator.MaxDepthIndex)
            {

                // Change state
                Calculator.State = new ZRSWorking(this);
            }
            
        }
    }

    public class ZRSWorking : IZoneRecoveryState
    {
        public ZRSWorking(IZoneRecoveryState state)
        {
            Step = state.Step;
            Calculator = state.Calculator;
            TPDirection = state.TPDirection;

            Console.WriteLine(WhereAmI(GetType().Name));
        }

        public override void Evaluate()
        {
            if (Calculator.ZRBatchLedger.ContainsKey(Calculator.RunningBatchNr))
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:{Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus}");
            else
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:<No status>");

            // Change only if needed else, do nothing
            if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.Waiting)
            {
                Calculator.State = new ZRSWaiting(this);
            }
            else if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.ReadyForNext)
            {
                Calculator.State = new ZRSCanceling(this);
            }
            else if (Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.Error
                  || Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus == ZoneRecoveryBatchStatus.Closed)
            {
                Calculator.State = new ZRSRepairing(this);
            }
        }
    }

    public class ZRSWaiting : IZoneRecoveryState
    {
        public ZRSWaiting(IZoneRecoveryState state, bool eval = false)
        {
            this.Step = state.Step;
            this.Calculator = state.Calculator;
            this.TPDirection = state.TPDirection;
            Console.WriteLine(WhereAmI(GetType().Name));

            if (eval)
                Evaluate();
        }

        public override void Evaluate()
        {
            if (Calculator.ZRBatchLedger.ContainsKey(Calculator.RunningBatchNr))
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:{Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus}");
            else
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:<No status>");

            // Check if the orders are filled as expected
            // [0] Desired setup, either TP with TL have been filled or REV has been filled.
            // [1] Only TP|TL is filled but TL|TP is still expected
            // [2] An undesired combination has been filled for some reason
            // [3] 
            // if (0) > Turn the wheel, close what must be closed and Calculator.State = new ZRSOrdering(this)
            // if (1) > Start a task on a new thread to check if TP|TL was still on its way
            // if (2) > Calculator.State = new ZRSRepairing(this);
            // if (3) > 
        }
    }

    public class ZRSCanceling : IZoneRecoveryState
    {
        public ZRSCanceling(IZoneRecoveryState state, bool eval = false) : this(state.Calculator, state.Step, state.TPDirection, eval) { }
        public ZRSCanceling(Calculator calc, int step, int tpDir, bool eval = false)
        {
            Step = step;
            Calculator = calc;
            TPDirection = tpDir;
            Console.WriteLine(WhereAmI(GetType().Name));

            if (eval)
                Evaluate();
        }

        public override void Evaluate()
        {
            if (Calculator.ZRBatchLedger.ContainsKey(Calculator.RunningBatchNr))
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:{Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus}");
            else
            {
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:<No status>");
                throw new Exception($"ZRSCanceling.Evaluate: ZRBatchLedger does not contain key");
            }
                
            foreach (ZoneRecoveryBatchOrder o in Calculator.ZRBatchLedger[Calculator.RunningBatchNr].ZROrdersList)
            {   
                if (o.CurrentStatus == ZoneRecoveryOrderStatus.New)
                    Calculator.RemoveOrderForAccount(o.Account, o.PostParams.ClOrdID);

                // Remove Orders from list
                Calculator.Orders[o.Account]
                    .RemoveAll(filter => (Calculator.Orders[o.Account]
                        .Any(x => x.ClOrdId == o.PostParams.ClOrdID)));
            }

            //TODO Turn the wheel and move on...
            ZoneRecoveryBatchType newType = TurnTheWheel();

            switch (newType)
            {
                case ZoneRecoveryBatchType.Error:
                case ZoneRecoveryBatchType.Undefined:
                    Calculator.State = new ZRSRepairing(this);
                    break;
                case ZoneRecoveryBatchType.PeggedStart:
                    Calculator.State = new ZRSOrdering(this, newType);
                    break;
                case ZoneRecoveryBatchType.Unwinding:
                case ZoneRecoveryBatchType.Winding:
                case ZoneRecoveryBatchType.UnwindingLast:
                case ZoneRecoveryBatchType.WindingFirst:
                    Calculator.State = new ZRSOrdering(this, newType);
                    break;
            }
        }
    }

    public class ZRSRepairing : IZoneRecoveryState
    {
        public ZRSRepairing(IZoneRecoveryState state, bool eval = false) : this(state.Calculator, state.Step, state.TPDirection, eval) { }
        public ZRSRepairing(Calculator calc, int step, int tpDir, bool eval = false)
        {
            this.Step = step;
            this.Calculator = calc;
            this.TPDirection = tpDir;
            Console.WriteLine(WhereAmI(GetType().Name));

            if (eval)
                Evaluate();
        }

        public override void Evaluate()
        {
            if (Calculator.ZRBatchLedger.ContainsKey(Calculator.RunningBatchNr))
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:{Calculator.ZRBatchLedger[Calculator.RunningBatchNr].BatchStatus}");
            else
                Console.WriteLine($"{WhereAmI(GetType().Name + "." + MethodBase.GetCurrentMethod().Name)}:<No status>");


        }
    }
}
