using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace GoLogger
{
    public class ConnectDB
    {
        SqlConnection cnn = null;
        public bool SQLIsConnect { get; set; }

        public void connect(string connectionString)
        {
            if (cnn == null)
            {
                cnn = new SqlConnection(connectionString);
                try
                {
                    cnn.Open();
                    SQLIsConnect = true;
                }
                catch (SqlException sqlex)
                {
                    SQLIsConnect = false;
                    throw sqlex;
                }
            }
        }
        public void disconnect()
        {
            if (cnn != null)
            {
                cnn.Close();
                cnn = null;
            }
        }

        public void insert(string sqlComand)
        {
            SqlCommand cmd = new SqlCommand(sqlComand, cnn);
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.InsertCommand = cmd;
            adapter.InsertCommand.ExecuteNonQuery();
            cmd.Dispose();
        }
    }
}
