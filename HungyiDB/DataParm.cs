using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungyiDB
{
    public class DataParm
    {
        public string _ParmName;
        public object _ParmValue;

        public DataParm(string ParmName, object ParmValue)
        {
            _ParmName = ParmName;
            _ParmValue = ParmValue;
        }
    }

    public class MsSQLData
    {
        SqlConnection cn;
        SqlCommand SqlCmd;
        public MsSQLData(string ConnStr, string Command)
        {
            cn = new SqlConnection(ConnStr);
            SqlCmd = new SqlCommand(Command, cn);
        }
        public MsSQLData(string ConnStr, string Command, params DataParm[] DataParm)
        {
            cn = new SqlConnection(ConnStr);
            SqlCmd = new SqlCommand(Command, cn);
            foreach (DataParm dp in DataParm)
            {
                SqlCmd.Parameters.AddWithValue(dp._ParmName, dp._ParmValue);
            }
        }
        public MsSQLData(string Sp,params DataParm[] DataParm)
        {
            string ConnStr = System.Configuration.ConfigurationSettings.AppSettings["SqlConnection"];
            cn = new SqlConnection(ConnStr);
            SqlCmd = new SqlCommand(Sp, cn);
            SqlCmd.CommandType = CommandType.StoredProcedure;
            foreach (DataParm dp in DataParm)
            {
                SqlCmd.Parameters.AddWithValue(dp._ParmName, dp._ParmValue);
            }
        }
        public DataTable SelectMsSQLData()
        {
            DataTable dt = new DataTable();
            cn.Open();
            dt.Load(SqlCmd.ExecuteReader());
            cn.Close();
            return dt;
        }
        public void MSSqlDataNoneParm()
        {
            cn.Open();
            SqlCmd.ExecuteNonQuery();
            cn.Close();
        }

        public int InsertAndGetIdentity()
        {
            DataTable dt = new DataTable();
            cn.Open();
            int OrderId =Convert.ToInt32(SqlCmd.ExecuteScalar().ToString());
            cn.Close();

            return OrderId;
        }

    }
}
