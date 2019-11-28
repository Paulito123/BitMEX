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
            this.btn2 = new System.Windows.Forms.Button();
            this.NUDMaxExp = new System.Windows.Forms.NumericUpDown();
            this.NUDLeverage = new System.Windows.Forms.NumericUpDown();
            this.NUDDepth = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.lblStopOrder = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.TBClOrdId = new System.Windows.Forms.TextBox();
            this.TBMarketOrder = new System.Windows.Forms.TextBox();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.btn3 = new System.Windows.Forms.Button();
            this.btn4 = new System.Windows.Forms.Button();
            this.btn5 = new System.Windows.Forms.Button();
            this.btn6 = new System.Windows.Forms.Button();
            this.btn8 = new System.Windows.Forms.Button();
            this.lblClOrdId = new System.Windows.Forms.Label();
            this.btn7 = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.LabelOnOff = new System.Windows.Forms.Label();
            this.lbl1 = new System.Windows.Forms.Label();
            this.lbl2 = new System.Windows.Forms.Label();
            this.lbl3 = new System.Windows.Forms.Label();
            this.lbl4 = new System.Windows.Forms.Label();
            this.lbl5 = new System.Windows.Forms.Label();
            this.LabelMainOutput = new System.Windows.Forms.Label();
            this.Heartbeat = new System.Windows.Forms.Timer(this.components);
            this.TimerTest = new System.Windows.Forms.Timer(this.components);
            this.NUDZonesize = new System.Windows.Forms.NumericUpDown();
            this.NUDMinProfit = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMaxExp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDLeverage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDDepth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDZonesize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMinProfit)).BeginInit();
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
            this.tableLayoutPanel1.Controls.Add(this.btn5, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btn7, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.btnClose, 5, 3);
            this.tableLayoutPanel1.Controls.Add(this.lbl1, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.btn3, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.btn2, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblConnectionStatus, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn4, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.LabelOnOff, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.btn6, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.btn8, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblStopOrder, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.label3, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.NUDMaxExp, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.NUDLeverage, 3, 1);
            this.tableLayoutPanel1.Controls.Add(this.NUDDepth, 3, 2);
            this.tableLayoutPanel1.Controls.Add(this.label8, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.lbl3, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.label9, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.lbl2, 2, 6);
            this.tableLayoutPanel1.Controls.Add(this.lbl4, 2, 7);
            this.tableLayoutPanel1.Controls.Add(this.lblClOrdId, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.lbl5, 3, 7);
            this.tableLayoutPanel1.Controls.Add(this.NUDZonesize, 3, 3);
            this.tableLayoutPanel1.Controls.Add(this.NUDMinProfit, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.TBMarketOrder, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.TBClOrdId, 5, 1);
            this.tableLayoutPanel1.Controls.Add(this.textBox1, 5, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
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
            // btn2
            // 
            this.btn2.Location = new System.Drawing.Point(132, 189);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(123, 23);
            this.btn2.TabIndex = 1;
            this.btn2.Text = "Test 2";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Click += new System.EventHandler(this.btn2_Click);
            // 
            // NUDMaxExp
            // 
            this.NUDMaxExp.DecimalPlaces = 2;
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
            this.NUDLeverage.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
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
            this.NUDDepth.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
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
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(648, 65);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(123, 20);
            this.textBox1.TabIndex = 14;
            // 
            // TBClOrdId
            // 
            this.TBClOrdId.Location = new System.Drawing.Point(648, 34);
            this.TBClOrdId.Name = "TBClOrdId";
            this.TBClOrdId.Size = new System.Drawing.Size(123, 20);
            this.TBClOrdId.TabIndex = 14;
            // 
            // TBMarketOrder
            // 
            this.TBMarketOrder.Location = new System.Drawing.Point(648, 3);
            this.TBMarketOrder.Name = "TBMarketOrder";
            this.TBMarketOrder.Size = new System.Drawing.Size(123, 20);
            this.TBMarketOrder.TabIndex = 7;
            this.TBMarketOrder.Text = "XBTUSD";
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblConnectionStatus.Location = new System.Drawing.Point(132, 0);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(89, 16);
            this.lblConnectionStatus.TabIndex = 11;
            this.lblConnectionStatus.Text = "SYMBOL >>";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(261, 93);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(66, 17);
            this.label8.TabIndex = 11;
            this.label8.Text = "Zonesize";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(261, 124);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 17);
            this.label9.TabIndex = 11;
            this.label9.Text = "Min Profit %";
            // 
            // btn3
            // 
            this.btn3.Location = new System.Drawing.Point(3, 127);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(123, 23);
            this.btn3.TabIndex = 2;
            this.btn3.Text = "Test3";
            this.btn3.UseVisualStyleBackColor = true;
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
            // btn5
            // 
            this.btn5.Location = new System.Drawing.Point(3, 65);
            this.btn5.Name = "btn5";
            this.btn5.Size = new System.Drawing.Size(123, 23);
            this.btn5.TabIndex = 4;
            this.btn5.Text = "Test5";
            this.btn5.UseVisualStyleBackColor = true;
            this.btn5.Click += new System.EventHandler(this.btn5_Click);
            // 
            // btn6
            // 
            this.btn6.Location = new System.Drawing.Point(3, 158);
            this.btn6.Name = "btn6";
            this.btn6.Size = new System.Drawing.Size(123, 23);
            this.btn6.TabIndex = 15;
            this.btn6.Text = "Test6";
            this.btn6.UseVisualStyleBackColor = true;
            this.btn6.Click += new System.EventHandler(this.btn6_Click);
            // 
            // btn8
            // 
            this.btn8.Location = new System.Drawing.Point(3, 189);
            this.btn8.Name = "btn8";
            this.btn8.Size = new System.Drawing.Size(123, 23);
            this.btn8.TabIndex = 18;
            this.btn8.Text = "Test 8";
            this.btn8.UseVisualStyleBackColor = true;
            // 
            // lblClOrdId
            // 
            this.lblClOrdId.AutoSize = true;
            this.lblClOrdId.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClOrdId.Location = new System.Drawing.Point(132, 155);
            this.lblClOrdId.Name = "lblClOrdId";
            this.lblClOrdId.Size = new System.Drawing.Size(75, 17);
            this.lblClOrdId.TabIndex = 13;
            this.lblClOrdId.Text = "ClOrdId >>";
            // 
            // btn7
            // 
            this.btn7.Location = new System.Drawing.Point(3, 96);
            this.btn7.Name = "btn7";
            this.btn7.Size = new System.Drawing.Size(123, 23);
            this.btn7.TabIndex = 16;
            this.btn7.Text = "Test 7";
            this.btn7.UseVisualStyleBackColor = true;
            this.btn7.Click += new System.EventHandler(this.btn7_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(648, 96);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(123, 23);
            this.btnClose.TabIndex = 16;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
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
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(132, 124);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(52, 13);
            this.lbl1.TabIndex = 9;
            this.lbl1.Text = "OUTPUT";
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(261, 186);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(52, 13);
            this.lbl2.TabIndex = 9;
            this.lbl2.Text = "OUTPUT";
            // 
            // lbl3
            // 
            this.lbl3.AutoSize = true;
            this.lbl3.Location = new System.Drawing.Point(261, 155);
            this.lbl3.Name = "lbl3";
            this.lbl3.Size = new System.Drawing.Size(52, 13);
            this.lbl3.TabIndex = 9;
            this.lbl3.Text = "OUTPUT";
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
            // lbl5
            // 
            this.lbl5.AutoSize = true;
            this.lbl5.Location = new System.Drawing.Point(390, 217);
            this.lbl5.Name = "lbl5";
            this.lbl5.Size = new System.Drawing.Size(52, 13);
            this.lbl5.TabIndex = 9;
            this.lbl5.Text = "OUTPUT";
            // 
            // LabelMainOutput
            // 
            this.LabelMainOutput.AutoSize = true;
            this.LabelMainOutput.Location = new System.Drawing.Point(9, 310);
            this.LabelMainOutput.Name = "LabelMainOutput";
            this.LabelMainOutput.Size = new System.Drawing.Size(16, 13);
            this.LabelMainOutput.TabIndex = 9;
            this.LabelMainOutput.Text = "...";
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
            // NUDZonesize
            // 
            this.NUDZonesize.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.NUDZonesize.Location = new System.Drawing.Point(390, 96);
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
            // NUDMinProfit
            // 
            this.NUDMinProfit.DecimalPlaces = 2;
            this.NUDMinProfit.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.NUDMinProfit.Location = new System.Drawing.Point(390, 127);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.LabelMainOutput);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMaxExp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDLeverage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDDepth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDZonesize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMinProfit)).EndInit();
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
        private System.Windows.Forms.Label lblClOrdId;
        private System.Windows.Forms.Button btn6;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lblStopOrder;
        private System.Windows.Forms.NumericUpDown NUDLeverage;
        private System.Windows.Forms.Button btn7;
        private System.Windows.Forms.Button btn8;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.Timer Heartbeat;
        private System.Windows.Forms.Label LabelOnOff;
        private System.Windows.Forms.Label lbl3;
        private System.Windows.Forms.Label lbl5;
        private System.Windows.Forms.Label LabelMainOutput;
        private System.Windows.Forms.Label lbl4;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Timer TimerTest;
        private System.Windows.Forms.NumericUpDown NUDZonesize;
        private System.Windows.Forms.NumericUpDown NUDMinProfit;
    }
}

