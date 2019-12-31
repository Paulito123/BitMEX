﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using BitMEX.Client;
using BitMEX.Model;
using Bitmex.Client.Websocket.Responses.Orders;

namespace MoneyTron
{
    public partial class MTMainForm : Form, IMTMainForm
    {
        #region Linked variables

        public string AccountAID
        {
            get => lblAAccountID.Text;
            set => SetLabelOnGuiThread(lblAAccountID, value);
        }
        public string AccountBID
        {
            get => lblBAccountID.Text;
            set => SetLabelOnGuiThread(lblBAccountID, value);
        }
        public string Bid
        {
            get => lblBid.Text;
            set => SetLabelOnGuiThread(lblBid, value);
        }
        public string Ask
        {
            get => lblAsk.Text;
            set => SetLabelOnGuiThread(lblAsk, value);
        }
        public string BidAmount
        {
            get => lblBidAmount.Text;
            set => SetLabelOnGuiThread(lblBidAmount, value);
        }
        public string AskAmount
        {
            get => lblAskAmount.Text;
            set => SetLabelOnGuiThread(lblAskAmount, value);
        }
        public string Trades1Min
        {
            get => lblTrades1Min.Text;
            set => SetLabelOnGuiThread(lblTrades1Min, value);
        }
        public string Trades5Min
        {
            get => lblTrades5Min.Text;
            set => SetLabelOnGuiThread(lblTrades5Min, value);
        }
        public string Trades15Min
        {
            get => lblTrades15Min.Text;
            set => SetLabelOnGuiThread(lblTrades15Min, value);
        }
        public string Trades1Hour
        {
            get => lblTrades1Hour.Text;
            set => SetLabelOnGuiThread(lblTrades1Hour, value);
        }
        public string Trades24Hours
        {
            get => lblTrades24Hours.Text;
            set => SetLabelOnGuiThread(lblTrades24Hours, value);
        }
        public string ConnStatusA
        {
            
            get => tbStatusA.Text;
            set => SetTextBoxOnGuiThread(tbStatusA, value);
        }
        public string ConnStatusB
        {
            get => tbStatusA.Text;
            set => SetTextBoxOnGuiThread(tbStatusB, value);
        }
        public string ReconnectionsA
        {
            get => lblReconnectionsA.Text;
            set => SetLabelOnGuiThread(lblReconnectionsA, value);
        }
        public string ReconnectionsB
        {
            get => lblReconnectionsB.Text;
            set => SetLabelOnGuiThread(lblReconnectionsB, value);
        }
        public string DisconnectionsA
        {
            get => lblDisconnectionsA.Text;
            set => SetLabelOnGuiThread(lblDisconnectionsA, value);
        }
        public string DisconnectionsB
        {
            get => lblDisconnectionsB.Text;
            set => SetLabelOnGuiThread(lblDisconnectionsB, value);
        }
        public string ConnStartA
        {
            get => lblAConnStart.Text;
            set => SetLabelOnGuiThread(lblAConnStart, value);
        }
        public string ConnStartB
        {
            get => lblBConnStart.Text;
            set => SetLabelOnGuiThread(lblBConnStart, value);
        }
        public string TotalFundsA
        {
            get => lblATotalFunds.Text;
            set => SetLabelOnGuiThread(lblATotalFunds, value);
        }
        public string TotalFundsB
        {
            get => lblBTotalFunds.Text;
            set => SetLabelOnGuiThread(lblBTotalFunds, value);
        }
        public string AvailableFundsA
        {
            get => lblAAvailableFunds.Text;
            set => SetLabelOnGuiThread(lblAAvailableFunds, value);
        }
        public string AvailableFundsB
        {
            get => lblBAvailableFunds.Text;
            set => SetLabelOnGuiThread(lblBAvailableFunds, value);
        }
        public string MarginBalanceA
        {
            get => lblMarginBalanceA.Text;
            set => SetLabelOnGuiThread(lblMarginBalanceA, value);
        }
        public string MarginBalanceB
        {
            get => lblMarginBalanceB.Text;
            set => SetLabelOnGuiThread(lblMarginBalanceB, value);
        }
        public string TabPosBTitle
        {
            get => tabPagePosB.Text;
            set => SetTabTitleOnGuiThread(tabPagePosB, value);
        }
        public string TabPosATitle
        {
            get => tabPagePosA.Text;
            set => SetTabTitleOnGuiThread(tabPagePosA, value);
        }
        public string TabOrdersBTitle
        {
            get => tabPageOrdersB.Text;
            set => SetTabTitleOnGuiThread(tabPageOrdersB, value);
        }
        public string TabOrdersATitle
        {
            get => tabPageOrdersA.Text;
            set => SetTabTitleOnGuiThread(tabPageOrdersA, value);
        }
        public string PingL
        {
            get => lblPingA.Text;
            set => SetLabelOnGuiThread(lblPingA, value);
        }
        public string PingS
        {
            get => lblPingB.Text;
            set => SetLabelOnGuiThread(lblPingB, value);
        }
        public string ErrorsCounterA
        {
            get => lblErrorsA.Text;
            set => SetLabelOnGuiThread(lblErrorsA, value);
        }
        public string ErrorsCounterB
        {
            get => lblErrorsB.Text;
            set => SetLabelOnGuiThread(lblErrorsB, value);
        }
        public string ErrorsCounterTotal
        {
            get => lblErrorsTotal.Text;
            set => SetLabelOnGuiThread(lblErrorsTotal, value);
        }
        public string TimeConnected
        {
            get => lblTimeConnected.Text;
            set => SetLabelOnGuiThread(lblTimeConnected, value);
        }
        public string CashImbalance
        {
            get => lblCashImbalance.Text;
            set => SetLabelOnGuiThread(lblCashImbalance, value);
        }
        public string PNLA
        {
            get => lblPNLA.Text;
            set => SetLabelOnGuiThread(lblPNLA, value);
        }
        public string PNLB
        {
            get => lblPNLB.Text;
            set => SetLabelOnGuiThread(lblPNLB, value);
        }
        public string TotalCostA
        {
            get => lblTotalCostA.Text;
            set => SetLabelOnGuiThread(lblTotalCostA, value);
        }
        public string TotalCostB
        {
            get => lblTotalCostB.Text;
            set => SetLabelOnGuiThread(lblTotalCostB, value);
        }

        #endregion Linked variables

        #region Linked controls

        public BindingSource bSRCOrdersA
        {
            get => (BindingSource)dGVOrdersA.DataSource;
            set => SetBindingSRCOnGuiThread(dGVOrdersA, value);
        }
        public BindingSource bSRCOrdersB
        {
            get => (BindingSource)dGVOrdersB.DataSource;
            set => SetBindingSRCOnGuiThread(dGVOrdersB, value);
        }
        public BindingSource bSRCPosA
        {
            get => (BindingSource)dGVPosA.DataSource;
            set => SetBindingSRCOnGuiThread(dGVPosA, value);
        }
        public BindingSource bSRCPosB
        {
            get => (BindingSource)dGVPosB.DataSource;
            set => SetBindingSRCOnGuiThread(dGVPosB, value);
        }

        #endregion Linked controls

        #region Control updating methods

        public void StatusA(string value, StatusType type)
        {
            SetTextBoxOnGuiThread(tbStatusA, value);
            tbStatusA.ForeColor = GetForeColorFor(type);
        }
        public void StatusB(string value, StatusType type)
        {
            SetTextBoxOnGuiThread(tbStatusB, value);
            tbStatusB.ForeColor = GetForeColorFor(type);
        }

        void IMTMainForm.Trades1Min(string value, Side side)
        {
            Trades1Min = value;
            lblTrades1Min.ForeColor = GetForeColorFor(side);
        }
        void IMTMainForm.Trades5Min(string value, Side side)
        {
            Trades5Min = value;
            lblTrades5Min.ForeColor = GetForeColorFor(side);
        }
        void IMTMainForm.Trades15Min(string value, Side side)
        {
            Trades15Min = value;
            lblTrades15Min.ForeColor = GetForeColorFor(side);
        }
        void IMTMainForm.Trades1Hour(string value, Side side)
        {
            Trades1Hour = value;
            lblTrades1Hour.ForeColor = GetForeColorFor(side);
        }
        void IMTMainForm.Trades24Hours(string value, Side side)
        {
            Trades24Hours = value;
            lblTrades24Hours.ForeColor = GetForeColorFor(side);
        }

        #endregion Control updating methods

        #region Helper methods

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

        #endregion Helper methods

        #region Actions

        public Action OnInit { get; set; }
        public Action OnStartA { get; set; }
        public Action OnStop { get; set; }
        public Action OnStartB { get; set; }

        #endregion Actions

        #region Init methods

        public MTMainForm()
        {
            InitializeComponent();
            InitDataGrids();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OnInit?.Invoke();
            disconnectToolStripMenuItem.Enabled = false;
        }

        private void InitDataGrids()
        {
            InitOrdersDataGrid(dGVOrdersA);
            InitOrdersDataGrid(dGVOrdersB);
            InitPositionsDataGrid(dGVPosA);
            InitPositionsDataGrid(dGVPosB);
        }

        private void InitOrdersDataGrid(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.AutoSize = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Regular);
            dgv.DefaultCellStyle.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.ReadOnly = true;

            DataGridViewColumn column = new DataGridViewTextBoxColumn();
            
            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "OrderQty";
            column.Name = "Quantity";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Side";
            column.Name = "Side";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "Price";
            column.Name = "Price";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "ordStatus";
            column.Name = "Status";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "timestamp";
            column.Name = "Time";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);
        }

        private void InitPositionsDataGrid(DataGridView dgv)
        {
            dgv.AutoGenerateColumns = false;
            dgv.AutoSize = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;
            dgv.DefaultCellStyle.Font = new Font(dgv.Font, FontStyle.Regular);
            dgv.ReadOnly = true;

            DataGridViewColumn column = new DataGridViewTextBoxColumn();

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "symbol";
            column.Name = "Symbol";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "currentQty";
            column.Name = "Quantity";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "avgEntryPrice";
            column.Name = "Entry Price";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "liquidationPrice";
            column.Name = "Liq Price";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            column.CellTemplate.Style.ForeColor = Color.Red; 
            column.CellTemplate.Style.Font = new Font(dgv.Font, FontStyle.Bold);
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "unrealisedPnl";
            column.Name = "Unrealised PNL";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

            column = new DataGridViewTextBoxColumn();
            column.DataPropertyName = "realisedPnl";
            column.Name = "Realised PNL";
            column.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            column.HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter;
            dgv.Columns.Add(column);

        }

        #endregion Init methods

        #region Menu event handlers

        /// <summary>
        /// Connect BitMEX Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnStartA?.Invoke();
            OnStartB?.Invoke();
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
            OnStop?.Invoke();
            connectToolStripMenuItem.Enabled = true;
            disconnectToolStripMenuItem.Enabled = false;
        }

        #endregion Menu event handlers

        #region SetGUIThread methods

        private void SetBindingSRCOnGuiThread(DataGridView dgv, BindingSource bs)
        {
            if (!dgv.InvokeRequired)
            {
                dgv.DataSource = bs;
                ((BindingSource)dgv.DataSource).ResetBindings(false);
                return;
            }

            dgv.Invoke(new Action(() =>
            {
                dgv.DataSource = bs;
                ((BindingSource)dgv.DataSource).ResetBindings(false);
            }));
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

        private void SetTextBoxOnGuiThread(TextBox tb, string value)
        {
            if (tb.Text == value)
                return;

            if (!InvokeRequired)
            {
                tb.Text = value;
                return;
            }

            this.Invoke(new Action(() =>
            {
                tb.Text = value;
            }));
        }

        private void SetPBOnGuiThread(ProgressBar pb, int value)
        {
            if (pb.Value == value)
            {               return;
            }
                
            if (!InvokeRequired)
            {
                pb.Value = value;
                return;
            }

            this.Invoke(new Action(() =>
            {
                pb.Value = value;
            }));
        }

        #endregion SetGUIThread methods
    }
}
