using _03_Onvif_Network_Video_Recorder;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Cam_App
{
    public partial class formDangNhap : Form
    {
        private bool accountType;

        formDangKy dangky = new formDangKy();

        public formDangNhap()
        {
            InitializeComponent();
        }

        //kiểm tra tài khoản đúng không
        bool CheckLogin(string userName, string passWord)
        {
            SqlConnection cnn = new Connection().connect();
            SqlCommand cmd = cnn.CreateCommand();
            cmd.CommandText = "select * from users where TaiKhoan = '" + userName + "' and MatKhau = '" + passWord + "'";           
            cmd.CommandType = CommandType.Text;
            SqlDataReader adt = cmd.ExecuteReader();
            if (adt.HasRows)
            {
                return true;
            }
            return false;

        }

        private bool CheckLogin(string userName, string passWord, object accountType)
        {
            throw new NotImplementedException();
        }

        private void F_LogOut(object sender, EventArgs e)
        {
            (sender as MainForm).isExit = false;
            (sender as MainForm).Close();
            this.Show();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void formDangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        //hiển thị mật khẩu
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBoxHienThiMatKhau.Checked)
            {
                txtMatKhau.UseSystemPasswordChar = false;
            }    
            if(!checkBoxHienThiMatKhau.Checked)
            {
                txtMatKhau.UseSystemPasswordChar = true;
            }    
        }

        private void formDangNhap_Load(object sender, EventArgs e)
        {         
         
        }
        
        private void btnDangNhap_Click(object sender, EventArgs e)
        {   
            SqlConnection cnn = new SqlConnection(@"Data Source=DESKTOP-Q6N4J46\SQLEXPRESS;Initial Catalog=db_QuanLyCamera;Integrated Security=True");
            SqlDataAdapter da = new SqlDataAdapter("select * from TaiKhoan where TaiKhoan = '"+ txtTaiKhoan.Text + "' and MatKhau = '" + txtMatKhau.Text + "'", cnn);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {             
                this.Hide();                
                MainForm f = new MainForm(dt.Rows[0][2].ToString());
                f.Show();
            }
            else
            {
                this.Hide();
                MessageBox.Show("Sai tên tài khoản hoặc mật khẩu. Vui lòng thử lại.", "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                formDangNhap c = new formDangNhap();
                c.Show();
            }
        }
    }
}
