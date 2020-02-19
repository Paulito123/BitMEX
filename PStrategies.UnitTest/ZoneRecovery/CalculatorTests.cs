using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using PStrategies.ZoneRecovery;
using PStrategies.ZoneRecovery.State;

using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

namespace PStrategies.UnitTest.ZoneRecovery
{
    [TestClass]
    public class CalculatorTests
    {
        [TestMethod]
        public void CalculatorTests_LifeCycleTest2Windings_ReturnsCorrectTPInProfit()
        {
            // Create a calculator instance
            var calcBox = new Calculator();

            Dictionary<ZoneRecoveryAccount, List<Order>> Orders = new Dictionary<ZoneRecoveryAccount, List<Order>>();
            Dictionary<ZoneRecoveryAccount, List<Position>> Positions = new Dictionary<ZoneRecoveryAccount, List<Position>>();
            Mutex PositionStatsMutex = new Mutex();

            var bitmexApiServiceA = BitmexApiService_Test_POS_Outcome.CreateDefaultApi();
            var bitmexApiServiceB = BitmexApiService_Test_POS_Outcome.CreateDefaultApi();

            calcBox.Initialize(
                    bitmexApiServiceA, bitmexApiServiceB,
                    1,
                    Orders,
                    Positions,
                    PositionStatsMutex,
                    "XBTUSD", 4, 50, (decimal)0.05, 1, (decimal)0.02);

            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();
            dict.Add("Ask", 10001);
            dict.Add("Bid", 10000);
            
            if (dict != null)
                calcBox.UpdatePrices(dict);

            calcBox.Evaluate(ZoneRecoveryAccount.A, new List<string>() { "ID ID ID ID ID" });

            //var bitmexApiServiceA = BitmexApiService_Test_POS_Outcome.CreateDefaultApi();
            //var OrderParamsA = OrderPOSTRequestParams.CreateSimpleLimit("XBTUSD", "1234HoedjeVanPapier-1234", 150, (decimal)10150.0, OrderSide.Buy);
            //bitmexApiServiceA.Execute(BitmexApiUrls.Order.PostOrder, OrderParamsA).ContinueWith(HandleOrderResponse, TaskContinuationOptions.AttachedToParent);

            //var OrderParams = new OrderDELETERequestParams() { ClOrdID = "bladieblakakkahahhaha" };
            //var result = bitmexApiServiceA.Execute(BitmexApiUrls.Order.DeleteOrder, OrderParams);

            //// Create an initial OrderResponse that reflects the first position in the strategy
            //var orderResponse = new OrderResponse()
            //{
            //    OrderId = "1234HoedjeVanPapier",
            //    ClOrdId = MordoR.GenerateGUID(),
            //    //OrderQty = (int)calcBox.GetInitialVolume(),
            //    Price = 10000,
            //    LeavesQty = 0,
            //    CumQty = 1000,
            //    AvgPx = 10000,
            //    OrdType = "Limit",
            //    OrdStatus = "Filled",
            //    ExecInst = "",
            //    TimeInForce = "ImmediateOrCancel",
            //    Account = 51091,
            //    Symbol = "XBTUSD",
            //    Side = "Buy",
            //    StopPx = null,
            //    PegOffsetValue = null,
            //    PegPriceType = "",
            //    Currency = "USD",
            //    SettlCurrency = "XBt",
            //    ContingencyType = "",
            //    ExDestination = "XBME",
            //    Triggered = "",
            //    TransactTime = new DateTime(2019, 7, 7, 1, 1, 1, 1),
            //    Timestamp = new DateTime(2019, 7, 7, 1, 1, 1, 1)
            //};

            //// Create the expected next action
            ////var nextAction1 = new ZoneRecoveryAction(1);
            ////nextAction1.ReverseVolume = 180 * 2;
            ////nextAction1.ReversePrice = 9950;
            ////nextAction1.TPVolumeBuy = 
            ////nextAction1.TPVolumeSell = 
            ////nextAction1.TPPrice = 

            ////Assert.AreEqual(1, nextStep.PositionIndex);

            //orderResponse = new OrderResponse()
            //{
            //    OrderId = "5678WaHaddeNaVerwacht",
            //    ClOrdId = MordoR.GenerateGUID(),
            //    OrderQty = 1000,
            //    Price = 1000,
            //    LeavesQty = 0,
            //    CumQty = 1000,
            //    AvgPx = 9103.5,
            //    OrdType = "Limit",
            //    OrdStatus = "Filled",
            //    ExecInst = "",
            //    TimeInForce = "ImmediateOrCancel",
            //    Account = 51091,
            //    Symbol = "XBTUSD",
            //    Side = "Buy",
            //    StopPx = null,
            //    PegOffsetValue = null,
            //    PegPriceType = "",
            //    Currency = "USD",
            //    SettlCurrency = "XBt",
            //    ContingencyType = "",
            //    ExDestination = "XBME",
            //    Triggered = "",
            //    TransactTime = new DateTime(2019, 7, 7, 1, 1, 1, 1),
            //    Timestamp = new DateTime(2019, 7, 7, 1, 1, 1, 1)
            //};
            ////orderResponseList.Add(orderResponse);

            //orderResponse = new BitMEX.Model.OrderResponse()
            //{
            //    OrderId = "a758f23f-f767-739c-891f-3dff5e6d4558",
            //    ClOrdId = "a758f23f-f767-739c-891f-3dff5e6d4558",
            //    OrderQty = 1000,
            //    Price = 9103.5,
            //    LeavesQty = 0,
            //    CumQty = 1000,
            //    AvgPx = 9103.5,

            //    OrdType = "Limit",
            //    OrdStatus = "Filled",
            //    ExecInst = "",
            //    TimeInForce = "ImmediateOrCancel",

            //    Account = 51091,
            //    Symbol = "XBTUSD",
            //    Side = "Buy",
            //    StopPx = null,
            //    PegOffsetValue = null,
            //    PegPriceType = "",
            //    Currency = "USD",
            //    SettlCurrency = "XBt",
            //    ContingencyType = "",
            //    ExDestination = "XBME",
            //    Triggered = "",
            //    TransactTime = new DateTime(2019, 7, 7, 1, 1, 1, 1),
            //    Timestamp = new DateTime(2019, 7, 7, 1, 1, 1, 1)
            //};
            ////orderResponseList.Add(orderResponse);

            ////calcBox.SetNewPosition(orderResponseList);



            //// Get next step and assert...
            ////var nextStep = calcBox.GetNextAction();

            //// Assert
            ////Assert.AreEqual(1, nextStep.PositionIndex);
            ////Assert.AreEqual(300, nextStep.ReversePrice);
            ////Assert.AreEqual("123", nextStep.ReverseVolume);
            ////Assert.AreEqual(1, nextStep.TPPrice);
            ////Assert.AreEqual(1, nextStep.TPVolumeBuy);
            ////Assert.AreEqual(1, nextStep.TPVolumeSell);

            //#region Set new position 2
            ////orderResponseList = new List<BitMEX.Model.OrderResponse>();

            //orderResponse = new BitMEX.Model.OrderResponse()
            //{
            //    OrderId = "a758f23f-f767-739c-891f-3dff5e6d4558",
            //    ClOrdId = "a758f23f-f767-739c-891f-3dff5e6d4558",
            //    ClOrdLinkId = "",
            //    Account = 51091,
            //    Symbol = "XBTUSD",
            //    Side = "Sell",
            //    OrderQty = 1000,
            //    Price = 9103.5,
            //    DisplayQty = null,
            //    StopPx = null,
            //    PegOffsetValue = null,
            //    PegPriceType = "",
            //    Currency = "USD",
            //    SettlCurrency = "XBt",
            //    OrdType = "Market",
            //    TimeInForce = "ImmediateOrCancel",
            //    ExecInst = "",
            //    ContingencyType = "",
            //    ExDestination = "XBME",
            //    OrdStatus = "Filled",
            //    Triggered = "",
            //    LeavesQty = 0,
            //    CumQty = 1000,
            //    AvgPx = 9103.5,
            //    TransactTime = new DateTime(2019, 7, 7, 1, 2, 1, 1),
            //    Timestamp = new DateTime(2019, 7, 7, 1, 2, 1, 1)
            //};
            //orderResponseList.Add(orderResponse);

            //calcBox.SetNewPosition(orderResponseList);

            //#endregion Set new position 2

            //// Get next step and assert...
            //nextStep = calcBox.GetNextAction();

            // Assert
            //Assert.AreEqual(1, nextStep.PositionIndex);
            //Assert.AreEqual(300, nextStep.ReversePrice);
            //Assert.AreEqual("123", nextStep.ReverseVolume);
            //Assert.AreEqual(1, nextStep.TPPrice);
            //Assert.AreEqual(1, nextStep.TPVolumeBuy);
            //Assert.AreEqual(1, nextStep.TPVolumeSell);

        }
    }
}