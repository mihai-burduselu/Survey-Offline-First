using System;

namespace Survey_Offline_First
{
    public class InternetConnectionChangedEventArgs :EventArgs
    {
        public bool IsConnected { get; set; }

        public InternetConnectionChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}
