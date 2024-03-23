using System;
using System.Text;
using Ozeki.Camera;
using Ozeki.Media;
using Ozeki.VoIP;
using _03_Onvif_Network_Video_Recorder.LOG;
using System.Drawing;

namespace _03_Onvif_Network_Video_Recorder
{
    class IpCameraHandler
    {
        private MediaConnector Connector;

        public OzekiCamera Camera { get; private set; }
        public DrawingImageProvider ImageProvider { get; private set; }
        public SnapshotHandler Snapshot { get; private set; }

        private MPEG4Recorder _mpeg4Recorder;

        public event EventHandler<CameraStateEventArgs> CameraStateChanged;
        public event EventHandler<CameraErrorEventArgs> CameraErrorOccurred;
       
        public IpCameraHandler()
        {
            ImageProvider = new DrawingImageProvider();
            Snapshot = new SnapshotHandler();
            Connector = new MediaConnector();
        }

        public void ConnectOnvifCamera(string cameraUrl)
        {
            if (Camera != null)
                CloseCamera();

            // Gets the camera, which can be reached by the address, and requires authentication.
            Camera = new OzekiCamera(cameraUrl);

            if (Camera == null) return;
            Connector.Connect(Camera.VideoChannel, ImageProvider);
            Connector.Connect(Camera.VideoChannel, Snapshot);

            Camera.CameraStateChanged += Camera_CameraStateChanged;
            Camera.CameraErrorOccurred += Camera_CameraErrorOccurred;

            Camera.Start();
        }

        private void Camera_CameraErrorOccurred(object sender, CameraErrorEventArgs e)
        {
            // signal to GUI
            var handler = CameraErrorOccurred;
            if (handler != null)
                handler(this, e);
        }

        private void Camera_CameraStateChanged(object sender, CameraStateEventArgs e)
        {
            // signal to GUI
            var handler = CameraStateChanged;
            if (handler != null)
                handler(this, e);
        }

        public void Disconnect()
        {
            CloseCamera();
        }

        private void CloseCamera()
        {
            if (Camera == null)
                return;

            Connector.Disconnect(Camera.VideoChannel, ImageProvider);
            Connector.Disconnect(Camera.VideoChannel, Snapshot);
            Camera.Disconnect();
            Camera.CameraStateChanged -= Camera_CameraStateChanged;
            Camera.CameraErrorOccurred -= Camera_CameraErrorOccurred;
            Camera = null;
        }

        public void Stop()
        {
            if (Camera != null)
            {
                
               StopVideoCapture();
                CloseCamera();
                Connector.Dispose();
                ImageProvider.Dispose();
                Snapshot.Dispose();
            }
        }

        public string GetDeviceInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"Firmware - " + Camera.CameraInfo.DeviceInfo.Firmware + "\n");
            sb.AppendLine(@"Hardware ID - " + Camera.CameraInfo.DeviceInfo.HardwareId + "\n");
            sb.AppendLine(@"Manufacture - " + Camera.CameraInfo.DeviceInfo.Manufacturer + "\n");
            sb.AppendLine(@"Model - " + Camera.CameraInfo.DeviceInfo.Model + "\n");
            sb.AppendLine(@"Serial number - " + Camera.CameraInfo.DeviceInfo.SerialNumber + "\n");

            return sb.ToString();
        }

        public string StreamInfoAudio()
        {
            if (Camera.CurrentStream.AudioEncoding == null) return "";
            var sb = new StringBuilder();

            sb.AppendLine(" - Audio Encoding \n");
            sb.AppendLine("\t Bitrate - " + Camera.CurrentStream.AudioEncoding.Bitrate + "\n");
            sb.AppendLine("\t Encoding - " + Camera.CurrentStream.AudioEncoding.Encoding + "\n");
            sb.AppendLine("\t Samplerate - " + Camera.CurrentStream.AudioEncoding.SampleRate + "\n");
            sb.AppendLine("\t Session time out - " + Camera.CurrentStream.AudioEncoding.SessionTimeOut + "\n");
            sb.AppendLine("\t Use count - " + Camera.CurrentStream.AudioEncoding.UseCount + "\n");

            sb.AppendLine(" - Audio Source \n");
            sb.AppendLine("\t Channels - " + Camera.CurrentStream.AudioSource.Channels + "\n");
            sb.AppendLine("\t Use count - " + Camera.CurrentStream.AudioSource.UseCount + "\n");

            return sb.ToString();
        }

        public string StreamInfoVideo()
        {
            var sb = new StringBuilder();

            sb.AppendLine(" - Video Encoding \n");
            sb.AppendLine("\t Bitrate - " + Camera.CurrentStream.VideoEncoding.BitRate + "\n");
            sb.AppendLine("\t Encoding - " + Camera.CurrentStream.VideoEncoding.Encoding + "\n");
            sb.AppendLine("\t Encoding interval - " + Camera.CurrentStream.VideoEncoding.EncodingInterval + "\n");
            sb.AppendLine("\t Framerate - " + Camera.CurrentStream.VideoEncoding.FrameRate + "\n");
            sb.AppendLine("\t Quality - " + Camera.CurrentStream.VideoEncoding.Quality + "\n");
            sb.AppendLine("\t Resolution - " + Camera.CurrentStream.VideoEncoding.Resolution + "\n");
            sb.AppendLine("\t Session time out - " + Camera.CurrentStream.VideoEncoding.SessionTimeout + "\n");
            sb.AppendLine("\t Use count - " + Camera.CurrentStream.VideoEncoding.UseCount + "\n");

            sb.AppendLine(" - Video Source \n");
            sb.AppendLine("\t Bounds - " + Camera.CurrentStream.VideoSource.Bounds + "\n");
            sb.AppendLine("\t Use count - " + Camera.CurrentStream.VideoSource.UseCount + "\n");

            return sb.ToString();
        }

        public void Move(string direction)
        {
            if (Camera == null) return;
            switch (direction)
            {
                case "Up Left":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.LeftUp);
                    break;
                case "Up":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.Up);
                    break;
                case "Up Right":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.RightUp);
                    break;
                case "Left":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.Left);
                    break;
                case "Right":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.Right);
                    break;
                case "Down Left":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.LeftDown);
                    break;
                case "Down":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.Down);
                    break;
                case "Down Right":
                    Camera.CameraMovement.ContinuousMove(MoveDirection.RightDown);
                    break;
                case "Set home":
                    Camera.CameraMovement.SetHome();
                    break;
                case "In":
                    Camera.CameraMovement.Zoom(MoveDirection.In);
                    break;
                case "Out":
                    Camera.CameraMovement.Zoom(MoveDirection.Out);
                    break;

            }
        }

        public void CreateSnapShot(string path)
        {
            if (Camera == null) return;
            var date = DateTime.Now.Year + "y-" + DateTime.Now.Month + "m-" + DateTime.Now.Day + "d-" +
                       DateTime.Now.Hour + "h-" + DateTime.Now.Minute + "m-" + DateTime.Now.Second + "s";

            string currentpath;
            if (String.IsNullOrEmpty(path))
                currentpath = AppDomain.CurrentDomain.BaseDirectory + date + ".jpg";
            else
                currentpath = path + "\\" + date + ".jpg";

            var snapshot = Snapshot.TakeSnapshot();
            if (snapshot == null)
                return;

            var snapShotImage = (Image)snapshot.ToImage();
            snapShotImage.Save(currentpath, System.Drawing.Imaging.ImageFormat.Jpeg);
            Log.Write("Picture saved at the location: " + currentpath);
        }

        public void StartVideoCapture(string path)
        {
            if (Camera == null) return;

            var date = DateTime.Now.Year + "y-" + DateTime.Now.Month + "m-" + DateTime.Now.Day + "d-" +
                       DateTime.Now.Hour + "h-" + DateTime.Now.Minute + "m-" + DateTime.Now.Second + "s";

            string currentpath;
            if (String.IsNullOrEmpty(path))
                currentpath = AppDomain.CurrentDomain.BaseDirectory + date + ".mp4";
            else
                currentpath = path + "\\" + date + ".mp4";

            _mpeg4Recorder = new MPEG4Recorder(currentpath);
            _mpeg4Recorder.MultiplexFinished += Mpeg4Recorder_MultiplexFinished;

            Connector.Connect(Camera.AudioChannel, _mpeg4Recorder.AudioRecorder);
            Connector.Connect(Camera.VideoChannel, _mpeg4Recorder.VideoRecorder);

            Log.Write("Video capture has been started");
            Log.Write("The captured video will be saved: " + currentpath);
        }

        private void Mpeg4Recorder_MultiplexFinished(object sender, VoIPEventArgs<bool> e)
        {
            var recorder = sender as MPEG4Recorder;
            if (recorder == null) return;
            Connector.Disconnect(Camera.AudioChannel, recorder.AudioRecorder);
            Connector.Disconnect(Camera.VideoChannel, recorder.VideoRecorder);

            recorder.Dispose();

            Log.Write("The captured video has been saved");
        }

        public void StopVideoCapture()
        {
            if (Camera == null || _mpeg4Recorder == null) return;

            _mpeg4Recorder.Multiplex();

            Connector.Disconnect(Camera.AudioChannel, _mpeg4Recorder.AudioRecorder);
            Connector.Disconnect(Camera.VideoChannel, _mpeg4Recorder.VideoRecorder);

            Log.Write("Video capture has been stopped");

        }
    }
}