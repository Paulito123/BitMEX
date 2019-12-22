using System;

namespace MoneyTron
{
    interface IMTMainForm
    {
        string AccountATitle { get; set; }
        string AccountBTitle { get; set; }
        string AccountAID { get; set; }
        string AccountBID { get; set; }
        string ConnStatusA { get; set; }
        string ConnStatusB { get; set; }
        string ConnStartA { get; set; }
        string ConnStartB { get; set; }
        string MarkPriceA { get; set; }
        string MarkPriceB { get; set; }
        string IndexPriceA { get; set; }
        string IndexPriceB { get; set; }
        string TotalFundsA { get; set; }
        string TotalFundsB { get; set; }
        string AvailableFundsA { get; set; }
        string AvailableFundsB { get; set; }
        string TabPosSTitle { get; set; }
        string TabPosLTitle { get; set; }
        string TabOrdersSTitle { get; set; }
        string TabOrdersLTitle { get; set; }
        string PingL { get; set; }
        string PingS { get; set; }

        Action OnInit { get; set; }
        Action OnStart { set; }
        Action OnStop { set; }

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
}
