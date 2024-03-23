using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Ozeki.Media;
using _03_Onvif_Network_Video_Recorder.LOG;
using Ozeki.Camera;
using System.Drawing;
using Cam_App;
using Org.BouncyCastle.Utilities.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Media.Media3D;

namespace _03_Onvif_Network_Video_Recorder
{
    public partial class MainForm : Form
    {
        private IpCameraHandler _currentModel;

        private VideoViewerWF _currentVideoViewer;

        private List<VideoViewerWF> _videoViewerList;

        private List<IpCameraHandler> ModelList;

        private CameraURLBuilderWF _myCameraUrlBuilder;
        public bool isExit = true;

        public MainForm(string quyen)
        {
            InitializeComponent();

            Log.OnLogMessageReceived += Log_OnLogMessageReceived;

            _myCameraUrlBuilder = new CameraURLBuilderWF();

            _videoViewerList = new List<VideoViewerWF>();
            ModelList = new List<IpCameraHandler>();

            CreateVideoViewers();
            CreateIPCameraHandlers();

            if (quyen == "Admin")
            {
                btnDangky.Visible = true;
            }
            else
            {
                btnDangky.Visible = false;
            }
        }

        private void CreateVideoViewers()
        {
            _currentVideoViewer = videoViewerWF2;
            _videoViewerList.Add(_currentVideoViewer);
            _videoViewerList.Add(videoViewerWF3);
            _videoViewerList.Add(videoViewerWF4);
            _videoViewerList.Add(videoViewerWF1);
            _videoViewerList.Add(videoViewerWF5);
            _videoViewerList.Add(videoViewerWF6);
            _videoViewerList.Add(videoViewerWF7);
            _videoViewerList.Add(videoViewerWF8);
            _videoViewerList.Add(videoViewerWF9);
            _videoViewerList.Add(videoViewerWF10);
            _videoViewerList.Add(videoViewerWF11);
            _videoViewerList.Add(videoViewerWF12);
            _videoViewerList.Add(videoViewerWF13);
            _videoViewerList.Add(videoViewerWF14);
            _videoViewerList.Add(videoViewerWF15);
            _videoViewerList.Add(videoViewerWF16);
            _videoViewerList.Add(videoViewerWF17);
            _videoViewerList.Add(videoViewerWF18);
            _videoViewerList.Add(videoViewerWF19);
            _videoViewerList.Add(videoViewerWF20);
            _videoViewerList.Add(videoViewerWF21);
            _videoViewerList.Add(videoViewerWF22);
            _videoViewerList.Add(videoViewerWF23);
            _videoViewerList.Add(videoViewerWF24);
            _videoViewerList.Add(videoViewerWF25);
            _videoViewerList.Add(videoViewerWF26);
            _videoViewerList.Add(videoViewerWF27);
            _videoViewerList.Add(videoViewerWF28);
            _videoViewerList.Add(videoViewerWF29);
            _videoViewerList.Add(videoViewerWF30);
            _videoViewerList.Add(videoViewerWF31);
            _videoViewerList.Add(videoViewerWF32);
            _videoViewerList.Add(videoViewerWF33);
            _videoViewerList.Add(videoViewerWF34);
            _videoViewerList.Add(videoViewerWF35);
            _videoViewerList.Add(videoViewerWF36);
        }

        private void CreateIPCameraHandlers()
        {
            var i = 0;
            while (i < 36)
            {
                ModelList.Add(new IpCameraHandler());
                i++;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var i = 0;
            while (i < _videoViewerList.Count)
            {
                _videoViewerList[i].SetImageProvider(ModelList[i].ImageProvider);
                i++;
            }

            foreach (var item in ModelList)
            {
                item.CameraStateChanged += ModelCameraStateChanged;
                item.CameraErrorOccurred += ModelCameraErrorOccurred;
            }

            comboBox_Direction.DataSource = Enum.GetValues(typeof(PatrolDirection));

            ActiveCameraCombo.SelectedIndex = 0;
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            foreach (var item in ModelList)
                item.Stop();

            ModelList.Clear();
            ModelList = null;

            foreach (var viewer in _videoViewerList)
            {
                viewer.Stop();
                viewer.Dispose();
            }

            if (_currentModel != null)
                _currentModel.Stop();

            _videoViewerList.Clear();
            _videoViewerList = null;
        }

        private void ModelCameraStateChanged(object sender, CameraStateEventArgs e)
        {
            InvokeGuiThread(() =>
            {
                Log.Write("Camera state: " + e.State);
                switch (e.State)
                {
                    case CameraState.Streaming:
                        button_Connect.Enabled = false;
                        button_Disconnect.Enabled = true;
                        _currentVideoViewer.Start();
                        ClearFields();
                        GetCameraStreams();

                        if (_currentModel.Camera.UriType != CameraUriType.RTSP)
                            InitializeTrackBars();
                        break;

                    case CameraState.Disconnected:
                    case CameraState.Error:
                        button_Disconnect.Enabled = false;
                        _currentVideoViewer.Stop();
                        button_Connect.Enabled = true;
                        break;
                }
            });
        }

        private void GetCameraStreams()
        {
            if (_currentModel == null || _currentModel.Camera == null) return;
            if (_currentModel.Camera.AvailableStreams.Any())
            {
                var selected = 0;
                InvokeGuiThread(() =>
                {
                    for (var index = 0; index < _currentModel.Camera.AvailableStreams.Count(); index++)
                    {
                        if (_currentModel.Camera.CurrentStream.Name == _currentModel.Camera.AvailableStreams[index].Name)
                        {
                            selected = index;
                        }


                    }


                });
            }
        }

        private void ModelCameraErrorOccurred(object sender, CameraErrorEventArgs e)
        {
            InvokeGuiThread(() => Log.Write("Camera error: " + (e.Details ?? e.Error.ToString())));
        }

        #region Connect - Disconnect

        private void button_Connect_Click(object sender, EventArgs e)
        {
            ClearFields();

            if (tb_cameraUrl.Text.ToUpper().Trim().StartsWith("RTSP://"))
                Log.Write("Connecting to a stream of ONVIF device by RTSP");
            else if (tb_cameraUrl.Text.ToUpper().Trim().StartsWith("HTTP://"))
                Log.Write("Connecting to ONVIF device by HTTP");
            else if (tb_cameraUrl.Text.ToUpper().Trim().StartsWith("USB://"))
                Log.Write("Connecting to Webcamera device");
            ConnectIpCam();
        }

        private void button_Disconnect_Click(object sender, EventArgs e)
        {
            if (ActiveCameraCombo.SelectedIndex != -1 && ModelList[ActiveCameraCombo.SelectedIndex].Camera != null)
                _currentModel.Disconnect();

            ClearFields();
        }

        private void ConnectIpCam()
        {
            if (_currentModel == null) return;
            _currentModel.ConnectOnvifCamera(_myCameraUrlBuilder.CameraURL);
            _currentVideoViewer.Start();
        }

        #endregion

        #region LOG

        void Log_OnLogMessageReceived(object sender, LogEventArgs e)
        {
            InvokeGuiThread(() =>
            {
                logListBox.Items.Add(e.LogMessage);
                LogScroll();
            });
        }

        void LogScroll()
        {
            logListBox.SelectedIndex = logListBox.Items.Count - 1;
            logListBox.SelectedIndex = -1;
        }

        #endregion

        private void ClearFields()
        {
            InvokeGuiThread(() =>
            {


            });
        }

        #region Stream select
        private void GetInfos()
        {
            if (_currentModel.Camera.UriType == CameraUriType.RTSP) return;
            InvokeGuiThread(() =>
            {

            });
        }
        #endregion

        #region Image Settings





        private void RefreshFrameRate()
        {
            FrameRateLabel.Text = _currentModel.Camera.CurrentStream.VideoEncoding.FrameRate.ToString(CultureInfo.InvariantCulture);

        }

        private void InitializeTrackBars()
        {
            if (_currentModel == null || _currentModel.Camera == null || _currentModel.Camera.UriType == CameraUriType.RTSP) return;
            InvokeGuiThread(() =>
            {
                if (_currentModel.Camera.ImagingSettings != null)
                {







                }
                if (_currentModel.Camera.CurrentStream.VideoEncoding == null) return;
                FrameRateLabel.Text = _currentModel.Camera.CurrentStream.VideoEncoding.FrameRate.ToString(CultureInfo.InvariantCulture);
            });
        }

        #endregion


        private void InvokeGuiThread(Action action)
        {
            BeginInvoke(action);
        }

        private void Flip(object sender, EventArgs e)
        {

            {
                _currentVideoViewer.FlipMode = FlipMode.FlipY;
                return;
            }


        }

        private void MouseDownMove(object sender, MouseEventArgs e)
        {
            var button = sender as Button;
            if (_currentModel != null && button != null)
                _currentModel.Move(button.Text);
        }

        private void MouseUpMove(object sender, MouseEventArgs e)
        {
            if (_currentModel != null && _currentModel.Camera != null)
                _currentModel.Camera.CameraMovement.StopMovement();
        }

        private void button_Home_Click(object sender, EventArgs e)
        {
            if (_currentModel != null && _currentModel.Camera != null)
                _currentModel.Camera.CameraMovement.GoToHome();
        }

        private void button_SetHome_Click(object sender, EventArgs e)
        {
            if (_currentModel != null && _currentModel.Camera != null)
                _currentModel.Camera.CameraMovement.SetHome();
        }

        private void button_AddPreset_Click(object sender, EventArgs e)
        {
            if (_currentModel == null || _currentModel.Camera == null) return;
            _currentModel.Camera.CameraMovement.Preset.Add();
            comboBox_Presets.DataSource = _currentModel.Camera.CameraMovement.Preset.GetPresets();
        }

        private void button_PresetMove_Click(object sender, EventArgs e)
        {
            var preset = (IPCameraPreset)comboBox_Presets.SelectedItem;
            if (_currentModel == null || _currentModel.Camera == null || preset == null) return;
            _currentModel.Camera.CameraMovement.Preset.MoveTo(preset.Name);
        }

        private void button_PresetDelete_Click(object sender, EventArgs e)
        {
            if (_currentModel == null || _currentModel.Camera == null) return;
            var preset = (IPCameraPreset)comboBox_Presets.SelectedItem;
            if (preset == null) return;
            comboBox_Presets.DataSource = null;
            comboBox_Presets.Items.Remove(preset);
            _currentModel.Camera.CameraMovement.Preset.Remove(preset.Name);
            comboBox_Presets.DataSource = _currentModel.Camera.CameraMovement.Preset.GetPresets();
            comboBox_Presets.SelectedIndex = -1;
        }

        private void button_ScanStart_Click(object sender, EventArgs e)
        {
            try
            {
                if (_currentModel == null || _currentModel.Camera == null) return;
                if (String.IsNullOrEmpty(textBox_Duration.Text)) return;
                var patrol = (PatrolDirection)comboBox_Direction.SelectedItem;
                var duration = double.Parse(textBox_Duration.Text);
                _currentModel.Camera.CameraMovement.Patrol(patrol, duration);
            }
            catch (Exception exception)
            {
                Log.Write(exception.Message);
            }
        }

        private void button_ScanStop_Click(object sender, EventArgs e)
        {
            if (_currentModel == null || _currentModel.Camera == null) return;
            _currentModel.Camera.CameraMovement.StopMovement();
        }

        private void ActiveCameraCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentModel = ModelList[ActiveCameraCombo.SelectedIndex];
            _currentVideoViewer = _videoViewerList[ActiveCameraCombo.SelectedIndex];

            if (_currentModel.Camera == null)
            {
                tb_cameraUrl.Text = String.Empty;
                button_Connect.Enabled = false;
                button_Disconnect.Enabled = false;
            }
            else if (_currentModel.Camera.State == CameraState.Streaming)
            {
                tb_cameraUrl.Text = _currentModel.Camera.CameraURL;
                button_Connect.Enabled = false;
                button_Disconnect.Enabled = true;
            }

            ClearFields();
            GetCameraStreams();

            if (_currentModel == null || _currentModel.Camera == null) return;
            InitializeTrackBars();
        }
        private void btn_compose_Click(object sender, EventArgs e)
        {
            var result = _myCameraUrlBuilder.ShowDialog();

            if (result != DialogResult.OK) return;

            tb_cameraUrl.Text = _myCameraUrlBuilder.CameraURL;

            button_Connect.Enabled = true;
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void logListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button_ZoomIn_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
           
        }

        private void button7_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void groupBox9_Enter(object sender, EventArgs e)
        {

        }

        private void button_UpLeft_Click(object sender, EventArgs e)
        {

        }

        private void button_Up_Click(object sender, EventArgs e)
        {

        }

        private void button_UpRight_Click(object sender, EventArgs e)
        {

        }

        private void videoViewerWF6_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 6";
        }

        private void videoViewerWF5_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 5";
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            HideAllGroupBoxes();
            // Hiển thị GroupBox thứ nhất          
            CameraBox1.Visible = true;           
            //Lấp đầy Panel
            CameraBox1.Size = panel1.Size;
            videoViewerWF2.Size = CameraBox1.Size;

        }

        private void button6_Click(object sender, EventArgs e)
        {
            int SizeX = (panel1.Height)-1;
            int SizeY = (panel1.Width)-1;

            HideAllGroupBoxes();
            CameraBox1.Size = new Size(SizeY/2, SizeX/2);           
            CameraBox1.Location = new Point(0, 0);
            CameraBox1.Visible = true;
            videoViewerWF2.Size = CameraBox1.Size;

            CameraBox2.Size = new Size(SizeY/2, SizeX/2);
            int x = CameraBox1.Location.X + CameraBox1.Width + 1;
            int y = CameraBox1.Location.Y;
            CameraBox2.Location = new Point(x, y);
            CameraBox2.Visible = true;
            videoViewerWF3.Size = CameraBox2.Size;

            CameraBox3.Size = new Size(SizeY/2, SizeX/2);
            int a = CameraBox1.Location.X ;
            int b = CameraBox1.Location.Y + CameraBox1.Height + 1;
            CameraBox3.Location = new Point(a, b);
            CameraBox3.Visible = true;
            videoViewerWF4.Size = CameraBox1.Size;

            CameraBox4.Size = new Size(SizeY/2, SizeX/2);
            int c = CameraBox3.Location.X + CameraBox3.Width + 1;
            int d = CameraBox3.Location.Y;
            CameraBox4.Location = new Point(c, d);
            CameraBox4.Visible = true;
            videoViewerWF1.Size = CameraBox4.Size;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            int SizeX = (panel1.Height) - 3;
            int SizeY = (panel1.Width) - 3;

            HideAllGroupBoxes();
            CameraBox1.Size = new Size(SizeY/3, SizeX/3);
            CameraBox1.Location = new Point(0, 0);
            CameraBox1.Visible = true;
            videoViewerWF2.Size = CameraBox1.Size;
            //
            CameraBox2.Size = new Size(SizeY / 3, SizeX / 3);
            int x = CameraBox1.Location.X + CameraBox1.Width + 1;
            int y = CameraBox1.Location.Y;
            CameraBox2.Location = new Point(x, y);
            CameraBox2.Visible = true;
            videoViewerWF3.Size = CameraBox2.Size;
            //
            CameraBox3.Size = new Size(SizeY / 3, SizeX / 3);
            int a = CameraBox2.Location.X + CameraBox2.Width + 1;
            int b = CameraBox2.Location.Y;
            CameraBox3.Location = new Point(a, b);
            CameraBox3.Visible = true;
            videoViewerWF4.Size = CameraBox3.Size;
            //
            CameraBox4.Size = new Size(SizeY / 3, SizeX / 3);
            int c = CameraBox1.Location.X;
            int d = CameraBox1.Location.Y + CameraBox1.Height + 1 ;
            CameraBox4.Location = new Point(c, d);
            CameraBox4.Visible = true;
            videoViewerWF1.Size = CameraBox4.Size;
            //
            groupBox4.Size = new Size(SizeY / 3, SizeX / 3);
            int j = CameraBox4.Location.X + CameraBox4.Width + 1;
            int k = CameraBox4.Location.Y;
            groupBox4.Location = new Point(j, k);
            groupBox4.Visible = true;
            videoViewerWF5.Size = groupBox4.Size;
            //
            groupBox5.Size = new Size(SizeY / 3, SizeX / 3);
            int p = groupBox4.Location.X + CameraBox4.Width + 1;
            int q = groupBox4.Location.Y;
            groupBox5.Location = new Point(p, q);
            groupBox5.Visible = true;
            videoViewerWF6.Size = groupBox5.Size;
            //
            groupBox6.Size = new Size(SizeY / 3, SizeX / 3);
            int g = CameraBox4.Location.X;
            int f = CameraBox4.Location.Y + CameraBox4.Height + 1;
            groupBox6.Location = new Point(g, f);
            groupBox6.Visible = true;
            videoViewerWF7.Size = groupBox6.Size;
            //
            groupBox8.Size = new Size(SizeY / 3, SizeX / 3);
            int v = groupBox6.Location.X + groupBox6.Width + 1;
            int z = groupBox6.Location.Y;
            groupBox8.Location = new Point(v, z);
            groupBox8.Visible = true;
            videoViewerWF8.Size = groupBox8.Size;
            //
            groupBox9.Size = new Size(SizeY / 3, SizeX / 3);
            int r = groupBox8.Location.X + groupBox8.Width + 1;
            int t = groupBox8.Location.Y;
            groupBox9.Location = new Point(r, t);
            groupBox9.Visible = true;
            videoViewerWF9.Size = groupBox9.Size;
        }
        //Xem 36 Camera
        private void button9_Click(object sender, EventArgs e)
        {
            int SizeX = (panel1.Height) - 5;
            int SizeY = (panel1.Width) - 5;
            HideAllGroupBoxes();
            //
            CameraBox1.Size = new Size(SizeY / 6, SizeX / 6);
            CameraBox1.Location = new Point(0, 0);
            CameraBox1.Visible = true;
            videoViewerWF2.Size = CameraBox1.Size;
            //
            CameraBox2.Size = new Size(SizeY / 6, SizeX / 6);
            int x2 = CameraBox1.Location.X + CameraBox1.Width + 1;
            int y2 = CameraBox1.Location.Y;
            CameraBox2.Location = new Point(x2, y2);
            CameraBox2.Visible = true;
            videoViewerWF3.Size = CameraBox2.Size;
            //
            CameraBox3.Size = new Size(SizeY / 6, SizeX / 6);
            int x3 = CameraBox2.Location.X + CameraBox2.Width + 1;
            int y3 = CameraBox2.Location.Y;
            CameraBox3.Location = new Point(x3, y3);
            CameraBox3.Visible = true;
            videoViewerWF4.Size = CameraBox3.Size;
            //
            CameraBox4.Size = new Size(SizeY / 6, SizeX / 6);
            int x4 = CameraBox3.Location.X + CameraBox1.Width + 1;
            int y4 = CameraBox3.Location.Y;
            CameraBox4.Location = new Point(x4, y4);
            CameraBox4.Visible = true;
            videoViewerWF1.Size = CameraBox4.Size;
            //
            groupBox4.Size = new Size(SizeY / 6, SizeX / 6);
            int x5 = CameraBox4.Location.X + CameraBox4.Width + 1;
            int y5 = CameraBox4.Location.Y;
            groupBox4.Location = new Point(x5, y5);
            groupBox4.Visible = true;
            videoViewerWF5.Size = groupBox4.Size;
            //
            groupBox5.Size = new Size(SizeY / 6, SizeX / 6);
            int x6 = groupBox4.Location.X + CameraBox4.Width + 1;
            int y6 = groupBox4.Location.Y;
            groupBox5.Location = new Point(x6, y6);
            groupBox5.Visible = true;
            videoViewerWF6.Size = groupBox5.Size;
            //
            groupBox6.Size = new Size(SizeY / 6, SizeX / 6);
            int x7 = CameraBox1.Location.X;
            int y7 = CameraBox1.Location.Y + CameraBox1.Height + 1;
            groupBox6.Location = new Point(x7, y7);
            groupBox6.Visible = true;
            videoViewerWF7.Size = groupBox6.Size;
            //
            groupBox8.Size = new Size(SizeY / 6, SizeX / 6);
            int x8 = groupBox6.Location.X + groupBox6.Width + 1;
            int y8 = groupBox6.Location.Y;
            groupBox8.Location = new Point(x8, y8);
            groupBox8.Visible = true;
            videoViewerWF8.Size = groupBox8.Size;
            //
            groupBox9.Size = new Size(SizeY / 6, SizeX / 6);
            int x9 = groupBox8.Location.X + groupBox8.Width + 1;
            int y9 = groupBox8.Location.Y;
            groupBox9.Location = new Point(x9, y9);
            groupBox9.Visible = true;
            videoViewerWF9.Size = groupBox9.Size;
            //
            groupBox12.Size = new Size(SizeY / 6, SizeX / 6);
            int x10 = groupBox9.Location.X + groupBox9.Width + 1;
            int y10 = groupBox9.Location.Y;
            groupBox12.Location = new Point(x10, y10);
            groupBox12.Visible = true;
            videoViewerWF10.Size = groupBox12.Size;
            //
            groupBox13.Size = new Size(SizeY / 6, SizeX / 6);
            int x11 = groupBox12.Location.X + groupBox12.Width + 1;
            int y11 = groupBox12.Location.Y;
            groupBox13.Location = new Point(x11, y11);
            groupBox13.Visible = true;
            videoViewerWF11.Size = groupBox13.Size;
            //
            groupBox14.Size = new Size(SizeY / 6, SizeX / 6);
            int x12 = groupBox13.Location.X + groupBox8.Width + 1;
            int y12 = groupBox13.Location.Y;
            groupBox14.Location = new Point(x12, y12);
            groupBox14.Visible = true;
            videoViewerWF12.Size = groupBox14.Size;
            //
            groupBox15.Size = new Size(SizeY / 6, SizeX / 6);
            int x13 = groupBox6.Location.X;
            int y13 = groupBox6.Location.Y + groupBox6.Height + 1;
            groupBox15.Location = new Point(x13, y13);
            groupBox15.Visible = true;
            videoViewerWF13.Size = groupBox15.Size;
            //
            groupBox16.Size = new Size(SizeY / 6, SizeX / 6);
            int x14 = groupBox15.Location.X + groupBox15.Width + 1;
            int y14 = groupBox15.Location.Y;
            groupBox16.Location = new Point(x14, y14);
            groupBox16.Visible = true;
            videoViewerWF14.Size = groupBox16.Size;
            //
            groupBox17.Size = new Size(SizeY / 6, SizeX / 6);
            int x15 = groupBox16.Location.X + groupBox16.Width + 1;
            int y15 = groupBox16.Location.Y;
            groupBox17.Location = new Point(x15, y15);
            groupBox17.Visible = true;
            videoViewerWF15.Size = groupBox17.Size;
            //
            groupBox18.Size = new Size(SizeY / 6, SizeX / 6);
            int x16 = groupBox17.Location.X + groupBox17.Width + 1;
            int y16 = groupBox17.Location.Y;
            groupBox18.Location = new Point(x16, y16);
            groupBox18.Visible = true;
            videoViewerWF16.Size = groupBox18.Size;
            //
            groupBox19.Size = new Size(SizeY / 6, SizeX / 6);
            int x17 = groupBox18.Location.X + groupBox18.Width + 1;
            int y17 = groupBox18.Location.Y;
            groupBox19.Location = new Point(x17, y17);
            groupBox19.Visible = true;
            videoViewerWF17.Size = groupBox19.Size;
            //
            groupBox20.Size = new Size(SizeY / 6, SizeX / 6);
            int x18 = groupBox19.Location.X + groupBox19.Width + 1;
            int y18 = groupBox19.Location.Y;
            groupBox20.Location = new Point(x18, y18);
            groupBox20.Visible = true;
            videoViewerWF18.Size = groupBox20.Size;
            //
            groupBox21.Size = new Size(SizeY / 6, SizeX / 6);
            int x19 = groupBox15.Location.X;
            int y19 = groupBox15.Location.Y + groupBox15.Height + 1;
            groupBox21.Location = new Point(x19, y19);
            groupBox21.Visible = true;
            videoViewerWF19.Size = groupBox21.Size;
            //
            groupBox22.Size = new Size(SizeY / 6, SizeX / 6);
            int x20 = groupBox21.Location.X + groupBox21.Width + 1;
            int y20 = groupBox21.Location.Y;
            groupBox22.Location = new Point(x20, y20);
            groupBox22.Visible = true;
            videoViewerWF20.Size = groupBox22.Size;
            //
            groupBox23.Size = new Size(SizeY / 6, SizeX / 6);
            int x21 = groupBox22.Location.X + groupBox22.Width + 1;
            int y21 = groupBox22.Location.Y;
            groupBox23.Location = new Point(x21, y21);
            groupBox23.Visible = true;
            videoViewerWF21.Size = groupBox23.Size;
            //
            groupBox24.Size = new Size(SizeY / 6, SizeX / 6);
            int x22 = groupBox23.Location.X + groupBox23.Width + 1;
            int y22 = groupBox23.Location.Y;
            groupBox24.Location = new Point(x22, y22);
            groupBox24.Visible = true;
            videoViewerWF22.Size = groupBox24.Size;
            //
            groupBox25.Size = new Size(SizeY / 6, SizeX / 6);
            int x23 = groupBox24.Location.X + groupBox24.Width + 1;
            int y23 = groupBox24.Location.Y;
            groupBox25.Location = new Point(x23, y23);
            groupBox25.Visible = true;
            videoViewerWF23.Size = groupBox25.Size;
            //
            groupBox26.Size = new Size(SizeY / 6, SizeX / 6);
            int x24 = groupBox25.Location.X + groupBox25.Width + 1;
            int y24 = groupBox25.Location.Y;
            groupBox26.Location = new Point(x24, y24);
            groupBox26.Visible = true;
            videoViewerWF24.Size = groupBox26.Size;
            //
            groupBox27.Size = new Size(SizeY / 6, SizeX / 6);
            int x25 = groupBox21.Location.X;
            int y25 = groupBox21.Location.Y + groupBox21.Height + 1;
            groupBox27.Location = new Point(x25, y25);
            groupBox27.Visible = true;
            videoViewerWF25.Size = groupBox27.Size;
            //
            groupBox28.Size = new Size(SizeY / 6, SizeX / 6);
            int x26 = groupBox27.Location.X + groupBox27.Width + 1;
            int y26 = groupBox27.Location.Y;
            groupBox28.Location = new Point(x26, y26);
            groupBox28.Visible = true;
            videoViewerWF26.Size = groupBox28.Size;
            //
            groupBox29.Size = new Size(SizeY / 6, SizeX / 6);
            int x27 = groupBox28.Location.X + groupBox28.Width + 1;
            int y27 = groupBox28.Location.Y;
            groupBox29.Location = new Point(x27, y27);
            groupBox29.Visible = true;
            videoViewerWF27.Size = groupBox29.Size;
            //
            groupBox30.Size = new Size(SizeY / 6, SizeX / 6);
            int x28 = groupBox29.Location.X + groupBox29.Width + 1;
            int y28 = groupBox29.Location.Y;
            groupBox30.Location = new Point(x28, y28);
            groupBox30.Visible = true;
            videoViewerWF28.Size = groupBox30.Size;
            //
            groupBox31.Size = new Size(SizeY / 6, SizeX / 6);
            int x29 = groupBox30.Location.X + groupBox30.Width + 1;
            int y29 = groupBox30.Location.Y;
            groupBox31.Location = new Point(x29, y29);
            groupBox31.Visible = true;
            videoViewerWF29.Size = groupBox31.Size;
            //
            groupBox32.Size = new Size(SizeY / 6, SizeX / 6);
            int x30 = groupBox31.Location.X + groupBox31.Width + 1;
            int y30 = groupBox31.Location.Y;
            groupBox32.Location = new Point(x30, y30);
            groupBox32.Visible = true;
            videoViewerWF30.Size = groupBox32.Size;
            //
            groupBox33.Size = new Size(SizeY / 6, SizeX / 6);
            int x31 = groupBox27.Location.X;
            int y31 = groupBox27.Location.Y + groupBox27.Height+1;
            groupBox33.Location = new Point(x31, y31);
            groupBox33.Visible = true;
            videoViewerWF31.Size = groupBox33.Size;
            //
            groupBox34.Size = new Size(SizeY / 6, SizeX / 6);
            int x32 = groupBox33.Location.X + groupBox33.Width + 1;
            int y32 = groupBox33.Location.Y;
            groupBox34.Location = new Point(x32, y32);
            groupBox34.Visible = true;
            videoViewerWF32.Size = groupBox34.Size;
            //
            groupBox35.Size = new Size(SizeY / 6, SizeX / 6);
            int x33 = groupBox34.Location.X + groupBox34.Width + 1;
            int y33 = groupBox34.Location.Y;
            groupBox35.Location = new Point(x33, y33);
            groupBox35.Visible = true;
            videoViewerWF33.Size = groupBox35.Size;
            //
            groupBox36.Size = new Size(SizeY / 6, SizeX / 6);
            int x34 = groupBox35.Location.X + groupBox35.Width + 1;
            int y34 = groupBox35.Location.Y;
            groupBox36.Location = new Point(x34, y34);
            groupBox36.Visible = true;
            videoViewerWF34.Size = groupBox36.Size;
            //
            groupBox37.Size = new Size(SizeY / 6, SizeX / 6);
            int x35 = groupBox36.Location.X + groupBox36.Width + 1;
            int y35 = groupBox36.Location.Y;
            groupBox37.Location = new Point(x35, y35);
            groupBox37.Visible = true;
            videoViewerWF35.Size = groupBox37.Size;
            //
            groupBox38.Size = new Size(SizeY / 6, SizeX / 6);
            int x36 = groupBox37.Location.X + groupBox37.Width + 1;
            int y36 = groupBox37.Location.Y;
            groupBox38.Location = new Point(x36, y36);
            groupBox38.Visible = true;
            videoViewerWF36.Size = groupBox38.Size;
        }
        private void HideAllGroupBoxes()
        {
            //Ẩn tất cả các groupbox
            CameraBox1.Visible = false;
            CameraBox2.Visible = false;
            CameraBox3.Visible = false;
            CameraBox4.Visible = false;
            groupBox4.Visible = false;
            groupBox5.Visible = false;
            groupBox6.Visible = false;
            groupBox8.Visible = false;
            groupBox9.Visible = false;
            groupBox12.Visible = false;
            groupBox13.Visible = false;
            groupBox14.Visible = false;
            groupBox15.Visible = false;
            groupBox16.Visible = false;
            groupBox17.Visible = false;
            groupBox18.Visible = false;
            groupBox19.Visible = false;
            groupBox20.Visible = false;
            groupBox21.Visible = false;
            groupBox22.Visible = false;
            groupBox23.Visible = false;
            groupBox24.Visible = false;
            groupBox25.Visible = false;
            groupBox26.Visible = false;
            groupBox27.Visible = false;
            groupBox28.Visible = false;
            groupBox29.Visible = false;
            groupBox30.Visible = false;
            groupBox31.Visible = false;
            groupBox32.Visible = false;
            groupBox33.Visible = false;
            groupBox34.Visible = false;
            groupBox35.Visible = false;
            groupBox36.Visible = false;
            groupBox37.Visible = false;
            groupBox38.Visible = false;         
        }

        private void tb_cameraUrl_TextChanged(object sender, EventArgs e)
        {

        }

        private void CameraBox3_Enter(object sender, EventArgs e)
        {

        }

        private void videoViewerWF8_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 8";
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnDangxuat_Click(object sender, EventArgs e)
        {
            isExit = false;
            this.Close();
            formDangNhap dangnhap = new formDangNhap();
            dangnhap.Show();
        }

        private void btnDangky_Click(object sender, EventArgs e)
        {
            formDangKy f = new formDangKy();
            f.ShowDialog();
        }

        private void groupBox10_Enter(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
   
        private void CameraBox1_Enter(object sender, EventArgs e)
        {
           
        }

        private void videoViewerWF2_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 1";
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void videoViewerWF3_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 2";
        }

        private void videoViewerWF4_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 3";
        }

        private void videoViewerWF1_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 4";
        }

        private void videoViewerWF7_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 7";
        }

        private void videoViewerWF9_Click(object sender, EventArgs e)
        {
            label1.Text = "Camera 9";
        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }

        private void videoViewerWF10_Click(object sender, EventArgs e)
        {

        }

        private void button_ThemKhu_Click(object sender, EventArgs e)
        {
            string tenKhu = comboBox_KhuVuc.Text;

            if (!string.IsNullOrEmpty(tenKhu))
            {
                if (!comboBox_KhuVuc.Items.Contains(tenKhu))
                {
                    comboBox_KhuVuc.Items.Add(tenKhu);
                    string connectionString = "Data Source=DESKTOP-Q6N4J46\\SQLEXPRESS;Initial Catalog=db_QuanLyCamera;Integrated Security=True";
                    string query = "INSERT INTO tbl_KhuVuc (TenKhu) VALUES (@TenKhu)";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@TenKhu", tenKhu);

                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    MessageBox.Show("Tên khu đã tồn tại. Vui lòng nhập tên khu khác.", "Thông báo");
                }
            }
        }

        private void button_XoaKhu_Click(object sender, EventArgs e)
        {
            if (comboBox_KhuVuc.SelectedItem != null)
            {
                string tenKhu = comboBox_KhuVuc.SelectedItem.ToString();

                comboBox_KhuVuc.Items.Remove(tenKhu);

                string connectionString = "Data Source=DESKTOP-Q6N4J46\\SQLEXPRESS;Initial Catalog=db_QuanLyCamera;Integrated Security=True";
                string query = "DELETE FROM tbl_KhuVuc WHERE TenKhu = @TenKhu";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TenKhu", tenKhu);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}