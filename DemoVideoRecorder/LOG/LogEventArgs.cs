using System;

namespace _03_Onvif_Network_Video_Recorder.LOG
{
    class LogEventArgs : EventArgs
    {
        public string LogMessage;

        public LogEventArgs(string log)
        {
            LogMessage = log;
        }
    }
}
