using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
//using Serilog;
using BitMEX.Model;
using BitMEX.Client;
using System.Data.SqlClient;
//using Bitmex.Client.Websocket;
using log4net; 

namespace BitMEX.TestForm
{
    public partial class Form1 : Form
    {
        private MordoR mconn;
        ILog log;
        string guid;

        //private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);
        //private static readonly string API_KEY = "rTAFXRKn2dLARuG_t1dDOtgI";
        //private static readonly string API_SECRET = "K2LmL6aTbj8eW_LVj7OLa7iA6eZa8TJMllh3sjCynV4fpnMr";
        
        public Form1()
        {
            InitializeComponent();
            guid = MordoR.GenerateGUID();
            mconn = new MordoR();
            TBMarketOrder.Text = "XBTUSD";

            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        private void btnMarketOrder_Click(object sender, EventArgs e)
        {
            log.Info("btnMarketOrder_Click Clicked!");

            // Local environment checks...
            if (NUDMarketOrderQuantity.Value >= 1 || NUDMarketOrderQuantity.Value <= -1)
            {
                lock (guid)
                {
                    // Catch API and connection errors
                    try
                    {
                        object outcome = mconn.MarketOrder(
                            TBMarketOrder.Text.ToString(),
                            guid,
                            Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value));

                        log.Info("Marked order sent: Qty = " + NUDMarketOrderQuantity.Value.ToString());

                        if (outcome != null)
                        {
                            if (outcome.GetType().ToString() == "BitMEX.Model.OrderResponse")
                            {
                                // Successful API call with successful result...
                                log.Info((OrderResponse)outcome);
                                //MessageBox.Show("Order success: " + clOrdID + "=" + ((OrderResponse)outcome).ClOrdId.ToString());
                            }
                            else
                            {
                                // Successful API call with error as result...
                                log.Info((BaseError)outcome);
                                //MessageBox.Show("Order error: " + ((BaseError)outcome).Error.Message.ToString());
                            }
                        }
                        else
                        {
                            log.Error("Outcome is null");
                            //MessageBox.Show("Outcome is null");
                        }
                        guid = MordoR.GenerateGUID();
                        log.Info("Guid changed to " + guid);
                    }
                    catch (Exception exc)
                    {
                        // Catch all external exceptions like connection issues etc.
                        log.Error(exc);
                        //MessageBox.Show("Exception[" + exc.Message.ToString() + "]");
                    }
                }
            }
            log.Info("btnMarketOrder_Click End!");
        }
        
        private void btnLimitOrder_Click(object sender, EventArgs e)
        {
            log.Info("btnLimitOrder_Click Clicked!");

            // Local environment checks...
            if ((NUDMarketOrderQuantity.Value >= 1 || NUDMarketOrderQuantity.Value <= -1))
            {
                lock (guid)
                {
                    // Catch API and connection errors
                    try
                    {
                        OrderResponse orderResp = new OrderResponse();
                        BaseError orderErr = new BaseError();

                        object outcome = mconn.LimitOrder(
                            TBMarketOrder.Text.ToString(),
                            guid,
                            Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value),
                            Decimal.ToInt32((decimal)NUDPrice.Value));

                        log.Info("Limit order sent: Price = " + NUDPrice.Value.ToString() + " & = Qty = " + NUDMarketOrderQuantity.Value.ToString());

                        if (outcome.GetType() == orderResp.GetType())
                            log.Info((OrderResponse)outcome);
                        else if (outcome.GetType() == orderErr.GetType())
                            log.Error((BaseError)outcome);
                        else
                            log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");
                        TBClOrdId.Text = guid;
                        guid = MordoR.GenerateGUID();
                        log.Info("Guid changed to " + guid);
                    }
                    catch (Exception exc)
                    {
                        log.Error("Exception[" + exc.Message.ToString() + "]");
                    }
                }
            }
            log.Info("btnLimitOrder_Click End!");
        }

        private void btnGetOpenOrders_Click(object sender, EventArgs e)
        {
            log.Info("btnGetOpenOrders_Click Clicked!");
            
            // Local environment checks...
            if (!String.IsNullOrEmpty(TBClOrdId.Text))
            {
                
                try
                {
                    List<OrderResponse> orderResp = new List<OrderResponse>();
                    BaseError orderErr = new BaseError();
                    object outcome = mconn.GetOpenOrdersForSymbol(TBMarketOrder.Text.ToString());

                    log.Info("Get open orders sent");

                    if (outcome.GetType() == orderResp.GetType())
                    {
                        // Successful API call with successful result...
                        //string orderAccumulation = "";
                        orderResp = (List<OrderResponse>)outcome;

                        foreach (var resp in orderResp.Where(x => x.OrdStatus == "New")/*.Select(n => n.OrderId)*/)
                        {
                            log.Info(resp.ToString());
                            //orderAccumulation = orderAccumulation + "¦" + resp.ToString();
                        }
                        //MessageBox.Show(orderAccumulation);
                    }
                    else if (outcome.GetType() == orderErr.GetType())
                    {
                        // Successful API call with error as result...
                        log.Error(((BaseError)outcome).ToString());
                        //orderErr = (BaseError)outcome;
                        //MessageBox.Show("BitMEX API Error [" + orderErr.Error.Message.ToString() + "]");
                    }
                    else
                    {
                        log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");
                        // Should never happen...
                        //MessageBox.Show("Unknown return type [" + outcome.GetType().ToString() + "]");
                    }
                }
                catch (Exception exc)
                {
                    // Unsuccessful API call
                    log.Error("Exception[" + exc.Message.ToString() + "]");
                    //MessageBox.Show("Exception [" + exc.Message.ToString() + "]");
                }
            }
            log.Info("btnGetOpenOrders_Click End!");
        }
        
        private void btnStopOrder_Click(object sender, EventArgs e)
        {
            log.Info("btnStopOrder_Click Clicked!");
            
            lock (guid)
            {
                // Catch API and connection errors
                try
                {
                    OrderResponse orderResp = new OrderResponse();
                    BaseError orderErr = new BaseError();

                    object outcome = mconn.StopLimitOrder(
                        TBMarketOrder.Text.ToString(),
                        guid,
                        Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value),
                        Decimal.ToInt32((decimal)NUDPrice.Value),
                        Decimal.ToInt32((decimal)NUDStopOrder.Value)
                        );

                    log.Info("Stop order sent: stopPx = " + NUDStopOrder.Value.ToString() + " & = Qty = " + NUDMarketOrderQuantity.Value.ToString());

                    if (outcome.GetType() == orderResp.GetType())
                        log.Info((OrderResponse)outcome);
                    else if (outcome.GetType() == orderErr.GetType())
                        log.Error((BaseError)outcome);
                    else
                        log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");

                    TBClOrdId.Text = guid;
                    guid = MordoR.GenerateGUID();
                    log.Info("Guid changed to " + guid);
                }
                catch (Exception exc)
                {
                    log.Error("Exception[" + exc.Message.ToString() + "]");
                }
            }
            
            log.Info("btnStopOrder_Click End!");
        }

        private void btnGetOrdersForId_Click(object sender, EventArgs e)
        {
            //log.Info("btnGetOpenOrders_Click Clicked!");

            //// Local environment checks...
            //if (!String.IsNullOrEmpty(TBClOrdId.Text))
            //{
            //    string IDInput = TBClOrdId.Text;

            //    try
            //    {
            //        //object outcome = mconn.GetFilledOrdersForId(IDInput);

            //        log.Info("Get closed orders for ID:" + IDInput);

            //        if (outcome.GetType() == new List<OrderResponse>().GetType())
            //        {
            //            // Successful API call with successful result...
            //            var orderResp = (List<OrderResponse>)outcome;
            //            foreach (var resp in orderResp)
            //                log.Info(resp);
            //        }
            //        else if(outcome.GetType() == new OrderResponse().GetType())
            //            log.Info(outcome);
            //        else if (outcome.GetType() == new BaseError().GetType())
            //            log.Error(outcome);
            //        else
            //            log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");
            //    }
                
            //    catch (Exception exc)
            //    {
            //        // Unsuccessful API call
            //        log.Error("Exception [" + exc.Message.ToString() + "]");
            //    }
            //}
            //log.Info("btnGetOpenOrders_Click End!");
        }

        private void DBLogOperation(string operation, object obj)
        {
            try
            {
                //// Build connection string
                //SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                //builder.DataSource = "localhost";   // update me
                //builder.UserID = "sa";              // update me
                //builder.Password = "your_password";      // update me
                //builder.InitialCatalog = "master";

                string connectionString = @"Data Source=.\MYSQLSDB;Initial Catalog=Trading;Integrated Security=true;";

                // Connect to SQL
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Connected !

                    //String sql = "DROP DATABASE IF EXISTS [SampleDB]; CREATE DATABASE [SampleDB]";
                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{
                    //    command.ExecuteNonQuery();
                    //    Console.WriteLine("Done.");
                    //}

                    // Create a Table and insert some sample data
                    //StringBuilder sb = new StringBuilder();
                    //sb.Append("USE SampleDB; ");
                    //sb.Append("CREATE TABLE Employees ( ");
                    //sb.Append(" Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY, ");
                    //sb.Append(" Name NVARCHAR(50), ");
                    //sb.Append(" Location NVARCHAR(50) ");
                    //sb.Append("); ");
                    //sb.Append("INSERT INTO Employees (Name, Location) VALUES ");
                    //sb.Append("(N'Jared', N'Australia'), ");
                    //sb.Append("(N'Nikita', N'India'), ");
                    //sb.Append("(N'Tom', N'Germany'); ");
                    //sql = sb.ToString();
                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{
                    //    command.ExecuteNonQuery();
                    //    Console.WriteLine("Done.");
                    //}

                    //// INSERT demo
                    //Console.Write("Inserting a new row into table, press any key to continue...");
                    //Console.ReadKey(true);
                    //sb.Clear();
                    //sb.Append("INSERT Employees (Name, Location) ");
                    //sb.Append("VALUES (@name, @location);");
                    //sql = sb.ToString();
                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{
                    //    command.Parameters.AddWithValue("@name", "Jake");
                    //    command.Parameters.AddWithValue("@location", "United States");
                    //    int rowsAffected = command.ExecuteNonQuery();
                    //    Console.WriteLine(rowsAffected + " row(s) inserted");
                    //}

                    //// UPDATE demo
                    //String userToUpdate = "Nikita";
                    //Console.Write("Updating 'Location' for user '" + userToUpdate + "', press any key to continue...");
                    //Console.ReadKey(true);
                    //sb.Clear();
                    //sb.Append("UPDATE Employees SET Location = N'United States' WHERE Name = @name");
                    //sql = sb.ToString();
                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{
                    //    command.Parameters.AddWithValue("@name", userToUpdate);
                    //    int rowsAffected = command.ExecuteNonQuery();
                    //    Console.WriteLine(rowsAffected + " row(s) updated");
                    //}

                    //// DELETE demo
                    //String userToDelete = "Jared";
                    //Console.Write("Deleting user '" + userToDelete + "', press any key to continue...");
                    //Console.ReadKey(true);
                    //sb.Clear();
                    //sb.Append("DELETE FROM Employees WHERE Name = @name;");
                    //sql = sb.ToString();
                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{
                    //    command.Parameters.AddWithValue("@name", userToDelete);
                    //    int rowsAffected = command.ExecuteNonQuery();
                    //    Console.WriteLine(rowsAffected + " row(s) deleted");
                    //}

                    //// READ demo
                    //Console.WriteLine("Reading data from table, press any key to continue...");
                    //Console.ReadKey(true);
                    //sql = "SELECT Id, Name, Location FROM Employees;";
                    //using (SqlCommand command = new SqlCommand(sql, connection))
                    //{

                    //    using (SqlDataReader reader = command.ExecuteReader())
                    //    {
                    //        while (reader.Read())
                    //        {
                    //            Console.WriteLine("{0} {1} {2}", reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                    //        }
                    //    }
                    //}
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("All done. Press any key to finish...");
            Console.ReadKey(true);
        }

        #region HELPERS

        private static long ToUnixTimeSeconds(DateTimeOffset dateTimeOffset)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (dateTimeOffset.ToUniversalTime() - unixStart).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        private long GetExpires()
        {
            return ToUnixTimeSeconds(DateTimeOffset.UtcNow) + 3600; // set expires one hour in the future
        }

        #endregion HELPERS
        
        

        private void btnAmend_Click(object sender, EventArgs e)
        {
            log.Info("btnAmend_Click Clicked!");

            // Local environment checks...
            if ((NUDMarketOrderQuantity.Value >= 1 || NUDMarketOrderQuantity.Value <= -1))
            {
                lock (guid)
                {
                    // Catch API and connection errors
                    try
                    {
                        OrderResponse orderResp = new OrderResponse();
                        BaseError orderErr = new BaseError();

                        object outcome = mconn.AmendOrder(
                            TBClOrdId.Text,
                            Decimal.ToInt32((decimal)NUDPrice.Value),
                            Decimal.ToInt32((decimal)NUDMarketOrderQuantity.Value)
                            );

                        log.Info("Amend order sent: Price = " + NUDPrice.Value.ToString() + " & = Qty = " + NUDMarketOrderQuantity.Value.ToString());

                        if (outcome.GetType() == orderResp.GetType())
                            log.Info((OrderResponse)outcome);
                        else if (outcome.GetType() == orderErr.GetType())
                            log.Error((BaseError)outcome);
                        else
                            log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");
                        
                    }
                    catch (Exception exc)
                    {
                        log.Error("Exception[" + exc.Message.ToString() + "]");
                    }
                }
            }
            log.Info("btnAmend_Click End!");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //log.Info("btnClose_Click Clicked!");

            //try
            //{
            //    OrderResponse orderResp = new OrderResponse();
            //    BaseError orderErr = new BaseError();

            //    string IDInput = TBClOrdId.Text;
            //    object outcome = mconn.ClosePostion(
            //                    TBMarketOrder.Text,
            //                    (double)NUDPrice.Value,
            //                    RBBuy.Checked ? "Buy" : "Sell",
            //                    (double)NUDMarketOrderQuantity.Value
            //                    );

            //    log.Info("Cancel order [" + IDInput + "].");

            //    if (outcome.GetType() == orderResp.GetType())
            //        log.Info((OrderResponse)outcome);
            //    else if (outcome.GetType() == orderErr.GetType())
            //        log.Error((BaseError)outcome);
            //    else
            //        log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");

            //}
            //catch (Exception exc)
            //{
            //    log.Error("Exception[" + exc.Message.ToString() + "]");
            //}

            //log.Info("btnClose_Click End!");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            log.Info("btnCancel_Click Clicked!");

            try
            {
                OrderResponse orderResp = new OrderResponse();
                BaseError orderErr = new BaseError();

                string IDInput = TBClOrdId.Text;
                object outcome = mconn.CancelOrder(
                                IDInput,
                                "Schliessen die handel!"
                                );

                log.Info("Cancel order [" + IDInput + "].");

                if (outcome.GetType() == orderResp.GetType())
                    log.Info((OrderResponse)outcome);
                else if (outcome.GetType() == orderErr.GetType())
                    log.Error((BaseError)outcome);
                else
                    log.Error("Unknown return type [" + outcome.GetType().ToString() + "]");

            }
            catch (Exception exc)
            {
                log.Error("Exception[" + exc.Message.ToString() + "]");
            }

            log.Info("btnCancel_Click End!");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
