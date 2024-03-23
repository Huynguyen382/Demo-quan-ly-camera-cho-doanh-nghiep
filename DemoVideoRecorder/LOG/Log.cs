using System;

namespace _03_Onvif_Network_Video_Recorder.LOG
{
    class Log
    {
        private static string _log;
        public static void Write(string logMessage)
        {
            _log = DateTime.Now + " | " + logMessage;
            LogMessageReceived(_log);
        }

        public static event EventHandler<LogEventArgs> OnLogMessageReceived;

        static void LogMessageReceived(string msg)
        {
            var handler = OnLogMessageReceived;
            if (handler != null)
                handler(null, new LogEventArgs(msg));
        }
    }
}
