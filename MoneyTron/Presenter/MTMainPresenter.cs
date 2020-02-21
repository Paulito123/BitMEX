using System;
using System.IO;
using System.Configuration;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Windows;
using System.Collections;
using System.Windows.Forms;
using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Communicator;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Trades;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Executions;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Margins;
using Bitmex.Client.Websocket.Websockets;
using Bitmex.Client.Websocket.Utils;
using Websocket.Client;
using MoneyTron.ResponseHandlers;
using Serilog;

using PStrategies.ZoneRecovery;

using BitMEXRest.Authorization;
using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

namespace MoneyTron.Presenter
{
    class MTMainPresenter
    {
        #region Declarations

        private readonly IMTMainForm _view;
        
        //private ZoneRecoveryComputer ZRComputer;
        private Calculator ZRCalculator;
        private bool CalculatorEnabled = false;

        private IBitmexApiService bitmexApiServiceA;
        private IBitmexApiService bitmexApiServiceB;

        private OrderBookStatsComputer _orderBookStatsComputer;
        private TradeStatsComputer _tradeStatsComputer;
        private GeneralStatsHandler _generalStatsHandler;

        // TODO reduce nr of stats handlers by using mutex
        private OrdersStatsHandler _orderStatsHandler;
        private Mutex OrderStatsMutex = new Mutex();

        private PositionStatsHandler _posStatsHandler;
        private Mutex PositionStatsMutex = new Mutex();
        
        private ErrorStatsHandler _errorStatsHandler;
        private Mutex ErrorStatsMutex = new Mutex();
        
        private MarginStatsHandler _marginStatsHandler;
        private Mutex MarginStatsMutex = new Mutex();

        private Dictionary<MTAccount, long> Accounts;

        private IBitmexCommunicator _communicatorA;
        private IBitmexCommunicator _communicatorB;
        private BitmexWebsocketClient _clientA;
        private BitmexWebsocketClient _clientB;

        private IDisposable _pingSubscriptionA;
        private IDisposable _pingSubscriptionB;
        private DateTime _pingRequestA;
        private DateTime _pingRequestB;

        private readonly string _defaultPair = "XBTUSD";
        private readonly string _currency = "$";

        #endregion Declarations

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view"></param>
        public MTMainPresenter(IMTMainForm formInterface)
        {
            _view = formInterface;

            ConfigureAccounts();

            ConfigureRestApi();
            
            HandleTasks();
        }

        #region Configuration

        // TODO: Simplify
        private void ConfigureRestApi()
        {
            var appSettings = ConfigurationManager.AppSettings;
            string apiA, secA, apiB, secB;
            BitmexEnvironment env;

            if (_view.isTest)
            {
                apiA = appSettings["API_KEY_BITMEX_A_TEST"] ?? string.Empty;
                secA = appSettings["API_SECRET_BITMEX_A_TEST"] ?? string.Empty;
                apiB = appSettings["API_KEY_BITMEX_B_TEST"] ?? string.Empty;
                secB = appSettings["API_SECRET_BITMEX_B_TEST"] ?? string.Empty;
                env = BitmexEnvironment.Test;
            }
            else
            {
                apiA = appSettings["API_KEY_BITMEX_A_LIVE"] ?? string.Empty;
                secA = appSettings["API_SECRET_BITMEX_A_LIVE"] ?? string.Empty;
                apiB = appSettings["API_KEY_BITMEX_B_LIVE"] ?? string.Empty;
                secB = appSettings["API_SECRET_BITMEX_B_LIVE"] ?? string.Empty;
                env = BitmexEnvironment.Prod;
            }

            if (!string.IsNullOrEmpty(apiA) && !string.IsNullOrEmpty(secA) && !string.IsNullOrEmpty(apiB) && !string.IsNullOrEmpty(secB))
            {

                bitmexApiServiceA = BitmexApiService.CreateDefaultApi(new BitmexAuthorization
                {
                    BitmexEnvironment = env,
                    Key = apiA,
                    Secret = secA
                });

                bitmexApiServiceB = BitmexApiService.CreateDefaultApi(new BitmexAuthorization
                {
                    BitmexEnvironment = env,
                    Key = apiB,
                    Secret = secB
                });
            }
            else
                throw new NullReferenceException("API key or secret missing"); 
        }

        private void ConfigureAccounts()
        {
            Accounts = new Dictionary<MTAccount, long>();
            Accounts.Add(MTAccount.A, 51091);
            Accounts.Add(MTAccount.B, 170591);
        }

        private void HandleTasks()
        {
            _view.OnInit = OnInit;
            _view.OnStopA = OnStopA;
            _view.OnStopB = OnStopB;
            _view.OnStartA = async () => await OnStartA();
            _view.OnStartB = async () => await OnStartB();
            _view.OnStartZoneRecovery = OnStartZoneRecovery;
            _view.OnStopZoneRecovery = OnStopZoneRecovery;
            //_view.ZRComputer = ZRComputer;
        }

        #endregion Configuration

        #region Start and stop tasks

        private decimal GetTotalBalance()
        {
            double tmp, walletBalance;

            if (double.TryParse(_view.TotalFundsA, out tmp))
            {
                walletBalance = tmp;
                if (double.TryParse(_view.TotalFundsB, out tmp))
                {
                    return (decimal)(walletBalance + tmp);
                }
                else
                    return 0;
            }
            else
                return 0;
        }

        private void OnStartZoneRecovery()
        {
            if (ZRCalculator == null)
                ZRCalculator = new Calculator();

            try
            {
                //TODO Add all checks to see if all the necessary conditions are met...

                decimal ask = 0, bid = 0;
                Dictionary<string, decimal> dict = new Dictionary<string, decimal>();

                if (Decimal.TryParse(_view.Ask, out ask) && Decimal.TryParse(_view.Bid, out bid))
                {
                    dict.Add("Ask", ask);
                    dict.Add("Bid", bid);
                }
                  
                if (ask <= 0 || bid <= 0)
                    throw new Exception($"OnStartZoneRecovery: Bid or Ask not consistent");
                else
                    ZRCalculator.UpdatePrices(dict);

                if (_communicatorA != null && _communicatorB != null)
                {
                    var WalletBalance = GetTotalBalance();
                    var stats = _orderBookStatsComputer.GetStats();

                    ZRCalculator.Initialize(
                        bitmexApiServiceA, bitmexApiServiceB,
                        WalletBalance,
                        _orderStatsHandler.GetOrderDictionary(),
                        _posStatsHandler.GetPositionDictionary(),
                        PositionStatsMutex,
                        "XBTUSD", (int)_view.MaxDepth, _view.ZoneSize, _view.MaxExposure, _view.Leverage, _view.MinProfit);

                    CalculatorEnabled = true;
                }
                else
                    throw new Exception($"OnStartZoneRecovery: Communicators are null");

                // Start...
                ZRCalculator.SwitchedOn = true;
                ZRCalculator.Evaluate();
    }
            catch(Exception exc)
            {
                Log.Error(exc.Message);
                Console.WriteLine(exc.Message);
                CalculatorEnabled = false;
            }
        }

        private void OnStopZoneRecovery()
        {
            // TODO: Proper closing of all positions...
            CalculatorEnabled = false;
            ZRCalculator.SwitchedOn = false;
            
            //if (ZRComputer != null)
            //    ZRComputer.BringDown();
        }

        private void OnInit()
        {
            Clear(ZoneRecoveryAccount.A);
            Clear(ZoneRecoveryAccount.B);
        }
        
        private async Task OnStartA()
        {
            //if (ZRComputer == null)
            //    ZRComputer = new ZoneRecoveryComputer();

            DateTime dt = DateTime.Now;
            _generalStatsHandler = new GeneralStatsHandler(dt);

            var pair = _defaultPair.ToUpper();

            _posStatsHandler = new PositionStatsHandler();

            _tradeStatsComputer = new TradeStatsComputer();
            _orderBookStatsComputer = new OrderBookStatsComputer();
            _orderStatsHandler = new OrdersStatsHandler();
            _errorStatsHandler = new ErrorStatsHandler();
            _marginStatsHandler = new MarginStatsHandler();

            var url = BitmexValues.ApiWebsocketTestnetUrl;
            _communicatorA = new BitmexWebsocketCommunicator(url);
            _clientA = new BitmexWebsocketClient(_communicatorA);

            // Enable stream listeners
            Subscribe(_clientA, MTAccount.A);

            _communicatorA.ReconnectionHappened.Subscribe(async type =>
            {
                try
                {
                    ErrorStatsMutex.WaitOne();

                    if (type != ReconnectionType.Initial)
                    {
                        Log.Warning($"Reconnected A (type: {type})");
                        _errorStatsHandler.Add2Reconnections(1, ZoneRecoveryAccount.A);
                    }
                    else if (type == ReconnectionType.Error)
                        _errorStatsHandler.Add2ErrorCnt(1, ZoneRecoveryAccount.A);
                }
                catch (Exception exc)
                {
                    Log.Error($"_communicatorA.ReconnectionHappened: {exc.Message}");
                }
                finally
                {
                    ErrorStatsMutex.ReleaseMutex();
                }

                _view.StatusA($"Reconnected (type: {type})", StatusType.Info);
                _view.ConnStartA = dt.ToString("dd-MM-yy HH:mm:ss");

                await SendSubscriptions(_clientA, pair, MTAccount.A);
            });

            _communicatorA.DisconnectionHappened.Subscribe(type =>
            {
                try
                {
                    ErrorStatsMutex.WaitOne();

                    _errorStatsHandler.Add2Disconnections(1, ZoneRecoveryAccount.A);

                    if (type == DisconnectionType.Error)
                    {
                        _errorStatsHandler.Add2ErrorCnt(1, ZoneRecoveryAccount.A);
                        _view.StatusA($"Disconnected by error, next try in {_communicatorA.ErrorReconnectTimeoutMs / 1000} sec", StatusType.Error);
                        return;
                    }
                }
                catch (Exception exc)
                {
                    Log.Error($"_communicatorA.DisconnectionHappened: {exc.Message}");
                }
                finally
                {
                    ErrorStatsMutex.ReleaseMutex();
                }
                
                _view.StatusA($"Disconnected (type: {type})", StatusType.Warning);
            });
            
            await _communicatorA.Start();

            StartPingCheckA(_clientA);
        }

        private async Task OnStartB()
        {
            var pair = _defaultPair.ToUpper();

            var url = BitmexValues.ApiWebsocketTestnetUrl;
            _communicatorB = new BitmexWebsocketCommunicator(url);
            _clientB = new BitmexWebsocketClient(_communicatorB);
            
            Subscribe(_clientB, MTAccount.B);
            
            _communicatorB.ReconnectionHappened.Subscribe(async type =>
            {
                try
                {
                    ErrorStatsMutex.WaitOne();

                    if (type != ReconnectionType.Initial)
                    {
                        Log.Warning($"Reconnected B (type: {type})");
                        _errorStatsHandler.Add2Reconnections(1, ZoneRecoveryAccount.B);
                    }
                    else if (type == ReconnectionType.Error)
                        _errorStatsHandler.Add2ErrorCnt(1, ZoneRecoveryAccount.B);
                }
                catch (Exception exc)
                {
                    Log.Error($"_communicatorB.ReconnectionHappened: {exc.Message}");
                }
                finally
                {
                    ErrorStatsMutex.ReleaseMutex();
                }

                _view.StatusB($"Reconnected (type: {type})", StatusType.Info);
                _view.ConnStartB = System.DateTime.Now.ToString("dd-MM-yy HH:mm:ss");

                await SendSubscriptions(_clientB, pair, MTAccount.B);
            });
            
            _communicatorB.DisconnectionHappened.Subscribe(type =>
            {
                try
                {
                    ErrorStatsMutex.WaitOne();

                    _errorStatsHandler.Add2Disconnections(1, ZoneRecoveryAccount.B);

                    if (type == DisconnectionType.Error)
                    {
                        _errorStatsHandler.Add2ErrorCnt(1, ZoneRecoveryAccount.B);
                        _view.StatusB($"Disconnected by error, next try in {_communicatorB.ErrorReconnectTimeoutMs / 1000} sec", StatusType.Error);
                        return;
                    }
                }
                catch (Exception exc)
                {
                    Log.Error($"_communicatorB.DisconnectionHappened: {exc.Message}");
                }
                finally
                {
                    ErrorStatsMutex.ReleaseMutex();
                }
                
                _view.StatusB($"Disconnected (type: {type})", StatusType.Warning);
            });
            
            await _communicatorB.Start();
            
            StartPingCheckB(_clientB);
        }

        private void OnStopA()
        {
            OnStopZoneRecovery();
            if (_pingSubscriptionA != null)
                _pingSubscriptionA.Dispose();
            if (_clientA != null)
                _clientA.Dispose();
            if (_communicatorA != null)
                _communicatorA.Dispose();
            _clientA = null;
            _communicatorA = null;
            
            Clear(ZoneRecoveryAccount.A);
        }

        private void OnStopB()
        {
            //OnStopZoneRecovery();
            if (_pingSubscriptionB != null)
                _pingSubscriptionB.Dispose();
            if (_clientB != null)
                _clientB.Dispose();
            if (_communicatorB != null)
                _communicatorB.Dispose();
            _clientB = null;
            _communicatorB = null;
            Clear(ZoneRecoveryAccount.B);
        }

        #endregion Start and stop tasks

        #region Subscription methods

        private void Subscribe(BitmexWebsocketClient client, MTAccount acc)
        {

            client
                .Streams
                .MarginStream
                .Select(trade => Observable.FromAsync(async () => {
                    HandleMarginResponse(trade);
                }))
                .Concat()
                .Subscribe();

            client
                .Streams
                .OrderStream
                .Select(ord => Observable.FromAsync(async () => {
                    HandleOrderResponse(ord);
                }))
                .Concat()
                .Subscribe();

            //client
            //    .Streams
            //    .PositionStream
            //    .Select(pos => Observable.FromAsync(async () => {
            //        HandlePositionResponse(pos);
            //    }))
            //    .Concat()
            //    .Subscribe();

            //client.Streams.ErrorStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleErrorResponse);

            if (acc == MTAccount.A)
            {
                client.Streams.TradesStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleTrades);
                client.Streams.BookStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleOrderBook);
                client.Streams.PongStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePongA);
                client.Streams.PositionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePositionResponse);
            }
            else
            {
                client.Streams.PongStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePongB);
            }
        }

        /// <summary>
        /// Send the requests for the subscriptions needed.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="pair"></param>
        /// <param name="acc"></param>
        /// <returns></returns>
        private async Task SendSubscriptions(BitmexWebsocketClient client, string pair, MTAccount acc)
        {
            var appSettings = ConfigurationManager.AppSettings;
            string api, sec;

            if (acc == MTAccount.A)
            {
                if (_view.isTest)
                {
                    api = appSettings["API_KEY_BITMEX_A_TEST"] ?? string.Empty;
                    sec = appSettings["API_SECRET_BITMEX_A_TEST"] ?? string.Empty;
                }
                else
                {
                    api = appSettings["API_KEY_BITMEX_A_LIVE"] ?? string.Empty;
                    sec = appSettings["API_SECRET_BITMEX_A_LIVE"] ?? string.Empty;
                }

                // Trade stats
                //await client.Send(new TradesSubscribeRequest(pair));

                // Bid and Ask
                await client.Send(new BookSubscribeRequest(pair));
                
            }
            else
            {
                if (_view.isTest)
                {
                    api = appSettings["API_KEY_BITMEX_B_TEST"] ?? string.Empty;
                    sec = appSettings["API_SECRET_BITMEX_B_TEST"] ?? string.Empty;
                }
                else
                {
                    api = appSettings["API_KEY_BITMEX_B_LIVE"] ?? string.Empty;
                    sec = appSettings["API_SECRET_BITMEX_B_LIVE"] ?? string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(api) && !string.IsNullOrEmpty(sec))
                await client.Send(new AuthenticationRequest(api, sec));

            await client.Send(new OrderSubscribeRequest());
            await client.Send(new PositionSubscribeRequest());
            await client.Send(new MarginSubscribeRequest());
            //await client.Send(new ExecutionSubscribeRequest());
        }

        #endregion Subscription methods

        #region Main response handlers
        
        private void HandleOrderResponse(OrderResponse response)
        {
            // Continue without action when there is no Data
            if (response.Data.Count() == 0)
                return;

            try
            {
                // TODO: If error occurs because of access to multiple thread due to account A and B inflow, add mutex here...

                var acc = (response.Data.First().Account == Accounts[MTAccount.A]) ? ZoneRecoveryAccount.A : ZoneRecoveryAccount.B;
                List<string> ClOrdIdList = new List<string>();

                if (response.Action == BitmexAction.Insert || response.Action == BitmexAction.Partial)
                {
                    foreach (Order o in response.Data)
                    {
                        _orderStatsHandler.HandleNewOrder(o, acc);
                        ClOrdIdList.Add(o.ClOrdId);
                    }
                }
                else if (response.Action == BitmexAction.Update)
                {
                    foreach (Order o in response.Data)
                    {
                        var statusChanged = _orderStatsHandler.HandleUpdateOrder(o, acc);
                        if(statusChanged)
                            ClOrdIdList.Add(o.ClOrdId);
                    }
                }
                else if (response.Action == BitmexAction.Delete)
                {
                    foreach (Order o in response.Data)
                    {
                        _orderStatsHandler.HandleDeleteOrder(o, acc);
                    }
                }
                else if (response.Action == BitmexAction.Undefined)
                {
                    Log.Warning($"HandleOrderResponse: order response ");

                    foreach (Order o in response.Data)
                    {
                        _orderStatsHandler.HandleNewOrder(o, acc);
                    }
                }

                // Update the UI
                var bs = _orderStatsHandler.GetBindingSource(acc);

                if (acc == ZoneRecoveryAccount.A)
                {
                    _view.bSRCOrdersA = bs;
                    _view.TabOrdersATitle = $"Orders [{bs.Count.ToString()}]";
                }
                else if (acc == ZoneRecoveryAccount.B)
                {
                    _view.bSRCOrdersB = bs;
                    _view.TabOrdersBTitle = $"Orders [{bs.Count.ToString()}]";
                }

                // Calculate...
                if (CalculatorEnabled && ZRCalculator != null && ZRCalculator.SwitchedOn)
                {
                    Dictionary<string, decimal> dict = _orderBookStatsComputer.GetBidAsk();
                    if (dict != null)
                        ZRCalculator.UpdatePrices(dict);

                    ZRCalculator.Evaluate(acc, ClOrdIdList);
                }
                
                SetZRStats();
            }
            catch(Exception exc)
            {
                string message = $"[HandleOrderResponse]: {exc.Message}";
                Log.Error(message);
                Console.WriteLine(message);
            }
        }

        private void HandlePositionResponse(PositionResponse response)
        {
            if (response.Data.Count() == 0)
                return;

            try
            {
                PositionStatsMutex.WaitOne();

                var acc = (response.Data.First().Account == Accounts[MTAccount.A]) ? ZoneRecoveryAccount.A : ZoneRecoveryAccount.B;
                
                if (response.Action == BitmexAction.Insert || response.Action == BitmexAction.Partial)
                {
                    foreach (Position p in response.Data)
                    {
                        _posStatsHandler.HandleNewPosition(p, acc);
                    }
                }
                else if (response.Action == BitmexAction.Update)
                {
                    foreach (Position p in response.Data)
                    {
                        _posStatsHandler.HandleUpdatePosition(p, acc);
                    }
                }
                else if (response.Action == BitmexAction.Delete)
                {
                    foreach (Position p in response.Data)
                    {
                        _posStatsHandler.HandleDeletePosition(p, acc);
                    }
                }
                
                var bs = _posStatsHandler.GetBindingSource(acc);

                if (acc == ZoneRecoveryAccount.A)
                {
                    _view.bSRCPosA = bs;
                    _view.TabPosATitle = $"Positions [{bs.Count.ToString()}]";
                }
                else if (acc == ZoneRecoveryAccount.B)
                {
                    _view.bSRCPosB = bs;
                    _view.TabPosBTitle = $"Positions [{bs.Count.ToString()}]";
                }
            }
            catch (Exception exc)
            {
                string message = $"[HandlePositionResponse]: {exc.Message}";
                Log.Error(message);
                Console.WriteLine(message);
            }
            finally
            {
                PositionStatsMutex.ReleaseMutex();
            }
        }
        
        private void HandleMarginResponse(MarginResponse response)
        {
            if (response.Data.Count() == 0)
                return;

            try
            {
                MarginStatsMutex.WaitOne();

                if (response.Action == BitmexAction.Partial || response.Action == BitmexAction.Insert || response.Action == BitmexAction.Update)
                {
                    MarginStats ma = null;
                    MarginStats mb = null;
                    
                    foreach (Margin m in response.Data)
                    {
                        if (m.Account == Accounts[MTAccount.A])
                        {
                            _view.AccountAID = m.Account.ToString();

                            _marginStatsHandler.UpdateBalances(ZoneRecoveryAccount.A, m.WalletBalance, m.MarginBalance, m.AvailableMargin);
                            ma = _marginStatsHandler.GetMarginBalances(ZoneRecoveryAccount.A);
                            
                            _view.TotalFundsA = BitmexConverter.ConvertToBtc("XBt", ma.WalletBalance).ToString();
                            _view.AvailableFundsA = BitmexConverter.ConvertToBtc("XBt", ma.MarginBalance).ToString();
                            _view.MarginBalanceA = BitmexConverter.ConvertToBtc("XBt", ma.AvailableMargin).ToString();
                        }
                        else if (m.Account == Accounts[MTAccount.B])
                        {
                            _view.AccountBID = m.Account.ToString();

                            _marginStatsHandler.UpdateBalances(ZoneRecoveryAccount.B, m.WalletBalance, m.MarginBalance, m.AvailableMargin);
                            mb = _marginStatsHandler.GetMarginBalances(ZoneRecoveryAccount.B);

                            _view.TotalFundsB = BitmexConverter.ConvertToBtc("XBt", mb.WalletBalance).ToString();
                            _view.AvailableFundsB = BitmexConverter.ConvertToBtc("XBt", mb.MarginBalance).ToString();
                            _view.MarginBalanceB = BitmexConverter.ConvertToBtc("XBt", mb.AvailableMargin).ToString();
                        }
                    }

                    var a = string.IsNullOrEmpty(_view.TotalFundsA) ? 0.0 : double.Parse(_view.TotalFundsA);
                    var b = string.IsNullOrEmpty(_view.TotalFundsB) ? 0.0 : double.Parse(_view.TotalFundsB);
                    
                    _view.CashImbalance = "L " + Math.Round((a / (a + b) * 100), 2).ToString() + " - S " + Math.Round((b / (b + a) * 100), 2).ToString();
                }
            }
            catch (Exception exc)
            {
                string message = $"[HandleMargin]: {exc.Message}";
                Log.Error(message);
                Console.WriteLine(message);
            }
            finally
            {
                MarginStatsMutex.ReleaseMutex();
            }
        }

        #endregion Main response handlers

        #region Copied stuff

        private void SetZRStats()
        {
            //if (ZRComputer != null)
            //{
            //    _view.Direction = ZRComputer.CurrentDirection.ToString();
            //    _view.UnitSize = ZRComputer.UnitSize.ToString();
            //    _view.ZRStatus = ZRComputer.CurrentStatus.ToString();
            //    _view.ZRIndex = ZRComputer.CurrentZRPosition.ToString();
            //}
            if (CalculatorEnabled)
            {
                var stats = ZRCalculator.GetStats();
                _view.Direction = stats.Direction.ToString();
                _view.UnitSize = stats.UnitSize.ToString();
                //_view.ZRStatus = ZRCalculator.CurrentStatus.ToString();
                //_view.ZRIndex = ZRCalculator.CurrentZRPosition.ToString();
            }
        }

        private void HandleOrderBook(BookResponse response)
        {
            _orderBookStatsComputer.HandleOrderBook(response);

            var stats = _orderBookStatsComputer.GetStats();
            if (stats == OrderBookStats.NULL)
                return;

            _view.Bid = stats.Bid.ToString("#.0");
            _view.Ask = stats.Ask.ToString("#.0");

            _view.BidAmount = $"{stats.BidAmountPerc:###}%{Environment.NewLine}{FormatToMilions(stats.BidAmount)}";
            _view.AskAmount = $"{stats.AskAmountPerc:###}%{Environment.NewLine}{FormatToMilions(stats.AskAmount)}";
        }

        private void HandleTrades(TradeResponse response)
        {
            if (response.Action != BitmexAction.Insert && response.Action != BitmexAction.Partial)
                return;

            foreach (var trade in response.Data)
            {
                //Log.Information($"Received [{trade.Side}] trade, price: {trade.Price}, amount: {trade.Size}");
                _tradeStatsComputer.HandleTrade(trade);
            }

            FormatTradesStats(_view.Trades1Min, _tradeStatsComputer.GetStatsFor(1));
            FormatTradesStats(_view.Trades5Min, _tradeStatsComputer.GetStatsFor(5));
            FormatTradesStats(_view.Trades15Min, _tradeStatsComputer.GetStatsFor(15));
            FormatTradesStats(_view.Trades1Hour, _tradeStatsComputer.GetStatsFor(60));
            FormatTradesStats(_view.Trades24Hours, _tradeStatsComputer.GetStatsFor(60 * 24));
        }

        private void FormatTradesStats(Action<string, Side> setAction, TradeStats trades)
        {
            if (trades == TradeStats.NULL)
                return;

            if (trades.BuysPerc >= trades.SellsPerc)
            {
                setAction($"{trades.BuysPerc:###}% buys{Environment.NewLine}{trades.TotalCount}", Side.Buy);
                return;
            }
            setAction($"{trades.SellsPerc:###}% sells{Environment.NewLine}{trades.TotalCount}", Side.Sell);
        }

        private string FormatToMilions(double amount)
        {
            var milions = amount / 1000000;
            return $"{_currency}{milions:#.00} M";
        }

        private void StartPingCheckA(BitmexWebsocketClient client)
        {
            _pingSubscriptionA = Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Subscribe(async x =>
                {
                    _pingRequestA = DateTime.UtcNow;
                    await client.Send(new PingRequest());
                });
        }

        private void StartPingCheckB(BitmexWebsocketClient client)
        {
            _pingSubscriptionB = Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Subscribe(async x =>
                {
                    _pingRequestB = DateTime.UtcNow;
                    await client.Send(new PingRequest());
                });
        }

        private void HandlePongA(PongResponse pong)
        {
            var current = DateTime.UtcNow;
            ComputePing(current, _pingRequestA, MTAccount.A);
        }

        private void HandlePongB(PongResponse pong)
        {
            var current = DateTime.UtcNow;
            ComputePing(current, _pingRequestB, MTAccount.B);
        }

        private void ComputePing(DateTime current, DateTime before, MTAccount acc)
        {
            try
            {
                ErrorStatsMutex.WaitOne();

                var diff = current.Subtract(before);

                if (acc == MTAccount.A)
                {
                    _view.PingL = $"{diff.TotalMilliseconds:###} ms";
                    _view.StatusA("Connected", StatusType.Info);
                    _view.DisconnectionsA = $"{_errorStatsHandler.GetDisconnections(ZoneRecoveryAccount.A)}";
                    _view.ReconnectionsA = $"{_errorStatsHandler.GetReconnections(ZoneRecoveryAccount.A)}";
                    _view.ErrorsCounterA = $"{_errorStatsHandler.GetErrorCnt(ZoneRecoveryAccount.A)}";
                }
                else
                {
                    _view.PingS = $"{diff.TotalMilliseconds:###} ms";
                    _view.StatusB("Connected", StatusType.Info);
                    _view.DisconnectionsB = $"{_errorStatsHandler.GetDisconnections(ZoneRecoveryAccount.B)}";
                    _view.ReconnectionsB = $"{_errorStatsHandler.GetReconnections(ZoneRecoveryAccount.B)}";
                    _view.ErrorsCounterB = $"{_errorStatsHandler.GetErrorCnt(ZoneRecoveryAccount.B)}";
                }
                _view.TimeConnected = $"{_generalStatsHandler.GetTimeActive()}";
                _view.ErrorsCounterTotal = $"{_errorStatsHandler.GetErrorCnt(ZoneRecoveryAccount.A) + _errorStatsHandler.GetErrorCnt(ZoneRecoveryAccount.B)}";
            }
            catch (Exception exc)
            {
                Log.Error($"ComputePing: {exc.Message}");
            }
            finally
            {
                ErrorStatsMutex.ReleaseMutex();
            }
        }

        private void Clear(ZoneRecoveryAccount acc)
        {
            _view.Bid = string.Empty;
            _view.Ask = string.Empty;
            _view.BidAmount = string.Empty;
            _view.AskAmount = string.Empty;
            _view.Trades1Min(string.Empty, Side.Buy);
            _view.Trades5Min(string.Empty, Side.Buy);
            _view.Trades15Min(string.Empty, Side.Buy);
            _view.Trades1Hour(string.Empty, Side.Buy);
            _view.Trades24Hours(string.Empty, Side.Buy);

            _view.TimeConnected = string.Empty;
            _view.ErrorsCounterTotal = string.Empty;
            _view.CashImbalance = string.Empty;

            _view.Direction = string.Empty;
            _view.UnitSize = string.Empty;
            _view.ZRStatus = string.Empty;
            _view.ZRIndex = string.Empty;

            _view.Leverage = 1;
            _view.MaxDepth = 3;
            _view.ZoneSize = 20;
            _view.MaxExposure = (decimal)0.05;
            _view.MinProfit = (decimal)0.05;

            if (acc == ZoneRecoveryAccount.A)
            {
                _view.AccountAID = string.Empty;
                _view.AvailableFundsA = string.Empty;
                _view.ConnStartA = string.Empty;
                _view.ConnStatusA = string.Empty;
                _view.PingL = string.Empty;
                _view.TabOrdersATitle = "Orders [0]";
                _view.TabPosATitle = "Positions [0]";
                _view.TotalFundsA = string.Empty;
                _view.MarginBalanceA = string.Empty;
                _view.bSRCOrdersA = new BindingSource();
                _view.bSRCPosA = new BindingSource();

                _view.DisconnectionsA = string.Empty;
                _view.ReconnectionsA = string.Empty;
                _view.ErrorsCounterA = string.Empty;

                _view.TotalCostA = string.Empty;
                _view.PNLA = string.Empty;
            }
            else
            {
                _view.AccountBID = string.Empty;
                _view.AvailableFundsB = string.Empty;
                _view.ConnStartB = string.Empty;
                _view.ConnStatusB = string.Empty;
                _view.PingS = string.Empty;
                _view.TabOrdersBTitle = "Orders [0]";
                _view.TabPosBTitle = "Positions [0]";
                _view.TotalFundsB = string.Empty;
                _view.MarginBalanceB = string.Empty;
                _view.bSRCOrdersB = new BindingSource();
                _view.bSRCPosB = new BindingSource();

                _view.DisconnectionsB = string.Empty;
                _view.ReconnectionsB = string.Empty;
                _view.ErrorsCounterB = string.Empty;

                _view.TotalCostB = string.Empty;
                _view.PNLB = string.Empty;
            }
        }

        //private delegate void ShowMessageBoxDelegate(string strMessage, string strCaption/*, MessageBoxButton enmButton, MessageBoxImage enmImage*/);

        //// Method invoked on a separate thread that shows the message box.
        //private static void ShowMessageBox(string strMessage, string strCaption/*, MessageBoxButton enmButton, MessageBoxImage enmImage*/)
        //{
        //    System.Windows.MessageBox.Show(strMessage, strCaption/*, enmButton, enmImage*/);
        //}

        //// Shows a message box from a separate worker thread.
        //public static void ShowMessageBoxAsync(string strMessage, string strCaption/*, MessageBoxButton enmButton, MessageBoxImage enmImage*/)
        //{
        //    ShowMessageBoxDelegate caller = new ShowMessageBoxDelegate(ShowMessageBox);
        //    caller.Invoke(strMessage, strCaption/*, enmButton, enmImage, null, null*/);
        //}

        #endregion Copied stuff
    }
}