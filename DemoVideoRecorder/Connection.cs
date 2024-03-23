using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace Cam_App
{

    class Connection
    {
        private static string stringConnection = @"Data Source=LE-DUAN\SQLEXPRESS;Initial Catalog=db_QuanLyCamera;Integrated Security=True";

        public SqlConnection connect()
        {
            SqlConnection cnn = new SqlConnection(stringConnection);
            cnn.Open();
            return cnn;
        }
    }
}
