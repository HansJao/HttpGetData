using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HungyiDB;
using System.Data;
using System.Xml;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using Newtonsoft.Json;

namespace HttpGetData
{
    public partial class GetSqlData : System.Web.UI.Page
    {
        XmlDocument OrderData;
        protected void Page_Load(object sender, EventArgs e)
        {
            string ActionMethod;

            string data = new System.IO.StreamReader(this.Request.InputStream).ReadToEnd();
            if (data.Contains("OrderData"))
            {
                ActionMethod = "InsertOrderData";
                OrderData = JsonConvert.DeserializeXmlNode("{\"Content\":" + data + "}");
            }
            else
            {
                ActionMethod = this.Request.Form["ActionMethod"].ToString();
            }

            switch (ActionMethod)
            {
                case "GetCustomerData":
                    GetCustomerData();
                        break;
                case "GetProductData":
                    GetProductData();
                    break;
                case "GetCompleteData":
                    GetCompleteData();
                    break;
                case "InsertOrderData":
                    InsertOrderData();
                    break;
                default:
                    HttpResponse.RetrunXmlData(this, 123, "<![CDATA[TC1*1黑20.1kg]]>");
                    break;
            }
           // HttpResponse.RetrunXmlData(this, 123,"<StateMessage><![CDATA[" + ID + "]]></StateMessage>");
            
        }

        private void InsertOrderData()
        {
            /*
                Step1:Split ProductId 以','分隔
                Step2:新增一筆新的訂單 此筆訂單是出貨給某家 並且回傳該訂單單號
                Step3:把每一個ProductID的訂單編號更新為此筆訂單的編號
            */
            XmlNodeList ContentNode = OrderData.SelectNodes("Content/OrderData");
            string ProductID = "";
            string CustomerData = "";
            foreach (XmlNode xn in ContentNode)
            {
                ProductID = xn.SelectSingleNode("ProductID").InnerText;
                CustomerData = xn.SelectSingleNode("CustomerID").InnerText;
                //string ProductID = this.Request.Form["ProductID"].ToString();
                string[] ProductIDArray = ProductID.Split(',');
                string[] CustomerDataArray = CustomerData.Split(',');
                Program GetDB = new Program();
                int OrderID = GetDB.InsertOrderData(CustomerDataArray[0]);//第0筆資料為廠商id
                foreach (string Product_ID in ProductIDArray)
                {
                    GetDB.UpdateProduct(Product_ID.Substring(0, 12), OrderID.ToString());
                }
            }

        }


        private void GetCompleteData()
        {
            string ProductData = this.Request.Form["ProductData"].ToString();
            Program GetDB = new Program();
            DataTable dt = GetDB.GetCustomerAll("SELECT * FROM Product WHERE ProductID=" + "2");
            IWorkbook wb = new HSSFWorkbook();
            ISheet ws;

            ////建立Excel 2007檔案
            //IWorkbook wb = new XSSFWorkbook();
            //ISheet ws;

            if (dt.TableName != string.Empty)
            {
                ws = wb.CreateSheet(dt.TableName);
            }
            else
            {
                ws = wb.CreateSheet("Sheet1");
            }

            ws.CreateRow(0);//第一行為欄位名稱
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                ws.GetRow(0).CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ws.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    ws.GetRow(i + 1).CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            FileStream file = new FileStream(@"C:\ExcelTest\npoi.xls", FileMode.Create);//產生檔案
            wb.Write(file);
            file.Close();
        }
        private void GetCustomerData()
        {
            Program GetDB = new Program();
            DataTable dt = GetDB.GetCustomerAll("SELECT * FROM Customer");

            XmlDocument xDoc = new XmlDocument();
            XmlElement Content;
            XmlElement ele;
            Content = xDoc.CreateElement("Content");
            foreach (DataRow dr in dt.Rows)
            {
                ele = xDoc.CreateElement("CustomerID");
                ele.InnerText = dr["CustomerID"].ToString().Trim();
                Content.AppendChild(ele);

                ele = xDoc.CreateElement("CustomerName");
                ele.InnerText = dr["CustomerName"].ToString().Trim();
                Content.AppendChild(ele);
            }
            xDoc.AppendChild(Content);

            HttpResponse.RetrunXmlData(this, 123,xDoc.InnerXml);
        }
        private void GetProductData()
        {
            string ProductID = this.Request.Form["ProductID"].ToString();
            ProductID = ProductID.Substring(0, 12);
            Program GetDB = new Program();
            DataTable dt = GetDB.GetCustomerAll("SELECT * FROM Product WHERE ProductID="+ProductID);

            XmlDocument xDoc = new XmlDocument();
            XmlElement Content;
            XmlElement ele;
            Content = xDoc.CreateElement("Content");
            foreach(DataRow dr in dt.Rows)
            {
                ele = xDoc.CreateElement("ProductID");
                ele.InnerText = dr["ProductID"].ToString().Trim();
                Content.AppendChild(ele);

                ele = xDoc.CreateElement("ProductName");
                ele.InnerText = dr["ProductName"].ToString().Trim();
                Content.AppendChild(ele);

                ele = xDoc.CreateElement("TextileColor");
                ele.InnerText = dr["TextileColor"].ToString().Trim();
                Content.AppendChild(ele);

                ele = xDoc.CreateElement("ProductWeight");
                ele.InnerText = dr["ProductWeight"].ToString().Trim();
                Content.AppendChild(ele);
            }
            xDoc.AppendChild(Content);

            // HttpResponse.RetrunXmlData(this, 123, xDoc.InnerXml);
            HttpResponse.RetrunXmlData(this, 123, xDoc.InnerXml);

        }
    }
}