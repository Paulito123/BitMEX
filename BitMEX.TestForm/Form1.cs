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
using PStrategies.ZoneRecovery;
using System.Data.SqlClient;
using log4net; 

namespace BitMEX.TestForm
{
    public partial class Form1 : Form
    {
        private MordoR mconn;
        private MordoR connLong;
        private MordoR connShort;
        private Calculator calc;
        private Dictionary<long, MordoR> Connections;
        ILog log;
        string guid;


        // TESTING
        object _object;
        private DateTime NextServerReleaseDateTime;

        //private static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);
        //private static readonly string API_KEY = "QbpGewiOyIYMbyQ-ieaTKfOJ";
        //private static readonly string API_SECRET = "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf";

        // TESTLONG  [51091]    : "QbpGewiOyIYMbyQ-ieaTKfOJ" - "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf"
        // TESTSHORT [170591]   : "xEuMT-y7ffwxrvHA2yDwL1bZ" - "3l0AmJz7l3P47-gK__LwgZQQ23uOKCFhYJG4HeTLlGXadRm6"

        public Form1()
        {
            InitializeComponent();
            InitForm();
        }

        public void PrepareConnections()
        {
            Connections = new Dictionary<long, MordoR>();

            connLong = new MordoR("QbpGewiOyIYMbyQ-ieaTKfOJ", "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf");
            connShort = new MordoR("xEuMT-y7ffwxrvHA2yDwL1bZ", "3l0AmJz7l3P47-gK__LwgZQQ23uOKCFhYJG4HeTLlGXadRm6");

            if (connLong.TryConnect() == BitMEXConnectorStatus.Connected && connShort.TryConnect() == BitMEXConnectorStatus.Connected)
            {
                Connections.Add(connLong.Account, connLong);
                Connections.Add(connShort.Account, connShort);
                calc = new Calculator("XBTUSD", 0.1, 1, 4, 20, 0.02, connLong, connShort);
            }
        }

        private void InitForm()
        {
            btn1.Text = "Start/Stops";

            //btn8.Text = "calc.Evaluate()";
            
            lbl1.Text = "";
            LabelOnOff.Text = "OFF";

            Heartbeat.Interval = 2000;
            TimerTest.Interval = 250;

            btn2.Text = "Test timer";

            //TBMarketOrder.Text = "XBTUSD";

            //log4net.Config.XmlConfigurator.Configure();
            //log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

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

        #region Button handlers

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

        #endregion Button handlers

        private void btn1_Click(object sender, EventArgs e)
        {
            if (Heartbeat.Enabled)
            {
                Heartbeat.Stop();
                LabelOnOff.Text = "OFF";
            }
            else
            {
                PrepareConnections();
                Heartbeat.Start();
                LabelOnOff.Text = "ON";
            }
                
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

        #region Timer handlers

        private void Heartbeat_Tick(object sender, EventArgs e)
        {
            if (calc != null)
            {
                calc.Evaluate();

                lbl2.Text = "L:" + connLong.LastKnownRateLimit.ToString();
                lbl3.Text = "S:" + connShort.LastKnownRateLimit.ToString();
            }

            lbl1.Text = "Status:" + calc.GetStatus().ToString();
        }

        private void TimerTest_Tick(object sender, EventArgs e)
        {

        }

        #endregion Timer handlers

        private void btn2_Click(object sender, EventArgs e)
        {

        }
    }
}



//if (o is List<ZoneRecoveryOrder>)
//{
//    foreach (ZoneRecoveryOrder zo in (List<ZoneRecoveryOrder>)o)
//    {
//        //MessageBox.Show(zo.ToString());

//        if (zo.ServerResponseInitial is OrderResponse)
//        {
//            if(zo.ServerResponseInitial != null)
//                MessageBox.Show(((OrderResponse)zo.ServerResponseInitial).ClOrdId.ToString());
//            else
//                MessageBox.Show("ServerResponseInitial NULL > " + zo.ToString());
//        }
//        else if (zo.ServerResponseInitial is BaseError)
//        {
//            MessageBox.Show(((BaseError)zo.ServerResponseInitial).Error.Message);
//        }
//        else if (zo.ServerResponseInitial == null)
//        {
//            MessageBox.Show("NULL = " + zo.ToString());
//            //MessageBox.Show("NULL");
//        }
//        else
//        {
//            MessageBox.Show(zo.ServerResponseInitial.GetType().ToString());
//        }
//    }
//}
//else
//    MessageBox.Show("Dikke Sheiss");

//calc.SetUnitSize(1000);

//double breakEvenPrice = calc.CalculateBreakEvenPrice();
//double direction = -calc.GetNextDirection();
//double totalExposure = calc.CalculateTotalOpenExposure();
//double MinimumProfitPercentage = Convert.ToDouble(TBClOrdId.Text);

//double gewoon = breakEvenPrice + (direction * (totalExposure * MinimumProfitPercentage));
//double result = Math.Round(breakEvenPrice + (direction * (totalExposure * MinimumProfitPercentage)));

//OutputLabel.Text = OutputLabel.Text + MinimumProfitPercentage.ToString() + "=>" + gewoon.ToString() + "||"; //+ Environment.NewLine

//MessageBox.Show(breakEvenPrice.ToString());
//MessageBox.Show(direction.ToString());
//MessageBox.Show(totalExposure.ToString());
//MessageBox.Show(MinimumProfitPercentage.ToString());
//MessageBox.Show("gewoon=" + gewoon.ToString());
//MessageBox.Show("result=" + result.ToString());

//string s = "TP-Price=" + calc.CalculatePriceForOrderType(ZoneRecoveryOrderType.TP).ToString();
//s = s + Environment.NewLine + "TP-Qty=" + calc.CalculateQtyForOrderType(ZoneRecoveryOrderType.TP).ToString();
//s = s + Environment.NewLine + "BE-Price=" + calc.CalculateBreakEvenPrice().ToString();
//s = s + Environment.NewLine + "TE=" + calc.CalculateTotalOpenExposure().ToString();
//s = s + Environment.NewLine + "DIR=" + calc.GetNextDirection().ToString();
//s = s + Environment.NewLine + "-----------------";
//s = s + Environment.NewLine + "TP-Price=" + calc.CalculatePriceForOrderType(ZoneRecoveryOrderType.REV).ToString();
//s = s + Environment.NewLine + "TP-Qty=" + calc.CalculateQtyForOrderType(ZoneRecoveryOrderType.REV).ToString();
//MessageBox.Show(s);


// TODO Error: Price must be a number
// TODO Error: stopPx must be a number

//MessageBox.Show(o.ToString());
//if (o is string)
//    MessageBox.Show(o.ToString());
//else if(o is List<ZoneRecoveryOrder>)
//    MessageBox.Show(o.ToString());