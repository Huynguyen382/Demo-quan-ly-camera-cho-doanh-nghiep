using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Ozeki.Media;
using _03_Onvif_Network_Video_Recorder.LOG;
using Ozeki.Camera;

namespace _03_Onvif_Network_Video_Recorder
{
    public partial class MainForm : Form
    {
        private IpCameraHandler _currentModel;

        private VideoViewerWF _currentVideoViewer;

        private List<VideoViewerWF> _videoViewerList;

        private List<IpCameraHandler> ModelList;

        private CameraURLBuilderWF _myCameraUrlBuilder;

        public MainForm()
        {
            InitializeComponent();

            Log.OnLogMessageReceived += Log_OnLogMessageReceived;

            _myCameraUrlBuilder = new CameraURLBuilderWF();

            _videoViewerList = new List<VideoViewerWF>();
            ModelList = new List<IpCameraHandler>();

            CreateVideoViewers();
            CreateIPCameraHandlers();
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
        }

        private void CreateIPCameraHandlers()
        {
            var i = 0;
            while (i < 11)
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
            MainForm f = new MainForm();
            f.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            MainForm f = new MainForm();
            f.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MainForm f = new MainForm();
            f.Show();
        } 
    }
}