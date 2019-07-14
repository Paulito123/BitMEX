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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnMarketOrder = new System.Windows.Forms.Button();
            this.btnSwagger = new System.Windows.Forms.Button();
            this.btnLimitOrder = new System.Windows.Forms.Button();
            this.btnGetOrders = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.TBMarketOrder = new System.Windows.Forms.TextBox();
            this.NUDMarketOrderQuantity = new System.Windows.Forms.NumericUpDown();
            this.OutputLabel = new System.Windows.Forms.Label();
            this.NUDPrice = new System.Windows.Forms.NumericUpDown();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMarketOrderQuantity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDPrice)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 8;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 12.5F));
            this.tableLayoutPanel1.Controls.Add(this.btnMarketOrder, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSwagger, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.btnLimitOrder, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnGetOrders, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.button5, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.TBMarketOrder, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.NUDMarketOrderQuantity, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.OutputLabel, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.NUDPrice, 3, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 10;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(776, 426);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // btnMarketOrder
            // 
            this.btnMarketOrder.Location = new System.Drawing.Point(3, 3);
            this.btnMarketOrder.Name = "btnMarketOrder";
            this.btnMarketOrder.Size = new System.Drawing.Size(75, 23);
            this.btnMarketOrder.TabIndex = 0;
            this.btnMarketOrder.Text = "MarketOrder";
            this.btnMarketOrder.UseVisualStyleBackColor = true;
            this.btnMarketOrder.Click += new System.EventHandler(this.btnMarketOrder_Click);
            // 
            // btnSwagger
            // 
            this.btnSwagger.Location = new System.Drawing.Point(3, 45);
            this.btnSwagger.Name = "btnSwagger";
            this.btnSwagger.Size = new System.Drawing.Size(75, 23);
            this.btnSwagger.TabIndex = 1;
            this.btnSwagger.Text = "Swagger";
            this.btnSwagger.UseVisualStyleBackColor = true;
            this.btnSwagger.Click += new System.EventHandler(this.btnSwagger_Click);
            // 
            // btnLimitOrder
            // 
            this.btnLimitOrder.Location = new System.Drawing.Point(3, 87);
            this.btnLimitOrder.Name = "btnLimitOrder";
            this.btnLimitOrder.Size = new System.Drawing.Size(75, 23);
            this.btnLimitOrder.TabIndex = 2;
            this.btnLimitOrder.Text = "LimitOrder";
            this.btnLimitOrder.UseVisualStyleBackColor = true;
            this.btnLimitOrder.Click += new System.EventHandler(this.btnLimitOrder_Click);
            // 
            // btnGetOrders
            // 
            this.btnGetOrders.Location = new System.Drawing.Point(3, 129);
            this.btnGetOrders.Name = "btnGetOrders";
            this.btnGetOrders.Size = new System.Drawing.Size(75, 23);
            this.btnGetOrders.TabIndex = 3;
            this.btnGetOrders.Text = "GetOrders";
            this.btnGetOrders.UseVisualStyleBackColor = true;
            this.btnGetOrders.Click += new System.EventHandler(this.btnGetOrders_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(3, 171);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 4;
            this.button5.Text = "button5";
            this.button5.UseVisualStyleBackColor = true;
            // 
            // TBMarketOrder
            // 
            this.TBMarketOrder.Location = new System.Drawing.Point(100, 3);
            this.TBMarketOrder.Name = "TBMarketOrder";
            this.TBMarketOrder.Size = new System.Drawing.Size(91, 20);
            this.TBMarketOrder.TabIndex = 7;
            this.TBMarketOrder.Text = "XBTUSD";
            // 
            // NUDMarketOrderQuantity
            // 
            this.NUDMarketOrderQuantity.Location = new System.Drawing.Point(197, 3);
            this.NUDMarketOrderQuantity.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.NUDMarketOrderQuantity.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
            this.NUDMarketOrderQuantity.Name = "NUDMarketOrderQuantity";
            this.NUDMarketOrderQuantity.Size = new System.Drawing.Size(91, 20);
            this.NUDMarketOrderQuantity.TabIndex = 8;
            // 
            // OutputLabel
            // 
            this.OutputLabel.AutoSize = true;
            this.OutputLabel.Location = new System.Drawing.Point(3, 210);
            this.OutputLabel.Name = "OutputLabel";
            this.OutputLabel.Size = new System.Drawing.Size(52, 13);
            this.OutputLabel.TabIndex = 9;
            this.OutputLabel.Text = "OUTPUT";
            // 
            // NUDPrice
            // 
            this.NUDPrice.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.NUDPrice.Location = new System.Drawing.Point(294, 87);
            this.NUDPrice.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.NUDPrice.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NUDPrice.Name = "NUDPrice";
            this.NUDPrice.Size = new System.Drawing.Size(91, 20);
            this.NUDPrice.TabIndex = 10;
            this.NUDPrice.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NUDMarketOrderQuantity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NUDPrice)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnMarketOrder;
        private System.Windows.Forms.Button btnSwagger;
        private System.Windows.Forms.Button btnLimitOrder;
        private System.Windows.Forms.Button btnGetOrders;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox TBMarketOrder;
        private System.Windows.Forms.NumericUpDown NUDMarketOrderQuantity;
        private System.Windows.Forms.Label OutputLabel;
        private System.Windows.Forms.NumericUpDown NUDPrice;
    }
}

