using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungyiDB
{
    public class Program
    {
        static void Main(string[] args)
        {
        }
        public DataTable SelectData(string cmd)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"]);
            SqlDataAdapter SqlDA = new SqlDataAdapter(cmd, cn);
            SqlDA.Fill(ds, "MyTable");
            dt = ds.Tables[0];
            return dt;
        }

        public void SelectDataWithParm(string cmd)
        {
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            SqlConnection cn = new SqlConnection(System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"]);
            cn.Open();
            SqlCommand SqlCmd = new SqlCommand(cmd, cn);
            SqlCmd.Parameters.AddWithValue("@Name", "Hanhan");
            SqlCmd.Parameters.AddWithValue("@PWD", "8520");
          
            dt.Load(SqlCmd.ExecuteReader());
            cn.Close();
            //dt = ds.Tables[0];
        }

        public DataTable GetCustomerData(string cmd, string Name, string PWD)
        {
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];

            MsSQLData msData = new MsSQLData(cn, cmd,
                new DataParm("@Name", Name),
                new DataParm("@PWD", PWD));

            DataTable dt = msData.SelectMsSQLData();
            return dt;
        }

        public DataTable GetCustomerAll(string cmd)
        {
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            MsSQLData msData = new MsSQLData(cn, cmd);
            DataTable dt = msData.SelectMsSQLData();
            return dt;
        }

        public DataTable GetSqlDataNoParm(string cmd)
        {
           // string cmd = @"SELECT [Order].OrderID,[Customer].CustomerName,[Order].WhoShip,[Order].CreateDate from [Order],Customer where [Customer].CustomerID =[Order].ShipToWhere";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            MsSQLData msData = new MsSQLData(cn, cmd);
            DataTable dt = msData.SelectMsSQLData();
            return dt;
        }

        public DataTable GetOrderDetailData(string OrderID)
        {
            string Sp = "SEL_OrderDetail";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];

            MsSQLData msData = new MsSQLData( Sp,
                new DataParm("@OrderID", OrderID));

            DataTable dt = msData.SelectMsSQLData();
            return dt;
        }

        public DataTable GetDetailOrderData(string OrderID)
        {
            string cmd = @"select * from Product where OrderID = @OrderID";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];

            MsSQLData msData = new MsSQLData(cn, cmd,
                new DataParm("@OrderID", OrderID));

            DataTable dt = msData.SelectMsSQLData();
            return dt;
        }

        public int InsertOrderData(string ShipToWhere)//訂單開立並且回傳該訂單的identityID
        {
            string cmd = @"INSERT INTO [Order](ShipToWhere,WhoShip,CreateDate)"+
                @"OUTPUT INSERTED.OrderID values (@ShipToWhere,'test2',GETDATE())";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            MsSQLData msData = new MsSQLData(cn, cmd,
                new DataParm("@ShipToWhere",ShipToWhere));
            
            return msData.InsertAndGetIdentity();
        }

        public void UpdateProduct(string ProductID,string OrderID)
        {
            string cmd = @"update product set OrderID=@OrderID where ProductID=@ProductID";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            MsSQLData msData = new MsSQLData(cn, cmd,
                new DataParm("@ProductID", ProductID),
                new DataParm("@OrderID", OrderID));

            msData.MSSqlDataNoneParm();
        }

        public void UpdataProductUnitPrice(string ProductName,string TextileColor,string UnitPrice)
        {

            string cmd = @"update Product set UnitPrice=@UnitPrice where ProductName=@ProductName AND TextileColor=@TextileColor";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            MsSQLData msData = new MsSQLData(cn, cmd,
                new DataParm("@ProductName", ProductName),
                new DataParm("@TextileColor", TextileColor),
                new DataParm("@UnitPrice",UnitPrice));

            msData.MSSqlDataNoneParm();

        }
        public void InsertProduct(string ProductName,string TextileColor,string ProductWeight,string UnitPrice)
        {
            string cmd = @"Insert into Product (ProductName,TextileColor,ProductWeight,UnitPrice,CreateDate)
                           values (@ProductName,@TextileColor,@ProductWeight,@UnitPrice,GETDATE())";
            string cn = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            MsSQLData msData = new MsSQLData(cn, cmd,
                new DataParm("@ProductName", ProductName),
                new DataParm("@TextileColor", TextileColor),
                new DataParm("@ProductWeight",ProductWeight),
                new DataParm("@UnitPrice", UnitPrice));

            msData.MSSqlDataNoneParm();
        }
    }
}
