namespace BitMEX.TestForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btn1 = new System.Windows.Forms.Button();
            this.btn4 = new System.Windows.Forms.Button();
            this.LabelOnOff = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btn8 = new System.Windows.Forms.Button();
            this.lblStopOrder = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.NUDMaxExp = new System.Windows.Forms.NumericUpDown();
            this.NUDLeverage = new System.Windows.Forms.NumericUpDown();
            this.NUDDepth = new System.Windows.Forms.NumericUpDown();
            this.lbl4 = new System.Windows.Forms.Label();
            this.btn2 = new System.Windows.Forms.Button();
            this.lbl2 = new System.Windows.Forms.Label();
            this.btn5 = new System.Windows.Forms.Button();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.lbl5 = new System.Windows.Forms.Label();
            this.TBMarketOrder = new System.Windows.Forms.TextBox();
            this.TBClOrdId = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.btn7 = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.NUDMinProfit = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.NUDZonesize = new System.Windows.Forms.NumericUpDown();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btn3 = new System.Windows.Forms.Button();
            this.btn6 = new System.Windows.Forms.Button();
            this.lblLastPrice = new System.Windows.Forms.Label();
            this.lblUS = new System.Windows.Forms.Label();
            this.Heartbeat = new System.Windows.Forms.Timer(this.components);
            this.TimerTest = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.connectionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bitMEXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.liveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.disconnectToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dGV = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMaxExp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDLeverage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMinProfit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDZonesize)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGV)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 6;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 16.66667F));
            this.tableLayoutPanel1.Controls.Add(this.btn1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.LabelOnOff, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn8, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblStopOrder, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.NUDMaxExp, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.NUDLeverage, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.NUDDepth, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl4, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.btn2, 0, 7);
            this.tableLayoutPanel1.Controls.Add(this.lbl2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btn5, 1, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblConnectionStatus, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lbl1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lbl5, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.TBMarketOrder, 5, 7);
            this.tableLayoutPanel1.Controls.Add(this.TBClOrdId, 5, 6);
            this.tableLayoutPanel1.Controls.Add(this.btnClose, 4, 7);
            this.tableLayoutPanel1.Controls.Add(this.btn7, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.label9, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.NUDMinProfit, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.label8, 4, 1);
            this.tableLayoutPanel1.Controls.Add(this.NUDZonesize, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 4, 6);
            this.tableLayoutPanel1.Controls.Add(this.btn3, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.btn6, 4, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblLastPrice, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblUS, 2, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 45);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(776, 254);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // btn1
            // 
            this.btn1.Location = new System.Drawing.Point(3, 3);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(123, 23);
            this.btn1.TabIndex = 0;
            this.btn1.Text = "Connect";
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // btn4
            // 
            this.btn4.Location = new System.Drawing.Point(3, 34);
            this.btn4.Name = "btn4";
            this.btn4.Size = new System.Drawing.Size(123, 23);
            this.btn4.TabIndex = 3;
            this.btn4.Text = "Evaluate";
            this.btn4.UseVisualStyleBackColor = true;
            this.btn4.Click += new System.EventHandler(this.btn4_Click);
            // 
            // LabelOnOff
            // 
            this.LabelOnOff.AutoSize = true;
            this.LabelOnOff.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LabelOnOff.Location = new System.Drawing.Point(132, 31);
            this.LabelOnOff.Name = "LabelOnOff";
            this.LabelOnOff.Size = new System.Drawing.Size(37, 16);
            this.LabelOnOff.TabIndex = 9;
            this.LabelOnOff.Text = "OFF";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(261, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 17);
            this.label2.TabIndex = 12;
            this.label2.Text = "Max Exposure";
            // 
            // btn8
            // 
            this.btn8.Location = new System.Drawing.Point(3, 189);
            this.btn8.Name = "btn8";
            this.btn8.Size = new System.Drawing.Size(123, 23);
            this.btn8.TabIndex = 18;
            this.btn8.Text = "Add";
            this.btn8.UseVisualStyleBackColor = true;
            this.btn8.Click += new System.EventHandler(this.btn8_Click);
            // 
            // lblStopOrder
            // 
            this.lblStopOrder.AutoSize = true;
            this.lblStopOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStopOrder.Location = new System.Drawing.Point(261, 31);
            this.lblStopOrder.Name = "lblStopOrder";
            this.lblStopOrder.Size = new System.Drawing.Size(68, 17);
            this.lblStopOrder.TabIndex = 13;
            this.lblStopOrder.Text = "Leverage";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(261, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 17);
            this.label3.TabIndex = 13;
            this.label3.Text = "Depth";
            // 
            // NUDMaxExp
            // 
            this.NUDMaxExp.DecimalPlaces = 2;
            this.NUDMaxExp.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.NUDMaxExp.Location = new System.Drawing.Point(390, 3);
            this.NUDMaxExp.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUDMaxExp.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.NUDMaxExp.Name = "NUDMaxExp";
            this.NUDMaxExp.Size = new System.Drawing.Size(123, 20);
            this.NUDMaxExp.TabIndex = 8;
            this.NUDMaxExp.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // NUDLeverage
            // 
            this.NUDLeverage.DecimalPlaces = 2;
            this.NUDLeverage.Location = new System.Drawing.Point(390, 34);
            this.NUDLeverage.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUDLeverage.Name = "NUDLeverage";
            this.NUDLeverage.Size = new System.Drawing.Size(123, 20);
            this.NUDLeverage.TabIndex = 10;
            this.NUDLeverage.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // NUDDepth
            // 
            this.NUDDepth.Location = new System.Drawing.Point(390, 65);
            this.NUDDepth.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.NUDDepth.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUDDepth.Name = "NUDDepth";
            this.NUDDepth.Size = new System.Drawing.Size(123, 20);
            this.NUDDepth.TabIndex = 10;
            this.NUDDepth.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // lbl4
            // 
            this.lbl4.AutoSize = true;
            this.lbl4.Location = new System.Drawing.Point(261, 217);
            this.lbl4.Name = "lbl4";
            this.lbl4.Size = new System.Drawing.Size(52, 13);
            this.lbl4.TabIndex = 9;
            this.lbl4.Text = "OUTPUT";
            // 
            // btn2
            // 
            this.btn2.Location = new System.Drawing.Point(3, 220);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(123, 23);
            this.btn2.TabIndex = 1;
            this.btn2.Text = "Remove";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Click += new System.EventHandler(this.btn2_Click);
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(132, 62);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(31, 13);
            this.lbl2.TabIndex = 9;
            this.lbl2.Text = "RLim";
            // 
            // btn5
            // 
            this.btn5.Location = new System.Drawing.Point(132, 220);
            this.btn5.Name = "btn5";
            this.btn5.Size = new System.Drawing.Size(123, 23);
            this.btn5.TabIndex = 4;
            this.btn5.Text = "Test5";
            this.btn5.UseVisualStyleBackColor = true;
            this.btn5.Click += new System.EventHandler(this.btn5_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionStatus.Location = new System.Drawing.Point(3, 62);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(39, 16);
            this.lblConnectionStatus.TabIndex = 11;
            this.lblConnectionStatus.Text = ".-.-.-.-.";
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(132, 0);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(40, 13);
            this.lbl1.TabIndex = 9;
            this.lbl1.Text = "Status:";
            // 
            // lbl5
            // 
            this.lbl5.AutoSize = true;
            this.lbl5.Location = new System.Drawing.Point(3, 93);
            this.lbl5.Name = "lbl5";
            this.lbl5.Size = new System.Drawing.Size(69, 13);
            this.lbl5.TabIndex = 9;
            this.lbl5.Text = "Return code:";
            // 
            // TBMarketOrder
            // 
            this.TBMarketOrder.Location = new System.Drawing.Point(648, 220);
            this.TBMarketOrder.Name = "TBMarketOrder";
            this.TBMarketOrder.Size = new System.Drawing.Size(123, 20);
            this.TBMarketOrder.TabIndex = 7;
            this.TBMarketOrder.Text = "XBTUSD";
            // 
            // TBClOrdId
            // 
            this.TBClOrdId.Location = new System.Drawing.Point(648, 189);
            this.TBClOrdId.Name = "TBClOrdId";
            this.TBClOrdId.Size = new System.Drawing.Size(123, 20);
            this.TBClOrdId.TabIndex = 14;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(519, 220);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(123, 23);
            this.btnClose.TabIndex = 16;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btn7
            // 
            this.btn7.Location = new System.Drawing.Point(132, 189);
            this.btn7.Name = "btn7";
            this.btn7.Size = new System.Drawing.Size(123, 23);
            this.btn7.TabIndex = 16;
            this.btn7.Text = "Test 7";
            this.btn7.UseVisualStyleBackColor = true;
            this.btn7.Click += new System.EventHandler(this.btn7_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(519, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 17);
            this.label9.TabIndex = 11;
            this.label9.Text = "Min Profit %";
            // 
            // NUDMinProfit
            // 
            this.NUDMinProfit.DecimalPlaces = 2;
            this.NUDMinProfit.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.NUDMinProfit.Location = new System.Drawing.Point(648, 3);
            this.NUDMinProfit.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.NUDMinProfit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.NUDMinProfit.Name = "NUDMinProfit";
            this.NUDMinProfit.Size = new System.Drawing.Size(123, 20);
            this.NUDMinProfit.TabIndex = 10;
            this.NUDMinProfit.Value = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(519, 31);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 17);
            this.label8.TabIndex = 11;
            this.label8.Text = "Zonesize";
            // 
            // NUDZonesize
            // 
            this.NUDZonesize.Location = new System.Drawing.Point(648, 34);
            this.NUDZonesize.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.NUDZonesize.Minimum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.NUDZonesize.Name = "NUDZonesize";
            this.NUDZonesize.Size = new System.Drawing.Size(123, 20);
            this.NUDZonesize.TabIndex = 10;
            this.NUDZonesize.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(519, 189);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(123, 20);
            this.textBox1.TabIndex = 14;
            // 
            // btn3
            // 
            this.btn3.Location = new System.Drawing.Point(261, 189);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(123, 23);
            this.btn3.TabIndex = 2;
            this.btn3.Text = "Testr";
            this.btn3.UseVisualStyleBackColor = true;
            this.btn3.Click += new System.EventHandler(this.btn3_Click);
            // 
            // btn6
            // 
            this.btn6.Location = new System.Drawing.Point(519, 65);
            this.btn6.Name = "btn6";
            this.btn6.Size = new System.Drawing.Size(123, 23);
            this.btn6.TabIndex = 15;
            this.btn6.Text = "MarketNow";
            this.btn6.UseVisualStyleBackColor = true;
            this.btn6.Click += new System.EventHandler(this.btn6_Click);
            // 
            // lblLastPrice
            // 
            this.lblLastPrice.AutoSize = true;
            this.lblLastPrice.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLastPrice.Location = new System.Drawing.Point(132, 93);
            this.lblLastPrice.Name = "lblLastPrice";
            this.lblLastPrice.Size = new System.Drawing.Size(56, 13);
            this.lblLastPrice.TabIndex = 13;
            this.lblLastPrice.Text = "Last price:";
            // 
            // lblUS
            // 
            this.lblUS.AutoSize = true;
            this.lblUS.Location = new System.Drawing.Point(261, 93);
            this.lblUS.Name = "lblUS";
            this.lblUS.Size = new System.Drawing.Size(50, 13);
            this.lblUS.TabIndex = 9;
            this.lblUS.Text = "Unit size:";
            // 
            // Heartbeat
            // 
            this.Heartbeat.Interval = 2000;
            this.Heartbeat.Tick += new System.EventHandler(this.Heartbeat_Tick);
            // 
            // TimerTest
            // 
            this.TimerTest.Tick += new System.EventHandler(this.TimerTest_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectionsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 24);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // connectionsToolStripMenuItem
            // 
            this.connectionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bitMEXToolStripMenuItem});
            this.connectionsToolStripMenuItem.Name = "connectionsToolStripMenuItem";
            this.connectionsToolStripMenuItem.Size = new System.Drawing.Size(86, 20);
            this.connectionsToolStripMenuItem.Text = "Connections";
            // 
            // bitMEXToolStripMenuItem
            // 
            this.bitMEXToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem,
            this.liveToolStripMenuItem});
            this.bitMEXToolStripMenuItem.Name = "bitMEXToolStripMenuItem";
            this.bitMEXToolStripMenuItem.Size = new System.Drawing.Size(112, 22);
            this.bitMEXToolStripMenuItem.Text = "BitMEX";
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem,
            this.disconnectToolStripMenuItem});
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
            this.testToolStripMenuItem.Text = "Test";
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.connectToolStripMenuItem.Text = "Connect";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.connectToolStripMenuItem_Click);
            // 
            // disconnectToolStripMenuItem
            // 
            this.disconnectToolStripMenuItem.Name = "disconnectToolStripMenuItem";
            this.disconnectToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.disconnectToolStripMenuItem.Text = "Disconnect";
            // 
            // liveToolStripMenuItem
            // 
            this.liveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectToolStripMenuItem1,
            this.disconnectToolStripMenuItem1});
            this.liveToolStripMenuItem.Name = "liveToolStripMenuItem";
            this.liveToolStripMenuItem.Size = new System.Drawing.Size(95, 22);
            this.liveToolStripMenuItem.Text = "Live";
            // 
            // connectToolStripMenuItem1
            // 
            this.connectToolStripMenuItem1.Name = "connectToolStripMenuItem1";
            this.connectToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.connectToolStripMenuItem1.Text = "Connect";
            // 
            // disconnectToolStripMenuItem1
            // 
            this.disconnectToolStripMenuItem1.Name = "disconnectToolStripMenuItem1";
            this.disconnectToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.disconnectToolStripMenuItem1.Text = "Disconnect";
            // 
            // dGV
            // 
            this.dGV.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dGV.Location = new System.Drawing.Point(12, 305);
            this.dGV.Name = "dGV";
            this.dGV.Size = new System.Drawing.Size(776, 133);
            this.dGV.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dGV);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMaxExp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDLeverage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMinProfit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDZonesize)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dGV)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Button btn3;
        private System.Windows.Forms.Button btn4;
        private System.Windows.Forms.Button btn5;
        private System.Windows.Forms.TextBox TBMarketOrder;
        private System.Windows.Forms.NumericUpDown NUDMaxExp;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.NumericUpDown NUDDepth;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TBClOrdId;
        private System.Windows.Forms.Label lblLastPrice;
        private System.Windows.Forms.Button btn6;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStopOrder;
        private System.Windows.Forms.NumericUpDown NUDLeverage;
        private System.Windows.Forms.Button btn7;
        private System.Windows.Forms.Button btn8;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Timer Heartbeat;
        private System.Windows.Forms.Label LabelOnOff;
        private System.Windows.Forms.Label lblUS;
        private System.Windows.Forms.Label lbl5;
        private System.Windows.Forms.Label lbl4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Timer TimerTest;
        private System.Windows.Forms.NumericUpDown NUDZonesize;
        private System.Windows.Forms.NumericUpDown NUDMinProfit;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem connectionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem bitMEXToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem testToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem liveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem disconnectToolStripMenuItem1;
        private System.Windows.Forms.DataGridView dGV;
    }
}

