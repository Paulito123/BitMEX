using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMEX.Client;
using BitMEX.Model;

namespace MoneyTron
{
    public partial class MTMainForm : Form, IMTMainForm
    {
        //private Dictionary<long, MordoR> Conns;

        public string AccountATitle
        {
            get => labelAAccount.Text;
            set => SetLabelOnGuiThread(labelAAccount, value);
        }

        public string AccountBTitle
        {
            get => labelBAccount.Text;
            set => SetLabelOnGuiThread(labelBAccount, value);
        }

        public string AccountAID
        {
            get => lblLAccountID.Text;
            set => SetLabelOnGuiThread(lblLAccountID, value);
        }

        public string AccountBID
        {
            get => lblSAccountID.Text;
            set => SetLabelOnGuiThread(lblSAccountID, value);
        }

        public string ConnStatusA
        {
            get => lblLConnStatus.Text;
            set => SetLabelOnGuiThread(lblLConnStatus, value);
        }

        public string ConnStatusB
        {
            get => lblSConnStatus.Text;
            set => SetLabelOnGuiThread(lblSConnStatus, value);
        }

        public string ConnStartA
        {
            get => lblLConnStart.Text;
            set => SetLabelOnGuiThread(lblLConnStart, value);
        }

        public string ConnStartB
        {
            get => lblSConnStart.Text;
            set => SetLabelOnGuiThread(lblSConnStart, value);
        }

        public string MarkPriceA
        {
            get => lblLMarkPrice.Text;
            set => SetLabelOnGuiThread(lblLMarkPrice, value);
        }

        public string MarkPriceB
        {
            get => lblSMarkPrice.Text;
            set => SetLabelOnGuiThread(lblSMarkPrice, value);
        }

        public string IndexPriceA
        {
            get => lblLIndexPrice.Text;
            set => SetLabelOnGuiThread(lblLIndexPrice, value);
        }

        public string IndexPriceB
        {
            get => lblSIndexPrice.Text;
            set => SetLabelOnGuiThread(lblSIndexPrice, value);
        }

        public string TotalFundsA
        {
            get => lblLTotalFunds.Text;
            set => SetLabelOnGuiThread(lblLTotalFunds, value);
        }

        public string TotalFundsB
        {
            get => lblSTotalFunds.Text;
            set => SetLabelOnGuiThread(lblSTotalFunds, value);
        }

        public string AvailableFundsA
        {
            get => lblLAvailableFunds.Text;
            set => SetLabelOnGuiThread(lblLAvailableFunds, value);
        }

        public string AvailableFundsB
        {
            get => lblSAvailableFunds.Text;
            set => SetLabelOnGuiThread(lblSAvailableFunds, value);
        }

        public string TabPosSTitle
        {
            get => tabPagePosShort.Text;
            set => SetTabTitleOnGuiThread(tabPagePosShort, value);
        }

        public string TabPosLTitle
        {
            get => tabPagePosLong.Text;
            set => SetTabTitleOnGuiThread(tabPagePosLong, value);
        }

        public string TabOrdersSTitle
        {
            get => tabPageOrdersShort.Text;
            set => SetTabTitleOnGuiThread(tabPageOrdersShort, value);
        }

        public string TabOrdersLTitle
        {
            get => tabPageOrdersLong.Text;
            set => SetTabTitleOnGuiThread(tabPageOrdersLong, value);
        }

        public string PingL
        {
            get => lblPingL.Text;
            set => SetLabelOnGuiThread(lblPingL, value);
        }

        public string PingS
        {
            get => lblPingS.Text;
            set => SetLabelOnGuiThread(lblPingS, value);
        }

        public void StatusA(string value, StatusType type)
        {
            SetLabelOnGuiThread(lblLConnStatus, value);
            lblLConnStatus.ForeColor = GetForeColorFor(type);
        }

        public void StatusB(string value, StatusType type)
        {
            SetLabelOnGuiThread(lblSConnStatus, value);
            lblSConnStatus.ForeColor = GetForeColorFor(type);
        }

        public Action OnInit { get; set; }
        public Action OnStart { get; set; }
        public Action OnStop { get; set; }

        private Color GetForeColorFor(Side side)
        {
            if (side == Side.Buy)
                return Color.GreenYellow;
            return Color.LightCoral;
        }

        private Color GetForeColorFor(StatusType type)
        {
            switch (type)
            {
                case StatusType.Error:
                    return Color.IndianRed;
                case StatusType.Warning:
                    return Color.DarkOrange;
                default:
                    return Color.DarkSeaGreen;
            }
        }

        public MTMainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OnInit?.Invoke();
            disconnectToolStripMenuItem.Enabled = false;
            labelAAccount.Text = "Not connected";
            labelBAccount.Text = "Not connected";
        }

        /// <summary>
        /// Connect BitMEX Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelAAccount.Text = "BitMEX - Long";
            labelBAccount.Text = "BitMEX - Short";
            connectToolStripMenuItem.Enabled = false;
            disconnectToolStripMenuItem.Enabled = true;
        }

        /// <summary>
        /// Disconnect BitMEX Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            labelAAccount.Text = "Not connected";
            labelBAccount.Text = "Not connected";
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
        }

        private void SetLabelOnGuiThread(Label lb, string value)
        {
            if (lb.Text == value)
                return;

            if (!InvokeRequired)
            {
                lb.Text = value;
                return;
            }

            this.Invoke(new Action(() =>
            {
                lb.Text = value;
            }));
        }

        private void SetTabTitleOnGuiThread(TabPage tp, string value)
        {
            if (tp.Text == value)
                return;

            if (!InvokeRequired)
            {
                tp.Text = value;
                return;
            }

            this.Invoke(new Action(() =>
            {
                tp.Text = value;
            }));
        }
    }
}
