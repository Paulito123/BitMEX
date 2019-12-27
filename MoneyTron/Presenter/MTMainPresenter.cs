using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
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

namespace MoneyTron.Presenter
{
    class MTMainPresenter
    {
        private readonly IMTMainForm _view;

        private OrderBookStatsComputer _orderBookStatsComputer;
        private TradeStatsComputer _tradeStatsComputer;
        private OrdersStatsHandler _orderStatsHandlerA;
        private OrdersStatsHandler _orderStatsHandlerB;

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="view"></param>
        public MTMainPresenter(IMTMainForm view)
        {
            _view = view;

            ConfigureAccounts();
            HandleCommands();
        }

        private void ConfigureAccounts()
        {
            Accounts = new Dictionary<MTAccount, long>();
            Accounts.Add(MTAccount.A, 51091);
            Accounts.Add(MTAccount.B, 170591);
        }

        private void HandleCommands()
        {
            _view.OnInitA = OnInitA;
            _view.OnStartA = async () => await OnStartA();
            _view.OnStopA = OnStopA;
            _view.OnInitB = OnInitB;
            _view.OnStartB = async () => await OnStartB();
            _view.OnStopB = OnStopB;
        }

        private void OnInitA()
        {
            Clear(MTAccount.A);
        }

        private void OnInitB()
        {
            Clear(MTAccount.B);
        }

        private async Task OnStartA()
        {
            var pair = _defaultPair.ToUpper();

            _tradeStatsComputer = new TradeStatsComputer();
            _orderBookStatsComputer = new OrderBookStatsComputer();
            _orderStatsHandlerA = new OrdersStatsHandler();
            _orderStatsHandlerB = new OrdersStatsHandler();

            var url = BitmexValues.ApiWebsocketTestnetUrl;
            _communicatorA = new BitmexWebsocketCommunicator(url);
            _clientA = new BitmexWebsocketClient(_communicatorA);

            Subscribe(_clientA, MTAccount.A);

            _communicatorA.ReconnectionHappened.Subscribe(async type =>
            {
                _view.StatusA($"Reconnected (type: {type})", StatusType.Info);
                _view.ConnStartA = System.DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
                await SendSubscriptions(_clientA, pair, MTAccount.A);
            });

            _communicatorA.DisconnectionHappened.Subscribe(type =>
            {
                if (type == DisconnectionType.Error)
                {
                    _view.StatusA($"Disconnected by error, next try in {_communicatorA.ErrorReconnectTimeoutMs / 1000} sec", StatusType.Error);
                    return;
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
                _view.StatusB($"Reconnected (type: {type})", StatusType.Info);
                _view.ConnStartB = System.DateTime.Now.ToString("dd-MM-yy HH:mm:ss");
                await SendSubscriptions(_clientB, pair, MTAccount.B);
            });
            
            _communicatorB.DisconnectionHappened.Subscribe(type =>
            {
                if (type == DisconnectionType.Error)
                {
                    _view.StatusB($"Disconnected by error, next try in {_communicatorB.ErrorReconnectTimeoutMs / 1000} sec", StatusType.Error);
                    return;
                }
                _view.StatusB($"Disconnected (type: {type})", StatusType.Warning);
            });
            
            await _communicatorB.Start();
            
            StartPingCheckB(_clientB);
        }

        private void OnStopA()
        {
            if (_pingSubscriptionA != null)
                _pingSubscriptionA.Dispose();
            if (_clientA != null)
                _clientA.Dispose();
            if (_communicatorA != null)
                _communicatorA.Dispose();
            _clientA = null;
            _communicatorA = null;
            Clear(MTAccount.A);
        }

        private void OnStopB()
        {
            if (_pingSubscriptionB != null)
                _pingSubscriptionB.Dispose();
            if (_clientB != null)
                _clientB.Dispose();
            if (_communicatorB != null)
                _communicatorB.Dispose();
            _clientB = null;
            _communicatorB = null;
            Clear(MTAccount.B);
        }

        private void Subscribe(BitmexWebsocketClient client, MTAccount acc)
        {

            client
                .Streams
                .MarginStream
                .Select(trade => Observable.FromAsync(async () => {
                    HandleMargin(trade);
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


            //client.Streams.MarginStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleMargin);
            //client.Streams.OrderStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleOrderResponse);

            //client.Streams.AuthenticationStream.ObserveOn(TaskPoolScheduler.Default).Subscribe();
            //client.Streams.ErrorStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleErrorResponse);

            if (acc == MTAccount.A)
            {
                //client.Streams.PositionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePositionResponseA);
                //client.Streams.ExecutionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleExecutionResponseA);
                //client.Streams.TradesStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleTrades);
                //client.Streams.BookStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleOrderBook);
                //client.Streams.OrderStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(blablaA);
                client.Streams.PongStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePongA);
            }
            else
            {
                //client.Streams.PositionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePositionResponseB);
                //client.Streams.ExecutionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleExecutionResponseB);
                //client.Streams.OrderStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(blablaB);
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
            if (acc == MTAccount.A)
            {
                await client.Send(new AuthenticationRequest("QbpGewiOyIYMbyQ-ieaTKfOJ", "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf"));
                //await client.Authenticate("QbpGewiOyIYMbyQ-ieaTKfOJ", "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf");
                //await client.Send(new TradesSubscribeRequest(pair));
                //await client.Send(new BookSubscribeRequest(pair));
            }
            else
            {
                await client.Send(new AuthenticationRequest("xEuMT-y7ffwxrvHA2yDwL1bZ", "3l0AmJz7l3P47-gK__LwgZQQ23uOKCFhYJG4HeTLlGXadRm6"));
                //await client.Authenticate("xEuMT-y7ffwxrvHA2yDwL1bZ", "3l0AmJz7l3P47-gK__LwgZQQ23uOKCFhYJG4HeTLlGXadRm6");
            }

            await client.Send(new OrderSubscribeRequest());
            //await client.Send(new PositionSubscribeRequest());
            await client.Send(new MarginSubscribeRequest());
            //await client.Send(new ExecutionSubscribeRequest());
        }

        private void HandleErrorResponse(ErrorResponse response)
        {

        }
        
        private void HandleOrderResponse(OrderResponse response)
        {
            Log.Information($"Order received with action [{response.Action}]");

            var acc = response.Data.First().Account;

            try
            {
                if (acc == Accounts[MTAccount.A])
                {
                    foreach (Order o in response.Data)
                    {
                        _orderStatsHandlerA.HandleOrder(o);
                    }

                    var bs = _orderStatsHandlerA.GetBindingSource();
                    _view.bSRCOrdersA = bs;
                }
                else if (acc == Accounts[MTAccount.B])
                {
                    foreach (Order o in response.Data)
                    {
                        _orderStatsHandlerB.HandleOrder(o);
                    }

                    var bs = _orderStatsHandlerB.GetBindingSource();
                    _view.bSRCOrdersB = bs;
                }
            }
            catch(Exception exc)
            {
                Log.Error(exc.Message);
            }
        }

        private void HandlePositionResponseA(PositionResponse response)
        {
            //_view.DebugOutput = response.Data.ToString();
        }

        private void HandlePositionResponseB(PositionResponse response)
        {
            //_view.DebugOutput = response.Data.ToString();
        }

        private void HandleExecutionResponseA(ExecutionResponse response)
        {
            //_view.DebugOutput = response.Data.ToString();
        }

        private void HandleExecutionResponseB(ExecutionResponse response)
        {
            //_view.DebugOutput = response.Data.ToString();
        }

        private void HandleMargin(MarginResponse response)
        {
            Log.Information($"Margin received with action [{response.Action}]");

            if (response.Action == BitmexAction.Partial || response.Action == BitmexAction.Insert || response.Action == BitmexAction.Update)
            {
                foreach(Margin m in response.Data)
                {
                    if (m.Account == Accounts[MTAccount.A])
                    {
                        if (m.WalletBalance > 0)
                            _view.TotalFundsA = BitmexConverter.ConvertToBtc("XBt", m.WalletBalance ?? 0).ToString();
                        if (m.MarginBalance > 0)
                            _view.AvailableFundsA = BitmexConverter.ConvertToBtc("XBt", m.MarginBalance ?? 0).ToString();
                        if (m.AvailableMargin > 0)
                            _view.MarginBalanceA = BitmexConverter.ConvertToBtc("XBt", m.AvailableMargin ?? 0).ToString();
                        _view.AccountAID = m.Account.ToString();
                    }
                    else if (m.Account == Accounts[MTAccount.B])
                    {
                        if (m.WalletBalance > 0)
                            _view.TotalFundsB = BitmexConverter.ConvertToBtc("XBt", m.WalletBalance ?? 0).ToString();
                        if (m.MarginBalance > 0)
                            _view.AvailableFundsB = BitmexConverter.ConvertToBtc("XBt", m.MarginBalance ?? 0).ToString();
                        if (m.AvailableMargin > 0)
                            _view.MarginBalanceB = BitmexConverter.ConvertToBtc("XBt", m.AvailableMargin ?? 0).ToString();
                        _view.AccountBID = m.Account.ToString();
                    }
                }
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
            var diff = current.Subtract(before);
            if (acc == MTAccount.A)
            {
                _view.PingL = $"{diff.TotalMilliseconds:###} ms";
                _view.StatusA("Connected", StatusType.Info);
            }
            else
            {
                _view.PingS = $"{diff.TotalMilliseconds:###} ms";
                _view.StatusB("Connected", StatusType.Info);
            }
        }

        private void Clear(MTAccount acc)
        {
            _view.DebugOutput = "...";

            if (acc == MTAccount.A)
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

                _view.Bid = string.Empty;
                _view.Ask = string.Empty;
                _view.BidAmount = string.Empty;
                _view.AskAmount = string.Empty;
                _view.Trades1Min(string.Empty, Side.Buy);
                _view.Trades5Min(string.Empty, Side.Buy);
                _view.Trades15Min(string.Empty, Side.Buy);
                _view.Trades1Hour(string.Empty, Side.Buy);
                _view.Trades24Hours(string.Empty, Side.Buy);
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
            }
        }


        private delegate void ShowMessageBoxDelegate(string strMessage, string strCaption/*, MessageBoxButton enmButton, MessageBoxImage enmImage*/);

        // Method invoked on a separate thread that shows the message box.
        private static void ShowMessageBox(string strMessage, string strCaption/*, MessageBoxButton enmButton, MessageBoxImage enmImage*/)
        {
            System.Windows.MessageBox.Show(strMessage, strCaption/*, enmButton, enmImage*/);
        }

        // Shows a message box from a separate worker thread.
        public static void ShowMessageBoxAsync(string strMessage, string strCaption/*, MessageBoxButton enmButton, MessageBoxImage enmImage*/)
        {
            ShowMessageBoxDelegate caller = new ShowMessageBoxDelegate(ShowMessageBox);
            caller.Invoke(strMessage, strCaption/*, enmButton, enmImage, null, null*/);
        }

    }
}