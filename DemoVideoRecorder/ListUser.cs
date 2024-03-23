using onvif.services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cam_App
{
    public class ListUser
    {
        private static ListUser instance;

        private List<User> listAccountUser;
        public List<User> ListAccountUser { get => listAccountUser; set => listAccountUser = value; }
        public static ListUser Instance
        {
            get
            {
                if (instance == null)
                    instance = new ListUser();
                return instance;
            }
            set => instance = value;
        }

        private ListUser()
        {
            listAccountUser = new List<User>();
            //listAccountUser.Add(new User("admin", "123456", true));
            //listAccountUser.Add(new User("huonghtt", "170701", false));
        }
    }
}
