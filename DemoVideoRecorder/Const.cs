using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Cam_App
{
    class Const
    {
        public static bool AccountType;
        public void check(bool account)
        {
            SqlConnection cnn = new Connection().connect();

            SqlCommand cmd = cnn.CreateCommand();
            string var = cmd.CommandText = "select * from users where LoaiTaiKhoan = '" + account + "'";

            cmd.CommandType = CommandType.Text;
            SqlDataReader adt = cmd.ExecuteReader();

        }

    }
}
