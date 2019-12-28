using System;
using System.Windows.Forms;
using Bitmex.Client.Websocket.Responses.Orders;
using Bitmex.Client.Websocket.Responses.Positions;

namespace MoneyTron
{
    interface IMTMainForm
    {
        string AccountAID { get; set; }
        string AccountBID { get; set; }
        string ConnStatusA { get; set; }
        string ConnStatusB { get; set; }
        string ConnStartA { get; set; }
        string ConnStartB { get; set; }
        string Bid { get; set; }
        string Ask { get; set; }
        string BidAmount { get; set; }
        string AskAmount { get; set; }
        string TotalFundsA { get; set; }
        string TotalFundsB { get; set; }
        string AvailableFundsA { get; set; }
        string AvailableFundsB { get; set; }
        string MarginBalanceA { get; set; }
        string MarginBalanceB { get; set; }
        string TabPosBTitle { get; set; }
        string TabPosATitle { get; set; }
        string TabOrdersATitle { get; set; }
        string TabOrdersBTitle { get; set; }
        string PingL { get; set; }
        string PingS { get; set; }
        
        BindingSource bSRCOrdersA { get; set; }
        BindingSource bSRCOrdersB { get; set; }
        BindingSource bSRCPosA { get; set; }
        BindingSource bSRCPosB { get; set; }

        Action OnInitA { get; set; }
        Action OnStartA { set; }
        Action OnStopA { set; }
        
        Action OnStartB { set; }
        Action OnStopB { set; }
        
        void Trades1Min(string value, Side side);
        void Trades5Min(string value, Side side);
        void Trades15Min(string value, Side side);
        void Trades1Hour(string value, Side side);
        void Trades24Hours(string value, Side side);

        void StatusA(string value, StatusType type);
        void StatusB(string value, StatusType type);
    }

    public enum Side
    {
        Buy,
        Sell
    }

    public enum StatusType
    {
        Info,
        Warning,
        Error
    }

    public enum MTAccount
    {
        A,
        B
    }

    public enum MTActionType
    {
        Insert,
        Update,
        Delete
    }
}
