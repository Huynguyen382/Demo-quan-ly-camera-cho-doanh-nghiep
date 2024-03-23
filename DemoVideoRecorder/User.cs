using System;
using System.Collections.Generic;
using System.Text;

namespace Cam_App
{
    public class User
    {
        private string userName;
        private string passWord;
        private string account;
        private bool accountType; //phân loại tài khoản
        public string UserName { get => userName; set => userName = value; }
        public string PassWord { get => passWord; set => passWord = value; }

        public string Account { get => account; set => account = value; }
        public bool AccountType { get => accountType; set => accountType = value; }

        /*public User(string userName, string passWord, string acccount, bool accountType)
        {
            this.userName = userName;
            this.passWord = passWord;
            this.account = account;
            this.AccountType = accountType;
        }*/

        public User(string userName, string passWord, string account)
        {
            this.userName = userName;
            this.passWord = passWord;
            this.account = account;
        }
    }
}
