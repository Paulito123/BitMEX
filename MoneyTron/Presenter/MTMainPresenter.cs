using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Communicator;
using Bitmex.Client.Websocket.Requests;
using Bitmex.Client.Websocket.Responses;
using Bitmex.Client.Websocket.Responses.Books;
using Bitmex.Client.Websocket.Responses.Trades;
using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Websockets;
using Websocket.Client;

namespace MoneyTron.Presenter
{
    class MTMainPresenter
    {
        private readonly IMTMainForm _view;
        
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

            HandleCommands();
        }

        private void HandleCommands()
        {
            _view.OnInit = OnInit;
            _view.OnStart = async () => await OnStart();
            _view.OnStop = OnStop;
        }

        private void OnInit()
        {
            Clear();
        }

        private async Task OnStart()
        {
            var pair = _defaultPair;
            pair = pair.ToUpper();
          
            var url = BitmexValues.ApiWebsocketTestnetUrl;
            _communicatorA = new BitmexWebsocketCommunicator(url);
            _communicatorB = new BitmexWebsocketCommunicator(url);
            _clientA = new BitmexWebsocketClient(_communicatorA);
            _clientB = new BitmexWebsocketClient(_communicatorB);

            Subscribe(_clientA, MTAccount.A);
            Subscribe(_clientB, MTAccount.B);

            _communicatorA.ReconnectionHappened.Subscribe(async type =>
            {
                _view.StatusA($"Reconnected (type: {type})", StatusType.Info);
                await SendSubscriptions(_clientA, pair);
            });

            _communicatorB.ReconnectionHappened.Subscribe(async type =>
            {
                _view.StatusB($"Reconnected (type: {type})", StatusType.Info);
                await SendSubscriptions(_clientB, pair);
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

            _communicatorB.DisconnectionHappened.Subscribe(type =>
            {
                if (type == DisconnectionType.Error)
                {
                    _view.StatusB($"Disconnected by error, next try in {_communicatorB.ErrorReconnectTimeoutMs / 1000} sec", StatusType.Error);
                    return;
                }
                _view.StatusB($"Disconnected (type: {type})", StatusType.Warning);
            });

            await _communicatorA.Start();
            await _communicatorB.Start();

            StartPingCheck(_clientA, MTAccount.A);
            StartPingCheck(_clientB, MTAccount.B);
        }

        private void OnStop()
        {
            _pingSubscriptionA.Dispose();
            _pingSubscriptionB.Dispose();
            _clientA.Dispose();
            _clientB.Dispose();
            _communicatorA.Dispose();
            _communicatorB.Dispose();
            _clientA = null;
            _clientB = null;
            _communicatorA = null;
            _communicatorB = null;
            Clear();
        }

        private void Subscribe(BitmexWebsocketClient client, MTAccount acc)
        {
            if (acc == MTAccount.A)
            {
                //client.Streams.InfoStream.ObserveOn(TaskPoolScheduler.Default).Subscribe();
                client.Streams.PositionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePositionsA);
                //client.Streams.TradesStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleTrades);
                //client.Streams.BookStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleOrderBook);
                client.Streams.PongStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePongA);
            }
            else
            {
                //client.Streams.InfoStream.ObserveOn(TaskPoolScheduler.Default).Subscribe();
                client.Streams.PositionStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePositionsB);
                //client.Streams.TradesStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleTrades);
                //client.Streams.BookStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandleOrderBook);
                client.Streams.PongStream.ObserveOn(TaskPoolScheduler.Default).Subscribe(HandlePongB);
            }

        }

        private async Task SendSubscriptions(BitmexWebsocketClient client, string pair)
        {
            await client.Send(new TradesSubscribeRequest(pair));
            //await client.Send(new BookSubscribeRequest(pair));
        }

        private void HandlePositionsA(PositionResponse response)
        {
            
        }

        private void HandlePositionsB(PositionResponse response)
        {

        }

        private void HandleTrades(TradeResponse response)
        {
            if (response.Action != BitmexAction.Insert && response.Action != BitmexAction.Partial)
                return;

            //foreach (var trade in response.Data)
            //{
            //    Log.Information($"Received [{trade.Side}] trade, price: {trade.Price}, amount: {trade.Size}");
            //    _tradeStatsComputer.HandleTrade(trade);
            //}

            //FormatTradesStats(_view.Trades1Min, _tradeStatsComputer.GetStatsFor(1));
            //FormatTradesStats(_view.Trades5Min, _tradeStatsComputer.GetStatsFor(5));
            //FormatTradesStats(_view.Trades15Min, _tradeStatsComputer.GetStatsFor(15));
            //FormatTradesStats(_view.Trades1Hour, _tradeStatsComputer.GetStatsFor(60));
            //FormatTradesStats(_view.Trades24Hours, _tradeStatsComputer.GetStatsFor(60 * 24));
        }

        //private void FormatTradesStats(Action<string, Side> setAction, TradeStats trades)
        //{
        //    if (trades == TradeStats.NULL)
        //        return;

        //    if (trades.BuysPerc >= trades.SellsPerc)
        //    {
        //        setAction($"{trades.BuysPerc:###}% buys{Environment.NewLine}{trades.TotalCount}", Side.Buy);
        //        return;
        //    }
        //    setAction($"{trades.SellsPerc:###}% sells{Environment.NewLine}{trades.TotalCount}", Side.Sell);
        //}

        //private void HandleOrderBook(BookResponse response)
        //{
        //    _orderBookStatsComputer.HandleOrderBook(response);

        //    var stats = _orderBookStatsComputer.GetStats();
        //    if (stats == OrderBookStats.NULL)
        //        return;

        //    _view.Bid = stats.Bid.ToString("#.0");
        //    _view.Ask = stats.Ask.ToString("#.0");

        //    _view.BidAmount = $"{stats.BidAmountPerc:###}%{Environment.NewLine}{FormatToMilions(stats.BidAmount)}";
        //    _view.AskAmount = $"{stats.AskAmountPerc:###}%{Environment.NewLine}{FormatToMilions(stats.AskAmount)}";
        //}

        //private string FormatToMilions(double amount)
        //{
        //    var milions = amount / 1000000;
        //    return $"{_currency}{milions:#.00} M";
        //}

        private void StartPingCheck(BitmexWebsocketClient client, MTAccount acc)
        {
            if (acc == MTAccount.A)
            {
                _pingSubscriptionA = Observable
                    .Interval(TimeSpan.FromSeconds(5))
                    .Subscribe(async x =>
                    {
                        _pingRequestA = DateTime.UtcNow;
                        await client.Send(new PingRequest());
                    });
            }
            else
            {
                _pingSubscriptionB = Observable
                    .Interval(TimeSpan.FromSeconds(5))
                    .Subscribe(async x =>
                    {
                        _pingRequestB = DateTime.UtcNow;
                        await client.Send(new PingRequest());
                    });
            }
            
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

        private void Clear()
        {
            _view.AccountAID = string.Empty;
            _view.AccountBID = string.Empty;
            _view.AccountATitle = string.Empty;
            _view.AccountBTitle = string.Empty;
            _view.AvailableFundsA = string.Empty;
            _view.AvailableFundsB = string.Empty;
            _view.ConnStartA = string.Empty;
            _view.ConnStartB = string.Empty;
            _view.ConnStatusA = string.Empty;
            _view.ConnStatusB = string.Empty;
            _view.IndexPriceA = string.Empty;
            _view.IndexPriceB = string.Empty;
            _view.MarkPriceA = string.Empty;
            _view.MarkPriceB = string.Empty;
            _view.PingL = string.Empty;
            _view.PingS = string.Empty;
            _view.TabOrdersLTitle = "Orders [0]";
            _view.TabOrdersSTitle = "Orders [0]";
            _view.TabPosLTitle = "Positions [0]";
            _view.TabPosSTitle = "Positions [0]";
            _view.TotalFundsA = string.Empty;
            _view.TotalFundsB = string.Empty;
        }
    }
}
