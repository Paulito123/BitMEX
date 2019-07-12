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
    }
}
