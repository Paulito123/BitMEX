using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMEX;
using BitMEX.JSONClass.Order;
using IO.Swagger.Api;
using IO.Swagger.Client;
using IO.Swagger.Model;

namespace BitMEX.TestForm
{
    public partial class Form1 : Form
    {
        private MordoR mconn;

        public Form1()
        {
            InitializeComponent();
            mconn = new MordoR();
            TBMarketOrder.Text = "XBTUSD";
        }

        private void btnMarketOrder_Click(object sender, EventArgs e)
        {
            if (NUDMarketOrderQuantity.Value >= 1 || NUDMarketOrderQuantity.Value <= -1)
            {
                try
                {
                    string lastOrderID;
                    object outcome = mconn.MarketOrder(TBMarketOrder.Text.ToString(), out lastOrderID, Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value));

                    switch (outcome.GetType().ToString())
                    {
                        case "BitMEX.JSONClass.Order.OrderResponse":
                            OutputLabel.Text = ((OrderResponse)outcome).ClOrdId.ToString();
                            MessageBox.Show(((OrderResponse)outcome).ClOrdId.ToString());
                            break;
                        case "BitMEX.JSONClass.Order.OrderError":
                            MessageBox.Show(((OrderError)outcome).Error.Message.ToString());
                            break;
                        default:
                            MessageBox.Show("bla");
                            break;
                    }
                }
                catch (Exception exc)
                {
                    // Catch all external exceptions like connection issues etc.
                    MessageBox.Show(exc.Message.ToString());
                }
            }
        }

        private void btnSwagger_Click(object sender, EventArgs e)
        {
            // Examples: https://github.com/BitMEX/api-connectors/blob/master/auto-generated/csharp/docs/OrderApi.md#ordernew

            // Configure API key authorization: apiKey
            Configuration.Default.AddApiKey("api-key", "rTAFXRKn2dLARuG_t1dDOtgI");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("api-key", "Bearer");
            // Configure API key authorization: apiNonce
            Configuration.Default.AddApiKey("api-nonce", "rTAFXRKn2dLARuG_t1dDOtgI");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("api-nonce", "Bearer");
            // Configure API key authorization: apiSignature
            Configuration.Default.AddApiKey("api-signature", "K2LmL6aTbj8eW_LVj7OLa7iA6eZa8TJMllh3sjCynV4fpnMr");
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.AddApiKeyPrefix("api-signature", "Bearer");
            // Configure API key authorization: apiExpires
            Configuration.Default.AddDefaultHeader("api-expires", GetExpires().ToString());
            // Uncomment below to setup prefix (e.g. Bearer) for API key, if needed
            // Configuration.Default.ApiKeyPrefix.Add("api-expires", "Bearer");

            var apiInstance = new OrderApi();
            var symbol = TBMarketOrder.Text.ToString();  // string | Instrument symbol. e.g. 'XBTUSD'.
            var side = (Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value) >= 0) ? "Buy" : "Sell";  // string | Order side. Valid options: Buy, Sell. Defaults to 'Buy' unless `orderQty` or `simpleOrderQty` is negative. (optional) 
            //var simpleOrderQty = 0;  // double? | Order quantity in units of the underlying instrument (i.e. Bitcoin). (optional,Deprecated) 
            var orderQty = Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value);  // decimal? | Order quantity in units of the instrument (i.e. contracts). (optional) 
            //var price = 1.2;  // double? | Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders. (optional) 
            //var displayQty = 8.14;  // decimal? | Optional quantity to display in the book. Use 0 for a fully hidden order. (optional) 
            //var stopPx = 1.2;  // double? | Optional trigger price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders. Use a price below the current price for stop-sell orders and buy-if-touched orders. Use `execInst` of 'MarkPrice' or 'LastPrice' to define the current price used for triggering. (optional) 
            var clOrdID = "123456789";  // string | Optional Client Order ID. This clOrdID will come back on the order and any related executions. (optional) 
            //var clOrdLinkID = clOrdLinkID_example;  // string | Optional Client Order Link ID for contingent orders. (optional,Deprecated) 
            //var pegOffsetValue = 1.2;  // double? | Optional trailing offset from the current price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders; use a negative offset for stop-sell orders and buy-if-touched orders. Optional offset from the peg price for 'Pegged' orders. (optional) 
            //var pegPriceType = pegPriceType_example;  // string | Optional peg price type. Valid options: LastPeg, MidPricePeg, MarketPeg, PrimaryPeg, TrailingStopPeg. (optional) 
            var ordType = "Market";  // string | Order type. Valid options: Market, Limit, Stop, StopLimit, MarketIfTouched, LimitIfTouched, MarketWithLeftOverAsLimit, Pegged. Defaults to 'Limit' when `price` is specified. Defaults to 'Stop' when `stopPx` is specified. Defaults to 'StopLimit' when `price` and `stopPx` are specified. (optional)  (default to Limit)
            //var timeInForce = timeInForce_example;  // string | Time in force. Valid options: Day, GoodTillCancel, ImmediateOrCancel, FillOrKill. Defaults to 'GoodTillCancel' for 'Limit', 'StopLimit', 'LimitIfTouched', and 'MarketWithLeftOverAsLimit' orders. (optional) 
            //var execInst = execInst_example;  // string | Optional execution instructions. Valid options: ParticipateDoNotInitiate, AllOrNone, MarkPrice, IndexPrice, LastPrice, Close, ReduceOnly, Fixed. 'AllOrNone' instruction requires `displayQty` to be 0. 'MarkPrice', 'IndexPrice' or 'LastPrice' instruction valid for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders. (optional) 
            //var contingencyType = contingencyType_example;  // string | Optional contingency type for use with `clOrdLinkID`. Valid options: OneCancelsTheOther, OneTriggersTheOther, OneUpdatesTheOtherAbsolute, OneUpdatesTheOtherProportional. (optional,Deprecated) 
            //var text = text_example;  // string | Optional order annotation. e.g. 'Take profit'. (optional) 

            try
            {
                // Create a new order.
                Order result = apiInstance.OrderNew(symbol, side, null, orderQty, null, null, null, clOrdID, null, null, null, ordType, null, null, null, null);
                MessageBox.Show(result.ClOrdID.ToString());
            }
            catch (Exception exc)
            {
                MessageBox.Show("Exception when calling OrderApi.OrderNew: " + exc.Message);
            }
        }

        private long GetExpires()
        {
            return ToUnixTimeSeconds(DateTimeOffset.UtcNow) + 3600; // set expires one hour in the future
        }

        private static long ToUnixTimeSeconds(DateTimeOffset dateTimeOffset)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (dateTimeOffset.ToUniversalTime() - unixStart).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }
    }
}
