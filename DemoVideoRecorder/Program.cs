using Cam_App;
using System;
using System.Windows.Forms;

namespace _03_Onvif_Network_Video_Recorder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new formDangNhap());
        }
    }
}
