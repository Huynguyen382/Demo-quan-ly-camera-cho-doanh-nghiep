using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using _03_Onvif_Network_Video_Recorder;

namespace Cam_App
{
    public partial class formDangKy : Form
    {
        SqlConnection cnn;
        SqlCommand cmd;
        string str = @"Data Source=DESKTOP-Q6N4J46\SQLEXPRESS;Initial Catalog=db_QuanLyCamera;Integrated Security=True";
        SqlDataAdapter adapter = new SqlDataAdapter();
        DataTable table = new DataTable();

        List<string> listAccountType = new List<string>() { "Admin", "User" };
        int index = -1;
        public formDangKy()
        {
            InitializeComponent();
        }
        //hiện danh sách user lên dtgv
        void LoadData()
        {
            cmd = cnn.CreateCommand();
            cmd.CommandText = "select * from TaiKhoan";
            adapter.SelectCommand = cmd;
            table.Clear();
            adapter.Fill(table);
            dtgvUser.DataSource = table;
        }

        private void formDangKy_Load(object sender, EventArgs e)
        {
            cboLoaitaikhoan.DataSource = listAccountType;
            LoadListUser();
            cnn = new SqlConnection(str);
            cnn.Open();
            LoadData();
        }

        private void dtgvUser_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            /*int i;
            i = dtgvUser.CurrentRow.Index;
            txtTaiKhoan.Text = dtgvUser.Rows[i].Cells[1].Value.ToString();
            txtMatKhau.Text = dtgvUser.Rows[i].Cells[2].Value.ToString();
            cboLoaitaikhoan.Text = dtgvUser.Rows[i].Cells[3].Value.ToString();*/

        }

        void LoadListUser()
        {
            dtgvUser.DataSource = null;
            dtgvUser.DataSource = ListUser.Instance.ListAccountUser;
            dtgvUser.Refresh();
        }

        //phần khi click vào 1 dòng thì thông tin sẽ hiện lên ô nhập
        private void dtgvUser_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int i;
            i = dtgvUser.CurrentRow.Index;
            txtTaiKhoan.Text = dtgvUser.Rows[i].Cells[0].Value.ToString();
            txtMatKhau.Text = dtgvUser.Rows[i].Cells[1].Value.ToString();
            cboLoaitaikhoan.Text = dtgvUser.Rows[i].Cells[2].Value.ToString();
            LoadData();

        }

        private void btnThem_Click(object sender, EventArgs e)
        {

            cmd = cnn.CreateCommand();
            cmd.CommandText = "insert into TaiKhoan values('" + txtTaiKhoan.Text + "', '" + txtMatKhau.Text + "', '" + cboLoaitaikhoan.Text + "')";
            cmd.ExecuteNonQuery();
            LoadData();
            txtTaiKhoan.Text = "";
            txtMatKhau.Text = "";
            cboLoaitaikhoan.Text = "";
            //formDangKy dangky = new formDangKy();
            //MainForm mainform = new MainForm("");
            //this.Close();

            // mainform.ShowDialog();
            //dangky.Show();
        }
        private void btnSua_Click(object sender, EventArgs e)
        {
            cmd = cnn.CreateCommand();
            cmd.CommandText = "update TaiKhoan set MatKhau = '" + txtMatKhau.Text + "', Quyen = '" + cboLoaitaikhoan.Text + "' where TaiKhoan = '" + txtTaiKhoan.Text + "'";
            cmd.ExecuteNonQuery();
            LoadData();
            txtTaiKhoan.Text = "";
            txtMatKhau.Text = "";
            cboLoaitaikhoan.Text = "";

        }
        //xóa tài khoản
        private void btnXoa_Click(object sender, EventArgs e)
        {
            //txtTaiKhoan.ReadOnly = true; //-> không được sửa tên tài khoản
            //cần thêm phần kiểm tra ô tài khoản trống thì hiện thông báo

            cmd = cnn.CreateCommand();
            cmd.CommandText = "delete from TaiKhoan where TaiKhoan = '" + txtTaiKhoan.Text + "'";
            cmd.ExecuteNonQuery();
            LoadData();
            txtTaiKhoan.Text = "";
            txtMatKhau.Text = "";
            cboLoaitaikhoan.Text = "";

        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            //formDangKy dangky = new formDangKy();
            // MainForm main = new MainForm("");
            this.Close();

            // main.ShowDialog();
            //dangky.Show();
        }
    }
}
